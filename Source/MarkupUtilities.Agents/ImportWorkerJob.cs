using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MarkupUtilities.Helpers;
using MarkupUtilities.Helpers.Exceptions;
using MarkupUtilities.Helpers.Models;
using MarkupUtilities.Helpers.Rsapi.Interfaces;
using Relativity.API;
using Query = MarkupUtilities.Helpers.Utility.Query;

namespace MarkupUtilities.Agents
{
  public class ImportWorkerJob : AgentJobBase
  {
    public string BatchTableName
    {
      get
      {
        return _batchTableName ?? (_batchTableName = $"{Constant.Names.TablePrefix}Import_Worker_{Guid.NewGuid()}_{AgentId}");
      }
    }
    private string _batchTableName;
    private IErrorQueries ErrorQueries { get; set; }
    private IArtifactQueries ArtifactQueries { get; set; }
    private IAuditRecordHelper AuditRecordHelper { get; set; }
    private IMarkupTypeHelper MarkupTypeHelper { get; set; }
    private int _importedRedactionCount;
    private int _skippedRedactionCount;
    private int _errorRedactionCount;
    private string _errorContext = string.Empty;
    private ImportWorkerQueueRecord _importWorkerQueueRecord;
    private string _redactionData = string.Empty;
    private bool _duplicateRedactionInserted;
    private int _importJobArtifactId;
    private readonly ExecutionIdentity _executionIdentity;
    private int _markupSetMultiplechoiceFieldChoiceTypeId;
    private ChoiceModel _hasRedactionsChoiceModel;
    private ChoiceModel _hasHighlightsChoiceModel;

