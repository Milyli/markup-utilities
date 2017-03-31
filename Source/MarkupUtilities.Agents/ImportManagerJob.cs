using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MarkupUtilities.Helpers;
using MarkupUtilities.Helpers.Exceptions;
using MarkupUtilities.Helpers.Models;
using MarkupUtilities.Helpers.Rsapi.Interfaces;
using Relativity.API;

namespace MarkupUtilities.Agents
{
  public class ImportManagerJob : AgentJobBase
  {
    private IArtifactQueries ArtifactQueries { get; set; }
    private IImportFileParser ImportFileParser { get; set; }
    private IWorkspaceQueries WorkspaceQueryHelper { get; set; }
    private MarkupUtilityImportJob _markupUtilityImportJob;
    private IErrorQueries ErrorQueries { get; set; }
    private IMarkupTypeHelper MarkupTypeHelper { get; set; }
    private string _importManagerHoldingTable;
    public string ImportManagerHoldingTable
    {
      get
      {
        return _importManagerHoldingTable ?? (_importManagerHoldingTable = $"{Constant.Names.ImportManagerHoldingTablePrefix}{Guid.NewGuid()}_{AgentId}");
      }
    }
    private int _importFileRedactionCount;
    private int _expectedRedactionCount;
    private string _errorContext;
    private int _importJobArtifactId;

    public ImportManagerJob(int agentId, IAgentHelper agentHelper, IQuery queryHelper, DateTime processedOnDateTime, IEnumerable<int> resourceGroupIds, IArtifactQueries artifactQueries, IImportFileParser importFileParser, IWorkspaceQueries workspaceQueryHelper, IErrorQueries errorQueries, IMarkupTypeHelper markupTypeHelper)
    {
      RecordId = 0;
      WorkspaceArtifactId = -1;
      AgentId = agentId;
      AgentHelper = agentHelper;
      QueryHelper = queryHelper;
      ProcessedOnDateTime = processedOnDateTime;
      QueueTable = Constant.Tables.ImportManagerQueue;
      AgentResourceGroupIds = resourceGroupIds;
      ArtifactQueries = artifactQueries;
      ImportFileParser = importFileParser;
      WorkspaceQueryHelper = workspaceQueryHelper;
      ErrorQueries = errorQueries;
      MarkupTypeHelper = markupTypeHelper;
    }

