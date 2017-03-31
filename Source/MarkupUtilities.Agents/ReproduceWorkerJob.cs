using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using MarkupUtilities.Helpers;
using MarkupUtilities.Helpers.Models;
using MarkupUtilities.Helpers.Rsapi.Interfaces;
using Relativity.API;

namespace MarkupUtilities.Agents
{
  /// <summary>
  /// This class abstracts the agent logic to allow unit testing without IIS dependencies
  /// </summary>
  public class ReproduceWorkerJob : AgentJobBase
  {
    private readonly IArtifactQueries _artifactQueries;
    private string _errorContext;
    private readonly IAuditRecordHelper _auditRecordHelper;
    private IErrorQueries ErrorQueries { get; }


    public ReproduceWorkerJob(int agentId, IAgentHelper agentHelper, IQuery queryHelper, IArtifactQueries artifactQueries, IEnumerable<int> resourceGroupIds, IErrorQueries errorQueries)
    {
      AgentId = agentId;
      AgentHelper = agentHelper;
      QueryHelper = queryHelper;
      _artifactQueries = artifactQueries;
      AgentResourceGroupIds = resourceGroupIds;
      ErrorQueries = errorQueries;
      QueueTable = Constant.Tables.ReproduceWorkerQueue;
      _auditRecordHelper = new AuditRecordHelper(QueryHelper);
    }

    public override async Task ExecuteAsync()
    {
      try
      {
        //Check for jobs which stopped unexpectedly on this agent thread
        RaiseMessage($"Resetting records which failed. [Table = {QueueTable}]");
        await ResetUnfishedJobsAsync(AgentHelper.GetDBContext(-1));

        //Retrieve the next record to work on
        RaiseMessage($"Retrieving next record(s) in the queue. [Table = {QueueTable}]");
        var delimitedListOfResourceGroupIds = GetCommaDelimitedListOfResourceIds(AgentResourceGroupIds);

        if (delimitedListOfResourceGroupIds != string.Empty)
        {
          var next = await RetrieveNextAsync(delimitedListOfResourceGroupIds);

          if (TableIsNotEmpty(next))
          {
            var workerQueueRecord = new ReproduceWorkerQueueRecord(next.Rows[0]);
            WorkspaceArtifactId = workerQueueRecord.WorkspaceArtifactId;

            //Process document redactions
            var markupUtilityReproduceJob = await RetrieveReproduceJobAsync(workerQueueRecord);
            //someone cancelled the job or managed to delete it  
            if (markupUtilityReproduceJob == null)
            {
              await Finish(workerQueueRecord, null);
            }
            else if (markupUtilityReproduceJob.Status == Constant.Status.Job.CANCELREQUESTED || markupUtilityReproduceJob.Status == Constant.Status.Job.CANCELLED)
            {
              //you can end up in state Cancel Requestd if you cancel just after the ManagerQueue job completed, in which case update the the status to Cancelled
              await Finish(workerQueueRecord, Constant.Status.Job.CANCELLED);
            }
            else
            {
              await UpdateStatusFieldAsync(workerQueueRecord.ReproduceJobArtifactId, Constant.Status.Job.IN_PROGRESS_WORKER);
              await ProcessRedactionsAsync(workerQueueRecord, markupUtilityReproduceJob);
            }
            RaiseMessage($"Retrieved record(s) in the queue. [Table = {QueueTable}, ID = {RecordId}, Workspace Artifact ID = {WorkspaceArtifactId}]");
          }
          else
          {
            RaiseMessage("No records in the queue for this resource pool.");
          }
        }
        else
        {
          RaiseMessage(Constant.AgentRaiseMessages.AGENT_SERVER_NOT_PART_OF_ANY_RESOURCE_POOL);
        }
      }
      catch (Exception ex)
      {
        RaiseMessage($"Logging error.");
        await LogErrorAsync(ex);
      }
    }

