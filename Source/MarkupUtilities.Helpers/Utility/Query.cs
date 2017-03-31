using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Relativity.API;

namespace MarkupUtilities.Helpers.Utility
{
  public class Query : IQuery
  {
    public DataTable RetrieveApplicationWorkspaces(IDBContext eddsDbContext, Guid applicationGuid)
    {
      const string sql = @"DECLARE @appArtifactID INT
				SET @appArtifactID = (SELECT ArtifactID FROM ArtifactGuid WHERE ArtifactGuid = @appGuid)

				SELECT  C.ArtifactID, C.Name
				FROM CaseApplication (NOLOCK) CA
					INNER JOIN eddsdbo.[ExtendedCase] C ON CA.CaseID = C.ArtifactID
					INNER JOIN eddsdbo.ResourceServer RS ON C.ServerID = RS.ArtifactID
					INNER JOIN eddsdbo.Artifact A (NOLOCK) ON C.ArtifactID = A.ArtifactID
					INNER JOIN eddsdbo.[ApplicationInstall] as AI on CA.CurrentApplicationInstallID = AI.ApplicationInstallID
				WHERE CA.ApplicationID = @appArtifactId
					AND AI.[Status] = 6 --Installed
				ORDER BY A.CreatedOn";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@appGuid", SqlDbType.UniqueIdentifier) {Value = applicationGuid}
      };