    public ImportWorkerJob(int agentId, IAgentHelper agentHelper, IQuery queryHelper, DateTime processedOnDateTime, IEnumerable<int> resourceGroupIds, IErrorQueries errorQueries, IArtifactQueries artifactQueries, IAuditRecordHelper auditRecordHelper, IMarkupTypeHelper markupTypeHelper)
    {
      RecordId = 0;
      WorkspaceArtifactId = -1;
      AgentId = agentId;
      AgentHelper = agentHelper;
      QueryHelper = queryHelper;
      QueueTable = Constant.Tables.ImportWorkerQueue;
      ProcessedOnDateTime = processedOnDateTime;
      AgentResourceGroupIds = resourceGroupIds;
      ErrorQueries = errorQueries;
      ArtifactQueries = artifactQueries;
      _importedRedactionCount = 0;
      _skippedRedactionCount = 0;
      _errorRedactionCount = 0;
      AuditRecordHelper = auditRecordHelper;
      MarkupTypeHelper = markupTypeHelper;
      _executionIdentity = ExecutionIdentity.CurrentUser;
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
        string commaDelimitedListOfResourceIds = GetCommaDelimitedListOfResourceIds(AgentResourceGroupIds);

        if (commaDelimitedListOfResourceIds != string.Empty)
        {
          DataTable next = await RetrieveNextAsync(commaDelimitedListOfResourceIds);

          if (TableIsNotEmpty(next))
          {
            try
            {
              ImportWorkerQueueRecord firstImportWorkerQueueRecord = new ImportWorkerQueueRecord(next.Rows[0]);

              // Sets the workspaceArtifactID and RecordID so the agent will have access to them in case of an exception
              WorkspaceArtifactId = firstImportWorkerQueueRecord.WorkspaceArtifactId;
              RecordId = firstImportWorkerQueueRecord.Id;
              int importJobArtifactId = firstImportWorkerQueueRecord.ImportJobArtifactId;
              RaiseMessage($"Retrieved record(s) in the queue. [Table = {QueueTable}, ID = {RecordId}, Workspace Artifact ID = {WorkspaceArtifactId}, ImportJobArtifactId = {importJobArtifactId}]");

              //set _imporJobArtifactId
              _importJobArtifactId = firstImportWorkerQueueRecord.ImportJobArtifactId;

              //create error context
              _errorContext = $"[WorkspaceArtifactId = {WorkspaceArtifactId}, ImportJobArtifactId = {_importJobArtifactId}]";

              //Process the record(s)
              string importJobType = firstImportWorkerQueueRecord.JobType;

              await ProcessRecordsAsync(next, importJobType);
            }
            catch (Exception ex)
            {
              throw new MarkupUtilityException("Error encountered while Executing Import Worker.", ex);
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
        string innerMostExceptionMessage = await ConstructDetailsExceptionMessageAsync(ex);

        //update import job status field to complete with errors and details field to inner most exception message
        await UpdateImportJobStatusAndDetailsFieldAsync(Constant.Status.Job.ERROR, innerMostExceptionMessage);

        //log error
        await LogErrorAsync(ex);
      }
      finally
      {
        //drop batch table
        await FinishAsync();
      }
    }

    private Boolean TableIsNotEmpty(DataTable table)
    {
      return (table != null && table.Rows.Count > 0);
    }

    public async Task<DataTable> RetrieveNextAsync(string delimitedListOfResourceGroupIds)
    {
      RaiseMessage($"Creating batch table. [BatchTableName = {BatchTableName}]");
      DataTable next = await QueryHelper.RetrieveNextBatchInImportWorkerQueueAsync(AgentHelper.GetDBContext(-1), AgentId, Constant.Sizes.ImportJobBatchSize, BatchTableName, delimitedListOfResourceGroupIds);
      RaiseMessage($"Created batch table. [BatchTableName = {BatchTableName}]");
      return next;
    }

    public async Task ProcessRecordsAsync(DataTable dataTable, string importJobType)
    {
      RaiseMessage($"Processing record(s). [Table = {QueueTable}, ID = {RecordId}, Workspace Artifact ID = {WorkspaceArtifactId}]");

      switch (importJobType)
      {
        case Constant.ImportJobType.IMPORT:
          await ProcessImportJobAsync(dataTable);
          break;

        case Constant.ImportJobType.REVERT:
          await RevertJobAsync();
          break;

        default:
          throw new MarkupUtilityException("Invalid Import Job Type");
      }

      RaiseMessage($"Processed record(s). [Table = {QueueTable}, ID = {RecordId}, Workspace Artifact ID = {WorkspaceArtifactId}]");
    }

    private async Task ProcessImportJobAsync(DataTable dataTable)
    {
      //reset redaction counts
      _importedRedactionCount = 0;
      _skippedRedactionCount = 0;
      _errorRedactionCount = 0;

      //update status field of the import job to worker in progress and details field to empty string
      await UpdateImportJobStatusAndDetailsFieldAsync(Constant.Status.Job.IN_PROGRESS_WORKER, string.Empty);

      //retrieve import job
      MarkupUtilityImportJob markupUtilityImportJob = await RetrieveImportJobAsync(_importJobArtifactId);

      //set default markup set related field values
      await QueryMarkupSetRelatedFieldValuesAsync(markupUtilityImportJob.MarkupSetArtifactId);

      for (int i = 0; i < dataTable.Rows.Count; i++)
      {
        RaiseMessage($"Processing record - {i + 1}");

        DataRow currentDataRow = dataTable.Rows[i];
        await ProcessSingleImportJobAsync(currentDataRow);

        RaiseMessage($"Processed record - {i + 1}");
      }

      //update all redaction count fields on import job
      await UpdateAllRedactionCountFieldValuesAsync();

      //Remove the record from the import worker queue
      await RemoveRecordsFromImportWorkerQueueTableAsync();

      //update import job status and details field if no records exist in import worker queue for the workspace and import job
      await UpdateImportJobStatusToCompleteIfNoRecordsExistsInImportWorkerQueueForJobAsync();
    }

    private async Task ProcessSingleImportJobAsync(DataRow dataRow)
    {
      _duplicateRedactionInserted = false;

      try
      {
        //convert datarow to object
        _importWorkerQueueRecord = new ImportWorkerQueueRecord(dataRow);

        //set _imporJobArtifactId 
        _importJobArtifactId = _importWorkerQueueRecord.ImportJobArtifactId;

        //create error context
        _errorContext = $"[RecordId = {_importWorkerQueueRecord.Id}, WorkspaceArtifactId = {WorkspaceArtifactId}, ImportJobArtifactId = {_importWorkerQueueRecord.ImportJobArtifactId}]";

        //retrieve import job
        var markupUtilityImportJob = await RetrieveImportJobAsync(_importJobArtifactId);

        //construct redaction data for history record
        _redactionData = await ConstructRedactionDataAsync();

        //get document artifact id
        var documentArtifactId = await GetDocumentArtifactIdAsync();

        //get fileGuid for the document and fileOrder
        var fileGuid = await GetFileGuidForDocumentFileOrderAsync(documentArtifactId);

        //check if markup set exists
        await ArtifactQueries.VerifyIfMarkupSetExistsAsync(AgentHelper.GetServicesManager(), _executionIdentity, WorkspaceArtifactId, markupUtilityImportJob.MarkupSetArtifactId);

        //insert redaction into redaction table
        await InsertMarkupIntoRedactionTableAsync(fileGuid, markupUtilityImportJob, documentArtifactId);
      }
      catch (Exception ex)
      {
        RaiseMessage($"An exception occured when processing import job. {_errorContext}. [Error Message = {ex.Message}]");

        //create Markup Utility history record as completed with errors
        await CreateFailureHistoryRecordAsync(ex);

        //update error redaction count
        _errorRedactionCount++;

        //log error
        await LogErrorAsync(ex);
      }
    }

    private static async Task RevertJobAsync()
    {
      await Task.Run(() => { });
    }

    #region helper methods

    public async Task UpdateImportJobStatusToCompleteIfNoRecordsExistsInImportWorkerQueueForJobAsync()
    {
      string status;

      if (_errorRedactionCount > 0 && _skippedRedactionCount > 0)
      {
        status = Constant.Status.Job.COMPLETED_WITH_ERRORS_AND_SKIPPED_DOCUMENTS;
      }
      else if (_errorRedactionCount > 0)
      {
        status = Constant.Status.Job.COMPLETED_WITH_ERRORS;
      }
      else if (_skippedRedactionCount > 0)
      {
        status = Constant.Status.Job.COMPLETED_WITH_SKIPPED_DOCUMENTS;
      }
      else
      {
        status = Constant.Status.Job.COMPLETED;
      }

      //check if any records exist in import worker queue for the workspace and import job
      Boolean importWorkerQueueHasRecords = await QueryHelper.VerifyIfImportWorkerQueueContainsRecordsForJobAsync(AgentHelper.GetDBContext(-1), WorkspaceArtifactId, _importJobArtifactId);

      if (!importWorkerQueueHasRecords)
      {
        //update status field of the import job to complete and details field to empty string
        await UpdateImportJobStatusAndDetailsFieldAsync(status, string.Empty);
      }
    }

    private async Task<MarkupUtilityImportJob> RetrieveImportJobAsync(int importJobArtifactId)
    {
      RaiseMessage($"Retrieving import job. {_errorContext}]");

      return await ArtifactQueries.RetrieveImportJobAsync(
        AgentHelper.GetServicesManager(),
        _executionIdentity,
        WorkspaceArtifactId,
        importJobArtifactId);
    }

    private async Task UpdateAllRedactionCountFieldValuesAsync()
    {
      //update imported redaction count
      await UpdateRedactionCountFieldValueAsync(Constant.Guids.Field.MarkupUtilityImportJob.ImportedRedactionCount);

      //update skipped redaction count
      await UpdateRedactionCountFieldValueAsync(Constant.Guids.Field.MarkupUtilityImportJob.SkippedRedactionCount);

      //update error redaction count
      await UpdateRedactionCountFieldValueAsync(Constant.Guids.Field.MarkupUtilityImportJob.ErrorRedactionCount);
    }

    private async Task UpdateRedactionCountFieldValueAsync(Guid countFieldGuid)
    {
      var countFieldName = await GetCountFieldNameAsync(countFieldGuid);
      var countFieldValue = await GetCountFieldValueAsync(countFieldGuid);

      RaiseMessage($"Retrieving current {countFieldName}");

      //retrieve imported redaction count field on import job
      var currentRedactionCount = await ArtifactQueries.RetrieveImportJobRedactionCountFieldValueAsync(
        AgentHelper.GetServicesManager(),
        _executionIdentity,
        WorkspaceArtifactId,
        _importJobArtifactId,
        countFieldGuid);

      RaiseMessage($"Current {countFieldName} = {currentRedactionCount}");
      var newRedactionCount = currentRedactionCount + countFieldValue;
      RaiseMessage($"New {countFieldName} = {newRedactionCount}");
      RaiseMessage($"Updating current {countFieldName}");

      //update imported redaction count field on import job
      await ArtifactQueries.UpdateImportJobRedactionCountFieldValueAsync(
        AgentHelper.GetServicesManager(),
        _executionIdentity,
        WorkspaceArtifactId,
        _importJobArtifactId,
        countFieldGuid,
        newRedactionCount);

      RaiseMessage($"Updated {countFieldName}");
    }

    private async Task<string> GetCountFieldNameAsync(Guid countFieldGuid)
    {
      var retVal = string.Empty;

      await Task.Run(() =>
      {
        if (countFieldGuid == Constant.Guids.Field.MarkupUtilityImportJob.ImportedRedactionCount)
        {
          retVal = Constant.ImportJobRedactionCountFieldNames.IMPORTED_REDACTION_COUNT;
        }
        else if (countFieldGuid == Constant.Guids.Field.MarkupUtilityImportJob.SkippedRedactionCount)
        {
          retVal = Constant.ImportJobRedactionCountFieldNames.SKIPPED_REDACTION_COUNT;
        }
        else if (countFieldGuid == Constant.Guids.Field.MarkupUtilityImportJob.ErrorRedactionCount)
        {
          retVal = Constant.ImportJobRedactionCountFieldNames.ERROR_REDACTION_COUNT;
        }
        else
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.NOT_A_VALID_REDACTION_COUNT_FIELD_ON_THE_IMPORT_JOB_RDO);
        }
      });

      return retVal;
    }