    public override async Task ExecuteAsync()
    {
      //reset count properties
      _importFileRedactionCount = 0;
      _expectedRedactionCount = 0;

      try
      {
        //Check for jobs which stopped unexpectedly on this agent thread
        RaiseMessage($"Resetting records which failed. [Table = {QueueTable}]");
        await ResetUnfishedJobsAsync(AgentHelper.GetDBContext(-1));

        //Retrieve the next record to work on
        RaiseMessage($"Retrieving next record(s) in the queue. [Table = {QueueTable}]");
        var commaDelimitedListOfResourceIds = GetCommaDelimitedListOfResourceIds(AgentResourceGroupIds);
        if (commaDelimitedListOfResourceIds != string.Empty)
        {
          var next = await RetrieveNextAsync(commaDelimitedListOfResourceIds);

          if (TableIsNotEmpty(next))
          {
            var importManagerQueueRecord = new ImportManagerQueueRecord(next.Rows[0]);

            // Sets the workspaceArtifactID and RecordID so the agent will have access to them in case of an exception
            WorkspaceArtifactId = importManagerQueueRecord.WorkspaceArtifactId;
            RecordId = importManagerQueueRecord.Id;
            RaiseMessage($"Retrieved record(s) in the queue. [Table = {QueueTable}, ID = {RecordId}, Workspace Artifact ID = {WorkspaceArtifactId}]");

            //set _importJobArtifactId
            _importJobArtifactId = importManagerQueueRecord.ImportJobArtifactId;

            //Process the record(s)
            await ProcessRecordsAsync(importManagerQueueRecord);

            //delete import job from queue
            await FinishAsync();
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
        var innerMostExceptionMessage = await ConstructDetailsExceptionMessageAsync(ex);

        //update import job status field to complete with errors and details field to inner most exception message
        await UpdateImportJobStatusAndDetailsFieldAsync(Constant.Status.Job.ERROR, innerMostExceptionMessage);

        //log error
        await LogErrorAsync(ex);
      }
    }

    public async Task ProcessRecordsAsync(ImportManagerQueueRecord importManagerQueueRecord)
    {
      RaiseMessage($"Processing record(s). [Table = {QueueTable}, ID = {RecordId}, Workspace Artifact ID = {WorkspaceArtifactId}]");

      switch (importManagerQueueRecord.JobType)
      {
        case Constant.ImportJobType.VALIDATE:
          await ValidateImportJobAsync(importManagerQueueRecord);
          break;

        case Constant.ImportJobType.IMPORT:
          await ImportJobAsync(importManagerQueueRecord);
          break;

        case Constant.ImportJobType.REVERT:
          await RevertJobAsync();
          break;

        default:
          throw new MarkupUtilityException("Invalid Import Job Type");
      }

      RaiseMessage($"Processed record(s). [Table = {QueueTable}, ID = {RecordId}, Workspace Artifact ID = {WorkspaceArtifactId}]");
    }

    private async Task ValidateImportJobAsync(ImportManagerQueueRecord importManagerQueueRecord)
    {
      _errorContext = $"[WorkspaceArtifactId = {WorkspaceArtifactId}, ImportJobArtifactId = {importManagerQueueRecord.ImportJobArtifactId}]";
      RaiseMessage($"Validating import job. {_errorContext}");

      try
      {
        //retrieve import job
        _markupUtilityImportJob = await RetrieveImportJobAsync(importManagerQueueRecord);

        //update status field of the import job to validating and details field to empty string
        await UpdateImportJobStatusAndDetailsFieldAsync(Constant.Status.Job.VALIDATING, string.Empty);

        //read contents of the import job file
        var fileContentsStream = await ReadImportJobFileContentsAsync();

        //validate contents of the import job file
        await ValidateImportJobFileContentsAsync(fileContentsStream);

        //update status field of the import job to validating and details field to empty string
        await UpdateImportJobStatusAndDetailsFieldAsync(Constant.Status.Job.VALIDATED, string.Empty);
      }
      catch (Exception ex)
      {
        RaiseMessage($"An exception occured when validating import job. {_errorContext}. [Error Message = {ex.Message}]");

        var innerMostExceptionMessage = await ConstructDetailsExceptionMessageAsync(ex);

        //update status field of the import job to validation fail and details field to exception message
        await UpdateImportJobStatusAndDetailsFieldAsync(Constant.Status.Job.VALIDATION_FAILED, innerMostExceptionMessage);

        //log error
        await LogErrorAsync(ex);
      }

      RaiseMessage($"Validated import job. {_errorContext}");
    }

    private async Task ImportJobAsync(ImportManagerQueueRecord importManagerQueueRecord)
    {
      _errorContext = $"[WorkspaceArtifactId = {WorkspaceArtifactId}, ImportJobArtifactId = {importManagerQueueRecord.ImportJobArtifactId}]";
      RaiseMessage($"Processing import job. {_errorContext}");

      try
      {
        //Create import manager holding table
        await CreateImportManagerHoldingTableAsync();

        //retrieve import job
        _markupUtilityImportJob = await RetrieveImportJobAsync(importManagerQueueRecord);

        //update status field of the import job to manager in progress and details field to empty string
        await UpdateImportJobStatusAndDetailsFieldAsync(Constant.Status.Job.IN_PROGRESS_MANAGER, string.Empty);

        //read contents of the import job file
        var fileContentsStream = await ReadImportJobFileContentsAsync();

        //parse import job file for contents
        await ParseImportJobFileContentsAsync(fileContentsStream);

        //bulk copy data from import manager holding table to import worker queue table
        await BulkCopyDataFromImportManagerHoldingTableIntoImportWorkerQueueTableAsync();

        //update status field of the import job to manager complete and details field to empty string
        await UpdateImportJobStatusAndDetailsFieldAsync(Constant.Status.Job.COMPLETED_MANAGER, string.Empty);
      }
      catch (Exception ex)
      {
        RaiseMessage($"An exception occured when processing import job. {_errorContext}. [Error Message = {ex.Message}]");

        var innerMostExceptionMessage = await ConstructDetailsExceptionMessageAsync(ex);

        //update status field of the import job to parsing fail and details field to exception message
        await UpdateImportJobStatusAndDetailsFieldAsync(Constant.Status.Job.ERROR, innerMostExceptionMessage);

        //log error
        await LogErrorAsync(ex);
      }
      finally
      {
        //drop import manager holding table
        await DropImportManagerHoldingTableAsync();
      }

      RaiseMessage($"Processed import job. {_errorContext}");
    }

    private static async Task RevertJobAsync()
    {
      // *******************************************************************************************************
      //
      //       *********
      //       *       *
      //  *********    *
      //  *    *  *    *
      //  *    *********
      //  *       *
      //  *********
      //
      //  This functionality is currently missing.
      //
      //  If you are interested in building this functionality, please make sure to write associated unit tests.
      //  The basic concept is that the Import Worker Agent will retrieve all the redactrions that were imported
      //  for the selected job and delete them from the Redaction table.
      // *******************************************************************************************************
      await Task.Run(() =>
{

});
    }

    #region helper methods

    private Boolean TableIsNotEmpty(DataTable dataTable)
    {
      return (dataTable != null && dataTable.Rows.Count > 0);
    }

    public async Task<DataTable> RetrieveNextAsync(string commaDelimitedResourceAgentIds)
    {
      DataTable next = await QueryHelper.RetrieveNextInImportManagerQueueAsync(AgentHelper.GetDBContext(-1), AgentId, commaDelimitedResourceAgentIds);
      return next;
    }

    private async Task ValidateImportJobFileContentsAsync(StreamReader fileContentsStream)
    {
      RaiseMessage($"Validating contents of import file for import job. {_errorContext}");

      await ImportFileParser.ValidateFileContentsAsync(fileContentsStream);
    }

    private async Task<StreamReader> ReadImportJobFileContentsAsync()
    {
      RaiseMessage($"Reading contents of import file for import job. {_errorContext}");

      return await ArtifactQueries.GetFileFieldContentsAsync(
        AgentHelper.GetServicesManager(),
        ExecutionIdentity.CurrentUser,
        WorkspaceArtifactId,
        Constant.Guids.Field.MarkupUtilityFile.File,
        _markupUtilityImportJob.FileArtifactId);
    }

    private async Task UpdateImportJobStatusAndDetailsFieldAsync(string status, string details)
    {
      await UpdateImportJobStatusFieldAsync(status);
      await UpdateImportJobDetailsFieldAsync(details);
    }

    private async Task UpdateImportJobStatusFieldAsync(string status)
    {
      RaiseMessage($"Updating import job status to {status}. {_errorContext}]");

      await ArtifactQueries.UpdateRdoJobTextFieldAsync(
        AgentHelper.GetServicesManager(),
        WorkspaceArtifactId,
        ExecutionIdentity.CurrentUser,
        Constant.Guids.ObjectType.MarkupUtilityImportJob,
        _importJobArtifactId,
        Constant.Guids.Field.MarkupUtilityImportJob.Status,
        status);
    }

    private async Task UpdateImportJobDetailsFieldAsync(string details)
    {
      RaiseMessage($"Updating import job details. {_errorContext}");

      await ArtifactQueries.UpdateRdoJobTextFieldAsync(
        AgentHelper.GetServicesManager(),
        WorkspaceArtifactId,
        ExecutionIdentity.CurrentUser,
        Constant.Guids.ObjectType.MarkupUtilityImportJob,
        _importJobArtifactId,
        Constant.Guids.Field.MarkupUtilityImportJob.Details,
        details);
    }

    private async Task<MarkupUtilityImportJob> RetrieveImportJobAsync(ImportManagerQueueRecord importManagerQueueRecord)
    {
      RaiseMessage($"Retrieving import job. {_errorContext}]");

      return await ArtifactQueries.RetrieveImportJobAsync(
        AgentHelper.GetServicesManager(),
        ExecutionIdentity.CurrentUser,
        WorkspaceArtifactId,
        importManagerQueueRecord.ImportJobArtifactId);
    }

    private async Task BulkCopyDataFromImportManagerHoldingTableIntoImportWorkerQueueTableAsync()
    {
      RaiseMessage($"Bulk copying data from import manager holding table into import worker queue table. {_errorContext}");

      await QueryHelper.BulkCopyDataFromImportManagerHoldingTableIntoImportWorkerQueueTableAsync(AgentHelper.GetDBContext(-1), ImportManagerHoldingTable);
    }

    private async Task ParseImportJobFileContentsAsync(StreamReader fileContentsStream)
    {
      RaiseMessage($"Parsing contents of import file for import job. {_errorContext}");

      await ImportFileParser.ParseFileContentsAsync(fileContentsStream, ProcessEachLineAsync, AfterProcessingAllLinesAsync);
    }

    private async Task CreateImportManagerHoldingTableAsync()
    {
      RaiseMessage($"Creating import manager holding table for import job [ImportManagerHoldingTable = {ImportManagerHoldingTable}]. {_errorContext}");

      await QueryHelper.CreateImportManagerHoldingTableAsync(AgentHelper.GetDBContext(-1), ImportManagerHoldingTable);
    }

    private async Task DropImportManagerHoldingTableAsync()
    {
      RaiseMessage($"Dropping import manager holding table for import job [ImportManagerHoldingTable = {ImportManagerHoldingTable}]. {_errorContext}");

      await QueryHelper.DropTableAsync(AgentHelper.GetDBContext(-1), ImportManagerHoldingTable);
    }

    private async Task<Boolean> ProcessEachLineAsync(ImportFileRecord importFileRecord)
    {
      Boolean retVal = false;

      try
      {
        if (importFileRecord != null)
        {
          int resourceGroupId = await WorkspaceQueryHelper.GetResourcePoolAsync(AgentHelper.GetServicesManager(), ExecutionIdentity.System, WorkspaceArtifactId);

          //get selected markup sub types in the import job
          List<int> selectedMarkupSubTypes = _markupUtilityImportJob
            .MarkupUtilityTypes
            .Select(x =>
              MarkupTypeHelper.GetMarkupSubTypeValueAsync(x.Name).Result)
            .ToList();

          //insert into import manager holding table only if the sub type is selected in the import job
          if (selectedMarkupSubTypes.Contains(importFileRecord.MarkupSubType))
          {
            await QueryHelper.InsertRowIntoImportManagerHoldingTableAsync(
              AgentHelper.GetDBContext(-1),
              WorkspaceArtifactId,
              importFileRecord.DocumentIdentifier,
              importFileRecord.FileOrder,
              resourceGroupId,
              _markupUtilityImportJob.ArtifactId,
              _markupUtilityImportJob.MarkupSetArtifactId,
              _markupUtilityImportJob.JobType,
              importFileRecord,
              _markupUtilityImportJob.SkipDuplicateRedactions,
              ImportManagerHoldingTable);

            _expectedRedactionCount++;
          }

          retVal = true;
        }
      }
      catch (Exception ex)
      {
        throw new MarkupUtilityException("An error occured when inserting redaction record to the import manager holding table.", ex);
      }
      finally
      {
        _importFileRedactionCount++;

        if (_importFileRedactionCount % 100 == 0)
        {
          RaiseMessage($"Parsed {_importFileRedactionCount} records in import file.");
        }
      }

      return retVal;
    }

    private async Task<Boolean> AfterProcessingAllLinesAsync()
    {
      string errorContext = $"An error occured when updating total redaction count on the import job. [ImportJobArtifactId: {_importJobArtifactId}]";

      try
      {
        //update import file redaction count field on import job
        await ArtifactQueries.UpdateImportJobRedactionCountFieldValueAsync(
          AgentHelper.GetServicesManager(),
          ExecutionIdentity.CurrentUser,
          WorkspaceArtifactId,
          _importJobArtifactId,
          Constant.Guids.Field.MarkupUtilityImportJob.ImportFileRedactionCount,
          _importFileRedactionCount);

        RaiseMessage($"Updated import file redaction count ({_importFileRedactionCount})");

        //update expected redaction count field on import job
        await ArtifactQueries.UpdateImportJobRedactionCountFieldValueAsync(
          AgentHelper.GetServicesManager(),
          ExecutionIdentity.CurrentUser,
          WorkspaceArtifactId,
          _importJobArtifactId,
          Constant.Guids.Field.MarkupUtilityImportJob.ExpectedRedactionCount,
          _expectedRedactionCount);

        RaiseMessage($"Updated expected redaction count ({_expectedRedactionCount})");
      }
      catch (Exception ex)
      {
        throw new MarkupUtilityException(errorContext, ex);
      }

      return true;
    }

    private async Task<string> ConstructDetailsExceptionMessageAsync(Exception exception)
    {
      string retVal = await ExceptionMessageHelper.GetInnerMostExceptionMessageAsync(exception);
      retVal += $". {Constant.ErrorMessages.REFER_TO_ERRORS_TAB_FOR_MORE_DETAILS}";
      return retVal;
    }

    public async Task FinishAsync()
    {
      RaiseMessage($"Removing record(s) from the queue. [Table = {QueueTable}, ID = {RecordId}, Workspace Artifact ID = {WorkspaceArtifactId}]");

      await QueryHelper.RemoveRecordFromTableByIdAsync(AgentHelper.GetDBContext(-1), QueueTable, RecordId);

      RaiseMessage($"Removed record(s) from the queue. [Table = {QueueTable}, ID = {RecordId}, Workspace Artifact ID = {WorkspaceArtifactId}]");
    }

    private async Task LogErrorAsync(Exception ex)
    {
      RaiseMessage($"Logging error. {_errorContext}]");

      //Add the error to our custom Errors table
      await QueryHelper.InsertRowIntoImportErrorLogAsync(
        AgentHelper.GetDBContext(-1),
        WorkspaceArtifactId,
        Constant.Tables.ImportManagerQueue,
        RecordId,
        AgentId,
        ex.ToString());

      //Add the error to the Relativity Errors tab
      ErrorQueries.WriteError(AgentHelper.GetServicesManager(), ExecutionIdentity.System, WorkspaceArtifactId, ex);
    }

    #endregion
  }
}
