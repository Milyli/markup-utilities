using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
  public class ReproduceManagerJob : AgentJobBase
  {
    private readonly IArtifactQueries _artifactQueries;
    private readonly Helpers.Utility.IQuery _utilityQueryHelper;
    private IErrorQueries ErrorQueries { get; }
    private string _savedSearchHoldingTable;
    private string _redactionsHoldingTable;
    public string SavedSearchHoldingTable
    {
      get
      {
        return _savedSearchHoldingTable ?? (_savedSearchHoldingTable = $"{Constant.Names.ReproduceWorkerHoldingTablePrefix}{Guid.NewGuid()}_{AgentId}");
      }
    }

    public string RedactionsHoldingTable
    {
      get
      {
        return _redactionsHoldingTable ?? (_redactionsHoldingTable = $"{Constant.Names.ReproduceWorkerHoldingTablePrefix}{Guid.NewGuid()}_{AgentId}");
      }
    }

    public ReproduceManagerJob(int agentId, IAgentHelper agentHelper, IQuery queryHelper, DateTime processedOnDateTime, IEnumerable<int> resourceGroupIds, IArtifactQueries artifactQueries, Helpers.Utility.IQuery utilityQueryHelper, IErrorQueries errorQueries)
    {
      RecordId = 0;
      WorkspaceArtifactId = -1;
      AgentId = agentId;
      AgentHelper = agentHelper;
      QueryHelper = queryHelper;
      ProcessedOnDateTime = processedOnDateTime;
      QueueTable = Constant.Tables.ReproduceManagerQueue;
      AgentResourceGroupIds = resourceGroupIds;
      _artifactQueries = artifactQueries;
      _utilityQueryHelper = utilityQueryHelper;
      ErrorQueries = errorQueries;
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
            var reproduceManagerQueueRecord = new ReproduceManagerQueueRecord(next.Rows[0]);

            // Sets the workspaceArtifactId and RecordID so the agent will have access to them in case of an exception
            WorkspaceArtifactId = reproduceManagerQueueRecord.WorkspaceArtifactId;
            RecordId = reproduceManagerQueueRecord.RecordId;
            RaiseMessage(
                $"Retrieved record(s) in the queue. [Table = {QueueTable}, ID = {RecordId}, Workspace Artifact ID = {WorkspaceArtifactId}]");
            var reproduceJob = await RetrieveReproduceJobAsync(reproduceManagerQueueRecord);

            //Check for Cancellation RequestGetDBContext
            if (reproduceJob != null && reproduceJob.Status != Constant.Status.Job.CANCELREQUESTED)
            {
              //Process the record(s)
              await ProcessRecordsAsync(reproduceManagerQueueRecord, reproduceJob);
              //Remove the record from the manager queue
              await FinishAsync();
            }
            else
            {
              //Remove the record from the manager queue
              await FinishAsync();

              if (reproduceJob != null)
              {
                //Set the status of the Export Job to Cancelled
                await UpdateStatusFieldAsync(reproduceManagerQueueRecord.ReproduceJobArtifactId,
                    Constant.Status.Job.CANCELLED);
                //Flush the Details of the Export Job 
                await UpdateDetailsFieldAsync(reproduceManagerQueueRecord.ReproduceJobArtifactId,
                    string.Empty);
              }
            }
          }
          else
          {
            RaiseMessage(Constant.AgentRaiseMessages.NO_RECORDS_IN_QUEUE_FOR_THIS_RESOURCE_POOL);
          }
        }
        else
        {
          RaiseMessage(Constant.AgentRaiseMessages.AGENT_SERVER_NOT_PART_OF_ANY_RESOURCE_POOL);
        }
      }
      catch (Exception ex)
      {
        //log error
        RaiseMessage($"Logging error.");
        await LogErrorAsync(ex);
      }
      finally
      {
        //Remove the record from the manager queue
        await FinishAsync();
        //Delete Holding Tables
        await DeleteSavedSearchHoldingTableAsync();
        await DeleteRedactionsHoldingTableAsync();
      }
    }

    private async Task<MarkupUtilityReproduceJob> RetrieveReproduceJobAsync(ReproduceManagerQueueRecord reproduceManagerQueueRecord)
    {

      return await _artifactQueries.RetrieveReproduceJobAsync(
          AgentHelper.GetServicesManager(),
          ExecutionIdentity.CurrentUser,
          WorkspaceArtifactId,
          reproduceManagerQueueRecord.ReproduceJobArtifactId);
    }

    private static bool TableIsNotEmpty(DataTable table)
    {
      return (table != null && table.Rows.Count > 0);
    }

    public async Task<DataTable> RetrieveNextAsync(string delimiitedListOfResourceGroupIds)
    {
      var next = await QueryHelper.RetrieveNextInJobManagerQueueAsync(AgentHelper.GetDBContext(-1), AgentId, delimiitedListOfResourceGroupIds, Constant.Tables.ReproduceManagerQueue, "ReproduceJobArtifactID");
      return next;
    }

    public async Task ProcessRecordsAsync(ReproduceManagerQueueRecord reproduceManagerQueueRecord, MarkupUtilityReproduceJob reproduceJob)
    {
      RaiseMessage($"Processing record(s). [Table = {QueueTable}, ID = {RecordId}, Workspace Artifact ID = {WorkspaceArtifactId}]");

      try
      {
        //Update status of the Job to Manager In Progress
        await UpdateStatusFieldAsync(reproduceManagerQueueRecord.ReproduceJobArtifactId, Constant.Status.Job.IN_PROGRESS_MANAGER);

        //Flush the Details of the Job 
        await UpdateDetailsFieldAsync(reproduceManagerQueueRecord.ReproduceJobArtifactId, string.Empty);
        var includeRelationalGroup = reproduceJob.RelationalField > 0;

        if (reproduceJob.HasAutoRedactionsField > 0)
        {
          var columnName = await RetrieveDocumentColumnAsync(reproduceManagerQueueRecord.WorkspaceArtifactId, reproduceJob.HasAutoRedactionsField);
          reproduceJob.HasAutoRedactionsFieldColumnName = columnName;
        }

        //Create holding tables
        if (includeRelationalGroup)
        {
          await CreateSavedSearchHoldingTableAsync(reproduceManagerQueueRecord.WorkspaceArtifactId, true);
          var relationalColumnName = await RetrieveDocumentColumnAsync(reproduceManagerQueueRecord.WorkspaceArtifactId, reproduceJob.RelationalField);
          reproduceJob.RelationalFieldColumnName = relationalColumnName;
        }
        else
        {
          await CreateSavedSearchHoldingTableAsync(reproduceManagerQueueRecord.WorkspaceArtifactId, false);
        }

        await CreateRedactionsHoldingTableAsync(reproduceManagerQueueRecord.WorkspaceArtifactId, includeRelationalGroup);

        //Add Documents from Saved Search in the Export Worker Queue
        var isSuccessful = await AddDocumentsToSavedSearchHoldingTableAsync(reproduceManagerQueueRecord.WorkspaceArtifactId, reproduceJob);
        var jobIsComplete = false;
        var jobMessage = "";

        if (isSuccessful)
        {
          var dataTable = await RetrieveMinMaxIdAsync(reproduceManagerQueueRecord.WorkspaceArtifactId);
          var min = (int)dataTable.Rows[0][0];
          var max = (int)dataTable.Rows[0][1];

          var tasks = new List<Task<int>>();
          for (var i = min; i <= max; i = i + Constant.Sizes.ReproduceJobBatchSize)
          {
            //sql between has inclusive end
            tasks.Add(AddDocumentsToRedactionsHoldingTableAsync(reproduceManagerQueueRecord.WorkspaceArtifactId, reproduceJob.SourceMarkupSetArtifactId, reproduceJob.DestinationMarkupSetArtifactId, i, i + Constant.Sizes.ReproduceJobBatchSize - 1, includeRelationalGroup));
          }

          var results = await Task.WhenAll(tasks.ToArray());
          var existRedactions = Array.Exists(results, r => r > 0);

          if (existRedactions)
          {
            var result = await RetrieveZCodesAsync(reproduceManagerQueueRecord.WorkspaceArtifactId, reproduceJob.DestinationMarkupSetArtifactId);
            var codeTypeId = (int)result.Rows[0]["CodeTypeID"];
            var markupSetRedactionCodeArtifactId = (int)result.Rows[0]["RedactionCodeArtifactID"];
            var markupSetAnnotationCodeArtifactId = (int)result.Rows[0]["AnnotationCodeArtifactID"];

            int count;

            if (reproduceJob.RelationalField > 0)
            {
              count = await ReproduceAcrossRelationalGroup(reproduceManagerQueueRecord, reproduceJob, codeTypeId, markupSetRedactionCodeArtifactId, markupSetAnnotationCodeArtifactId);
            }
            else
            {
              count = await ReproduceAcrossDocumentSet(reproduceManagerQueueRecord, reproduceJob, min, max, codeTypeId, markupSetRedactionCodeArtifactId, markupSetAnnotationCodeArtifactId);
            }

            if (count == 0)
            {
              jobIsComplete = true;
              jobMessage = "No redactions found for selected criteria.";
            }
          }
          else
          {
            jobIsComplete = true;
            jobMessage = "No redactions found for selected criteria.";
          }
        }
        else
        {
          jobIsComplete = true;
          jobMessage = "No Documents found in the selected Saved Search.";
        }

        if (jobIsComplete)
        {
          var tasks = new List<Task>
          {
            UpdateStatusFieldAsync(reproduceManagerQueueRecord.ReproduceJobArtifactId, Constant.Status.Job.COMPLETED), UpdateDetailsFieldAsync(reproduceManagerQueueRecord.ReproduceJobArtifactId, jobMessage), QueryHelper.DropTableAsync(AgentHelper.GetDBContext(reproduceManagerQueueRecord.WorkspaceArtifactId), RedactionsHoldingTable), QueryHelper.DropTableAsync(AgentHelper.GetDBContext(reproduceManagerQueueRecord.WorkspaceArtifactId), SavedSearchHoldingTable)
          };

          //Update status of the Export Job to Completed
          //Update Details of the Export Job indicating no Documents in Saved Search
          await Task.WhenAll(tasks);
        }
        else
        {
          //Update status of the Export Job to Completed - Manager
          await UpdateStatusFieldAsync(reproduceManagerQueueRecord.ReproduceJobArtifactId, Constant.Status.Job.COMPLETED_MANAGER);
          //Flush the Details of the Export Job 
          await UpdateDetailsFieldAsync(reproduceManagerQueueRecord.ReproduceJobArtifactId, string.Empty);
        }

        //delete import job from queue
        RaiseMessage($"Removing record(s) from the queue. [Table = {QueueTable}, ID = {RecordId}, Workspace Artifact ID = {WorkspaceArtifactId}]");
        await FinishAsync();
        await DeleteSavedSearchHoldingTableAsync();
        await DeleteRedactionsHoldingTableAsync();
        RaiseMessage($"Removed record(s) from the queue. [Table = {QueueTable}, ID = {RecordId}, Workspace Artifact ID = {WorkspaceArtifactId}]");
        RaiseMessage($"Processed record(s). [Table = {QueueTable}, ID = {RecordId}, Workspace Artifact ID = {WorkspaceArtifactId}]");
      }
      catch (Exception ex)
      {
        //Update status of the Export Job to Error
        await UpdateStatusFieldAsync(reproduceManagerQueueRecord.ReproduceJobArtifactId, Constant.Status.Job.ERROR);
        //Update Details of the Export Job 
        await UpdateDetailsFieldAsync(reproduceManagerQueueRecord.ReproduceJobArtifactId, ex.ToString());
        //log error
        await LogErrorAsync(ex);

        //delete import job from queue
        RaiseMessage($"Removing record(s) from the queue. [Table = {QueueTable}, ID = {RecordId}, Workspace Artifact ID = {WorkspaceArtifactId}]");
        await FinishAsync();
        await DeleteSavedSearchHoldingTableAsync();
        await DeleteRedactionsHoldingTableAsync();
        RaiseMessage($"Removed record(s) from the queue. [Table = {QueueTable}, ID = {RecordId}, Workspace Artifact ID = {WorkspaceArtifactId}]");
        RaiseMessage($"Processed record(s). [Table = {QueueTable}, ID = {RecordId}, Workspace Artifact ID = {WorkspaceArtifactId}]");
      }
    }

    private async Task<int> ReproduceAcrossDocumentSet(ReproduceManagerQueueRecord reproduceManagerQueueRecord, MarkupUtilityReproduceJob reproduceJob, int min, int max, int codeTypeId, int markupSetRedactionCodeArtifactId, int markupSetAnnotationCodeArtifactId)
    {
      //create Worker records splitting the records
      var insertTasks = new List<Task>();
      for (var i = min; i <= max; i = i + Constant.Sizes.ReproduceJobInsertBatchSize)
      {
        insertTasks.Add(InsertRecordIntoToReproduceWorkerQueueAsync(
          reproduceManagerQueueRecord.WorkspaceArtifactId,
          i,
          i + Constant.Sizes.ReproduceJobInsertBatchSize - 1,
          reproduceJob.DestinationMarkupSetArtifactId,
          reproduceJob.ArtifactId,
          reproduceManagerQueueRecord.ResourceGroupId, codeTypeId, markupSetRedactionCodeArtifactId,
          markupSetAnnotationCodeArtifactId, null, reproduceJob.HasAutoRedactionsFieldColumnName, null));
      }

      await Task.WhenAll(insertTasks.ToArray());
      return insertTasks.Count;
    }

    private async Task<int> ReproduceAcrossRelationalGroup(ReproduceManagerQueueRecord reproduceManagerQueueRecord, MarkupUtilityReproduceJob reproduceJob, int codeTypeId, int markupSetRedactionCodeArtifactId, int markupSetAnnotationCodeArtifactId)
    {
      var dataTable = await RetrieveRelationalGroupsAsync(reproduceManagerQueueRecord.WorkspaceArtifactId);

      //create a new record for each relational group
      var insertTasks = new List<Task>();
      foreach (DataRow row in dataTable.Rows)
      {
        var relationalGroup = (string)row["RelationalGroup"];
        insertTasks.Add(InsertRecordIntoToReproduceWorkerQueueAsync(
            reproduceManagerQueueRecord.WorkspaceArtifactId,
            -1,
            -1,
            -1,
            reproduceJob.ArtifactId,
            reproduceManagerQueueRecord.ResourceGroupId, codeTypeId, markupSetRedactionCodeArtifactId,
            markupSetAnnotationCodeArtifactId, reproduceJob.RelationalFieldColumnName,
            reproduceJob.HasAutoRedactionsFieldColumnName, relationalGroup));
      }

      await Task.WhenAll(insertTasks.ToArray());
      return insertTasks.Count;
    }

    private async Task<bool> AddDocumentsToSavedSearchHoldingTableAsync(int workspaceArtifactId, MarkupUtilityReproduceJob reproduceJob)
    {
      List<SqlBulkCopyColumnMapping> columnMappingsHoldingTable;
      //Create column mappings for holding table
      if (reproduceJob.RelationalFieldColumnName != null)
      {
        var sqlBulkCopyColumnMappings = new List<SqlBulkCopyColumnMapping>();
        sqlBulkCopyColumnMappings.Add(new SqlBulkCopyColumnMapping("DocumentArtifactID", "DocumentArtifactID"));
        sqlBulkCopyColumnMappings.Add(new SqlBulkCopyColumnMapping(reproduceJob.RelationalFieldColumnName, "RelationalGroup"));
        columnMappingsHoldingTable = sqlBulkCopyColumnMappings;
      }
      else
      {
        columnMappingsHoldingTable =
            await GenerateColumnMappingsHoldingTableAsync(new List<string> { "DocumentArtifactID" });
      }

      return await _artifactQueries.AddDocumentsToHoldingTableAsync(AgentHelper.GetServicesManager(), AgentHelper.GetDBContext(workspaceArtifactId), _utilityQueryHelper, ExecutionIdentity.CurrentUser, WorkspaceArtifactId, reproduceJob.SavedSearchArtifactId, SavedSearchHoldingTable, columnMappingsHoldingTable);
    }

    private async Task<int> AddDocumentsToRedactionsHoldingTableAsync(int workspaceArtifactId, int sourceMarkupSetArtifactId, int destinationMarkupSetArtifactId, int start, int end, bool relationaGroup)
    {
      return await QueryHelper.InsertRowsIntoRedactionsHoldingTableAsync(AgentHelper.GetDBContext(workspaceArtifactId), RedactionsHoldingTable, SavedSearchHoldingTable, sourceMarkupSetArtifactId, destinationMarkupSetArtifactId, start, end, relationaGroup);
    }

    private async Task<List<SqlBulkCopyColumnMapping>> GenerateColumnMappingsHoldingTableAsync(List<string> columns)
    {
      return await Task.Run(() => _utilityQueryHelper.GetMappingsForWorkerQueue(columns));
    }

    private async Task InsertRecordIntoToReproduceWorkerQueueAsync(int workspaceArtifactId, int documentIdStart, int documentIdEnd, int destinationMarkupSetArtifactId, int reproduceJobArtifactId, int resourceGroupArtifactId, int codeTypeId, int markupSetRedactionCodeArtifactId, int markupSetAnnotationCodeArtifactId, string relationalGroupColumn, string hasAutoRedactionsColumn, string relationalGroup)
    {
      var proceed = true;
      if (!string.IsNullOrEmpty(relationalGroup))
      {
        var dataTable = await QueryHelper.RelationalGroupHasImagesAsync(AgentHelper.GetDBContext(workspaceArtifactId), relationalGroupColumn, relationalGroup);
        proceed = dataTable.Rows.Count > 0;
      }

      if (proceed)
      {
        await QueryHelper.InsertRowIntoReproduceWorkerQueueAsync(AgentHelper.GetDBContext(-1),
                workspaceArtifactId, documentIdStart, documentIdEnd, SavedSearchHoldingTable,
                RedactionsHoldingTable, destinationMarkupSetArtifactId, reproduceJobArtifactId,
                resourceGroupArtifactId, codeTypeId, markupSetRedactionCodeArtifactId,
                markupSetAnnotationCodeArtifactId, relationalGroupColumn, hasAutoRedactionsColumn,
                relationalGroup);
      }
    }

    private async Task<DataTable> RetrieveZCodesAsync(int workspaceArtifactId, int destinationMarkupSetArtifactId)
    {
      var dataTable = await QueryHelper.RetrieveZCodesAsync(AgentHelper.GetDBContext(workspaceArtifactId), destinationMarkupSetArtifactId);
      return dataTable;
    }

    public async Task DeleteSavedSearchHoldingTableAsync()
    {
      //Delete the Saved Search Holding table
      RaiseMessage($"Deleting the Reproduce Manager Saved Search Holding table. [Table = {SavedSearchHoldingTable}, Agent ID = {AgentId}]");
      await QueryHelper.DropTableAsync(AgentHelper.GetDBContext(-1), SavedSearchHoldingTable);
      RaiseMessage($"Deleted the Reproduce Manager Saved Search Holding table. [Table = {SavedSearchHoldingTable}, Agent ID = {AgentId}]");
    }

    public async Task DeleteRedactionsHoldingTableAsync()
    {
      //Delete the Redactions Holding table
      RaiseMessage($"Deleting the Reproduce Manager Redactions Holding table. [Table = {RedactionsHoldingTable}, Agent ID = {AgentId}]");
      await QueryHelper.DropTableAsync(AgentHelper.GetDBContext(-1), RedactionsHoldingTable);
      RaiseMessage($"Deleted the Reproduce Manager Redactions Holding table. [Table = {RedactionsHoldingTable}, Agent ID = {AgentId}]");
    }

    private async Task<string> RetrieveDocumentColumnAsync(int workspaceArtifactId, int fieldArtifactId)
    {
      var dataTable = await QueryHelper.RetrieveDocumentColumnAsync(AgentHelper.GetDBContext(workspaceArtifactId), fieldArtifactId);
      return (string)dataTable.Rows[0]["ColumnName"];
    }

    private async Task CreateSavedSearchHoldingTableAsync(int workspaceArtifactId, bool includeRelationalGroup)
    {
      await QueryHelper.CreateSavedSearchHoldingTableAsync(AgentHelper.GetDBContext(workspaceArtifactId), SavedSearchHoldingTable, includeRelationalGroup);
    }

    private async Task<DataTable> RetrieveMinMaxIdAsync(int workspaceArtifactId)
    {
      var dataTable = await QueryHelper.RetrieveMinMaxIdAsync(AgentHelper.GetDBContext(workspaceArtifactId), SavedSearchHoldingTable);
      return dataTable;
    }

    private async Task<DataTable> RetrieveRelationalGroupsAsync(int workspaceArtifactId)
    {
      var dataTable = await QueryHelper.RetrieveRelationalGroupsTask(AgentHelper.GetDBContext(workspaceArtifactId), SavedSearchHoldingTable);
      return dataTable;
    }

    private async Task CreateRedactionsHoldingTableAsync(int workspaceArtifactId, bool includeRelationalGroup)
    {
      await QueryHelper.CreateRedactionsHoldingTableAsync(AgentHelper.GetDBContext(workspaceArtifactId), RedactionsHoldingTable, includeRelationalGroup);
    }

    public async Task FinishAsync()
    {
      RaiseMessage($"Removing record(s) from the queue. [Table = {QueueTable}, ID = {RecordId}, Workspace Artifact ID = {WorkspaceArtifactId}]");
      await QueryHelper.RemoveRecordFromTableByIdAsync(AgentHelper.GetDBContext(-1), QueueTable, RecordId);
      RaiseMessage($"Removed record(s) from the queue. [Table = {QueueTable}, ID = {RecordId}, Workspace Artifact ID = {WorkspaceArtifactId}]");
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

    private async Task UpdateStatusFieldAsync(int reproduceJobArtifactId, string exportJobStatus)
    {
      await _artifactQueries.UpdateRdoJobTextFieldAsync(
        AgentHelper.GetServicesManager(),
        WorkspaceArtifactId,
        ExecutionIdentity.CurrentUser,
        Constant.Guids.ObjectType.MarkupUtilityReproduceJob,
        reproduceJobArtifactId,
        Constant.Guids.Field.MarkupUtilityReproduceJob.Status,
        exportJobStatus);
    }

    private async Task UpdateDetailsFieldAsync(int reproduceJobArtifactId, string exportJobStatus)
    {
      await _artifactQueries.UpdateRdoJobTextFieldAsync(
        AgentHelper.GetServicesManager(),
        WorkspaceArtifactId,
        ExecutionIdentity.CurrentUser,
        Constant.Guids.ObjectType.MarkupUtilityReproduceJob,
        reproduceJobArtifactId,
        Constant.Guids.Field.MarkupUtilityReproduceJob.Details,
        exportJobStatus);
    }
  }
}
