using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using DTOs = kCura.Relativity.Client.DTOs;
using System.Net.Http;
using MarkupUtilities.Helpers.Exceptions;
using MarkupUtilities.Helpers.Models;
using MarkupUtilities.Helpers.Rsapi.Interfaces;
using MarkupUtilities.Helpers.Utility;

namespace MarkupUtilities.Helpers.Rsapi
{
  public class ArtifactQueries : IArtifactQueries
  {
    //Do not convert to async
    public bool DoesUserHaveAccessToArtifact(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, Guid guid, string artifactTypeName)
    {
      var result = DoesUserHaveAccessToRdoByType(svcMgr, identity, workspaceArtifactId, guid, artifactTypeName);
      var hasAccess = result.Success;

      return hasAccess;
    }

    //Do not convert to async
    public Response<bool> DoesUserHaveAccessToRdoByType(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, Guid guid, string artifactTypeName)
    {
      ResultSet<RDO> results;

      using (var client = svcMgr.CreateProxy<IRSAPIClient>(identity))
      {
        client.APIOptions.WorkspaceID = workspaceArtifactId;
        var relApp = new RDO(guid)
        {
          ArtifactTypeName = artifactTypeName
        };

        results = client.Repositories.RDO.Read(relApp);
      }

      var res = new Response<bool>
      {
        Results = results.Success,
        Success = results.Success,
        Message = MessageFormatter.FormatMessage(results.Results.Select(x => x.Message).ToList(), results.Message, results.Success)
      };

      return res;
    }

    public async Task<string> RetrieveRdoJobStatusAsync(IServicesMgr svcMgr, int workspaceArtifactId, ExecutionIdentity identity, int artifactId)
    {
      try
      {
        using (var client = svcMgr.CreateProxy<IRSAPIClient>(identity))
        {
          client.APIOptions.WorkspaceID = workspaceArtifactId;
          RDO rdoJob;

          try
          {
            rdoJob = await Task.Run(() => client.Repositories.RDO.ReadSingle(artifactId));
          }
          catch (Exception ex)
          {
            throw new MarkupUtilityException("An error occurred when querying for the status of the RDO (ReadSingle).", ex);
          }

          return rdoJob.Fields.Get("Status").ToString();
        }
      }
      catch (Exception ex)
      {
        throw new MarkupUtilityException("An error occurred when querying for the status of the RDO.", ex);
      }
    }

    public async Task UpdateRdoJobTextFieldAsync(IServicesMgr svcMgr, int workspaceArtifactId, ExecutionIdentity identity, Guid objectTypeGuid, int artifactId, Guid textFieldGuid, string textFieldValue)
    {
      try
      {
        using (var client = svcMgr.CreateProxy<IRSAPIClient>(identity))
        {
          client.APIOptions.WorkspaceID = workspaceArtifactId;
          var rdoJob = new RDO(artifactId)
          {
            ArtifactTypeGuids = new List<Guid> { objectTypeGuid },
            Fields = new List<FieldValue>
            {
              new FieldValue(textFieldGuid) { Value = textFieldValue }
            }
          };

          try
          {
            await Task.Run(() => client.Repositories.RDO.UpdateSingle(rdoJob));
          }
          catch (Exception ex)
          {
            var variables = $@"Workspace ArtifactID: {workspaceArtifactId}, GUID: {objectTypeGuid}, ArtifactID: {artifactId}, TextFieldGuid: {textFieldGuid}, TextFieldValue: {textFieldValue}";
            throw new MarkupUtilityException($@"An error occurred while updating the text field of an RDO (UpdateSingle). Variables: {variables} ", ex);
          }
        }
      }
      catch (Exception ex)
      {
        throw new MarkupUtilityException("An error occurred while updating for the text field of an RDO.", ex);
      }
    }


    public async Task UpdateRdoJobJobTypeAsync(IServicesMgr svcMgr, int workspaceArtifactId, ExecutionIdentity identity, Guid objectTypeGuid, int artifactId, Guid fieldGuid, Guid choiceGuid)
    {
      try
      {
        using (var client = svcMgr.CreateProxy<IRSAPIClient>(identity))
        {
          client.APIOptions.WorkspaceID = workspaceArtifactId;
          var rdoJob = new RDO(artifactId)
          {
            ArtifactTypeGuids = new List<Guid> { objectTypeGuid },
            Fields = new List<FieldValue> { new FieldValue(fieldGuid, choiceGuid) }
          };

          try
          {
            await Task.Run(() => client.Repositories.RDO.UpdateSingle(rdoJob));
          }
          catch (Exception ex)
          {
            throw new MarkupUtilityException("An error occurred while updating the Job Type of an RDO (UpdateSingle).", ex);
          }
        }
      }
      catch (Exception ex)
      {
        throw new MarkupUtilityException("An error occurred while updating for the Job Type of an RDO.", ex);
      }
    }

    public async Task CreateMarkupUtilityTypeRdoRecordAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, string name, Guid categoryChoiceGuid)
    {
      try
      {
        using (var client = svcMgr.CreateProxy<IRSAPIClient>(identity))
        {
          client.APIOptions.WorkspaceID = workspaceArtifactId;

          var rdoToCreate = new RDO();
          rdoToCreate.ArtifactTypeGuids.Add(Constant.Guids.ObjectType.MarkupUtilityType);
          rdoToCreate.Fields.Add(new FieldValue(Constant.Guids.Field.MarkupUtilityType.Name, name));
          var choiceArtifactIdbyGuid = await GetChoiceArtifactIdbyGuidAsync(svcMgr, identity, workspaceArtifactId, categoryChoiceGuid);
          rdoToCreate.Fields.Add(new FieldValue(Constant.Guids.Field.MarkupUtilityType.Category, choiceArtifactIdbyGuid));

          try
          {
            await Task.Run(() => client.Repositories.RDO.CreateSingle(rdoToCreate));
          }
          catch (Exception ex)
          {
            throw new MarkupUtilityException($"An error occurred while creating {typeof(Constant.Names.Rdos)} RDO (CreateSingle).", ex);
          }
        }
      }
      catch (Exception ex)
      {
        throw new MarkupUtilityException($"An error occurred while creating {typeof(Constant.Names.Rdos)} RDO.", ex);
      }
    }