    private async Task ProcessRedactionsAsync(ReproduceWorkerQueueRecord reproduceWorkerQueueRecord, MarkupUtilityReproduceJob markupUtilityReproduceJob)
    {
      try
      {
        //Bulk insert the redactions for the document batch
        DataTable dataTable;
        var relationalGroup = markupUtilityReproduceJob.RelationalField > 0;
        if (relationalGroup)
        {
          dataTable = await ReproduceAcrossRelationalGroup(reproduceWorkerQueueRecord, markupUtilityReproduceJob);
        }
        else
        {
          dataTable = await ReproduceAcrossDocumentSet(reproduceWorkerQueueRecord, markupUtilityReproduceJob);
        }

        //Get the ids from the result and add to audit and history tables. Ideally, this should be in the same db transaction as above....
        await CreateHistoryAndAuditRecords(reproduceWorkerQueueRecord, dataTable, markupUtilityReproduceJob, relationalGroup);
        await Finish(reproduceWorkerQueueRecord, Constant.Status.Job.COMPLETED);
      }
      catch (Exception ex)
      {
        //Set the status of the Export Job to Error
        await UpdateStatusFieldAsync(reproduceWorkerQueueRecord.ReproduceJobArtifactId, Constant.Status.Job.ERROR);
        await UpdateDetailsFieldAsync(reproduceWorkerQueueRecord.ReproduceJobArtifactId, await ConstructDetailsExceptionMessageAsync(ex));
        RaiseMessage($"Logging error.");
        await LogErrorAsync(ex);
      }
    }

    private async Task<DataTable> ReproduceAcrossDocumentSet(ReproduceWorkerQueueRecord reproduceWorkerQueueRecord, MarkupUtilityReproduceJob markupUtilityReproduceJob)
    {
      var dataTable = await BulkInsertRedactionRecordsForDocumentRange(reproduceWorkerQueueRecord);

      if (!string.IsNullOrEmpty(reproduceWorkerQueueRecord.HasAutoRedactionsColumn))
      {
        await BulkUpdateHasAutoRedactionsForDocumentRange(reproduceWorkerQueueRecord);
      }

      await UpdateHasRedactionsOrHighlightsAsync(reproduceWorkerQueueRecord, markupUtilityReproduceJob.DestinationMarkupSetArtifactId, false);
      return dataTable;
    }

    private async Task<DataTable> ReproduceAcrossRelationalGroup(ReproduceWorkerQueueRecord reproduceWorkerQueueRecord, MarkupUtilityReproduceJob markupUtilityReproduceJob)
    {
      var dataTable = await BulkInsertRedactionRecordsForRelationalGroup(reproduceWorkerQueueRecord);

      if (!string.IsNullOrEmpty(reproduceWorkerQueueRecord.HasAutoRedactionsColumn))
      {
        await BulkUpdateHasAutoRedactionsFieldForRelationalGroup(reproduceWorkerQueueRecord);
      }

      await UpdateHasRedactionsOrHighlightsAsync(reproduceWorkerQueueRecord, markupUtilityReproduceJob.DestinationMarkupSetArtifactId, true);
      return dataTable;
    }

    private async Task Finish(ReproduceWorkerQueueRecord reproduceWorkerQueueRecord, string jobStatus)
    {
      await DeleteRecordFromReproduceWorkerQueueAsync(reproduceWorkerQueueRecord.RecordId);
      await CleanUpIfLast(reproduceWorkerQueueRecord, jobStatus);
    }

    private async Task CreateHistoryAndAuditRecords(ReproduceWorkerQueueRecord reproduceWorkerQueueRecord, DataTable dataTable, MarkupUtilityReproduceJob markupUtilityReproduceJob, bool relationalGroup)
    {
      var details = Constant.Status.History.Details.REPRODUCED_REDACTION_IN_DOCUMENT_SET;
      if (relationalGroup)
      {
        details = Constant.Status.History.Details.REPRODUCED_REDACTION_IN_RELATIONAL_GROUP;
      }

      foreach (DataRow row in dataTable.Rows)
      {
        var redactionId = (int)row["ID"];
        var table = await RetrieveRedactionInfoAsync(reproduceWorkerQueueRecord, redactionId);
        var dataRow = table.Rows[0];
        var identifier = (string)dataRow["Identifier"];
        var pageNumber = (int)dataRow["Order"] + 1;
        var markupUtilityHistoryRecordAsync = _artifactQueries.CreateMarkupUtilityHistoryRecordAsync(
            AgentHelper.GetServicesManager(),
            ExecutionIdentity.CurrentUser,
            WorkspaceArtifactId,
            -1,
            identifier,
            pageNumber,
            Constant.ImportJobType.REPRODUCE,
            null,
            Constant.Status.History.COMPLETED,
            details,
            ToStringRedactionData(dataRow),
            redactionId,
            reproduceWorkerQueueRecord.ReproduceJobArtifactId);

        var auditRecordAsync = CreateAuditRecordAsync(dataRow, markupUtilityReproduceJob, (int)dataRow["DocumentArtifactID"]);

        await Task.WhenAll(markupUtilityHistoryRecordAsync, auditRecordAsync);
      }
    }


