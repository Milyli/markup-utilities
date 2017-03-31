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
  public class ExportManagerJob : AgentJobBase
  {
    private readonly IServicesMgr _serviceMgr;
    private readonly IArtifactQueries _artifactQueries;
    private readonly Helpers.Utility.IQuery _utilityQueryHelper;
    private readonly IMarkupTypeHelper _markupTypeHelper;
    private string _exportManagerHoldingTable;
    private MarkupUtilityExportJob _markupUtilityExportJob;
    private IErrorQueries ErrorQueries { get; }

    public string ExportManagerHoldingTable
    {
      get
      {
        return _exportManagerHoldingTable ?? (_exportManagerHoldingTable = $"{Constant.Names.ExportManagerHoldingTablePrefix}{Guid.NewGuid()}_{AgentId}");
      }
    }

    public ExportManagerJob(int agentId, IServicesMgr serviceMgr, IAgentHelper agentHelper, IQuery queryHelper, DateTime processedOnDateTime, IEnumerable<int> resourceGroupIds, IArtifactQueries artifactQueries, Helpers.Utility.IQuery utilityQueryHelper, IErrorQueries errorQueries, IMarkupTypeHelper markupTypeHelper)
    {
      RecordId = 0;
      WorkspaceArtifactId = -1;
      AgentId = agentId;
      AgentHelper = agentHelper;
      QueryHelper = queryHelper;
      ProcessedOnDateTime = processedOnDateTime;
      QueueTable = Constant.Tables.ExportManagerQueue;
      AgentResourceGroupIds = resourceGroupIds;
      _serviceMgr = serviceMgr;
      _artifactQueries = artifactQueries;
      _utilityQueryHelper = utilityQueryHelper;
      _markupTypeHelper = markupTypeHelper;
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
            var exportManagerQueueRecord = new ExportManagerQueueRecord(next.Rows[0]);

            // Sets the workspaceArtifactID and RecordID so the agent will have access to them in case of an exception
            WorkspaceArtifactId = exportManagerQueueRecord.WorkspaceArtifactId;
            RecordId = exportManagerQueueRecord.RecordId;
            RaiseMessage($"Retrieved record(s) in the queue. [Table = {QueueTable}, ID = {RecordId}, Workspace Artifact ID = {WorkspaceArtifactId}]");

            //Check for Cancellation RequestGetDBContext
            var jobStatus = await RetrieveJobStatusAsync(_serviceMgr, WorkspaceArtifactId, ExecutionIdentity.System, exportManagerQueueRecord.ExportJobArtifactId);

            if (jobStatus != Constant.Status.Job.CANCELREQUESTED)
            {
              //Process the record(s)
              await ProcessRecordsAsync(exportManagerQueueRecord);
              //Remove the record from the manager queue
              await FinishAsync();
            }
            else
            {
              //Remove the record from the manager queue
              await FinishAsync();
              //Set the status of the Export Job to Cancelled
              await UpdateStatusFieldAsync(exportManagerQueueRecord.ExportJobArtifactId, Constant.Status.Job.CANCELLED);
              //Flush the Details of the Export Job 
              await UpdateDetailsFieldAsync(exportManagerQueueRecord.ExportJobArtifactId, string.Empty);
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
    }

    private async Task<string> RetrieveJobStatusAsync(IServicesMgr svcManager, int workspaceArtifactId, ExecutionIdentity identity, int artifactId)
    {
      var status = await _artifactQueries.RetrieveRdoJobStatusAsync(svcManager, workspaceArtifactId, identity, artifactId);
      return status;
    }

    private static bool TableIsNotEmpty(DataTable table)
    {
      return (table != null && table.Rows.Count > 0);
    }

    public async Task<DataTable> RetrieveNextAsync(string delimiitedListOfResourceGroupIds)
    {
      var next = await QueryHelper.RetrieveNextInJobManagerQueueAsync(AgentHelper.GetDBContext(-1), AgentId, delimiitedListOfResourceGroupIds, Constant.Tables.ExportManagerQueue, "ExportJobArtifactID");
      return next;
    }

    public async Task ProcessRecordsAsync(ExportManagerQueueRecord exportManagerQueueRecord)
    {
      RaiseMessage($"Processing record(s). [Table = {QueueTable}, ID = {RecordId}, Workspace Artifact ID = {WorkspaceArtifactId}]");

      try
      {
        //Retrieve export job
        _markupUtilityExportJob = await _artifactQueries.RetrieveExportJobAsync(_serviceMgr, ExecutionIdentity.CurrentUser, WorkspaceArtifactId, exportManagerQueueRecord.ExportJobArtifactId);

        //Update status of the Export Job to Manager In Progress
        await UpdateStatusFieldAsync(exportManagerQueueRecord.ExportJobArtifactId, Constant.Status.Job.IN_PROGRESS_MANAGER);
        //Flush the Details of the Export Job 
        await UpdateDetailsFieldAsync(exportManagerQueueRecord.ExportJobArtifactId, string.Empty);

        //Generate MarkupSubTypes
        var markupSubTypes = await GenerateMarkupSubTypesAsync();

        //Create holding table
        await CreateHoldingTableAsync();

        //Add Documents from Saved Search in the Export Worker Queue
        var isSuccessful = await AddDocumentsToHoldingTableAsync();

        if (isSuccessful)
        {
          //Copy records from Holding table to Export Worker Queue table
          await CopyRecordsToExportWorkerQueueAsync(markupSubTypes, exportManagerQueueRecord.ExportJobArtifactId, exportManagerQueueRecord.ResourceGroupId);
          //Update status of the Export Job to Completed - Manager
          await UpdateStatusFieldAsync(exportManagerQueueRecord.ExportJobArtifactId, Constant.Status.Job.COMPLETED_MANAGER);
          //Flush the Details of the Export Job 
          await UpdateDetailsFieldAsync(exportManagerQueueRecord.ExportJobArtifactId, string.Empty);
        }
        else
        {
          //Update status of the Export Job to Completed
          await UpdateStatusFieldAsync(exportManagerQueueRecord.ExportJobArtifactId, Constant.Status.Job.COMPLETED);
          //Update Details of the Export Job indicating no Documents in Saved Search
          await UpdateDetailsFieldAsync(exportManagerQueueRecord.ExportJobArtifactId, "No Documents found in the selected Saved Search.");
          //Update Exported Redation Count to 0
          await _artifactQueries.UpdateRdoJobTextFieldAsync(_serviceMgr, WorkspaceArtifactId, ExecutionIdentity.CurrentUser, Constant.Guids.ObjectType.MarkupUtilityExportJob, _markupUtilityExportJob.ArtifactId, Constant.Guids.Field.MarkupUtilityExportJob.ExportedRedactionCount, "0");
        }
      }
      catch (Exception ex)
      {
        //Update status of the Export Job to Error
        await UpdateStatusFieldAsync(exportManagerQueueRecord.ExportJobArtifactId, Constant.Status.Job.ERROR);
        //Update Details of the Export Job 
        await UpdateDetailsFieldAsync(exportManagerQueueRecord.ExportJobArtifactId, ex.ToString());
        //Delete Holding table
        await DeleteHoldingTableAsync();
        //log error
        await LogErrorAsync(ex);
      }

      //delete import job from queue
      RaiseMessage($"Removing record(s) from the queue. [Table = {QueueTable}, ID = {RecordId}, Workspace Artifact ID = {WorkspaceArtifactId}]");
      await FinishAsync();
      await DeleteHoldingTableAsync();
      RaiseMessage($"Removed record(s) from the queue. [Table = {QueueTable}, ID = {RecordId}, Workspace Artifact ID = {WorkspaceArtifactId}]");
      RaiseMessage($"Processed record(s). [Table = {QueueTable}, ID = {RecordId}, Workspace Artifact ID = {WorkspaceArtifactId}]");
    }

    private async Task<bool> AddDocumentsToHoldingTableAsync()
    {
      //Create column mappings for holding table
      var columnMappingsHoldingTable = await GenerateColumnMappingsHoldingTableAsync();
      return await _artifactQueries.AddDocumentsToHoldingTableAsync(_serviceMgr, AgentHelper.GetDBContext(-1), _utilityQueryHelper, ExecutionIdentity.CurrentUser, WorkspaceArtifactId, _markupUtilityExportJob.SavedSearchArtifactId, ExportManagerHoldingTable, columnMappingsHoldingTable);

    }

    private async Task<List<SqlBulkCopyColumnMapping>> GenerateColumnMappingsHoldingTableAsync()
    {
      var columns = new List<string> { "DocumentArtifactID" };
      return await Task.Run(() => _utilityQueryHelper.GetMappingsForWorkerQueue(columns));
    }

    private async Task<string> GenerateMarkupSubTypesAsync()
    {
      var redactionList = new List<int>();

      foreach (var markupUtilityType in _markupUtilityExportJob.MarkupUtilityTypes)
      {
        var redactionType = await _markupTypeHelper.GetMarkupSubTypeValueAsync(markupUtilityType.Name);
        redactionList.Add(redactionType);
      }

      var retVal = string.Join(",", redactionList);
      return retVal;
    }

    private async Task CopyRecordsToExportWorkerQueueAsync(string markupSubTypes, int exportJobArtifactId, int resourceGroupArtifactId)
    {
      await QueryHelper.CopyRecordsToExportWorkerQueueAsync(AgentHelper.GetDBContext(-1), ExportManagerHoldingTable, WorkspaceArtifactId, _markupUtilityExportJob.MarkupSetArtifactId, exportJobArtifactId, markupSubTypes, resourceGroupArtifactId);
    }

    private async Task CreateHoldingTableAsync()
    {
      await QueryHelper.CreateHoldingTableAsync(AgentHelper.GetDBContext(-1), ExportManagerHoldingTable);
    }

    private async Task DeleteHoldingTableAsync()
    {
      await QueryHelper.DropTableAsync(AgentHelper.GetDBContext(-1), ExportManagerHoldingTable);
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
        ex.ToString(),
                Constant.Tables.ExportErrorLog);

      //Add the error to the Relativity Errors tab
      ErrorQueries.WriteError(AgentHelper.GetServicesManager(), ExecutionIdentity.System, WorkspaceArtifactId, ex);
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

    private async Task UpdateDetailsFieldAsync(int exportJobArtifactId, string exportJobStatus)
    {
      await _artifactQueries.UpdateRdoJobTextFieldAsync(
        _serviceMgr,
        WorkspaceArtifactId,
        ExecutionIdentity.CurrentUser,
        Constant.Guids.ObjectType.MarkupUtilityExportJob,
        exportJobArtifactId,
        Constant.Guids.Field.MarkupUtilityExportJob.Details,
        exportJobStatus);
    }
  }
}
