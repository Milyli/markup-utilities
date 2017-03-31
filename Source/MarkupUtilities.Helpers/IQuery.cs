using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MarkupUtilities.Helpers.Models;
using Relativity.API;

namespace MarkupUtilities.Helpers
{
  public interface IQuery
  {
    Task CreateReproduceManagerQueueTableAsync(IDBContext eddsDbContext);

    Task CreateExportManagerQueueTableAsync(IDBContext eddsDbContext);

    Task CreateExportWorkerQueueTableAsync(IDBContext eddsDbContext);

    Task CreateReproduceWorkerQueueTableAsync(IDBContext eddsDbContext);

    Task CreateExportErrorLogTableAsync(IDBContext eddsDbContext);

    Task CreateReproduceErrorLogTableAsync(IDBContext eddsDbContext);

    Task CreateImportManagerQueueTableAsync(IDBContext eddsDbContext);

    Task CreateImportWorkerQueueTableAsync(IDBContext eddsDbContext);

    Task CreateImportManagerHoldingTableAsync(IDBContext eddsDbContext, string tableName);

    Task CreateExportWorkerHoldingTableAsync(IDBContext eddsDbContext, string tableName);

    Task CreateImportErrorLogTableAsync(IDBContext eddsDbContext);

    Task CreateExportResultsTableAsync(IDBContext eddsDbContext);

    Task InsertRowIntoJobErrorLogAsync(IDBContext eddsDbContext, int workspaceArtifactId, string queueTableName, int queueRecordId, int agentId, string errorMessage, string jobErrorTable);

    Task InsertRowIntoImportErrorLogAsync(IDBContext eddsDbContext, int workspaceArtifactId, string queueTableName, int queueRecordId, int agentId, string errorMessage);

    Task<DataTable> RetrieveNextInJobManagerQueueAsync(IDBContext eddsDbContext, int agentId, string commaDelimitedResourceAgentIds, string jobManagerQueueTable, string jobArtifactId);

    Task<DataTable> RetrieveNextInReproduceWorkerQueueAsync(IDBContext eddsDbContext, int agentId, string commaDelimitedResourceAgentIds);

    Task<DataTable> RetrieveNextInImportManagerQueueAsync(IDBContext eddsDbContext, int agentId, string commaDelimitedResourceAgentIds);

    Task ResetUnfishedJobsAsync(IDBContext eddsDbContext, int agentId, string queueTableName);

    Task RemoveRecordFromTableByIdAsync(IDBContext eddsDbContext, string queueTableName, int id);

    Task<DataTable> RetrieveRedactionInfoAsync(IDBContext dbContext, int redactionId);

    Task InsertRowsIntoExportWorkerQueueAsync(IDBContext eddsDbContext, int jobId, int workspaceArtifactId, int parentRecordArtifactId, int resourceGroupId);

    Task InsertRowIntoReproduceWorkerQueueAsync(IDBContext eddsDbContext, int workspaceArtifactId, int documentIdStart, int documentIdEnd, string savedSearchHoldingTable, string redactionsHoldingTable, int destinationMarkupSetArtifactId, int reproduceJobArtifactId, int resourceGroupId, int codeTypeId, int markupSetRedactionCodeArtifactId, int markupSetAnnotationCodeArtifactId, string relationalGroupColumn, string hasAutoRedactionsColumn, string relationalGroup);

    Task<DataTable> RelationalGroupHasImagesAsync(IDBContext eddsDbContext, string relationalGroupColumn, string relationalGroup);

    Task<int> InsertRowsIntoRedactionsHoldingTableAsync(IDBContext eddsDbContext, string redactionsTable, string savedSearchTable, int sourceMarkupSetArtifactId, int destinationMarkupSetArtifactId, int start, int end, bool relationalGroup);

    Task InsertRowIntoImportManagerHoldingTableAsync(IDBContext eddsDbContext, int workspaceArtifactId, string documentIdentifier, int fileOrder, int resourceGroupId, int importJobArtifactId, int markupSetArtifactId, string jobType, ImportFileRecord importFileRecord, bool skipDuplicateRedactions, string tableName);

    Task BulkCopyDataFromImportManagerHoldingTableIntoImportWorkerQueueTableAsync(IDBContext eddsDbContext, string importManagerHoldingTableName);

