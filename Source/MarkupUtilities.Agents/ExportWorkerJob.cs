using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using MarkupUtilities.Helpers;
using MarkupUtilities.Helpers.Exceptions;
using MarkupUtilities.Helpers.Models;
using MarkupUtilities.Helpers.Rsapi.Interfaces;
using Relativity.API;

namespace MarkupUtilities.Agents
{
  /// <summary>
  /// This class abstracts the agent logic to allow unit testing without IIS dependencies
  /// </summary>
  public class ExportWorkerJob : AgentJobBase
  {
    private readonly IServicesMgr _serviceMgr;
    private readonly IArtifactQueries _artifactQueries;
    private string _errorContext;
    private string _exportWorkerHoldingTable;
    private string _batchTableName;
    private readonly Helpers.Utility.IQuery _utilityQueryHelper;
    private MarkupUtilityExportJob _markupUtilityExportJob;
    private readonly IExportFileCreator _exportFileCreator;
    private IErrorQueries ErrorQueries { get; }

    public string ExportWorkerHoldingTable
    {
      get
      {
        return _exportWorkerHoldingTable ?? (_exportWorkerHoldingTable = $"{Constant.Names.ExportWorkerHoldingTablePrefix}{Guid.NewGuid()}_{AgentId}");
      }
    }

    public string BatchTableName
    {
      get
      {
        return _batchTableName ?? (_batchTableName = $"{Constant.Names.TablePrefix}Export_Worker_{Guid.NewGuid()}_{AgentId}");
      }
    }

    public ExportWorkerJob(int agentId, IServicesMgr serviceMgr, IAgentHelper agentHelper, IQuery queryHelper, IArtifactQueries artifactQueries, Helpers.Utility.IQuery utilityQueryHelper, DateTime processedOnDateTime, IEnumerable<int> resourceGroupIds, IExportFileCreator exportFileCreator, IErrorQueries errorQueries)
    {
      AgentId = agentId;
      _serviceMgr = serviceMgr;
      AgentHelper = agentHelper;
      QueryHelper = queryHelper;
      _artifactQueries = artifactQueries;
      _utilityQueryHelper = utilityQueryHelper;
      ProcessedOnDateTime = processedOnDateTime;
      AgentResourceGroupIds = resourceGroupIds;
      _exportFileCreator = exportFileCreator;
      ErrorQueries = errorQueries;
      RecordId = 0;
      WorkspaceArtifactId = -1;
      QueueTable = Constant.Tables.ExportWorkerQueue;
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
            var firstExportWorkerQueueRecord = new ExportWorkerQueueRecord(next.Rows[0]);
            WorkspaceArtifactId = firstExportWorkerQueueRecord.WorkspaceArtifactId;

            //Retrieve export job
            _markupUtilityExportJob = await _artifactQueries.RetrieveExportJobAsync(_serviceMgr, ExecutionIdentity.CurrentUser, WorkspaceArtifactId, firstExportWorkerQueueRecord.ExportJobArtifactId);

            //Create Export Worker Job holding table
            await CreateExportWorkerHoldingTableAsync();

            //Set the status of the Export Job to In Progess - Export Worker
            await UpdateStatusFieldAsync(firstExportWorkerQueueRecord.ExportJobArtifactId, Constant.Status.Job.IN_PROGRESS_WORKER);

            //Process document redactions
            await ProcessDocumentRedactionsAsync(next, firstExportWorkerQueueRecord.WorkspaceArtifactId, firstExportWorkerQueueRecord.ExportJobArtifactId);

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
      finally
      {
        //Remove the Export Worker Holding table
        await FinishHoldingTableAsync();

        //Remove records from Batch table and Delete the Export Worker Batch table
        await FinishAsync();
      }
    }