    public string ToStringRedactionData(DataRow dataRow)
    {
      var sb = new StringBuilder();

      var fields = new[]
      {
        "Order", "X", "Y", "Width", "Height", "MarkupType", "FillA", "FillR", "FillG", "FillB", "BorderSize",
        "BorderA", "BorderR", "BorderG", "BorderB", "BorderStyle",
        "FontName", "FontA", "FontR", "FontG", "FontB", "FontSize", "FontStyle",
        "Text", "ZOrder", "DrawCrossLines"
      };

      foreach (var field in fields)
      {
        sb.Append($" {field}: {dataRow[field]}");
      }

      var retVal = sb.ToString();
      return retVal;
    }

    private async Task<MarkupUtilityReproduceJob> RetrieveReproduceJobAsync(ReproduceWorkerQueueRecord reproduceManagerQueueRecord)
    {
      return await _artifactQueries.RetrieveReproduceJobAsync(
          AgentHelper.GetServicesManager(),
          ExecutionIdentity.CurrentUser,
          WorkspaceArtifactId,
          reproduceManagerQueueRecord.ReproduceJobArtifactId);
    }

    private async Task CreateAuditRecordAsync(DataRow datarow, MarkupUtilityReproduceJob job, int documentArtifactId)
    {
      RaiseMessage($"creating audit record. {_errorContext}");
      await _auditRecordHelper.CreateRedactionAuditRecordAsync(
          AgentHelper.GetDBContext(WorkspaceArtifactId),
          Constant.AuditRecord.AuditAction.REDACTION_CREATED,
          documentArtifactId,
          job.CreatedBy,
          (string)datarow["FileGuid"],
          (int)datarow["ID"],
          (int)datarow["MarkupSetArtifactID"],
          (int)datarow["Order"] + 1);
    }

    private async Task CleanUpIfLast(ReproduceWorkerQueueRecord reproduceWorkerQueueRecord, string jobStatus)
    {
      _errorContext = $"An error occurred while creating the export file. [WorkspaceArtifactId = {WorkspaceArtifactId}, ReproduceJobArtifactId = {reproduceWorkerQueueRecord.ReproduceJobArtifactId}]";

      var queueCount = await QueryHelper.GetJobWorkerRecordCountAsync(AgentHelper.GetDBContext(-1), WorkspaceArtifactId, reproduceWorkerQueueRecord.ReproduceJobArtifactId, Constant.Tables.ReproduceWorkerQueue, "ReproduceJobArtifactID");

      if (queueCount == 0)
      {
        var tasks = new List<Task>();
        if (jobStatus != null)
        {
          tasks.Add(UpdateStatusFieldAsync(reproduceWorkerQueueRecord.ReproduceJobArtifactId, jobStatus));
        }
        tasks.Add(QueryHelper.DropTableAsync(AgentHelper.GetDBContext(reproduceWorkerQueueRecord.WorkspaceArtifactId), reproduceWorkerQueueRecord.RedactionsHoldingTable));
        tasks.Add(QueryHelper.DropTableAsync(AgentHelper.GetDBContext(reproduceWorkerQueueRecord.WorkspaceArtifactId), reproduceWorkerQueueRecord.SavedSearchHoldingTable));
        await Task.WhenAll(tasks);
      }
    }


    private static bool TableIsNotEmpty(DataTable table)
    {
      return (table != null && table.Rows.Count > 0);
    }

    public async Task<DataTable> RetrieveNextAsync(string delimitedListOfResourceGroupIds)
    {
      var next = await QueryHelper.RetrieveNextInReproduceWorkerQueueAsync(AgentHelper.GetDBContext(-1), AgentId, delimitedListOfResourceGroupIds);
      return next;
    }


    public async Task DeleteRecordFromReproduceWorkerQueueAsync(int id)
    {
      RaiseMessage($"Deleting record from ReproduceWorkerQueue. [ID = {id}, Workspace Artifact ID = {WorkspaceArtifactId}, Agent ID = {AgentId}]");
      await QueryHelper.RemoveRecordFromReproduceWorkerQueueAsync(AgentHelper.GetDBContext(-1), id);
      RaiseMessage($"Deleted record from ReproduceWorkerQueue. [ID = {id}, Workspace Artifact ID = {WorkspaceArtifactId}, Agent ID = {AgentId}]");
    }