    Task<int> InsertRowIntoRedactionTableAsync(IDBContext workspaceDbContext, string fileGuid, int markupSetArtifactId, ImportWorkerQueueRecord importWorkerQueueRecord);

    Task<bool> DoesRedactionExistAsync(IDBContext workspaceDbContext, string fileGuid, int markupSetArtifactId, ImportWorkerQueueRecord importWorkerQueueRecord);

    Task UpdateStatusInJobManagerQueueAsync(IDBContext eddsDbContext, int statusId, int id, string jobManagerQueue);

    Task UpdateStatusInImportManagerQueueAsync(IDBContext eddsDbContext, int statusId, int id);

    Task UpdateStatusInExportWorkerQueueAsync(IDBContext eddsDbContext, int statusId, string uniqueTableName);

    Task UpdateStatusInImportWorkerQueueAsync(IDBContext eddsDbContext, int statusId, string uniqueTableName);

    Task<DataTable> RetrieveNextBatchInExportWorkerQueueAsync(IDBContext eddsDbContext, int agentId, int batchSize, string uniqueTableName, string commaDelimitedResourceAgentIds);

    Task<DataTable> RetrieveMinMaxIdAsync(IDBContext dbContext, string tableName);

    Task<DataTable> RetrieveRelationalGroupsTask(IDBContext dbContext, string tableName);

    Task<DataTable> RetrieveNextBatchInImportWorkerQueueAsync(IDBContext eddsDbContext, int agentId, int batchSize, string uniqueTableName, string commaDelimitedResourceAgentIds);

    Task RemoveBatchFromExportWorkerQueueAsync(IDBContext eddsDbContext, string uniqueTableName);

    Task RemoveRecordFromReproduceWorkerQueueAsync(IDBContext eddsDbContext, int id);

    Task RemoveBatchFromImportWorkerQueueAsync(IDBContext eddsDbContext, string uniqueTableName);

    Task CreateHoldingTableAsync(IDBContext eddsDbContext, string tableName);

    Task CreateSavedSearchHoldingTableAsync(IDBContext eddsDbContext, string tableName, bool includeRelationalGroup);

    Task CreateRedactionsHoldingTableAsync(IDBContext eddsDbContext, string tableName, bool includeRelationalGroup);

    Task DropTableAsync(IDBContext dbContext, string tableName);

    Task UpdateStatusInReproduceWorkerQueueAsync(IDBContext eddsDbContext, int statusId, string id);

    Task<DataTable> RetrieveAllInExportManagerQueueAsync(IDBContext dbContext);

    Task<DataTable> RetrieveAllInImportManagerQueueAsync(IDBContext dbContext);

    Task<DataRow> RetrieveSingleInJobManagerQueueByArtifactIdAsync(IDBContext dbContext, int artifactId, int workspaceArtifactId, string jobManagerQueueTable);

    Task<DataRow> RetrieveSingleInImportManagerQueueByArtifactIdAsync(IDBContext dbContext, int artifactId, int workspaceArtifactId);

    Task<DataTable> RetrieveAllInExportWorkerQueueAsync(IDBContext dbContext);

    Task<DataTable> RetrieveAllInImportWorkerQueueAsync(IDBContext dbContext);

    Task InsertRowIntoExportManagerQueueAsync(IDBContext eddsDbContext, int workspaceArtifactId, int priority, int userId, int artifactId, int resourceGroupId);

    Task InsertRowIntoImportManagerQueueAsync(IDBContext eddsDbContext, int workspaceArtifactId, int priority, int userId, int artifactId, int resourceGroupId);

    Task<DataTable> RetrieveOffHoursAsync(IDBContext eddsDbContext);

    Task InsertImportJobToImportManagerQueueAsync(IDBContext eddsDbContext, int workspaceArtifactId, int importJobArtifactId, int userId, int statusQueue, string importJobType, int resourceGroupId);

    Task InsertJobToManagerQueueAsync(IDBContext eddsDbContext, int workspaceArtifactId, int jobArtifactId, int userId, int statusQueue, int resourceGroupId, string jobManagerQueueTable, string jobArtifactIdColumnName);

    Task<DataTable> RetrieveMarkupTypesAsync(IDBContext workspaceDbContext);

    Task<DataTable> RetrieveMarkupSubTypesAsync(IDBContext workspaceDbContext);