    private async Task ProcessDocumentRedactionsAsync(DataTable dtExportJobWorkerQueueRecords, int workspaceArtifactId, int exportJobArtifactId)
    {
      try
      {
        foreach (DataRow row in dtExportJobWorkerQueueRecords.Rows)
        {
          var exportWorkerQueueRecord = new ExportWorkerQueueRecord(row);

          //Sets the workspaceArtifactID and RecordID so the agent will have access to them in case of an exception
          WorkspaceArtifactId = exportWorkerQueueRecord.WorkspaceArtifactId;
          RecordId = exportWorkerQueueRecord.RecordId;
          _errorContext = $"[WorkspaceArtifactId = {WorkspaceArtifactId}, Id = {RecordId}]";

          //Retrieve the Document redactions
          await ProcessDocumentRedactionsSingleAsync(exportWorkerQueueRecord);
        }

        //Bulk copy from Holding table to Export Results table
        await AddRecordsToExportResultsAsync(ExportWorkerHoldingTable);

        //Remove the Export Worker Holding table
        await FinishHoldingTableAsync();

        //Remove records from the Export Worker Batch table
        await DeleteRecordsFromExportWorkerBatchTableAsync();

        //Check if no more records exist in the Export Worker queue, if not, proceed with creating CSV for attachment to Export Job
        await VerifyIfExportWorkerQueueContainsRecordsForJobAsync(workspaceArtifactId, exportJobArtifactId);
      }
      catch (Exception ex)
      {
        //Set the status of the Export Job to Error
        await UpdateStatusFieldAsync(_markupUtilityExportJob.ArtifactId, Constant.Status.Job.ERROR);
        await UpdateDetailsFieldAsync(_markupUtilityExportJob.ArtifactId, await ConstructDetailsExceptionMessageAsync(ex));
        RaiseMessage($"Logging error.");
        await LogErrorAsync(ex);

        //Remove the Export Worker Holding table
        await FinishHoldingTableAsync();
      }
    }

    private async Task VerifyIfExportWorkerQueueContainsRecordsForJobAsync(int workspaceArtifactId, int exportJobArtifactId)
    {
      _errorContext = $"An error occurred while creating the export file. [WorkspaceArtifactId = {WorkspaceArtifactId}, ExportJobArtifactId = {exportJobArtifactId}]";

      var exportWorkerQueueRecordCount = await QueryHelper.GetExportResultsRecordCountAsync(AgentHelper.GetDBContext(-1), workspaceArtifactId, exportJobArtifactId, AgentId);
      if (exportWorkerQueueRecordCount > 0)
      {
        try
        {
          //Create export.csv in a Temp folder
          var exportFullFilePath = await _exportFileCreator.CreateExportFileAsync(_markupUtilityExportJob.Name);

          //Write Records to Export File
          await WriteResultsToExportFileAsync();

          //Create Markup Utility File record name
          var redactionFileName = _exportFileCreator.ExportFileName;

          //Create new RDO File record
          var redactionFileRdoArtifactId = await _artifactQueries.CreateMarkupUtilityFileRdoRecordAsync(_serviceMgr, ExecutionIdentity.CurrentUser, workspaceArtifactId, redactionFileName);

          //Attach export csv to File RDO
          var fileArtifactId = await QueryHelper.GetArtifactIdByGuidAsync(AgentHelper.GetDBContext(workspaceArtifactId), Constant.Guids.Field.MarkupUtilityFile.File);
          await _artifactQueries.AttachFileToMarkupUtilityFileRecord(_serviceMgr, ExecutionIdentity.CurrentUser, workspaceArtifactId, redactionFileRdoArtifactId, exportFullFilePath, fileArtifactId);

          //Attach RDO File record to Export Job
          await _artifactQueries.AttachRedactionFileToExportJob(_serviceMgr, ExecutionIdentity.CurrentUser, workspaceArtifactId, _markupUtilityExportJob.ArtifactId, redactionFileRdoArtifactId);

          //Update Exported Redaction Count
          await _artifactQueries.UpdateRdoJobTextFieldAsync(_serviceMgr, workspaceArtifactId, ExecutionIdentity.CurrentUser, Constant.Guids.ObjectType.MarkupUtilityExportJob, _markupUtilityExportJob.ArtifactId, Constant.Guids.Field.MarkupUtilityExportJob.ExportedRedactionCount, exportWorkerQueueRecordCount.ToString());
        }
        catch (Exception ex)
        {
          RaiseMessage(_errorContext);
          throw new MarkupUtilityException(_errorContext, ex);
        }
        finally
        {
          //Delete CSV file
          await _exportFileCreator.DeleteExportFileAsync();
        }

        //Set the status of the Export Job to Complete
        await UpdateStatusFieldAsync(_markupUtilityExportJob.ArtifactId, Constant.Status.Job.COMPLETED);
      }
      else
      {
        //Query Worker Queue to see if any other records exist for the Export Job, if not, set status to Completed
        //This is for a scenario where no redactions were found for the Export Job
        var exportWorkerQueueCount = await QueryHelper.GetJobWorkerRecordCountAsync(AgentHelper.GetDBContext(-1), workspaceArtifactId, exportJobArtifactId, Constant.Tables.ExportWorkerQueue, "ExportJobArtifactID");
        if (exportWorkerQueueCount == 0)
        {
          //Set the status of the Export Job to Complete
          await UpdateStatusFieldAsync(_markupUtilityExportJob.ArtifactId, Constant.Status.Job.COMPLETED);

          //Update Exported Redation Count to 0
          await _artifactQueries.UpdateRdoJobTextFieldAsync(_serviceMgr, workspaceArtifactId, ExecutionIdentity.CurrentUser, Constant.Guids.ObjectType.MarkupUtilityExportJob, _markupUtilityExportJob.ArtifactId, Constant.Guids.Field.MarkupUtilityExportJob.ExportedRedactionCount, "0");
        }
      }
    }