    private async Task UpdateStatusFieldAsync(int jobArtifactId, string jobStatus)
    {
      await _artifactQueries.UpdateRdoJobTextFieldAsync(
          AgentHelper.GetServicesManager(),
          WorkspaceArtifactId,
          ExecutionIdentity.CurrentUser,
          Constant.Guids.ObjectType.MarkupUtilityReproduceJob,
          jobArtifactId,
          Constant.Guids.Field.MarkupUtilityReproduceJob.Status,
          jobStatus);
    }

    private async Task UpdateDetailsFieldAsync(int jobArtifactId, string jobDetails)
    {
      await _artifactQueries.UpdateRdoJobTextFieldAsync(
          AgentHelper.GetServicesManager(),
          WorkspaceArtifactId,
          ExecutionIdentity.CurrentUser,
          Constant.Guids.ObjectType.MarkupUtilityReproduceJob,
          jobArtifactId,
          Constant.Guids.Field.MarkupUtilityReproduceJob.Details,
          jobDetails);
    }

    private async Task<DataTable> BulkInsertRedactionRecordsForRelationalGroup(ReproduceWorkerQueueRecord reproduceWorkerQueueRecord)
    {
      RaiseMessage($"Bulk inserting redaction records for worker record id {reproduceWorkerQueueRecord.RecordId}. {_errorContext}");
      return await QueryHelper.BulkInsertRedactionRecordsForRelationalGroup(AgentHelper.GetDBContext(reproduceWorkerQueueRecord.WorkspaceArtifactId), reproduceWorkerQueueRecord.SavedSearchHoldingTable, reproduceWorkerQueueRecord.RedactionsHoldingTable, reproduceWorkerQueueRecord.RelationalGroup);
    }

    private async Task<DataTable> BulkUpdateHasAutoRedactionsFieldForRelationalGroup(ReproduceWorkerQueueRecord reproduceWorkerQueueRecord)
    {
      RaiseMessage($"Bulk updating document hasAutoRedactions field for worker record id {reproduceWorkerQueueRecord.RecordId}. {_errorContext}");
      return await QueryHelper.BulkUpdateHasAutoRedactionsFieldForRelationalGroup(AgentHelper.GetDBContext(reproduceWorkerQueueRecord.WorkspaceArtifactId), reproduceWorkerQueueRecord.SavedSearchHoldingTable, reproduceWorkerQueueRecord.RedactionsHoldingTable, reproduceWorkerQueueRecord.RelationalGroup, reproduceWorkerQueueRecord.HasAutoRedactionsColumn);
    }

    private async Task<DataTable> BulkInsertRedactionRecordsForDocumentRange(ReproduceWorkerQueueRecord reproduceWorkerQueueRecord)
    {
      RaiseMessage($"Bulk inserting redaction records for worker record id {reproduceWorkerQueueRecord.RecordId}. {_errorContext}");
      return await QueryHelper.BulkInsertRedactionRecordsForDocumentRange(AgentHelper.GetDBContext(reproduceWorkerQueueRecord.WorkspaceArtifactId), reproduceWorkerQueueRecord.SavedSearchHoldingTable, reproduceWorkerQueueRecord.RedactionsHoldingTable, reproduceWorkerQueueRecord.DocumentIdStart, reproduceWorkerQueueRecord.DocumentIdEnd);
    }

    private async Task<DataTable> BulkUpdateHasAutoRedactionsForDocumentRange(ReproduceWorkerQueueRecord reproduceWorkerQueueRecord)
    {
      RaiseMessage($"Bulk update has auto redactions fields for worker record id {reproduceWorkerQueueRecord.RecordId}. {_errorContext}");
      return await QueryHelper.BulkUpdateHasAutoRedactionsForDocumentRange(AgentHelper.GetDBContext(reproduceWorkerQueueRecord.WorkspaceArtifactId), reproduceWorkerQueueRecord.SavedSearchHoldingTable, reproduceWorkerQueueRecord.RedactionsHoldingTable, reproduceWorkerQueueRecord.DocumentIdStart, reproduceWorkerQueueRecord.DocumentIdEnd, reproduceWorkerQueueRecord.HasAutoRedactionsColumn);
    }