    private async Task<int> GetCountFieldValueAsync(Guid countFieldGuid)
    {
      var retVal = 0;

      await Task.Run(() =>
      {
        if (countFieldGuid == Constant.Guids.Field.MarkupUtilityImportJob.ImportedRedactionCount)
        {
          retVal = _importedRedactionCount;
        }
        else if (countFieldGuid == Constant.Guids.Field.MarkupUtilityImportJob.SkippedRedactionCount)
        {
          retVal = _skippedRedactionCount;
        }
        else if (countFieldGuid == Constant.Guids.Field.MarkupUtilityImportJob.ErrorRedactionCount)
        {
          retVal = _errorRedactionCount;
        }
        else
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.NOT_A_VALID_REDACTION_COUNT_FIELD_ON_THE_IMPORT_JOB_RDO);
        }
      });

      return retVal;
    }

    private async Task InsertMarkupIntoRedactionTableAsync(string fileGuid, MarkupUtilityImportJob markupUtilityImportJob, int documentArtifactId)
    {
      //verify if redaction insert has to skipped for duplicate redaction
      var skipInsertRedactionIntoRedactionTable = await CheckToSkipInsertRedactionIntoRedactionTableAsync(fileGuid, markupUtilityImportJob);

      if (!skipInsertRedactionIntoRedactionTable)
      {
        RaiseMessage($"Inserting redaction into redaction table. {_errorContext}");

        //insert redaction into Redaction table
        var redactionId = await QueryHelper.InsertRowIntoRedactionTableAsync(
          AgentHelper.GetDBContext(WorkspaceArtifactId),
          fileGuid,
          markupUtilityImportJob.MarkupSetArtifactId,
          _importWorkerQueueRecord);

        if (redactionId > 0) //redaction was created
        {
          RaiseMessage($"Redaction inserted into redaction table. {_errorContext}");

          //update Has Redactions/Has Highlights choice values for the document
          await UpdateMarkupSetMultipleChoiceFieldValueAsync(documentArtifactId, _importWorkerQueueRecord.MarkupType);

          //create audit record
          await CreateAuditRecordAsync(fileGuid, markupUtilityImportJob, documentArtifactId, redactionId);

          //create Markup Utility history record as completed
          await CreateSuccessHistoryRecordAsync(redactionId);

          //update imported redaction count
          _importedRedactionCount++;
        }
        else //redaction creation failed
        {
          RaiseMessage($"An error occured when inserting redaction into redaction table. {_errorContext}");

          throw new MarkupUtilityException(Constant.ErrorMessages.INSERT_REDACTION_INTO_REDACTION_TABLE_ERROR);
        }
      }
      else
      {
        RaiseMessage($"Duplicate redaction found. skipping redaction insert into redaction table. {_errorContext}");

        //create Markup Utility history record as skipped
        await CreateSkippedHistoryRecordAsync();

        //update skipped redaction count
        _skippedRedactionCount++;
      }
    }

    private async Task CreateSuccessHistoryRecordAsync(int? redactionId)
    {
      RaiseMessage($"Creating success history record. {_errorContext}");

      await CreateHistoryRecordAsync(
        redactionId,
        Constant.Status.History.COMPLETED,
        _duplicateRedactionInserted
        ? Constant.Status.History.Details.DUPLICATE_REDACTION_FOUND
        : Constant.Status.History.Details.EMPTY_STRING);
    }

    private async Task CreateFailureHistoryRecordAsync(Exception exception)
    {
      RaiseMessage($"Creating failure history record. {_errorContext}");

      var innerMostExceptionMessage = await ConstructExceptionMessageAsync(exception);

      await CreateHistoryRecordAsync(
        null,
        Constant.Status.History.ERROR,
        innerMostExceptionMessage);
    }

    private async Task CreateSkippedHistoryRecordAsync()
    {
      RaiseMessage($"Creating skipped history record. {_errorContext}");

      await CreateHistoryRecordAsync(
        null,
        Constant.Status.History.SKIPPED,
        Constant.Status.History.Details.DUPLICATE_REDACTION_FOUND);
    }

    private async Task CreateHistoryRecordAsync(int? redactionId, string status, string details)
    {
      var redactionType = await MarkupTypeHelper.GetMarkupSubTypeNameAsync(_importWorkerQueueRecord.MarkupSubType);
      var pageNumber = _importWorkerQueueRecord.FileOrder + 1;

      await ArtifactQueries.CreateMarkupUtilityHistoryRecordAsync(
        AgentHelper.GetServicesManager(),
        _executionIdentity,
        WorkspaceArtifactId,
        _importWorkerQueueRecord.ImportJobArtifactId,
        _importWorkerQueueRecord.DocumentIdentifier,
        pageNumber,
        Constant.ImportJobType.IMPORT,
        redactionType,
        status,
        details,
        _redactionData,
        redactionId, -1);
    }

    private async Task CreateAuditRecordAsync(string fileGuid, MarkupUtilityImportJob markupUtilityImportJob, int documentArtifactId, int redactionId)
    {
      RaiseMessage($"creating audit record. {_errorContext}");

      await AuditRecordHelper.CreateRedactionAuditRecordAsync(
        AgentHelper.GetDBContext(WorkspaceArtifactId),
        Constant.AuditRecord.AuditAction.REDACTION_CREATED,
        documentArtifactId,
        markupUtilityImportJob.CreatedBy,
        _importWorkerQueueRecord,
        markupUtilityImportJob.MarkupSetArtifactId,
        redactionId,
        fileGuid);
    }

    private async Task<bool> CheckToSkipInsertRedactionIntoRedactionTableAsync(string fileGuid, MarkupUtilityImportJob markupUtilityImportJob)
    {
      RaiseMessage($"Checking for duplicate redaction. {_errorContext}");

      var retVal = true;

      //check for duplicate redaction
      var hasRedactions = await QueryHelper.DoesRedactionExistAsync(AgentHelper.GetDBContext(WorkspaceArtifactId), fileGuid, markupUtilityImportJob.MarkupSetArtifactId, _importWorkerQueueRecord);

      if (hasRedactions)
      {
        _duplicateRedactionInserted = true;
      }

      if (!_importWorkerQueueRecord.SkipDuplicateRedactions || (_importWorkerQueueRecord.SkipDuplicateRedactions && !hasRedactions))
      {
        retVal = false;
      }

      return retVal;
    }

    private async Task<string> GetFileGuidForDocumentFileOrderAsync(int documentArtifactId)
    {
      RaiseMessage($"Checking if document has image(s). {_errorContext}");

      //check if document has images
      var hasImages = await QueryHelper.DoesDocumentHasImagesAsync(AgentHelper.GetDBContext(WorkspaceArtifactId), documentArtifactId, _importWorkerQueueRecord.FileOrder);
      if (!hasImages)
      {
        throw new MarkupUtilityException("Document does not have image.");
      }

      //get fileGuid for the document and fileOrder
      return await QueryHelper.GetFileGuidForDocumentAsync(AgentHelper.GetDBContext(WorkspaceArtifactId), documentArtifactId, _importWorkerQueueRecord.FileOrder);
    }

    private async Task<int> GetDocumentArtifactIdAsync()
    {
      RaiseMessage($"Getting document identifier field name. {_errorContext}");

      //get document identifier field name
      var documentIdentifierFieldName = await QueryHelper.GetDocumentIdentifierFieldNameAsync(AgentHelper.GetDBContext(WorkspaceArtifactId));

      RaiseMessage($"Checking if document exists. {_errorContext}");

      //check if document exists
      var doesDocumentExists = await ArtifactQueries.DoesDocumentExistAsync(AgentHelper.GetServicesManager(), _executionIdentity, WorkspaceArtifactId, documentIdentifierFieldName, _importWorkerQueueRecord.DocumentIdentifier);
      if (!doesDocumentExists)
      {
        throw new MarkupUtilityException("Document does not exist.");
      }

      RaiseMessage($"Getting document artifact id. {_errorContext}");

      //get document artifact id
      var documentArtifactId = await ArtifactQueries.RetrieveDocumentArtifactIdAsync(AgentHelper.GetServicesManager(), _executionIdentity, WorkspaceArtifactId, documentIdentifierFieldName, _importWorkerQueueRecord.DocumentIdentifier);
      return documentArtifactId;
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
        _executionIdentity,
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
        _executionIdentity,
        Constant.Guids.ObjectType.MarkupUtilityImportJob,
        _importJobArtifactId,
        Constant.Guids.Field.MarkupUtilityImportJob.Details,
        details);
    }

    private async Task<string> ConstructExceptionMessageAsync(Exception exception)
    {
      var retVal = await ExceptionMessageHelper.GetInnerMostExceptionMessageAsync(exception);
      return retVal;
    }

    private async Task<string> ConstructDetailsExceptionMessageAsync(Exception exception)
    {
      var retVal = await ExceptionMessageHelper.GetInnerMostExceptionMessageAsync(exception);
      retVal += $". {Constant.ErrorMessages.REFER_TO_ERRORS_TAB_FOR_MORE_DETAILS}";
      return retVal;
    }

    private async Task<string> ConstructRedactionDataAsync()
    {
      var retVal = await Task.Run(() => _importWorkerQueueRecord.ToStringRedactionData());
      return retVal;
    }

    public async Task FinishAsync()
    {
      RaiseMessage($"Dropping batch table. [BatchTableName = {BatchTableName}]");
      await QueryHelper.DropTableAsync(AgentHelper.GetDBContext(-1), BatchTableName);
      RaiseMessage($"Dropped batch table. [BatchTableName = {BatchTableName}]");
    }

    private async Task RemoveRecordsFromImportWorkerQueueTableAsync()
    {
      RaiseMessage($"Removing record(s) from the queue. [Table = {QueueTable}, ID = {RecordId}, Workspace Artifact ID = {WorkspaceArtifactId}]");
      await QueryHelper.RemoveBatchFromImportWorkerQueueAsync(AgentHelper.GetDBContext(-1), BatchTableName);
      RaiseMessage($"Removed record(s) from the queue. [Table = {QueueTable}, ID = {RecordId}, Workspace Artifact ID = {WorkspaceArtifactId}]");
    }

    private async Task LogErrorAsync(Exception exception)
    {
      RaiseMessage($"Logging error. {_errorContext}");

      //Add the error to our custom Errors table
      await QueryHelper.InsertRowIntoImportErrorLogAsync(
        AgentHelper.GetDBContext(-1),
        WorkspaceArtifactId,
        Constant.Tables.ImportWorkerQueue,
        RecordId,
        AgentId,
        exception.ToString());

      //Add the error to the Relativity Errors tab
      ErrorQueries.WriteError(AgentHelper.GetServicesManager(), ExecutionIdentity.System, WorkspaceArtifactId, exception);
    }

    private async Task QueryMarkupSetRelatedFieldValuesAsync(int markupSetArtifactId)
    {
      //retrieve markup set multipe choice field name
      var markupSetName = await ArtifactQueries.RetreiveMarkupSetNameAsync(AgentHelper.GetServicesManager(), _executionIdentity, WorkspaceArtifactId, markupSetArtifactId);
      string markupSetMultichoiceFieldName = $"{Constant.MarkupSet.MARKUP_SET_FIELD_NAME_PREFIX}{markupSetName}";

      //retrieve markup set multipe choice field artifact id
      _markupSetMultiplechoiceFieldChoiceTypeId = await ArtifactQueries.RetreiveMarkupSetMultipleChoiceFieldTypeIdAsync(AgentHelper.GetServicesManager(), _executionIdentity, WorkspaceArtifactId, markupSetMultichoiceFieldName);

      //retrieve markup set multipe choice field choice values artifact id's
      var choices = await ArtifactQueries.QueryAllMarkupSetMultipleChoiceFieldValuesAsync(AgentHelper.GetServicesManager(), _executionIdentity, WorkspaceArtifactId, markupSetMultichoiceFieldName);

      _hasRedactionsChoiceModel = choices.First(x => x.Name.Equals(Constant.MarkupSet.MarkupSetMultiChoiceValues.HAS_REDACTIONS));
      _hasHighlightsChoiceModel = choices.First(x => x.Name.Equals(Constant.MarkupSet.MarkupSetMultiChoiceValues.HAS_HIGHLIGHTS));
    }

    private async Task UpdateMarkupSetMultipleChoiceFieldValueAsync(int documentArtifactId, int markupType)
    {
      const string errorContext = "An error occured when updating markup set multiple choice field value.";

      try
      {
        int choiceArtifactId;
        switch (markupType)
        {
          case Constant.MarkupType.Redaction.VALUE:
            choiceArtifactId = _hasRedactionsChoiceModel.ArtifactId;
            break;
          case Constant.MarkupType.Highlight.VALUE:
            choiceArtifactId = _hasHighlightsChoiceModel.ArtifactId;
            break;
          default:
            throw new MarkupUtilityException($"Invalid Markup Type. [MarkupType = {markupType}]. {errorContext}");
        }

        await QueryHelper.UpdateMarkupSetMultipleChoiceFieldAsync(
          AgentHelper.GetDBContext(WorkspaceArtifactId),
          documentArtifactId,
          _markupSetMultiplechoiceFieldChoiceTypeId,
          choiceArtifactId);
      }
      catch (Exception ex)
      {
        throw new MarkupUtilityException($"{errorContext}", ex);
      }
    }

    #endregion
  }
}