    private async Task WriteResultsToExportFileAsync()
    {
      var exportResultsRecords = await QueryExportResults(AgentHelper.GetDBContext(-1), WorkspaceArtifactId, _markupUtilityExportJob.ArtifactId);

      while (exportResultsRecords.Count > 0)
      {
        //Loop through records and create export CSV file
        await _exportFileCreator.WriteToExportFileAsync(exportResultsRecords);

        //Delete selected Results records from Results table
        var recordIdList = exportResultsRecords.Select(x => x.Id).ToList();
        await QueryHelper.DeleteExportResultsAsync(AgentHelper.GetDBContext(-1), recordIdList);

        //Check if any remaining results records exist
        exportResultsRecords = await QueryExportResults(AgentHelper.GetDBContext(-1), WorkspaceArtifactId, _markupUtilityExportJob.ArtifactId);
      }
    }

    private async Task<List<ExportResultsRecord>> QueryExportResults(IDBContext eddsDbContext, int workspaceArtifactId, int exportJobArtifactId)
    {
      var retVal = new List<ExportResultsRecord>();

      var dtRedactionResults = await QueryHelper.GetExportResultsAsync(eddsDbContext, workspaceArtifactId, exportJobArtifactId, AgentId);
      if (dtRedactionResults != null && dtRedactionResults.Rows.Count > 0)
      {
        retVal.AddRange(dtRedactionResults.AsEnumerable().Select(x => new ExportResultsRecord(x)).ToList());
      }
      return retVal;
    }

    private static bool TableIsNotEmpty(DataTable table)
    {
      return (table != null && table.Rows.Count > 0);
    }

    public async Task<DataTable> RetrieveNextAsync(string delimitedListOfResourceGroupIds)
    {
      var next = await QueryHelper.RetrieveNextBatchInExportWorkerQueueAsync(AgentHelper.GetDBContext(-1), AgentId, Constant.Sizes.ExportJobManagerBatchSize, BatchTableName, delimitedListOfResourceGroupIds);
      return next;
    }