      return eddsDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams);
    }

    public string RetrieveConfigurationValue(IDBContext eddsDbContext, string sectionName, string name)
    {
      const string sql = @"SELECT Value
				FROM EDDSDBO.Configuration
				WHERE Section = @sectionName
					AND Name = @name";

      var sqlParams = new SqlParameter[2];
      sqlParams[0] = new SqlParameter("@sectionName", SqlDbType.VarChar) { Value = sectionName };
      sqlParams[1] = new SqlParameter("@name", SqlDbType.VarChar) { Value = name };

      var result = eddsDbContext.ExecuteSqlStatementAsScalar(sql, sqlParams);

      if ((result != null) && (result != DBNull.Value))
      {
        return result.ToString();
      }
      return string.Empty;
    }

    public DataTable RetrieveWorkspaces(IDBContext eddsDbContext)
    {
      const string sql = @"
				SELECT
				ArtifactId [CaseArtifactID]
				,Name [CaseName]
				,DBLocation [ServerName]
				FROM EDDSDBO.[ExtendedCase] WITH(NOLOCK)				";

      return eddsDbContext.ExecuteSqlStatementAsDataTable(sql);
    }

    public static bool? DoesApplicationExistInWorkspace(IDBContext eddsDbContext, Guid applicationGuid, int workspaceArtifactId)
    {
      const string sql = @"
				DECLARE @appArtifactID INT
				SET @appArtifactID = (SELECT ArtifactID FROM ArtifactGuid WHERE ArtifactGuid = @appGuid)

				SELECT
					CASE WHEN EXISTS(
						SELECT
							C.ArtifactID
						FROM
							CaseApplication (NOLOCK) CA
							INNER JOIN eddsdbo.[ExtendedCase] C ON CA.CaseID = C.ArtifactID
							INNER JOIN eddsdbo.ResourceServer RS ON C.ServerID = RS.ArtifactID
							INNER JOIN eddsdbo.Artifact A (NOLOCK) ON C.ArtifactID = A.ArtifactID
							INNER JOIN eddsdbo.[ApplicationInstall] as AI on CA.CurrentApplicationInstallID = AI.ApplicationInstallID
						WHERE CA.ApplicationID = @appArtifactId
						AND AI.[Status] = 6 --Installed
						AND C.ArtifactId = @CaseArtifactId
						) THEN 1
						ELSE 0
					END AS DoesExist
				";

      var sqlParams = new List<SqlParameter>
        {
          new SqlParameter("@appGuid", SqlDbType.UniqueIdentifier) {Value = applicationGuid}
          ,new SqlParameter("@CaseArtifactId", SqlDbType.Int) {Value = workspaceArtifactId}
        };

      return Convert.ToBoolean(eddsDbContext.ExecuteSqlStatementAsScalar<int>(sql, sqlParams));
    }

    public string GetValueFromConfigTable(IDBContext eddsDbContext, string inputName)
    {
      const string sql = @"
				SELECT
						[Value]
				FROM EDDSDBO.Configuration
				WHERE [Name] = @inputName";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@inputName", SqlDbType.VarChar) {Value = inputName}
      };

      return eddsDbContext.ExecuteSqlStatementAsScalar<string>(sql, sqlParams);
    }

    #region SQL bulk insert

    //Bulk Insert data
    public void BulkInsertIntoTable(IDBContext dbContext, DataTable sourceDataTable, List<SqlBulkCopyColumnMapping> columnMappings, string destinationTableName)
    {
      using (var bulkCopy = new SqlBulkCopy(dbContext.GetConnection()))
      {
        bulkCopy.DestinationTableName = destinationTableName;

        foreach (var columnMapping in columnMappings)
        {
          bulkCopy.ColumnMappings.Add(columnMapping);
        }

        bulkCopy.WriteToServer(sourceDataTable);
        bulkCopy.Close();
      }
    }

    //Bulk Insert data in batches
    public void BulkInsertIntoTable(IDBContext dbContext, DataTable sourceDataTable, List<SqlBulkCopyColumnMapping> columnMappings, string destinationTableName, int batchSize)
    {
      using (var bulkCopy = new SqlBulkCopy(dbContext.GetConnection()))
      {
        bulkCopy.BatchSize = batchSize;
        bulkCopy.DestinationTableName = destinationTableName;

        foreach (var columnMapping in columnMappings)
        {
          bulkCopy.ColumnMappings.Add(columnMapping);
        }

        bulkCopy.WriteToServer(sourceDataTable);
        bulkCopy.Close();
      }
    }

    //Sql column mappings for Bulk Insert data
    public List<SqlBulkCopyColumnMapping> GetMappingsForWorkerQueue(List<string> columnNameList)
    {
      return columnNameList.Select(column => new SqlBulkCopyColumnMapping(column, column)).ToList();
    }

    #endregion SQL bulk insert

    #region Documnet Identifier Field

    public string GetDocumentIdentifierFieldColumnName(IDBContext workspaceDbContext)
    {
      const string sql = @"
			  SELECT AVF.ColumnName FROM [EDDSDBO].[ExtendedField] EF WITH(NOLOCK)
			  JOIN [EDDSDBO].[ArtifactViewField] AVF WITH(NOLOCK)
			  ON EF.TextIdentifier = AVF.HeaderName
			  WHERE EF.IsIdentifier = 1 AND EF.FieldArtifactTypeID = 10";

      var columnName = workspaceDbContext.ExecuteSqlStatementAsScalar(sql).ToString();
      return columnName;
    }

    public string GetDocumentIdentifierFieldName(IDBContext workspaceDbContext)
    {
      const string sql = @"
			  SELECT [TextIdentifier] FROM [EDDSDBO].[ExtendedField] WITH(NOLOCK)
			  WHERE IsIdentifier = 1 AND FieldArtifactTypeID = 10";

      var columnName = workspaceDbContext.ExecuteSqlStatementAsScalar(sql).ToString();
      return columnName;
    }

    public string GetDocumentIdentifierFieldValue(IDBContext workspaceDbContext, int documentArtifactId, string documentIdentifierFieldColumnName)
    {
      const string sql = @"
			  SELECT @documentIdentifierFieldColumnName FROM [EDDSDBO].[Document] WITH(NOLOCK)
			  WHERE ArtifactID=@documentArtifactId";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@documentArtifactId", SqlDbType.Int) {Value = documentArtifactId},
        new SqlParameter("@documentIdentifierFieldColumnName", SqlDbType.VarChar) {Value = documentIdentifierFieldColumnName}
      };

      var columnName = workspaceDbContext.ExecuteSqlStatementAsScalar(sql, sqlParams, 1200).ToString();
      return columnName;
    }

    #endregion Documnet Identifier Field
  }
}