    Task CopyRecordsToExportWorkerQueueAsync(IDBContext eddsDbContext, string batchTableName, int workspaceArtifactId, int markupSetArtifactId, int exportJobArtifactId, string markupSubTypes, int resourceGroupArtifactId);

    Task<string> GetDocumentIdentifierFieldNameAsync(IDBContext workspaceDbContext);

    Task<string> GetFileGuidForDocumentAsync(IDBContext workspaceDbContext, int documentArtifactId, int fileOrder);

    Task<bool> DoesDocumentHasImagesAsync(IDBContext workspaceDbContext, int documentArtifactId, int fileOrder);

    Task CreateAuditRecordAsync(IDBContext workspaceDbContext, RedactionAuditRecord redactionAuditRecord);

    Task<bool> VerifyIfImportWorkerQueueContainsRecordsForJobAsync(IDBContext eddsDbContext, int workspaceArtifactId, int importJobArtifactId);

    Task<string> GetDocumentIdentifierFieldColumnNameAsync(IDBContext workspaceDbContext);

    Task<DataTable> RetrieveRedactionsForDocumentAsync(IDBContext dbContext, int workspaceArtifactId, int exportJobArtifactId, int documentArtifactId, int markupSetArtifactId, string documentIdentifierColumnName, string markupSubTypes);

    Task CopyRecordsToExportResultsAsync(IDBContext eddsDbContext, string holdingTableName);

    Task<DataTable> BulkUpdateHasAutoRedactionsForDocumentRange(IDBContext eddsDbContext, string savedSearchTableName, string redactionSetTableName, int startId, int endId, string hasAutoRedactionsColumn);

    Task<DataTable> BulkInsertRedactionRecordsForDocumentRange(IDBContext eddsDbContext, string savedSearchTableName, string redactionSetTableName, int startId, int endId);

    Task<DataTable> BulkInsertRedactionRecordsForRelationalGroup(IDBContext eddsDbContext, string savedSearchTableName, string redactionSetTableName, string relationalGroup);

    Task<DataTable> BulkUpdateHasAutoRedactionsFieldForRelationalGroup(IDBContext eddsDbContext, string savedSearchTableName, string redactionSetTableName, string relationalGroup, string hasAutoReddactionsColumn);

    Task<int> GetExportResultsRecordCountAsync(IDBContext eddsDbContext, int workspaceArtifactId, int exportJobArtifactId, int agentId);

    Task<int> GetJobWorkerRecordCountAsync(IDBContext eddsDbContext, int workspaceArtifactId, int jobArtifactId, string tableName, string jobColumn);

    Task<DataTable> GetExportResultsAsync(IDBContext eddsDbContext, int workspaceArtifactId, int exportJobArtifactId, int agentId);

    Task DeleteExportResultsAsync(IDBContext eddsDbContext, List<int> recordIdList);

    Task<int> GetWorkspaceArtifactIdByGuidAsync(IDBContext workspaceDbContext, string fieldGuid);

    Task UpdateMarkupSetMultipleChoiceFieldAsync(IDBContext workspaceDbContext, int documentArtifactId, int choiceTypeId, int choiceArtifactId);

    Task<DataTable> RetrieveZCodesAsync(IDBContext getDbContext, int destinationMarkupSetArtifactId);

    Task<int> UpdateHasRedactionsOrHighlightsAsync(IDBContext eddsDbContext, int markupSetCodeTypeId, int redactionOrAnnotationCodeArtifactId, int markupType, string savedSearchHoldingTable, string redactionsHoldingTable, int markupSetArtifactId, int start, int end);

    Task<int> UpdateHasRedactionsOrHighlightsAsync(IDBContext eddsDbContext, int markupSetCodeTypeId, int redactionOrAnnotationCodeArtifactId, int markupType, string savedSearchHoldingTable, string redactionsHoldingTable, int markupSetArtifactId, string relationalGroup);

    Task<DataTable> RetrieveArtifactIDsAsync(IDBContext eddsDbContext, string tempTable);

    Task<DataTable> RetrieveReproduceWorkerQueueAsync(IDBContext eddsDbContext, int reproduceJobArtifactId);

    Task<DataTable> RetrieveDocumentColumnAsync(IDBContext workspaceDbContext, int fieldArtifactId);
  }
}
