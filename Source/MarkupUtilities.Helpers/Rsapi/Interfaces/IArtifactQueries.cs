using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Relativity.API;
using System.Data.SqlClient;
using MarkupUtilities.Helpers.Models;

namespace MarkupUtilities.Helpers.Rsapi.Interfaces
{
  public interface IArtifactQueries
  {
    bool DoesUserHaveAccessToArtifact(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, Guid guid, string artifactTypeName);
    Response<bool> DoesUserHaveAccessToRdoByType(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, Guid guid, string artifactTypeName);
    Task<string> RetrieveRdoJobStatusAsync(IServicesMgr svcMgr, int workspaceArtifactId, ExecutionIdentity identity, int artifactId);
    Task UpdateRdoJobTextFieldAsync(IServicesMgr svcMgr, int workspaceArtifactId, ExecutionIdentity identity, Guid objectTypeGuid, int artifactId, Guid textFieldGuid, string textFieldValue);
    Task CreateMarkupUtilityTypeRdoRecordAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, string name, Guid categoryChoiceGuid);
    Task<List<MarkupUtilityType>> QueryMarkupUtilityTypeRdoRecordAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, Guid rdoGuid);
    Task<int> GetChoiceArtifactIdbyGuidAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, Guid choiceGuid);
    Task<StreamReader> GetFileFieldContentsAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, int fileFieldArtifactId, int fileObjectArtifactId, string tempFileLocation);
    Task<MarkupUtilityImportJob> RetrieveImportJobAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, int importJobArtifactId);
    Task<MarkupUtilityExportJob> RetrieveExportJobAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, int exportJobArtifactId);
    Task<MarkupUtilityReproduceJob> RetrieveReproduceJobAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, int jobArtifactId);
    Task UpdateRdoJobJobTypeAsync(IServicesMgr svcMgr, int workspaceArtifactId, ExecutionIdentity identity, Guid objectTypeGuid, int artifactId, Guid fieldGuid, Guid choiceGuid);
    Task<MarkupUtilityType> RetrieveMarkupUtilityTypeAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, int markupUtilityTypeArtifactId);
    Task<List<int>> RetrieveDocumentArtifactIdsForSavedSearchAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, int savedSearchArtifactId);
    Task<bool> DoesDocumentExistAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, string documentIdentifierFieldName, string documentIdentifierValue);
    Task<bool> AddDocumentsToHoldingTableAsync(IServicesMgr svcMgr, IDBContext dbContext, Utility.IQuery utilityQueryHelper, ExecutionIdentity identity, int workspaceArtifactId, int savedSearchArtifactId, string holdingTableName, List<SqlBulkCopyColumnMapping> columnMappingsHoldingTable);
    Task<int> RetrieveDocumentArtifactIdAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, string documentIdentifierFieldName, string documentIdentifierValue);
    Task CreateMarkupUtilityHistoryRecordAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, int importJobArtifactId, string documentIdentifier, int pageNumber, string jobType, string redactionType, string status, string details, string redactionData, int? redactionId, int reproduceJobId);
    Task<int> RetrieveImportJobRedactionCountFieldValueAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, int importJobArtifactId, Guid countFieldGuid);
    Task UpdateImportJobRedactionCountFieldValueAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, int importJobArtifactId, Guid countFieldGuid, int countValue);
    Task<bool> VerifyIfMarkupSetExistsAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, int markupSetArtifactId);
    Task<bool> VerifyIfDocumentExistsAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, int documentArtifactId);
    Task AddRedactionsToTableAsync(IDBContext dbContext, Utility.IQuery utilityQueryHelper, string tableName, DataTable dtRedactions, List<SqlBulkCopyColumnMapping> columnMappings);
    Task<int> CreateMarkupUtilityFileRdoRecordAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, string name);
    Task AttachFileToMarkupUtilityFileRecord(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, int redactionFileArtifactId, string fileName, int fieldArtifactId);
    Task AttachRedactionFileToExportJob(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, int exportJobArtifactId, int redactionFileArtifactId);
    Task<string> RetreiveMarkupSetNameAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, int markupSetArtifactId);
    Task<int> RetreiveMarkupSetMultipleChoiceFieldTypeIdAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, string markupSetMultipleChoiceFieldName);
    Task<List<ChoiceModel>> QueryAllMarkupSetMultipleChoiceFieldValuesAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, string markupSetMultipleChoiceFieldName);
  }
}