    public async Task ProcessDocumentRedactionsSingleAsync(ExportWorkerQueueRecord exportWorkerQueueRecord)
    {
      _errorContext = $"[WorkspaceArtifactId = {WorkspaceArtifactId}, ExportJobArtifactId = {exportWorkerQueueRecord.ExportJobArtifactId}]";
      RaiseMessage($"Processing record(s). [Table = {QueueTable}, ID = {RecordId}, Workspace Artifact ID = {WorkspaceArtifactId}, DocumentArtifactId = {exportWorkerQueueRecord.DocumentArtifactId}, MarkupSetArtifactId = {exportWorkerQueueRecord.MarkupSetArtifactId}, MarkupSubTypes = {exportWorkerQueueRecord.MarkupSubType}]");

      //Check if Document exists
      await _artifactQueries.VerifyIfDocumentExistsAsync(_serviceMgr, ExecutionIdentity.CurrentUser, WorkspaceArtifactId, exportWorkerQueueRecord.DocumentArtifactId);

      //Retrieve the Column Name of the Document Identifier Field
      var documentIdentifierColumnName = await QueryHelper.GetDocumentIdentifierFieldColumnNameAsync(AgentHelper.GetDBContext(WorkspaceArtifactId));

      //Retrieve Redactions for the selected Documents from the Workspace, store in DataTable
      var dtDocumentRedactions = await QueryHelper.RetrieveRedactionsForDocumentAsync(AgentHelper.GetDBContext(exportWorkerQueueRecord.WorkspaceArtifactId), exportWorkerQueueRecord.WorkspaceArtifactId, exportWorkerQueueRecord.ExportJobArtifactId, exportWorkerQueueRecord.DocumentArtifactId, exportWorkerQueueRecord.MarkupSetArtifactId, documentIdentifierColumnName, exportWorkerQueueRecord.MarkupSubType);

      //Write records to holding table in EDDS
      await AddRedactionsFromHoldingToResultsTableAsync(dtDocumentRedactions, ExportWorkerHoldingTable);

      RaiseMessage($"Processed record(s). [Table = {QueueTable}, ID = {RecordId}, Workspace Artifact ID = {WorkspaceArtifactId}, DocumentArtifactId = {exportWorkerQueueRecord.DocumentArtifactId}, MarkupSetArtifactId = {exportWorkerQueueRecord.MarkupSetArtifactId}, MarkupSubTypes = {exportWorkerQueueRecord.MarkupSubType}]");
    }

    public async Task FinishAsync()
    {
      //Delete the Export Worker Batch table
      RaiseMessage($"Deleting the Export Worker Batch table. [Table = {BatchTableName}, Agent ID = {AgentId}]");
      await QueryHelper.DropTableAsync(AgentHelper.GetDBContext(-1), BatchTableName);
      RaiseMessage($"Deleted the Export Worker Batch table. [Table = {BatchTableName}, Agent ID = {AgentId}]");
    }

    public async Task DeleteRecordsFromExportWorkerBatchTableAsync()
    {
      //Remove records from the Export Worker Batch table
      RaiseMessage($"Removing record(s) from the Export Worker Batch table. [Table = {BatchTableName}, ID = {RecordId}, Workspace Artifact ID = {WorkspaceArtifactId}, Agent ID = {AgentId}]");
      await QueryHelper.RemoveBatchFromExportWorkerQueueAsync(AgentHelper.GetDBContext(-1), BatchTableName);
      RaiseMessage($"Removed record(s) from the Export Worker Batch table. [Table = {BatchTableName}, ID = {RecordId}, Workspace Artifact ID = {WorkspaceArtifactId}, Agent ID = {AgentId}]");
    }

    public async Task FinishHoldingTableAsync()
    {
      //Delete the Export Worker Holding table
      RaiseMessage($"Deleting the Export Worker Holding table. [Table = {ExportWorkerHoldingTable}, Agent ID = {AgentId}]");
      await QueryHelper.DropTableAsync(AgentHelper.GetDBContext(-1), ExportWorkerHoldingTable);
      //await Task.Run(() => { });
      RaiseMessage($"Deleted the Export Worker Holding table. [Table = {ExportWorkerHoldingTable}, Agent ID = {AgentId}]");
    }