    private async Task<IEnumerable<int>> UpdateHasRedactionsOrHighlightsAsync(ReproduceWorkerQueueRecord reproduceWorkerQueueRecord, int destinationMarkupSet, bool relationalGroup)
    {
      RaiseMessage($"Updating documents with Has Redactions/Highligths for worker record id {reproduceWorkerQueueRecord.RecordId}. {_errorContext}");

      Task<int> redactions;
      Task<int> highlights;
      if (relationalGroup)
      {
        redactions = QueryHelper.UpdateHasRedactionsOrHighlightsAsync(
          AgentHelper.GetDBContext(reproduceWorkerQueueRecord.WorkspaceArtifactId),
          reproduceWorkerQueueRecord.RedactionCodeTypeId,
          reproduceWorkerQueueRecord.MarkupSetRedactionCodeArtifactId, Constant.MarkupType.Redaction.VALUE,
          reproduceWorkerQueueRecord.SavedSearchHoldingTable,
          reproduceWorkerQueueRecord.RedactionsHoldingTable,
          destinationMarkupSet, reproduceWorkerQueueRecord.RelationalGroup);

        highlights = QueryHelper.UpdateHasRedactionsOrHighlightsAsync(
          AgentHelper.GetDBContext(reproduceWorkerQueueRecord.WorkspaceArtifactId),
          reproduceWorkerQueueRecord.RedactionCodeTypeId,
          reproduceWorkerQueueRecord.MarkupSetAnnotationCodeArtifactId,
          Constant.MarkupType.Highlight.VALUE,
          reproduceWorkerQueueRecord.SavedSearchHoldingTable,
          reproduceWorkerQueueRecord.RedactionsHoldingTable,
          destinationMarkupSet, reproduceWorkerQueueRecord.RelationalGroup);
      }
      else
      {
        redactions = QueryHelper.UpdateHasRedactionsOrHighlightsAsync(
          AgentHelper.GetDBContext(reproduceWorkerQueueRecord.WorkspaceArtifactId),
          reproduceWorkerQueueRecord.RedactionCodeTypeId,
          reproduceWorkerQueueRecord.MarkupSetRedactionCodeArtifactId, Constant.MarkupType.Redaction.VALUE,
          reproduceWorkerQueueRecord.SavedSearchHoldingTable,
          reproduceWorkerQueueRecord.RedactionsHoldingTable,
          destinationMarkupSet, reproduceWorkerQueueRecord.DocumentIdStart,
          reproduceWorkerQueueRecord.DocumentIdEnd);

        highlights = QueryHelper.UpdateHasRedactionsOrHighlightsAsync(
          AgentHelper.GetDBContext(reproduceWorkerQueueRecord.WorkspaceArtifactId),
          reproduceWorkerQueueRecord.RedactionCodeTypeId,
          reproduceWorkerQueueRecord.MarkupSetAnnotationCodeArtifactId,
          Constant.MarkupType.Highlight.VALUE,
          reproduceWorkerQueueRecord.SavedSearchHoldingTable,
          reproduceWorkerQueueRecord.RedactionsHoldingTable,
          destinationMarkupSet, reproduceWorkerQueueRecord.DocumentIdStart,
          reproduceWorkerQueueRecord.DocumentIdEnd);
      }

      var ints = await Task.WhenAll(redactions, highlights);

      RaiseMessage($"Updated {string.Join("/", ints)} Redaction/Highlight records with Has Redactions/Highligths for worker record id {reproduceWorkerQueueRecord.RecordId}. {_errorContext}");
      return ints;
    }

    private async Task<DataTable> RetrieveRedactionInfoAsync(ReproduceWorkerQueueRecord reproduceWorkerQueueRecord, int redactionId)
    {
      return await QueryHelper.RetrieveRedactionInfoAsync(AgentHelper.GetDBContext(reproduceWorkerQueueRecord.WorkspaceArtifactId), redactionId);
    }

    private async Task LogErrorAsync(Exception ex)
    {
      //Add the error to our custom Errors table
      await QueryHelper.InsertRowIntoJobErrorLogAsync(
        AgentHelper.GetDBContext(-1),
        WorkspaceArtifactId,
        QueueTable,
        RecordId,
        AgentId,
        ex.ToString(), Constant.Tables.ReproduceErrorLog);

      //Add the error to the Relativity Errors tab
      ErrorQueries.WriteError(AgentHelper.GetServicesManager(), ExecutionIdentity.System, WorkspaceArtifactId, ex);
    }

    private async Task<string> ConstructDetailsExceptionMessageAsync(Exception exception)
    {
      var retVal = await ExceptionMessageHelper.GetInnerMostExceptionMessageAsync(exception);
      retVal += $". {Constant.ErrorMessages.REFER_TO_ERRORS_TAB_FOR_MORE_DETAILS}";
      return retVal;
    }
  }
}