    public async Task<int> CreateMarkupUtilityFileRdoRecordAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, string name)
    {
      try
      {
        using (var client = svcMgr.CreateProxy<IRSAPIClient>(identity))
        {
          client.APIOptions.WorkspaceID = workspaceArtifactId;

          var rdoToCreate = new RDO();
          rdoToCreate.ArtifactTypeGuids.Add(Constant.Guids.ObjectType.MarkupUtilityFile);
          rdoToCreate.Fields.Add(new FieldValue(Constant.Guids.Field.MarkupUtilityFile.Name, name));

          try
          {
            var retVal = await Task.Run(() => client.Repositories.RDO.CreateSingle(rdoToCreate));
            return retVal;
          }
          catch (Exception ex)
          {
            throw new Exception($"An error occurred while creating {typeof(Constant.Names.Rdos)} RDO (CreateSingle).", ex);
          }
        }
      }
      catch (Exception ex)
      {
        throw new Exception($"An error occurred while creating {typeof(Constant.Names.Rdos)} RDO.", ex);
      }
    }

    public async Task<List<MarkupUtilityType>> QueryMarkupUtilityTypeRdoRecordAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, Guid rdoGuid)
    {
      var redactionTypes = new List<MarkupUtilityType>();

      try
      {
        using (var client = svcMgr.CreateProxy<IRSAPIClient>(identity))
        {
          client.APIOptions.WorkspaceID = workspaceArtifactId;

          var rdoToQuery = new Query<RDO>
          {
            ArtifactTypeGuid = rdoGuid,
            Fields = FieldValue.AllFields
          };

          QueryResultSet<RDO> rdoQueryResultSet;

          try
          {
            rdoQueryResultSet = await Task.Run(() => client.Repositories.RDO.Query(rdoToQuery));
          }
          catch (Exception ex)
          {
            throw new MarkupUtilityException($"An error occurred when querying {typeof(Constant.Names.Rdos)} RDO (Query).", ex);
          }

          if (!rdoQueryResultSet.Success)
          {
            throw new MarkupUtilityException($"An error occurred when querying {typeof(Constant.Names.Rdos)} RDO (Query). Error Message: " + rdoQueryResultSet.Message);
          }

          redactionTypes.AddRange(rdoQueryResultSet.Results.Select(x => new MarkupUtilityType(x.Artifact.Fields.Get(Constant.Guids.Field.MarkupUtilityType.Name).ValueAsFixedLengthText, x.Artifact.Fields.Get(Constant.Guids.Field.MarkupUtilityType.Category).ValueAsSingleChoice.Name)));
        }
      }
      catch (Exception ex)
      {
        throw new MarkupUtilityException($"An error occurred when querying {typeof(Constant.Names.Rdos)} RDO.", ex);
      }

      return redactionTypes;
    }

    public async Task<int> GetChoiceArtifactIdbyGuidAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, Guid choiceGuid)
    {
      int retVal;

      try
      {
        var returnChoice = new DTOs.Choice(choiceGuid);
        ResultSet<DTOs.Choice> choiceReadResults;
        returnChoice.Fields = FieldValue.NoFields;

        using (var client = svcMgr.CreateProxy<IRSAPIClient>(identity))
        {
          client.APIOptions.WorkspaceID = workspaceArtifactId;
          client.APIOptions.StrictMode = true;
          var choice = returnChoice;
          choiceReadResults = await Task.Run(() => client.Repositories.Choice.Read(choice));
        }

        if (!choiceReadResults.Success)
        {
          throw new MarkupUtilityException("Choice does not exist.");
        }

        returnChoice = choiceReadResults.Results.Single().Artifact;
        retVal = returnChoice.ArtifactID;
      }
      catch (Exception ex)
      {
        throw new MarkupUtilityException($"An error occured when querying for Choice Artifact ID. [ChoiceGuid= {choiceGuid}]", ex);
      }

      return retVal;
    }

    public async Task<StreamReader> GetFileFieldContentsAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, int fileFieldArtifactId, int fileObjectArtifactId, string tempFileLocation)
    {
      try
      {
        using (var rsapiClient = svcMgr.CreateProxy<IRSAPIClient>(identity))
        {
          rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

          var fileRequest = new FileRequest(rsapiClient.APIOptions)
          {
            Target =
            {
              WorkspaceId = workspaceArtifactId,
              FieldId = fileFieldArtifactId,
              ObjectArtifactId = fileObjectArtifactId
            }
          };

          try
          {
            await Task.Run(() => rsapiClient.Download(fileRequest, tempFileLocation));
          }
          catch (Exception ex)
          {
            throw new MarkupUtilityException("An error occurred calling GetFileFieldDownloadURL: {0}", ex);
          }

          try
          {
            var sr = new StreamReader(tempFileLocation);
            return sr;
          }
          catch (Exception ex)
          {
            throw new MarkupUtilityException("An error occurred when attempting to read the contents of the file.", ex);
          }
        }
      }
      catch (Exception ex)
      {
        throw new MarkupUtilityException("An error occurred when retrieving contents of the file field.", ex);
      }
    }

    public async Task<MarkupUtilityImportJob> RetrieveImportJobAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, int importJobArtifactId)
    {
      MarkupUtilityImportJob retVal;
      string errorContext = $"An error occured when querying for Import Job [{nameof(importJobArtifactId)} = {importJobArtifactId}].";

      try
      {
        using (var rsapiClient = svcMgr.CreateProxy<IRSAPIClient>(identity))
        {
          rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

          RDO importJobRdo;
          try
          {
            importJobRdo = await Task.Run(() => rsapiClient.Repositories.RDO.ReadSingle(importJobArtifactId));
          }
          catch (Exception ex)
          {
            throw new MarkupUtilityException($"{errorContext} ReadSingle.", ex);
          }

          var name = importJobRdo.TextIdentifier;
          var markupSetArtifactId = importJobRdo.Fields.Get(Constant.Guids.Field.MarkupUtilityImportJob.MarkupSet).ValueAsSingleObject.ArtifactID;
          var skipDuplicateRedactions = importJobRdo.Fields.Get(Constant.Guids.Field.MarkupUtilityImportJob.SkipDuplicateRedactions).ValueAsYesNo;
          var fileArtifact = (DTOs.Artifact)importJobRdo.Fields.Get(Constant.Guids.Field.MarkupUtilityImportJob.RedactionFile).Value;
          var markupUtilityFileArtifactId = fileArtifact.ArtifactID;
          var status = importJobRdo.Fields.Get(Constant.Guids.Field.MarkupUtilityImportJob.Status).ValueAsFixedLengthText;
          var details = importJobRdo.Fields.Get(Constant.Guids.Field.MarkupUtilityImportJob.Details).ValueAsLongText;
          var totalRedactionCount = importJobRdo.Fields.Get(Constant.Guids.Field.MarkupUtilityImportJob.ImportFileRedactionCount).ValueAsWholeNumber ?? 0;
          var importedRedactionCount = importJobRdo.Fields.Get(Constant.Guids.Field.MarkupUtilityImportJob.ImportedRedactionCount).ValueAsWholeNumber ?? 0;
          var jobType = importJobRdo.Fields.Get(Constant.Guids.Field.MarkupUtilityImportJob.JobType).ValueAsSingleChoice.Name;
          var createdBy = importJobRdo.Fields.Get(Constant.Guids.Field.MarkupUtilityImportJob.SystemCreatedBy).ValueAsSingleObject.ArtifactID;

          var selectedRedactionTypeArtifactIds = importJobRdo.Fields.Get(Constant.Guids.Field.MarkupUtilityImportJob.ImportRedactionType)
            .GetValueAsMultipleObject<DTOs.Artifact>()
            .Select(x => x.ArtifactID)
            .ToList();

          var markupUtilityTypes = selectedRedactionTypeArtifactIds
            .Select(x => RetrieveMarkupUtilityTypeAsync(svcMgr, identity, workspaceArtifactId, x).Result).ToList();

          retVal = new MarkupUtilityImportJob(importJobArtifactId, name, markupSetArtifactId, markupUtilityTypes, skipDuplicateRedactions, markupUtilityFileArtifactId, status, details, totalRedactionCount, importedRedactionCount, jobType, createdBy);
        }
      }
      catch (Exception ex)
      {
        throw new MarkupUtilityException(errorContext, ex);
      }

      return retVal;
    }

    public async Task<MarkupUtilityExportJob> RetrieveExportJobAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, int exportJobArtifactId)
    {
      MarkupUtilityExportJob retVal;
      string errorContext = $"An error occured when querying for Export Job [{nameof(exportJobArtifactId)} = {exportJobArtifactId}].";

      try
      {
        using (var rsapiClient = svcMgr.CreateProxy<IRSAPIClient>(identity))
        {
          rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

          RDO exportJobRdo;
          try
          {
            exportJobRdo = await Task.Run(() => rsapiClient.Repositories.RDO.ReadSingle(exportJobArtifactId));
          }
          catch (Exception ex)
          {
            throw new MarkupUtilityException($"{errorContext} ReadSingle.", ex);
          }

          var artifactId = exportJobRdo.ArtifactID;
          var name = exportJobRdo.TextIdentifier;
          var markupSetArtifactId = exportJobRdo.Fields.Get(Constant.Guids.Field.MarkupUtilityExportJob.MarkupSet).ValueAsSingleObject.ArtifactID;
          var fileArtifact = (DTOs.Artifact)exportJobRdo.Fields.Get(Constant.Guids.Field.MarkupUtilityExportJob.RedactionFile).Value;
          var savedSearchArtifactId = exportJobRdo.Fields.Get(Constant.Guids.Field.MarkupUtilityExportJob.SavedSearch).ValueAsSingleObject.ArtifactID;
          var markupUtilityFileArtifactId = fileArtifact.ArtifactID;
          var status = exportJobRdo.Fields.Get(Constant.Guids.Field.MarkupUtilityExportJob.Status).ValueAsFixedLengthText;
          var details = exportJobRdo.Fields.Get(Constant.Guids.Field.MarkupUtilityExportJob.Details).ValueAsLongText;
          var exportedRedactionCount = exportJobRdo.Fields.Get(Constant.Guids.Field.MarkupUtilityExportJob.ExportedRedactionCount).ValueAsWholeNumber ?? 0;

          var selectedRedactionTypeArtifactIds = exportJobRdo.Fields.Get(Constant.Guids.Field.MarkupUtilityExportJob.ExportRedactionType)
            .GetValueAsMultipleObject<DTOs.Artifact>()
            .Select(x => x.ArtifactID)
            .ToList();

          var markupUtilityTypes = selectedRedactionTypeArtifactIds
           .Select(x => RetrieveMarkupUtilityTypeAsync(svcMgr, identity, workspaceArtifactId, x).Result).ToList();

          retVal = new MarkupUtilityExportJob(artifactId, name, markupSetArtifactId, markupUtilityTypes, savedSearchArtifactId, markupUtilityFileArtifactId, status, details, exportedRedactionCount);
        }
      }
      catch (Exception ex)
      {
        throw new MarkupUtilityException(errorContext, ex);
      }

      return retVal;
    }


    public async Task<MarkupUtilityReproduceJob> RetrieveReproduceJobAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, int jobArtifactId)
    {
      using (var rsapiClient = svcMgr.CreateProxy<IRSAPIClient>(identity))
      {
        rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;
        var resultSet = await Task.Run(() => rsapiClient.Repositories.RDO.Read(new int[] { jobArtifactId }));

        if (resultSet.Results.Count != 1) return null;
        if (!resultSet.Results[0].Success) return null;

        var jobRdo = resultSet.Results[0].Artifact;
        var name = jobRdo.TextIdentifier;
        var sourceMarkupSetArtifactId = jobRdo.Fields.Get(Constant.Guids.Field.MarkupUtilityReproduceJob.SourceMarkupSet).ValueAsSingleObject.ArtifactID;
        var savedSearchArtifactId = jobRdo.Fields.Get(Constant.Guids.Field.MarkupUtilityReproduceJob.SavedSearch).ValueAsSingleObject.ArtifactID;
        var status = jobRdo.Fields.Get(Constant.Guids.Field.MarkupUtilityReproduceJob.Status).ValueAsFixedLengthText;
        var details = jobRdo.Fields.Get(Constant.Guids.Field.MarkupUtilityReproduceJob.Details).ValueAsLongText;
        var createdBy = jobRdo.Fields.Get(Constant.Guids.Field.MarkupUtilityReproduceJob.SystemCreatedBy).ValueAsSingleObject.ArtifactID;
        var destinationMarkupSetArtifactId = jobRdo.Fields.Get(Constant.Guids.Field.MarkupUtilityReproduceJob.DestinationMarkupSet).ValueAsSingleObject.ArtifactID;
        var jobType = jobRdo.Fields.Get(Constant.Guids.Field.MarkupUtilityReproduceJob.ReproduceJobType).ValueAsSingleObject.ArtifactID;
        var hasAutoRedactionsField = jobRdo.Fields.Get(Constant.Guids.Field.MarkupUtilityReproduceJob.HasAutoRedactionsField).ValueAsSingleObject.ArtifactID;
        var relationalField = jobRdo.Fields.Get(Constant.Guids.Field.MarkupUtilityReproduceJob.RelationalField).ValueAsSingleObject.ArtifactID;

        return new MarkupUtilityReproduceJob(jobArtifactId, name, savedSearchArtifactId, sourceMarkupSetArtifactId, destinationMarkupSetArtifactId, status, details, createdBy, jobType, hasAutoRedactionsField, relationalField);
      }
    }

    public async Task<List<int>> RetrieveDocumentArtifactIdsForSavedSearchAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, int savedSearchArtifactId)
    {
      var documentArtifactIds = new List<int>();
      string errorContext = $"An error occured when querying for Documents in the Saved Search [{nameof(savedSearchArtifactId)} = {savedSearchArtifactId}].";

      try
      {
        using (var rsapiClient = svcMgr.CreateProxy<IRSAPIClient>(identity))
        {
          rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

          try
          {
            var query = new Query<Document> { Condition = new SavedSearchCondition(savedSearchArtifactId) };
            var querySubset = await Task.Run(() => QuerySubset.PerformQuerySubset(rsapiClient.Repositories.Document, query, 1000));
            documentArtifactIds.AddRange(querySubset.Select(x => x.Artifact.ArtifactID).ToList());
          }
          catch (Exception ex)
          {
            throw new MarkupUtilityException($"{errorContext} ReadSingle.", ex);
          }
        }
      }
      catch (Exception ex)
      {
        throw new MarkupUtilityException(errorContext, ex);
      }

      return documentArtifactIds;
    }

    public async Task<MarkupUtilityType> RetrieveMarkupUtilityTypeAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, int markupUtilityTypeArtifactId)
    {
      MarkupUtilityType retVal;
      string errorContext = $"An error occured when querying for Markup Utility Type [{nameof(markupUtilityTypeArtifactId)} = {markupUtilityTypeArtifactId}].";

      try
      {
        using (var rsapiClient = svcMgr.CreateProxy<IRSAPIClient>(identity))
        {
          rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;
          RDO markupUtilityTypeRdo;

          try
          {
            markupUtilityTypeRdo = await Task.Run(() => rsapiClient.Repositories.RDO.ReadSingle(markupUtilityTypeArtifactId));
          }
          catch (Exception ex)
          {
            throw new MarkupUtilityException($"{errorContext} ReadSingle.", ex);
          }

          var name = markupUtilityTypeRdo.TextIdentifier;
          var category = markupUtilityTypeRdo.Fields.Get(Constant.Guids.Field.MarkupUtilityType.Category).ValueAsSingleChoice.Name;

          retVal = new MarkupUtilityType(name, category);
        }
      }
      catch (Exception ex)
      {
        throw new MarkupUtilityException(errorContext, ex);
      }

      return retVal;
    }

    public async Task<bool> DoesDocumentExistAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, string documentIdentifierFieldName, string documentIdentifierValue)
    {
      if (documentIdentifierFieldName == null)
      {
        throw new ArgumentNullException(nameof(documentIdentifierFieldName));
      }

      if (documentIdentifierValue == null)
      {
        throw new ArgumentNullException(nameof(documentIdentifierValue));
      }

      var retVal = true;
      string errorContext = $"An error occured when checking if the document exists. [{nameof(workspaceArtifactId)} = {workspaceArtifactId}, {nameof(documentIdentifierFieldName)} = {documentIdentifierFieldName}, {nameof(documentIdentifierValue)} = {documentIdentifierValue}].";

      try
      {
        using (var rsapiClient = svcMgr.CreateProxy<IRSAPIClient>(identity))
        {
          rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

          var documentQuery = new Query<Document>
          {
            Condition = new TextCondition(documentIdentifierFieldName, TextConditionEnum.EqualTo, documentIdentifierValue),
            Fields = FieldValue.AllFields
          };

          QueryResultSet<Document> documentQueryResultSet;
          try
          {
            documentQueryResultSet = await Task.Run(() => rsapiClient.Repositories.Document.Query(documentQuery));
          }
          catch (Exception ex)
          {
            throw new MarkupUtilityException($"{errorContext} Query.", ex);
          }

          if (documentQueryResultSet == null || !documentQueryResultSet.Success)
          {
            throw new MarkupUtilityException($"{errorContext}. Error Message = {documentQueryResultSet?.Message}");
          }

          if (documentQueryResultSet.Results.Count == 0)
          {
            retVal = false;
          }

          if (documentQueryResultSet.Results.Count == 1)
          {
            retVal = true;
          }

          if (documentQueryResultSet.Results.Count > 1)
          {
            throw new MarkupUtilityException("More than one document exists with the document identifier.");
          }
        }
      }
      catch (Exception ex)
      {
        throw new MarkupUtilityException(errorContext, ex);
      }

      return retVal;
    }

    public async Task<int> RetrieveDocumentArtifactIdAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, string documentIdentifierFieldName, string documentIdentifierValue)
    {
      int retVal;

      if (documentIdentifierFieldName == null)
      {
        throw new ArgumentNullException(nameof(documentIdentifierFieldName));
      }

      if (documentIdentifierValue == null)
      {
        throw new ArgumentNullException(nameof(documentIdentifierValue));
      }

      string errorContext = $"An error occured when querying for Document Artifact Id [{nameof(workspaceArtifactId)} = {workspaceArtifactId}, {nameof(documentIdentifierFieldName)} = {documentIdentifierFieldName}, {nameof(documentIdentifierValue)} = {documentIdentifierValue}].";

      try
      {
        using (var rsapiClient = svcMgr.CreateProxy<IRSAPIClient>(identity))
        {
          rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;
          var documentQuery = new Query<Document>
          {
            Condition = new TextCondition(documentIdentifierFieldName, TextConditionEnum.EqualTo, documentIdentifierValue),
            Fields = FieldValue.AllFields
          };

          QueryResultSet<Document> documentQueryResultSet;

          try
          {
            documentQueryResultSet = await Task.Run(() => rsapiClient.Repositories.Document.Query(documentQuery));
          }
          catch (Exception ex)
          {
            throw new MarkupUtilityException($"{errorContext} Query.", ex);
          }

          if (documentQueryResultSet == null || !documentQueryResultSet.Success)
          {
            throw new MarkupUtilityException($"{errorContext}. Error Message = {documentQueryResultSet?.Message}");
          }

          if (documentQueryResultSet.Results.Count == 0)
          {
            throw new MarkupUtilityException("No document with the document identifier exists.");
          }

          if (documentQueryResultSet.Results.Count > 1)
          {
            throw new MarkupUtilityException("More than one document exists with the document identifier.");
          }

          var document = documentQueryResultSet.Results.First().Artifact;
          retVal = document.ArtifactID;
        }
      }
      catch (Exception ex)
      {
        throw new MarkupUtilityException(errorContext, ex);
      }
      return retVal;
    }

    public async Task<bool> AddDocumentsToHoldingTableAsync(IServicesMgr svcMgr, IDBContext dbContext, Utility.IQuery utilityQueryHelper, ExecutionIdentity identity, int workspaceArtifactId, int savedSearchArtifactId, string holdingTableName, List<SqlBulkCopyColumnMapping> columnMappingsHoldingTable)
    {
      try
      {
        using (var rsapiClient = svcMgr.CreateProxy<IRSAPIClient>(identity))
        {
          rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;
          var fields = new List<FieldValue>();

          foreach (SqlBulkCopyColumnMapping sqlBulkCopyColumnMapping in columnMappingsHoldingTable)
          {
            if (!sqlBulkCopyColumnMapping.SourceColumn.Contains("DocumentArtifactID"))
            {
              fields.Add(new FieldValue(sqlBulkCopyColumnMapping.SourceColumn));
            }
          }

          var query = new Query<Document> { Condition = new SavedSearchCondition(savedSearchArtifactId), Fields = fields };
          QueryResultSet<Document> resultSet;

          try
          {
            resultSet = await Task.Run(() => rsapiClient.Repositories.Document.Query(query, Constant.Sizes.ExportJobHoldingTableBatchSize));

          }
          catch (Exception ex)
          {
            throw new MarkupUtilityException("An error occured when executing the Saved Search. Query.", ex);
          }

          if (resultSet.Success)
          {
            if (resultSet.Results.Count > 0)
            {
              var batchAvailable = !string.IsNullOrEmpty(resultSet.QueryToken);
              var nextStart = Constant.Sizes.ExportJobHoldingTableBatchSize + 1;
              await AddRecordsToHoldingTableAsync(dbContext, utilityQueryHelper, resultSet.Results, holdingTableName, columnMappingsHoldingTable);

              while (batchAvailable)
              {
                resultSet = rsapiClient.Repositories.Document.QuerySubset(resultSet.QueryToken, nextStart, Constant.Sizes.ExportJobHoldingTableBatchSize);
                await AddRecordsToHoldingTableAsync(dbContext, utilityQueryHelper, resultSet.Results, holdingTableName, columnMappingsHoldingTable);

                if (string.IsNullOrEmpty(resultSet.QueryToken))
                {
                  batchAvailable = false;
                }
                else
                {
                  nextStart += Constant.Sizes.ExportJobHoldingTableBatchSize;
                }
              }
            }
            else
            {
              //No Documents found in Saved Search
              return false;
            }
          }
          else
          {
            throw new MarkupUtilityException($"An error occured when executing the Saved Search. {resultSet.Message}");
          }
          return true;
        }
      }
      catch (Exception ex)
      {
        throw new MarkupUtilityException("An error occured when adding Documents to the holding table.", ex);
      }
    }

    private async Task AddRecordsToHoldingTableAsync(IDBContext dbContext, Utility.IQuery utilityQueryHelper, List<Result<Document>> results, string holdingTableName, List<SqlBulkCopyColumnMapping> columnMappings)
    {
      //Generate DataTable to be used by Bulk SQL import
      var dtDocuments = await GenerateExportJobDataTableAsync(results, columnMappings);

      //Bulk insert Documents into Export Worker queue
      await Task.Run(() => utilityQueryHelper.BulkInsertIntoTable(dbContext, dtDocuments, columnMappings, "[" + holdingTableName + "]"));
    }

    public async Task AddRedactionsToTableAsync(IDBContext dbContext, Utility.IQuery utilityQueryHelper, string tableName, DataTable dtRedactions, List<SqlBulkCopyColumnMapping> columnMappings)
    {
      //Bulk insert Documents into Export Holding table
      await Task.Run(() => utilityQueryHelper.BulkInsertIntoTable(dbContext, dtRedactions, columnMappings, "[" + tableName + "]"));
    }

    private async Task<DataTable> GenerateExportJobDataTableAsync(List<Result<Document>> results, List<SqlBulkCopyColumnMapping> columnMappings)
    {
      var dtDocuments = new DataTable();
      dtDocuments.Columns.Add(new DataColumn { ColumnName = "DocumentArtifactID", DataType = typeof(int) });

      foreach (var sqlBulkCopyColumnMapping in columnMappings)
      {
        if (sqlBulkCopyColumnMapping.SourceColumn != "DocumentArtifactID")
        {
          dtDocuments.Columns.Add(new DataColumn { ColumnName = sqlBulkCopyColumnMapping.SourceColumn });
        }
      }

      foreach (var document in results)
      {
        var dataRow = dtDocuments.NewRow();
        dataRow["DocumentArtifactID"] = document.Artifact.ArtifactID;
        var fieldValues = document.Artifact.Fields;
        foreach (var columnMapping in columnMappings)
        {
          var mapping = columnMapping;
          var fieldValue = fieldValues.Find(x => x.Name == mapping.SourceColumn);
          if (fieldValue != default(FieldValue))
          {
            dataRow[fieldValue.Name] = fieldValue.Value;
          }
        }
        dtDocuments.Rows.Add(dataRow);
      }

      return await Task.Run(() => dtDocuments);
    }

    public async Task CreateMarkupUtilityHistoryRecordAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, int importJobArtifactId, string documentIdentifier, int pageNumber, string jobType, string redactionType, string status, string details, string redactionData, int? redactionId, int reproduceJobId)
    {
      string errorContext = $"An error occured when creating {Constant.Names.ApplicationName} History Record. [DocumentIdentifier: {documentIdentifier}]";

      try
      {
        using (var rsapiClient = svcMgr.CreateProxy<IRSAPIClient>(identity))
        {
          rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;
          var historyRdo = new RDO();

          historyRdo.ArtifactTypeGuids.Add(Constant.Guids.ObjectType.MarkupUtilityHistory);
          historyRdo.Fields.Add(new FieldValue(Constant.Guids.Field.MarkupUtilityHistory.Name) { Value = Guid.NewGuid().ToString() });
          historyRdo.Fields.Add(new FieldValue(Constant.Guids.Field.MarkupUtilityHistory.DocumentIdentifier) { Value = documentIdentifier });
          historyRdo.Fields.Add(new FieldValue(Constant.Guids.Field.MarkupUtilityHistory.PageNumber) { Value = pageNumber });
          historyRdo.Fields.Add(new FieldValue(Constant.Guids.Field.MarkupUtilityHistory.JobType) { Value = jobType });
          historyRdo.Fields.Add(new FieldValue(Constant.Guids.Field.MarkupUtilityHistory.RedactionType) { Value = redactionType });
          historyRdo.Fields.Add(new FieldValue(Constant.Guids.Field.MarkupUtilityHistory.Status) { Value = status });
          historyRdo.Fields.Add(new FieldValue(Constant.Guids.Field.MarkupUtilityHistory.Details) { Value = details });
          historyRdo.Fields.Add(new FieldValue(Constant.Guids.Field.MarkupUtilityHistory.RedactionData) { Value = redactionData });
          historyRdo.Fields.Add(new FieldValue(Constant.Guids.Field.MarkupUtilityHistory.RedactionId) { Value = (redactionId == null ? string.Empty : redactionId.ToString()) });

          if (importJobArtifactId > -1)
          {
            var importJobRdo = new RDO(importJobArtifactId);
            historyRdo.Fields.Add(new FieldValue(Constant.Guids.Field.MarkupUtilityHistory.ImportJob) { Value = importJobRdo });
          }

          if (reproduceJobId > -1)
          {
            var job = new RDO(reproduceJobId);
            historyRdo.Fields.Add(new FieldValue(Constant.Guids.Field.MarkupUtilityHistory.ReproduceJob) { Value = job });
          }

          try
          {
            await Task.Run(() => rsapiClient.Repositories.RDO.CreateSingle(historyRdo));
          }
          catch (Exception ex)
          {
            throw new MarkupUtilityException($"{errorContext} CreateSingle.", ex);
          }
        }
      }
      catch (Exception ex)
      {
        throw new MarkupUtilityException(errorContext, ex);
      }
    }

    public async Task AttachFileToMarkupUtilityFileRecord(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, int redactionFileArtifactId, string fileName, int fieldArtifactId)
    {
      string errorContext = $"An error occured when attaching a file to the Markup Utility File record {Constant.Names.ApplicationName}. [MarkupUtilityFileArtifactID: {redactionFileArtifactId}]";

      try
      {
        using (var rsapiClient = svcMgr.CreateProxy<IRSAPIClient>(identity))
        {
          rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

          //Create and upload a file to the
          var fileUpReq = new UploadRequest(rsapiClient.APIOptions)
          {
            Target =
            {
              ObjectArtifactId = redactionFileArtifactId,
              FieldId = fieldArtifactId
            },
            Metadata = { FileName = fileName }
          };

          //Set the file name
          var info = new FileInfo(fileName);
          fileUpReq.Metadata.FileSize = info.Length;

          //Overwrite any existing file in the destination
          fileUpReq.Overwrite = true;

          try
          {
            await Task.Run(() => { rsapiClient?.Upload(fileUpReq); });
          }
          catch (Exception ex)
          {
            throw new MarkupUtilityException($"{errorContext} Upload File.", ex);
          }
        }
      }
      catch (Exception ex)
      {
        throw new MarkupUtilityException(errorContext, ex);
      }
    }

    public async Task AttachRedactionFileToExportJob(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, int exportJobArtifactId, int redactionFileArtifactId)
    {
      string errorContext = $"An error occured when attaching a Markup Utility File record to the Export Job RDO {Constant.Names.ApplicationName}. [ExportJobArtifactID: {exportJobArtifactId}], [MarkupUtilityFileArtifactID: {redactionFileArtifactId}]";

      try
      {
        using (var rsapiClient = svcMgr.CreateProxy<IRSAPIClient>(identity))
        {
          rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

          var exportJobRdoToUpdate = new RDO(exportJobArtifactId);
          exportJobRdoToUpdate.ArtifactTypeGuids.Add(Constant.Guids.ObjectType.MarkupUtilityExportJob);

          var redactionFileRdo = new RDO(redactionFileArtifactId);
          exportJobRdoToUpdate.Fields.Add(new FieldValue(Constant.Guids.Field.MarkupUtilityExportJob.RedactionFile) { Value = redactionFileRdo });

          try
          {
            await Task.Run(() => { rsapiClient.Repositories.RDO.UpdateSingle(exportJobRdoToUpdate); });
          }
          catch (Exception ex)
          {
            throw new MarkupUtilityException($"{errorContext} Upload File.", ex);
          }
        }
      }
      catch (Exception ex)
      {
        throw new MarkupUtilityException(errorContext, ex);
      }
    }

    public async Task<int> RetrieveImportJobRedactionCountFieldValueAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, int importJobArtifactId, Guid countFieldGuid)
    {
      string errorContext;
      int retVal;

      if (countFieldGuid == Constant.Guids.Field.MarkupUtilityImportJob.ImportedRedactionCount)
      {
        errorContext = "An error occured when retrieving imported redaction count field value on the import job.";
      }
      else if (countFieldGuid == Constant.Guids.Field.MarkupUtilityImportJob.SkippedRedactionCount)
      {
        errorContext = "An error occured when updating skipped redaction count field value on the import job.";
      }
      else if (countFieldGuid == Constant.Guids.Field.MarkupUtilityImportJob.ErrorRedactionCount)
      {
        errorContext = "An error occured when updating error redaction count field value on the import job.";
      }
      else
      {
        throw new MarkupUtilityException(Constant.ErrorMessages.NOT_A_VALID_REDACTION_COUNT_FIELD_ON_THE_IMPORT_JOB_RDO);
      }

      errorContext += $" [ImportJobArtifactId: {importJobArtifactId}]";

      try
      {
        using (var rsapiClient = svcMgr.CreateProxy<IRSAPIClient>(identity))
        {
          rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;
          RDO importJobRdo;

          try
          {
            importJobRdo = await Task.Run(() => rsapiClient.Repositories.RDO.ReadSingle(importJobArtifactId));
          }
          catch (Exception ex)
          {
            throw new MarkupUtilityException($"{errorContext} ReadSingle.", ex);
          }

          var importedRedactionCount = importJobRdo.Fields.Get(countFieldGuid).ValueAsWholeNumber;
          retVal = importedRedactionCount ?? 0;
        }
      }
      catch (Exception ex)
      {
        throw new MarkupUtilityException(errorContext, ex);
      }

      return retVal;
    }

    public async Task UpdateImportJobRedactionCountFieldValueAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, int importJobArtifactId, Guid countFieldGuid, int countValue)
    {
      string errorContext;

      if (countFieldGuid == Constant.Guids.Field.MarkupUtilityImportJob.ImportFileRedactionCount)
      {
        errorContext = "An error occured when updating import file redaction count field value on the import job.";
      }
      else if (countFieldGuid == Constant.Guids.Field.MarkupUtilityImportJob.ExpectedRedactionCount)
      {
        errorContext = "An error occured when updating expected redaction count field value on the import job.";
      }
      else if (countFieldGuid == Constant.Guids.Field.MarkupUtilityImportJob.ImportedRedactionCount)
      {
        errorContext = "An error occured when updating imported redaction count field value on the import job.";
      }
      else if (countFieldGuid == Constant.Guids.Field.MarkupUtilityImportJob.SkippedRedactionCount)
      {
        errorContext = "An error occured when updating skipped redaction count field value on the import job.";
      }
      else if (countFieldGuid == Constant.Guids.Field.MarkupUtilityImportJob.ErrorRedactionCount)
      {
        errorContext = "An error occured when updating error redaction count field value on the import job.";
      }
      else
      {
        throw new MarkupUtilityException("Not a valid redaction count field on the import job rdo.");
      }

      errorContext += $" [ImportJobArtifactId: {importJobArtifactId}]";

      try
      {
        using (var rsapiClient = svcMgr.CreateProxy<IRSAPIClient>(identity))
        {
          rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

          var importJobRdo = new RDO(importJobArtifactId)
          {
            ArtifactTypeGuids = new List<Guid> { Constant.Guids.ObjectType.MarkupUtilityImportJob },
            Fields = new List<FieldValue>
            {
              new FieldValue(countFieldGuid) { Value = countValue}
            }
          };

          try
          {
            await Task.Run(() => rsapiClient.Repositories.RDO.UpdateSingle(importJobRdo));
          }
          catch (Exception ex)
          {
            throw new MarkupUtilityException($"{errorContext} UpdateSingle.", ex);
          }
        }
      }
      catch (Exception ex)
      {
        throw new MarkupUtilityException(errorContext, ex);
      }
    }

    public async Task<bool> VerifyIfMarkupSetExistsAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, int markupSetArtifactId)
    {
      string errorContext = $"Markup Set does not exists. [MarkupSetArtifactId: {markupSetArtifactId}]";

      try
      {
        using (var rsapiClient = svcMgr.CreateProxy<IRSAPIClient>(identity))
        {
          rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

          try
          {
            var markupSetRdo = await Task.Run(() => rsapiClient.Repositories.RDO.ReadSingle(markupSetArtifactId));
          }
          catch (Exception ex)
          {
            throw new MarkupUtilityException($"{errorContext}", ex);
          }
        }
      }
      catch (Exception ex)
      {
        throw new MarkupUtilityException(errorContext, ex);
      }

      return true;
    }

    public async Task<bool> VerifyIfDocumentExistsAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, int documentArtifactId)
    {
      string errorContext = $"Document does not exists. [WorkspaceArtifactId: {workspaceArtifactId}], [DocumentArtifactId: {documentArtifactId}]";

      try
      {
        using (var rsapiClient = svcMgr.CreateProxy<IRSAPIClient>(identity))
        {
          rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

          try
          {
            var doc = await Task.Run(() => rsapiClient.Repositories.Document.ReadSingle(documentArtifactId));
          }
          catch (Exception ex)
          {
            throw new MarkupUtilityException($"{errorContext}. ReadSingle", ex);
          }
        }
      }
      catch (Exception ex)
      {
        throw new MarkupUtilityException(errorContext, ex);
      }

      return true;
    }

    public async Task<string> RetreiveMarkupSetNameAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, int markupSetArtifactId)
    {
      string errorContext = $"An error occured when retrieving Markup set name. [MarkupSetArtifactId = {markupSetArtifactId}]";
      string retVal;

      try
      {
        using (var rsapiClient = svcMgr.CreateProxy<IRSAPIClient>(identity))
        {
          rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

          RDO markupSetRdo;
          try
          {
            markupSetRdo = await Task.Run(() => rsapiClient.Repositories.RDO.ReadSingle(markupSetArtifactId));
          }
          catch (Exception ex)
          {
            throw new MarkupUtilityException($"{errorContext}. ReadSingle", ex);
          }

          retVal = markupSetRdo.TextIdentifier;
        }
      }
      catch (Exception ex)
      {
        throw new MarkupUtilityException($"{errorContext}", ex);
      }

      return retVal;
    }

    public async Task<int> RetreiveMarkupSetMultipleChoiceFieldTypeIdAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, string markupSetMultipleChoiceFieldName)
    {
      string errorContext = $"An error occured when retrieving Markup set multiple choice field artifact id. [MarkupSetMultipleChoiceFieldName = {markupSetMultipleChoiceFieldName}]";
      int retVal;

      if (markupSetMultipleChoiceFieldName == null)
        if (markupSetMultipleChoiceFieldName == null)
        {
          throw new ArgumentNullException(nameof(markupSetMultipleChoiceFieldName));
        }

      try
      {
        using (var rsapiClient = svcMgr.CreateProxy<IRSAPIClient>(identity))
        {
          rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

          var fieldQuery = new Query<DTOs.Field>
          {
            Fields = FieldValue.AllFields,
            Condition = new TextCondition(MarkupSetFieldNames.Name, TextConditionEnum.EqualTo, markupSetMultipleChoiceFieldName)
          };

          QueryResultSet<DTOs.Field> fieldQueryResultSet;
          try
          {
            fieldQueryResultSet = await Task.Run(() => rsapiClient.Repositories.Field.Query(fieldQuery));
          }
          catch (Exception ex)
          {
            throw new MarkupUtilityException($"{errorContext}. Query", ex);
          }

          if (fieldQueryResultSet.Success == false)
          {
            throw new MarkupUtilityException($"{errorContext}. Error message = {fieldQueryResultSet.Message}");
          }
          if (fieldQueryResultSet.Results.Count == 0)
          {
            throw new MarkupUtilityException($"Markup set multiple choice field does not exist. {errorContext}");
          }
          if (fieldQueryResultSet.Results.Count > 1)
          {
            throw new MarkupUtilityException($"More than one Markup set multiple choice field exists. {errorContext}");
          }

          var choiceTypeId = fieldQueryResultSet.Results.First().Artifact.ChoiceTypeID;
          if (choiceTypeId == null)
          {
            throw new MarkupUtilityException($"ChoiceTypeID is NULL. {errorContext}");
          }

          retVal = choiceTypeId.Value;
        }
      }
      catch (Exception ex)
      {
        throw new MarkupUtilityException($"{errorContext}", ex);
      }

      return retVal;
    }

    public async Task<List<ChoiceModel>> QueryAllMarkupSetMultipleChoiceFieldValuesAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, string markupSetMultipleChoiceFieldName)
    {
      string errorContext = $"An error occured when querying Markup set multiple choice field values. [MarkupSetMultipleChoiceFieldName = {markupSetMultipleChoiceFieldName}]";
      List<ChoiceModel> retVal;

      if (markupSetMultipleChoiceFieldName == null)
      {
        throw new ArgumentNullException(nameof(markupSetMultipleChoiceFieldName));
      }

      try
      {
        using (var rsapiClient = svcMgr.CreateProxy<IRSAPIClient>(identity))
        {
          rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

          //query for choice type id
          var fieldQuery = new Query<DTOs.Field>
          {
            Fields = FieldValue.AllFields,
            Condition = new TextCondition(FieldFieldNames.Name, TextConditionEnum.EqualTo, markupSetMultipleChoiceFieldName)
          };

          QueryResultSet<DTOs.Field> fieldQueryResultSet;

          try
          {
            fieldQueryResultSet = await Task.Run(() => rsapiClient.Repositories.Field.Query(fieldQuery));
          }
          catch (Exception ex)
          {
            throw new MarkupUtilityException($"{errorContext}. Query", ex);
          }

          if (fieldQueryResultSet.Success == false)
          {
            throw new MarkupUtilityException($"{errorContext}. Error message = {fieldQueryResultSet.Message}");
          }

          if (fieldQueryResultSet.Results.Count == 0)
          {
            throw new MarkupUtilityException($"Markup set multiple choice field does not exist. {errorContext}");
          }

          if (fieldQueryResultSet.Results.Count > 1)
          {
            throw new MarkupUtilityException($"More than one Markup set multiple choice field exists. {errorContext}");
          }

          var typeId = fieldQueryResultSet.Results.First().Artifact.ChoiceTypeID;
          if (typeId == null)
          {
            throw new MarkupUtilityException($"ChoiceTypeID is NULL. {errorContext}");
          }

          var choiceTypeId = typeId.Value;


          //query for choices
          try
          {
            var choiceQuery = new Query<DTOs.Choice>
            {
              Condition = new WholeNumberCondition(ChoiceFieldNames.ChoiceTypeID, NumericConditionEnum.EqualTo, choiceTypeId),
              Fields = FieldValue.AllFields
            };

            QueryResultSet<DTOs.Choice> choiceQueryResultSet;

            try
            {
              choiceQueryResultSet = await Task.Run(() => rsapiClient.Repositories.Choice.Query(choiceQuery, 0));
            }
            catch (Exception ex)
            {
              throw new MarkupUtilityException($"An error occurred when querying for choices. Query. {errorContext}", ex);
            }

            if (!choiceQueryResultSet.Success)
            {
              throw new MarkupUtilityException($"An error occurred when querying for choices. Error message = {choiceQueryResultSet.Message}. {errorContext}");
            }

            if (choiceQueryResultSet.Results.Count == 0)
            {
              throw new MarkupUtilityException($"Number of choices is zero. {errorContext}");
            }

            retVal = choiceQueryResultSet.Results.Select(x => new ChoiceModel(x.Artifact.ArtifactID, x.Artifact.Name)).ToList();
          }
          catch (Exception ex)
          {
            throw new MarkupUtilityException($"An error occurred when querying for choices. {errorContext}", ex);
          }
        }
      }
      catch (Exception ex)
      {
        throw new MarkupUtilityException($"{errorContext}", ex);
      }

      return retVal;
    }
  }
}