    private async Task CreateExportWorkerHoldingTableAsync()
    {
      RaiseMessage($"Creating Export Worker holding table for export job [ExportWorkerHoldingTable = {ExportWorkerHoldingTable}]. {_errorContext}");
      await QueryHelper.CreateExportWorkerHoldingTableAsync(AgentHelper.GetDBContext(-1), ExportWorkerHoldingTable);
      RaiseMessage($"Created Export Worker holding table for export job [ExportWorkerHoldingTable = {ExportWorkerHoldingTable}]. {_errorContext}");
    }

    private async Task UpdateStatusFieldAsync(int exportJobArtifactId, string exportJobStatus)
    {
      await _artifactQueries.UpdateRdoJobTextFieldAsync(
        _serviceMgr,
        WorkspaceArtifactId,
        ExecutionIdentity.CurrentUser,
        Constant.Guids.ObjectType.MarkupUtilityExportJob,
        exportJobArtifactId,
        Constant.Guids.Field.MarkupUtilityExportJob.Status,
        exportJobStatus);
    }

    private async Task UpdateDetailsFieldAsync(int exportJobArtifactId, string exportJobDetails)
    {
      await _artifactQueries.UpdateRdoJobTextFieldAsync(
        _serviceMgr,
        WorkspaceArtifactId,
        ExecutionIdentity.CurrentUser,
        Constant.Guids.ObjectType.MarkupUtilityExportJob,
        exportJobArtifactId,
        Constant.Guids.Field.MarkupUtilityExportJob.Details,
        exportJobDetails);
    }

    private async Task AddRedactionsFromHoldingToResultsTableAsync(DataTable dtDocumentRedactions, string tableName)
    {
      var columnMappingsHoldingTable = await GenerateColumnMappingsHoldingTableAsync();
      await Task.Run(() => _artifactQueries.AddRedactionsToTableAsync(AgentHelper.GetDBContext(-1), _utilityQueryHelper, tableName, dtDocumentRedactions, columnMappingsHoldingTable));
    }

    private async Task<List<SqlBulkCopyColumnMapping>> GenerateColumnMappingsHoldingTableAsync()
    {
      var columns = new List<string>
      {
        "TimeStampUTC",
        "WorkspaceArtifactID",
        "ExportJobArtifactID",
        "DocumentIdentifier",
        "FileOrder",
        "X",
        "Y",
        "Width",
        "Height",
        "MarkupSetArtifactID",
        "MarkupType",
        "FillA",
        "FillR",
        "FillG",
        "FillB",
        "BorderSize",
        "BorderA",
        "BorderR",
        "BorderG",
        "BorderB",
        "BorderStyle",
        "FontName",
        "FontA",
        "FontR",
        "FontG",
        "FontB",
        "FontSize",
        "FontStyle",
        "Text",
        "ZOrder",
        "DrawCrossLines",
        "MarkupSubType",
                "X_d",
                "Y_d",
                "Width_d",
                "Height_d",
            };
      return await Task.Run(() => _utilityQueryHelper.GetMappingsForWorkerQueue(columns));
    }

    private async Task AddRecordsToExportResultsAsync(string holdingTableName)
    {
      RaiseMessage($"Creating Export Worker Results from holding table for export job [ExportWorkerHoldingTable = {ExportWorkerHoldingTable}]. {_errorContext}");
      await QueryHelper.CopyRecordsToExportResultsAsync(AgentHelper.GetDBContext(-1), holdingTableName);
      RaiseMessage($"Created Export Worker Results from holding table for export job [ExportWorkerHoldingTable = {ExportWorkerHoldingTable}]. {_errorContext}");
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
        ex.ToString(), Constant.Tables.ExportErrorLog);

      //Add the error to the Relativity Errors tab
      ErrorQueries.WriteError(AgentHelper.GetServicesManager(), ExecutionIdentity.System, WorkspaceArtifactId, ex);
    }

    private static async Task<string> ConstructDetailsExceptionMessageAsync(Exception exception)
    {
      var retVal = await ExceptionMessageHelper.GetInnerMostExceptionMessageAsync(exception);
      retVal += $". {Constant.ErrorMessages.REFER_TO_ERRORS_TAB_FOR_MORE_DETAILS}";
      return retVal;
    }
  }
}
