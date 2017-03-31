using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Relativity.API;

namespace MarkupUtilities.Helpers.Utility
{
  public interface IQuery
  {
    DataTable RetrieveApplicationWorkspaces(IDBContext eddsDbContext, Guid applicationGuid);

    string RetrieveConfigurationValue(IDBContext eddsDbContext, string sectionName, string name);

    DataTable RetrieveWorkspaces(IDBContext eddsDbContext);

    string GetValueFromConfigTable(IDBContext eddsDbContext, string inputName);

    void BulkInsertIntoTable(IDBContext dbContext, DataTable sourceDataTable, List<SqlBulkCopyColumnMapping> columnMappings, string destinationTableName);

    void BulkInsertIntoTable(IDBContext dbContext, DataTable sourceDataTable, List<SqlBulkCopyColumnMapping> columnMappings, string destinationTableName, int batchSize);

    List<SqlBulkCopyColumnMapping> GetMappingsForWorkerQueue(List<string> columnNameList);

    string GetDocumentIdentifierFieldColumnName(IDBContext workspaceDbContext);

    string GetDocumentIdentifierFieldName(IDBContext workspaceDbContext);

    string GetDocumentIdentifierFieldValue(IDBContext workspaceDbContext, int documentArtifactId, string documentIdentifierFieldColumnName);
  }
}