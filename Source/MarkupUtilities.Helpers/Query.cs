using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using MarkupUtilities.Helpers.Exceptions;
using MarkupUtilities.Helpers.Models;
using Relativity.API;

namespace MarkupUtilities.Helpers
{
  public class Query : IQuery
  {
    public async Task CreateExportManagerQueueTableAsync(IDBContext eddsDbContext)
    {
      var sql = string.Format(@" 
				IF OBJECT_ID('[EDDSDBO].[{0}]') IS NULL BEGIN
					CREATE TABLE [EDDSDBO].[{0}]
					(
						ID INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
						TimeStampUTC DATETIME NOT NULL,
						WorkspaceArtifactID INT NOT NULL,
						QueueStatus INT NOT NULL,
						AgentID INT NULL,
						ExportJobArtifactID INT NOT NULL,
						CreatedBy NVARCHAR(MAX) NOT NULL,
						CreatedOn DATETIME NOT NULL,
						ResourceGroupID INT NOT NULL
					)
				END", Constant.Tables.ExportManagerQueue);

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql));
    }

    public async Task CreateReproduceManagerQueueTableAsync(IDBContext eddsDbContext)
    {
      var sql = string.Format(@" 
				IF OBJECT_ID('[EDDSDBO].[{0}]') IS NULL BEGIN
					CREATE TABLE [EDDSDBO].[{0}]
					(
						ID INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
						TimeStampUTC DATETIME NOT NULL,
						WorkspaceArtifactID INT NOT NULL,
						QueueStatus INT NOT NULL,
						AgentID INT NULL,
						ReproduceJobArtifactID INT NOT NULL,
						CreatedBy NVARCHAR(MAX) NOT NULL,
						CreatedOn DATETIME NOT NULL,
						ResourceGroupID INT NOT NULL
					)
				END", Constant.Tables.ReproduceManagerQueue);

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql));
    }

    public async Task CreateExportWorkerQueueTableAsync(IDBContext eddsDbContext)
    {
      var sql = string.Format(@" 
				IF OBJECT_ID('[EDDSDBO].[{0}]') IS NULL BEGIN
					CREATE TABLE [EDDSDBO].[{0}]
					(
						ID INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
						TimeStampUTC DATETIME NOT NULL,
						WorkspaceArtifactID INT NOT NULL,
						DocumentArtifactID INT NOT NULL,
						MarkupSetArtifactID INT NOT NULL,
						QueueStatus INT NOT NULL,
						AgentID INT NULL,
						ExportJobArtifactID INT NOT NULL,
						MarkupSubType NVARCHAR(MAX) NOT NULL,
						ResourceGroupID INT NOT NULL
					)
				END", Constant.Tables.ExportWorkerQueue);

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql));
    }

    public async Task CreateReproduceWorkerQueueTableAsync(IDBContext eddsDbContext)
    {
      var sql = string.Format(@" 
				IF OBJECT_ID('[EDDSDBO].[{0}]') IS NULL BEGIN
					CREATE TABLE [EDDSDBO].[{0}]
					(
						ID INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
						TimeStampUTC DATETIME NOT NULL,
						WorkspaceArtifactID INT NOT NULL,
						DocumentIDStart INT,
						DocumentIDEnd INT,
						SavedSearchHoldingTable VARCHAR(MAX) NOT NULL,
						RedactionsHoldingTable VARCHAR(MAX),
						DestinationMarkupSetArtifactID INT,
						QueueStatus INT NOT NULL,
						AgentID INT NULL,
						ReproduceJobArtifactID INT NOT NULL,
						ResourceGroupID INT NOT NULL,
						MarkupSetRedactionCodeArtifactID INT NOT NULL,
						MarkupSetAnnotationCodeArtifactID INT NOT NULL,
						RedactionCodeTypeID INT NOT NULL,
            RelationalGroupColumn VARCHAR(MAX),
            HasAutoRedactionsColumn VARCHAR(MAX),
            RelationalGroup NVARCHAR(MAX)
					)
				END", Constant.Tables.ReproduceWorkerQueue);

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql));
    }

    public async Task CreateExportErrorLogTableAsync(IDBContext eddsDbContext)
    {
      var sql = string.Format(@" 
				IF OBJECT_ID('[EDDSDBO].[{0}]') IS NULL BEGIN
					CREATE TABLE [EDDSDBO].[{0}]
					(
						ID INT IDENTITY(1,1),
						TimeStampUTC DATETIME,
						WorkspaceArtifactID INT,
						ApplicationName VARCHAR(500),
						ApplicationGuid UNIQUEIDENTIFIER,
						QueueTableName NVARCHAR(MAX),
						QueueRecordID INT,
						AgentID INT,
						Message NVARCHAR(MAX)
					)
				END", Constant.Tables.ExportErrorLog);

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql));
    }

    public async Task CreateReproduceErrorLogTableAsync(IDBContext eddsDbContext)
    {
      var sql = string.Format(@" 
				IF OBJECT_ID('[EDDSDBO].[{0}]') IS NULL BEGIN
					CREATE TABLE [EDDSDBO].[{0}]
					(
						ID INT IDENTITY(1,1),
						TimeStampUTC DATETIME,
						WorkspaceArtifactID INT,
						ApplicationName VARCHAR(500),
						ApplicationGuid UNIQUEIDENTIFIER,
						QueueTableName NVARCHAR(MAX),
						QueueRecordID INT,
						AgentID INT,
						Message NVARCHAR(MAX)
					)
				END", Constant.Tables.ReproduceErrorLog);

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql));
    }

    public async Task CreateImportManagerQueueTableAsync(IDBContext eddsDbContext)
    {
      var sql = string.Format(@" 
				IF OBJECT_ID('[EDDSDBO].[{0}]') IS NULL BEGIN
					CREATE TABLE [EDDSDBO].[{0}]
					(
						ID INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
						TimeStampUTC DATETIME NOT NULL,
						WorkspaceArtifactID INT NOT NULL,
						QueueStatus INT NOT NULL,
						AgentID INT NULL,
						ImportJobArtifactID INT NOT NULL,
						JobType NVARCHAR(MAX) NOT NULL,
						CreatedBy NVARCHAR(MAX) NOT NULL,
						CreatedOn DATETIME NOT NULL,
						ResourceGroupID INT NOT NULL
					)
				END", Constant.Tables.ImportManagerQueue);

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql));
    }

    public async Task CreateImportWorkerQueueTableAsync(IDBContext eddsDbContext)
    {
      await CreateImportWorkerQueueOrImportManagerHoldingTableAsync(eddsDbContext, Constant.Tables.ImportWorkerQueue);
    }

    public async Task CreateImportManagerHoldingTableAsync(IDBContext eddsDbContext, string tableName)
    {
      await CreateImportWorkerQueueOrImportManagerHoldingTableAsync(eddsDbContext, tableName);
    }

    private static async Task CreateImportWorkerQueueOrImportManagerHoldingTableAsync(IDBContext eddsDbContext, string tableName)
    {
      var sql = string.Format(@" 
				IF OBJECT_ID('[EDDSDBO].[{0}]') IS NULL BEGIN
					CREATE TABLE [EDDSDBO].[{0}]
					(
						[ID] INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
						[TimeStampUTC] DATETIME NOT NULL,
						[WorkspaceArtifactID] INT NOT NULL,
						[DocumentIdentifier] NVARCHAR(MAX) NOT NULL,
						[FileOrder] [int] NOT NULL,
						[QueueStatus] INT NOT NULL,
						[AgentID] INT NULL,
						[ImportJobArtifactID] INT NOT NULL,
						[JobType] NVARCHAR(MAX) NOT NULL,
						[X] INT NOT NULL,
						[Y] INT NOT NULL,
						[Width] INT NOT NULL,
						[Height] INT NOT NULL,
						[MarkupSetArtifactID] INT NOT NULL,
						[MarkupType] SMALLINT NULL,
						[FillA] SMALLINT NULL,
						[FillR] SMALLINT NULL,
						[FillG] SMALLINT NULL,
						[FillB] SMALLINT NULL,
						[BorderSize] INT NULL,
						[BorderA] SMALLINT NULL,
						[BorderR] SMALLINT NULL,
						[BorderG] SMALLINT NULL,
						[BorderB] SMALLINT NULL,
						[BorderStyle] SMALLINT NULL,
						[FontName] NVARCHAR(500) NULL,
						[FontA] SMALLINT NULL,
						[FontR] SMALLINT NULL,
						[FontG] SMALLINT NULL,
						[FontB] SMALLINT NULL,
						[FontSize] INT NULL,
						[FontStyle] SMALLINT NULL,
						[Text] NVARCHAR(MAX) NULL,
						[ZOrder] INT NULL,
						[DrawCrossLines] BIT NOT NULL,
						[MarkupSubType] SMALLINT NULL,
						[ResourceGroupID] INT NOT NULL,
						[SkipDuplicateRedactions] BIT NOT NULL,
            [X_d] decimal(14, 4) NULL,
	          [Y_d] decimal(14, 4) NULL,
	          [Width_d] decimal(14, 4) NULL,
	          [Height_d] decimal(14, 4) NULL
					)
				END", tableName);

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql));
    }

    public async Task CreateImportErrorLogTableAsync(IDBContext eddsDbContext)
    {
      var sql = string.Format(@" 
				IF OBJECT_ID('[EDDSDBO].[{0}]') IS NULL BEGIN
					CREATE TABLE [EDDSDBO].[{0}]
					(
						ID INT IDENTITY(1,1),
						TimeStampUTC DATETIME,
						WorkspaceArtifactID INT,
						ApplicationName VARCHAR(500),
						ApplicationGuid UNIQUEIDENTIFIER,
						QueueTableName NVARCHAR(MAX),
						QueueRecordID INT,
						AgentID INT,
						Message NVARCHAR(MAX)
					)
				END", Constant.Tables.ImportErrorLog);

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql));
    }

    public async Task CreateExportWorkerHoldingTableAsync(IDBContext eddsDbContext, string tableName)
    {
      var sql = string.Format(@" 
				IF OBJECT_ID('[EDDSDBO].[{0}]') IS NULL BEGIN
					CREATE TABLE [EDDSDBO].[{0}]
					(
						ID INT IDENTITY(1,1) NOT NULL,
						TimeStampUTC DATETIME NOT NULL,
						WorkspaceArtifactID INT NOT NULL,
						ExportJobArtifactID INT NOT NULL,
						DocumentIdentifier NVARCHAR(MAX) NOT NULL,
						FileOrder INT NOT NULL,
						X INT NOT NULL,
						Y INT NOT NULL,
						Width INT NOT NULL,
						Height INT NOT NULL,
						MarkupSetArtifactID INT NOT NULL,
						MarkupType SMALLINT NULL,
						FillA SMALLINT NULL,
						FillR SMALLINT NULL,
						FillG SMALLINT NULL,
						FillB SMALLINT NULL,
						BorderSize INT NULL,
						BorderA SMALLINT NULL,
						BorderR SMALLINT NULL,
						BorderG SMALLINT NULL,
						BorderB SMALLINT NULL,
						BorderStyle SMALLINT NULL,
						FontName NVARCHAR(500) NULL,
						FontA SMALLINT NULL,
						FontR SMALLINT NULL,
						FontG SMALLINT NULL,
						FontB SMALLINT NULL,
						FontSize INT NULL,
						FontStyle SMALLINT NULL,
						Text NVARCHAR(MAX) NULL,
						ZOrder INT NULL,
						DrawCrossLines BIT NOT NULL,
						MarkupSubType SMALLINT NULL,
            X_d decimal(14, 4) NULL,
	          Y_d decimal(14, 4) NULL,
	          Width_d decimal(14, 4) NULL,
	          Height_d decimal(14, 4) NULL
					)
				END", tableName);

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql));
    }

    public async Task<string> GetDocumentIdentifierFieldColumnNameAsync(IDBContext workspaceDbContext)
    {
      const string sql = @"
			  SELECT AVF.ColumnName 
        FROM [EDDSDBO].[ExtendedField] EF WITH(NOLOCK)	
			    JOIN [EDDSDBO].[ArtifactViewField] AVF WITH(NOLOCK) ON EF.TextIdentifier = AVF.HeaderName
			  WHERE 
          EF.IsIdentifier = 1 
          AND EF.FieldArtifactTypeID = 10";

      return await Task.Run(() => workspaceDbContext.ExecuteSqlStatementAsScalar(sql).ToString());
    }

    public async Task CreateExportResultsTableAsync(IDBContext eddsDbContext)
    {
      var sql = string.Format(@" 
				IF OBJECT_ID('[EDDSDBO].[{0}]') IS NULL BEGIN
					CREATE TABLE [EDDSDBO].[{0}]
					(
						ID INT IDENTITY(1,1) NOT NULL,
						AgentID INT NULL,
						TimeStampUTC DATETIME NOT NULL,
						WorkspaceArtifactID INT NOT NULL,
						ExportJobArtifactID INT NOT NULL,
						DocumentIdentifier NVARCHAR(MAX) NOT NULL,
						FileOrder INT NOT NULL,
						X INT NOT NULL,
						Y INT NOT NULL,
						Width INT NOT NULL,
						Height INT NOT NULL,
						MarkupSetArtifactID INT NOT NULL,
						MarkupType SMALLINT NULL,
						FillA SMALLINT NULL,
						FillR SMALLINT NULL,
						FillG SMALLINT NULL,
						FillB SMALLINT NULL,
						BorderSize INT NULL,
						BorderA SMALLINT NULL,
						BorderR SMALLINT NULL,
						BorderG SMALLINT NULL,
						BorderB SMALLINT NULL,
						BorderStyle SMALLINT NULL,
						FontName NVARCHAR(500) NULL,
						FontA SMALLINT NULL,
						FontR SMALLINT NULL,
						FontG SMALLINT NULL,
						FontB SMALLINT NULL,
						FontSize INT NULL,
						FontStyle SMALLINT NULL,
						Text NVARCHAR(MAX) NULL,
						ZOrder INT NULL,
						DrawCrossLines BIT NOT NULL,
						MarkupSubType SMALLINT NULL,
            X_d decimal(14, 4) NULL,
	          Y_d decimal(14, 4) NULL,
	          Width_d decimal(14, 4) NULL,
	          Height_d decimal(14, 4) NULL						
					)
				END", Constant.Tables.ExportResults);

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql));
    }

    public async Task InsertRowIntoJobErrorLogAsync(IDBContext eddsDbContext, int workspaceArtifactId, string queueTableName, int queueRecordId, int agentId, string errorMessage, string jobErrorTable)
    {
      var sql = $@" 
			INSERT INTO [EDDSDBO].[{jobErrorTable}]
			(
				[TimeStampUTC]
				,WorkspaceArtifactID
				,ApplicationName
				,ApplicationGuid
				,QueueTableName
				,QueueRecordID
				,AgentID
				,[Message]
			)
			VALUES 
			(
				GetUTCDate()
				,@workspaceArtifactId
				,@applicationName
				,@applicationGuid
				,@queueTableName
				,@queueRecordId
				,@agentId
				,@message
			)";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@workspaceArtifactId", SqlDbType.Int) {Value = workspaceArtifactId},
        new SqlParameter("@applicationName", SqlDbType.VarChar) {Value = Constant.Names.ApplicationName},
        new SqlParameter("@applicationGuid", SqlDbType.UniqueIdentifier) {Value = Constant.Guids.Application.ApplicationGuid},
        new SqlParameter("@queueTableName", SqlDbType.VarChar) {Value = queueTableName},
        new SqlParameter("@queueRecordId", SqlDbType.Int) {Value = queueRecordId},
        new SqlParameter("@agentId", SqlDbType.Int) {Value = agentId},
        new SqlParameter("@message", SqlDbType.NVarChar) {Value = errorMessage}
      };

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
    }

    public async Task InsertRowIntoImportErrorLogAsync(IDBContext eddsDbContext, int workspaceArtifactId, string queueTableName, int queueRecordId, int agentId, string errorMessage)
    {
      var sql = $@" 
			INSERT INTO [EDDSDBO].[{Constant.Tables.ImportErrorLog}]
			(
				[TimeStampUTC]
				,WorkspaceArtifactID
				,ApplicationName
				,ApplicationGuid
				,QueueTableName
				,QueueRecordID
				,AgentID
				,[Message]
			)
			VALUES 
			(
				GetUTCDate()
				,@workspaceArtifactId
				,@applicationName
				,@applicationGuid
				,@queueTableName
				,@queueRecordId
				,@agentId
				,@message
			)";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@workspaceArtifactId", SqlDbType.Int) {Value = workspaceArtifactId},
        new SqlParameter("@applicationName", SqlDbType.VarChar) {Value = Constant.Names.ApplicationName},
        new SqlParameter("@applicationGuid", SqlDbType.UniqueIdentifier) {Value = Constant.Guids.Application.ApplicationGuid},
        new SqlParameter("@queueTableName", SqlDbType.VarChar) {Value = queueTableName},
        new SqlParameter("@queueRecordId", SqlDbType.Int) {Value = queueRecordId},
        new SqlParameter("@agentId", SqlDbType.Int) {Value = agentId},
        new SqlParameter("@message", SqlDbType.NVarChar) {Value = errorMessage}
      };

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
    }


    public async Task<DataTable> RetrieveNextInReproduceWorkerQueueAsync(IDBContext eddsDbContext, int agentId, string commaDelimitedResourceAgentIds)
    {

      var sql = $@" 
				SET NOCOUNT ON 

				DECLARE @ID INT
				DECLARE @WorkspaceArtifactID INT
				DECLARE @DocumentIDStart INT
				DECLARE @DocumentIDEnd INT
				DECLARE @SavedSearchHoldingTable VARCHAR(MAX)
				DECLARE @RedactionsHoldingTable VARCHAR(MAX)
				DECLARE @QueueStatus INT
				DECLARE @ReproduceJobArtifactID INT
				DECLARE @ResourceGroupID INT
				DECLARE @RedactionCodeTypeID INT
				DECLARE @MarkupSetRedactionCodeArtifactID INT
				DECLARE @MarkupSetAnnotationCodeArtifactID INT
        DECLARE @RelationalGroupColumn VARCHAR(MAX)
        DECLARE @HasAutoRedactionsColumn VARCHAR(MAX)
        DECLARE @RelationalGroup NVARCHAR(MAX)

				BEGIN TRAN 
					SELECT TOP 1
						@ID = ID,
						@WorkspaceArtifactID = WorkspaceArtifactID,
						@DocumentIDStart = DocumentIDStart,
				    @DocumentIDEnd = DocumentIDEnd,
				    @SavedSearchHoldingTable = SavedSearchHoldingTable,
				    @RedactionsHoldingTable = RedactionsHoldingTable,
				    @QueueStatus = QueueStatus,
				    @ReproduceJobArtifactID = ReproduceJobArtifactID,
						@ResourceGroupID = ResourceGroupID,
						@RedactionCodeTypeID = RedactionCodeTypeID,
						@MarkupSetRedactionCodeArtifactID = MarkupSetRedactionCodeArtifactID,
						@MarkupSetAnnotationCodeArtifactID = MarkupSetAnnotationCodeArtifactID,
						@RelationalGroupColumn = RelationalGroupColumn,
						@HasAutoRedactionsColumn = HasAutoRedactionsColumn,
						@RelationalGroup = RelationalGroup
					FROM 
            [EDDSDBO].[{Constant.Tables.ReproduceWorkerQueue}] WITH(UPDLOCK,READPAST) 
					WHERE 
            [QueueStatus] = @notStartedQueueStatus
						AND ResourceGroupID IN ({commaDelimitedResourceAgentIds})
					ORDER BY 
						[TimeStampUTC] ASC

					UPDATE [EDDSDBO].[{Constant.Tables.ReproduceWorkerQueue}] 
          SET 
            [QueueStatus] = @inProgressQueueStatus, 
            AgentID = @agentId 
          WHERE [ID] = @ID 

				COMMIT 
				SET NOCOUNT OFF 

				SELECT 
					@ID ID,
					@WorkspaceArtifactID WorkspaceArtifactID,
					@DocumentIDStart DocumentIDStart,
				  @DocumentIDEnd DocumentIDEnd,
				  @SavedSearchHoldingTable SavedSearchHoldingTable,
				  @RedactionsHoldingTable RedactionsHoldingTable,
				  @QueueStatus QueueStatus,
				  @ReproduceJobArtifactID ReproduceJobArtifactID,
					@ResourceGroupID ResourceGroupID,
					@RedactionCodeTypeID RedactionCodeTypeID,
					@MarkupSetRedactionCodeArtifactID MarkupSetRedactionCodeArtifactID,
					@MarkupSetAnnotationCodeArtifactID MarkupSetAnnotationCodeArtifactID,
					@RelationalGroupColumn RelationalGroupColumn,
					@HasAutoRedactionsColumn HasAutoRedactionsColumn,
					@RelationalGroup RelationalGroup
				WHERE @ID IS NOT NULL";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@agentId", SqlDbType.Int) {Value = agentId},
        new SqlParameter("@notStartedQueueStatus", SqlDbType.Int) {Value = Constant.Status.Queue.NotStarted},
        new SqlParameter("@inProgressQueueStatus", SqlDbType.Int) {Value = Constant.Status.Queue.InProgress}
      };

      return await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));
    }


    public async Task<DataTable> RetrieveReproduceWorkerQueueAsync(IDBContext eddsDbContext, int reproduceJobArtifactId)
    {
      var sql = $@"SELECT DISTINCT WorkspaceArtifactID, SavedSearchHoldingTable, RedactionsHoldingTable FROM [EDDSDBO].[{Constant.Tables.ReproduceWorkerQueue}] WITH(NOLOCK) WHERE [ReproduceJobArtifactID] = @ReproduceJobArtifactID";
      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@ReproduceJobArtifactID", SqlDbType.Int) {Value = reproduceJobArtifactId }
      };

      return await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));
    }

    public async Task<DataTable> RetrieveArtifactIDsAsync(IDBContext eddsDbContext, string tempTable)
    {
      var sql = $@"SELECT ArtifactID FROM [EDDSResource].[EDDSDBO].[{tempTable}]";
      return await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsDataTable(sql));
    }

    public async Task<DataTable> RetrieveNextInJobManagerQueueAsync(IDBContext eddsDbContext, int agentId, string commaDelimitedResourceAgentIds, string jobManagerQueueTable, string jobArtifactId)
    {
      var sql = $@" 
				SET NOCOUNT ON 

				DECLARE @ID INT
				DECLARE @WorkspaceArtifactID INT
				DECLARE @ArtifactID INT
				DECLARE @ResourceGroupID INT

				BEGIN TRAN 
					SELECT TOP 1
						@ID = ID,
						@WorkspaceArtifactID = WorkspaceArtifactID,
						@ArtifactID = {jobArtifactId},
						@ResourceGroupID = ResourceGroupID
					FROM [EDDSDBO].[{jobManagerQueueTable}] WITH(UPDLOCK,READPAST) 
					WHERE 
            [QueueStatus] = @notStartedQueueStatus
						AND ResourceGroupID IN ({commaDelimitedResourceAgentIds})
					ORDER BY 
						[TimeStampUTC] ASC

					UPDATE [EDDSDBO].[{jobManagerQueueTable}] SET [QueueStatus] = @inProgressQueueStatus, AgentID = @agentId WHERE [ID] = @ID 

				COMMIT 
				SET NOCOUNT OFF 

				SELECT 
					@ID ID,
					@WorkspaceArtifactID WorkspaceArtifactID,
					@ArtifactID {jobArtifactId},
					@ResourceGroupID ResourceGroupID
				WHERE @ID IS NOT NULL";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@agentId", SqlDbType.Int) {Value = agentId},
        new SqlParameter("@notStartedQueueStatus", SqlDbType.Int) {Value = Constant.Status.Queue.NotStarted},
        new SqlParameter("@inProgressQueueStatus", SqlDbType.Int) {Value = Constant.Status.Queue.InProgress}
      };

      return await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));
    }

    public async Task<DataTable> RetrieveNextInImportManagerQueueAsync(IDBContext eddsDbContext, int agentId, string commaDelimitedResourceAgentIds)
    {
      var sql = $@" 
				SET NOCOUNT ON 

				DECLARE @ID INT
				DECLARE @TimeStampUTC DATETIME
				DECLARE @WorkspaceArtifactID INT
				DECLARE @QueueStatus INT
				DECLARE @ImportJobArtifactID INT
				DECLARE @JobType NVARCHAR(MAX)
				DECLARE @ResourceGroupID INT

				BEGIN TRAN 
					SELECT TOP 1
						@ID = [ID],
						@TimeStampUTC = [TimeStampUTC],
						@WorkspaceArtifactID = [WorkspaceArtifactID],
						@QueueStatus = [QueueStatus],
						@ImportJobArtifactID = [ImportJobArtifactID],
						@JobType = [JobType],
						@ResourceGroupID = [ResourceGroupID]
					FROM [EDDSDBO].[{Constant.Tables.ImportManagerQueue}] WITH(UPDLOCK,READPAST) 
					WHERE 
            [QueueStatus] = @notStartedQueueStatus
						AND [ResourceGroupID] IN ({commaDelimitedResourceAgentIds})
					ORDER BY 
						[TimeStampUTC] ASC

					UPDATE [EDDSDBO].[{Constant.Tables.ImportManagerQueue}] SET [QueueStatus] = @inProgressQueueStatus, [AgentID] = @agentId WHERE [ID] = @ID 

				COMMIT 
				SET NOCOUNT OFF 

				SELECT 
					@ID [ID],
					@TimeStampUTC [TimeStampUTC],
					@WorkspaceArtifactID [WorkspaceArtifactID],
					@QueueStatus [QueueStatus],
					@agentId [AgentID],
					@ImportJobArtifactID [ImportJobArtifactID],
					@JobType [JobType],
					@ResourceGroupID [ResourceGroupID]
				WHERE @ID IS NOT NULL";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@agentId", SqlDbType.Int) {Value = agentId},
        new SqlParameter("@notStartedQueueStatus", SqlDbType.Int) {Value = Constant.Status.Queue.NotStarted},
        new SqlParameter("@inProgressQueueStatus", SqlDbType.Int) {Value = Constant.Status.Queue.InProgress}
      };

      return await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));
    }

    public async Task ResetUnfishedJobsAsync(IDBContext eddsDbContext, int agentId, string queueTableName)
    {
      var sql = $@"UPDATE [EDDSDBO].[{queueTableName}] SET [QueueStatus] = @notStartedQueueStatus, AgentID = NULL WHERE AgentID = @agentId";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@agentId", SqlDbType.Int) {Value = agentId},
        new SqlParameter("@notStartedQueueStatus", SqlDbType.Int) {Value = Constant.Status.Queue.NotStarted}
      };

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
    }

    public async Task RemoveRecordFromTableByIdAsync(IDBContext eddsDbContext, string queueTableName, int id)
    {
      var sql = $@"DELETE FROM [EDDSDBO].[{queueTableName}] WHERE ID = @id";
      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@id", SqlDbType.Int) {Value = id}
      };

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
    }

    public async Task<int> InsertRowsIntoRedactionsHoldingTableAsync(IDBContext eddsDbContext, string redactionsTable, string savedSearchTable, int sourceMarkupSetArtifactId, int destinationMarkupSetArtifactId, int start, int end, bool includeRelationalGroup)
    {
      var relationalGroup = "";

      if (includeRelationalGroup)
      {
        relationalGroup = ",D.RelationalGroup";
      }

      var sql = string.Format(@" 
				INSERT INTO [EDDSDBO].[{0}]
        SELECT DISTINCT
	        F.[Order] AS [PageNumber],
	        R.X, 
	        R.Y, 
	        R.Width, 
	        R.Height, 
	        @DestionationMarkupSetID AS [MarkupSetArtifactID], 
	        R.MarkupType, 
	        R.FillA, 
	        R.FillR, 
	        R.FillG, 
	        R.FillB, 
	        R.BorderSize, 
	        R.BorderA, 
	        R.BorderR, 
	        R.BorderG, 
	        R.BorderB, 
	        R.BorderStyle, 
	        R.FontName, 
	        R.FontA, 
	        R.FontR, 
	        R.FontG, 
	        R.FontB, 
	        R.FontSize, 
	        R.FontStyle, 
	        R.[Text], 
	        R.ZOrder, 
	        R.DrawCrossLines, 
	        R.MarkupSubType{2},
	        R.X_d,
          R.Y_d,
	        R.Width_d,
	        R.Height_d
        FROM eddsdbo.Redaction R (NOLOCK)
          INNER JOIN eddsdbo.[File] F (NOLOCK) ON R.FileGuid = F.[Guid]
	        INNER JOIN  [EDDSDBO].[{1}] D (NOLOCK) ON D.DocumentArtifactID = F.DocumentArtifactID
        WHERE 
          R.MarkupSetArtifactID = @SourceMarkupTypeID 
          AND D.ID BETWEEN @Start AND @End
        ", redactionsTable, savedSearchTable, relationalGroup);

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@SourceMarkupTypeID", SqlDbType.Int) {Value = sourceMarkupSetArtifactId},
        new SqlParameter("@DestionationMarkupSetID", SqlDbType.Int) {Value = destinationMarkupSetArtifactId},
        new SqlParameter("@Start", SqlDbType.Int) {Value = start},
        new SqlParameter("@End", SqlDbType.Int) {Value = end}
      };

      return await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
    }

    public async Task InsertRowsIntoExportWorkerQueueAsync(IDBContext eddsDbContext, int jobId, int workspaceArtifactId, int parentRecordArtifactId, int resourceGroupId)
    {
      var sql = $@" 
				INSERT INTO [EDDSDBO].[{Constant.Tables.ExportWorkerQueue}]
				(
					[TimeStampUTC] 
					,WorkspaceArtifactID
					,ExportJobArtifactID
					,QueueStatus
					,AgentID
					,RecordArtifactID
					,ParentRecordArtifactID
					,ResourceGroupID
				)
				SELECT 
					@timeStamp
					,@workspaceArtifactId
					,@jobID
					,@notStartedQueueStatus
					,NULL
					,NULL
					,@parentRecordArtifactID
					,@resourceGroupID
				";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@jobID", SqlDbType.Int) {Value = jobId},
        new SqlParameter("@workspaceArtifactId", SqlDbType.Int) {Value = workspaceArtifactId},
        new SqlParameter("@timeStamp", SqlDbType.DateTime) {Value = DateTime.UtcNow},
        new SqlParameter("@parentRecordArtifactID", SqlDbType.Int) {Value = parentRecordArtifactId},
        new SqlParameter("@notStartedQueueStatus", SqlDbType.Int) {Value = Constant.Status.Queue.NotStarted},
        new SqlParameter("@resourceGroupID", SqlDbType.Int) {Value = resourceGroupId}
      };

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
    }

    public async Task<DataTable> RelationalGroupHasImagesAsync(IDBContext eddsDbContext, string relationalGroupColumn, string relationalGroup)
    {
      var sql = $@" 
				SELECT 1
        FROM [EDDSDBO].[File] F (NOLOCK)
	        INNER JOIN .[EDDSDBO].Document D (NOLOCK) on F.DocumentArtifactID=D.ArtifactID
        WHERE 
          D.[{relationalGroupColumn}] = @relationalGroup 
          AND F.[Type] = 1";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@relationalGroup", SqlDbType.NVarChar) {Value = relationalGroup ?? (object)DBNull.Value}
      };

      return await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));
    }

    public async Task InsertRowIntoReproduceWorkerQueueAsync(IDBContext eddsDbContext, int workspaceArtifactId, int documentIdStart, int documentIdEnd, string savedSearchHoldingTable, string redactionsHoldingTable, int destinationMarkupSetArtifactId, int reproduceJobArtifactId, int resourceGroupId, int codeTypeId, int markupSetRedactionCodeArtifactId, int markupSetAnnotationCodeArtifactId, string relationalGroupColumn, string hasAutoRedactionsColumn, string relationalGroup)
    {
      var sql = $@" 
				INSERT INTO [EDDSDBO].[{Constant.Tables.ReproduceWorkerQueue}]
				(
					[TimeStampUTC] 
					,WorkspaceArtifactID
          ,DocumentIDStart
          ,DocumentIDEnd
          ,SavedSearchHoldingTable
          ,RedactionsHoldingTable
          ,DestinationMarkupSetArtifactID
          ,QueueStatus
					,AgentID
					,ReproduceJobArtifactID
					,ResourceGroupID
					,RedactionCodeTypeID
					,MarkupSetRedactionCodeArtifactID
					,MarkupSetAnnotationCodeArtifactID
          ,RelationalGroupColumn
          ,HasAutoRedactionsColumn
          ,RelationalGroup
				)
				SELECT 
					@timeStamp
					,@workspaceArtifactId
					,@documentIDStart
					,@documentIDEnd
					,@savedSearchHoldingTable
					,@redactionsHoldingTable
					,@destinationMarkupSetArtifactID
					,@notStartedQueueStatus
					,NULL
					,@reproduceJobArtifactID
					,@resourceGroupID
					,@codeTypeId
					,@markupSetRedactionCodeArtifactID
					,@markupSetAnnotationCodeArtifactID
					,@relationalGroupColumn
					,@hasAutoRedactionsColumn
					,@relationalGroup";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@timeStamp", SqlDbType.DateTime) {Value = DateTime.UtcNow},
        new SqlParameter("@workspaceArtifactId", SqlDbType.Int) {Value = workspaceArtifactId},
        new SqlParameter("@documentIDStart", SqlDbType.Int) {Value = documentIdStart > -1 ? documentIdStart : (object)DBNull.Value},
        new SqlParameter("@documentIDEnd", SqlDbType.Int) {Value = documentIdEnd > -1 ? documentIdEnd : (object)DBNull.Value},
        new SqlParameter("@savedSearchHoldingTable", SqlDbType.VarChar) {Value = savedSearchHoldingTable},
        new SqlParameter("@redactionsHoldingTable", SqlDbType.VarChar) {Value = redactionsHoldingTable ?? (object)DBNull.Value},
        new SqlParameter("@destinationMarkupSetArtifactID", SqlDbType.Int) {Value = destinationMarkupSetArtifactId > -1 ? destinationMarkupSetArtifactId : (object)DBNull.Value},
        new SqlParameter("@notStartedQueueStatus", SqlDbType.Int) {Value = Constant.Status.Queue.NotStarted},
        new SqlParameter("@reproduceJobArtifactID", SqlDbType.Int) {Value = reproduceJobArtifactId},
        new SqlParameter("@resourceGroupID", SqlDbType.Int) {Value = resourceGroupId},
        new SqlParameter("@codeTypeId", SqlDbType.Int) {Value = codeTypeId},
        new SqlParameter("@markupSetRedactionCodeArtifactID", SqlDbType.Int) {Value = markupSetRedactionCodeArtifactId},
        new SqlParameter("@markupSetAnnotationCodeArtifactID", SqlDbType.Int) {Value = markupSetAnnotationCodeArtifactId},
        new SqlParameter("@relationalGroupColumn", SqlDbType.VarChar) {Value = relationalGroupColumn ?? (object)DBNull.Value},
        new SqlParameter("@hasAutoRedactionsColumn", SqlDbType.VarChar) {Value = hasAutoRedactionsColumn ?? (object)DBNull.Value},
        new SqlParameter("@relationalGroup", SqlDbType.VarChar) {Value = relationalGroup ?? (object)DBNull.Value}
      };

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
    }

    public async Task InsertRowIntoImportManagerHoldingTableAsync(IDBContext eddsDbContext, int workspaceArtifactId, string documentIdentifier, int fileOrder, int resourceGroupId, int importJobArtifactId, int markupSetArtifactId, string jobType, ImportFileRecord importFileRecord, bool skipDuplicateRedactions, string tableName)
    {
      string sql = $@"
				BEGIN TRAN 
					INSERT INTO [EDDSDBO].[{tableName}]
					(
						[TimeStampUTC]
						,[WorkspaceArtifactID]
						,[DocumentIdentifier]
						,[FileOrder]
						,[QueueStatus]
						,[AgentID]
						,[ImportJobArtifactID]
						,[JobType]
						,[X]
						,[Y]
						,[Width]
						,[Height]
						,[MarkupSetArtifactID]
						,[MarkupType]
						,[FillA]
						,[FillR]
						,[FillG]
						,[FillB]
						,[BorderSize]
						,[BorderA]
						,[BorderR]
						,[BorderG]
						,[BorderB]
						,[BorderStyle]
						,[FontName]
						,[FontA]
						,[FontR]
						,[FontG]
						,[FontB]
						,[FontSize]
						,[FontStyle]
						,[Text]
						,[ZOrder]
						,[DrawCrossLines]
						,[MarkupSubType]
						,[ResourceGroupID]
						,[SkipDuplicateRedactions]
            ,[X_d]
            ,[Y_d]
            ,[Width_d]
            ,[Height_d]
					)
					VALUES
					(
					  @timeStamp
					  ,@workspaceArtifactId
					  ,@documentIdentifier
					  ,@fileOrder
					  ,@notStartedQueueStatus
					  ,@agentId
					  ,@importJobArtifactId
					  ,@jobType
					  ,@x
					  ,@y
					  ,@width
					  ,@height
					  ,@markupSetArtifactId
					  ,@markupType
					  ,@fillA
					  ,@fillR
					  ,@fillG
					  ,@fillB
					  ,@borderSize
					  ,@borderA
					  ,@borderR
					  ,@borderG
					  ,@borderB
					  ,@borderStyle
					  ,@fontName
					  ,@fontA
					  ,@fontR
					  ,@fontG
					  ,@fontB
					  ,@fontSize
					  ,@fontStyle
					  ,@text
					  ,@zOrder
					  ,@drawCrossLines
					  ,@markupSubType
					  ,@resourceGroupID
					  ,@skipDuplicateRedactions
            ,@xD
            ,@yD
            ,@widthD
            ,@heightD
					)
				COMMIT
				";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@timeStamp", SqlDbType.DateTime) {Value = DateTime.UtcNow},
        new SqlParameter("@workspaceArtifactId", SqlDbType.Int) {Value = workspaceArtifactId},
        new SqlParameter("@documentIdentifier", SqlDbType.NVarChar) {Value = documentIdentifier},
        new SqlParameter("@fileOrder", SqlDbType.NVarChar) {Value = fileOrder},
        new SqlParameter("@notStartedQueueStatus", SqlDbType.Int) {Value = Constant.Status.Queue.NotStarted},
        new SqlParameter("@agentId", SqlDbType.Int) {Value = DBNull.Value},
        new SqlParameter("@importJobArtifactId", SqlDbType.Int) {Value = importJobArtifactId},
        new SqlParameter("@jobType", SqlDbType.NVarChar) {Value = jobType},
        new SqlParameter("@x", SqlDbType.Int) {Value = importFileRecord.X},
        new SqlParameter("@y", SqlDbType.Int) {Value = importFileRecord.Y},
        new SqlParameter("@width", SqlDbType.Int) {Value = importFileRecord.Width},
        new SqlParameter("@height", SqlDbType.Int) {Value = importFileRecord.Height},
        new SqlParameter("@markupSetArtifactId", SqlDbType.Int) {Value = markupSetArtifactId},
        new SqlParameter("@markupType", SqlDbType.SmallInt) {Value = importFileRecord.MarkupType},
        new SqlParameter("@fillA", SqlDbType.SmallInt) {Value = (object) importFileRecord.FillA ?? DBNull.Value},
        new SqlParameter("@fillR", SqlDbType.SmallInt) {Value = (object) importFileRecord.FillR ?? DBNull.Value},
        new SqlParameter("@fillG", SqlDbType.SmallInt) {Value = (object) importFileRecord.FillG ?? DBNull.Value},
        new SqlParameter("@fillB", SqlDbType.SmallInt) {Value = (object) importFileRecord.FillB ?? DBNull.Value},
        new SqlParameter("@borderSize", SqlDbType.Int) {Value = (object) importFileRecord.BorderSize ?? DBNull.Value},
        new SqlParameter("@borderA", SqlDbType.SmallInt) {Value = (object) importFileRecord.BorderA ?? DBNull.Value},
        new SqlParameter("@borderR", SqlDbType.SmallInt) {Value = (object) importFileRecord.BorderR ?? DBNull.Value},
        new SqlParameter("@borderG", SqlDbType.SmallInt) {Value = (object) importFileRecord.BorderG ?? DBNull.Value},
        new SqlParameter("@borderB", SqlDbType.SmallInt) {Value = (object) importFileRecord.BorderB ?? DBNull.Value},
        new SqlParameter("@borderStyle", SqlDbType.SmallInt) {Value = (object) importFileRecord.BorderStyle ?? DBNull.Value},
        new SqlParameter("@fontName", SqlDbType.NVarChar) {Value = importFileRecord.FontName},
        new SqlParameter("@fontA", SqlDbType.SmallInt) {Value = (object) importFileRecord.FontA ?? DBNull.Value},
        new SqlParameter("@fontR", SqlDbType.SmallInt) {Value = (object) importFileRecord.FontR ?? DBNull.Value},
        new SqlParameter("@fontG", SqlDbType.SmallInt) {Value = (object) importFileRecord.FontG ?? DBNull.Value},
        new SqlParameter("@fontB", SqlDbType.SmallInt) {Value = (object) importFileRecord.FontB ?? DBNull.Value},
        new SqlParameter("@fontSize", SqlDbType.Int) {Value = (object) importFileRecord.FontSize ?? DBNull.Value},
        new SqlParameter("@fontStyle", SqlDbType.SmallInt) {Value = (object) importFileRecord.FontStyle ?? DBNull.Value},
        new SqlParameter("@text", SqlDbType.NVarChar) {Value = importFileRecord.Text},
        new SqlParameter("@zOrder", SqlDbType.Int) {Value = importFileRecord.ZOrder},
        new SqlParameter("@drawCrossLines", SqlDbType.Bit) {Value = importFileRecord.DrawCrossLines},
        new SqlParameter("@markupSubType", SqlDbType.SmallInt) {Value = importFileRecord.MarkupSubType},
        new SqlParameter("@resourceGroupID", SqlDbType.Int) {Value = resourceGroupId},
        new SqlParameter("@skipDuplicateRedactions", SqlDbType.Bit) {Value = skipDuplicateRedactions},
        new SqlParameter("@xD", SqlDbType.Decimal) {Value = (object) importFileRecord.Xd ?? DBNull.Value},
        new SqlParameter("@yD", SqlDbType.Decimal) {Value = (object) importFileRecord.Yd ?? DBNull.Value},
        new SqlParameter("@widthD", SqlDbType.Decimal) {Value = (object) importFileRecord.WidthD ?? DBNull.Value},
        new SqlParameter("@heightD", SqlDbType.Decimal) {Value = (object) importFileRecord.HeightD ?? DBNull.Value}
      };

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
    }

    public async Task BulkCopyDataFromImportManagerHoldingTableIntoImportWorkerQueueTableAsync(IDBContext eddsDbContext, string importManagerHoldingTableName)
    {
      string sql = $@" 
				INSERT INTO [EDDSDBO].[{Constant.Tables.ImportWorkerQueue}]
				(
					[TimeStampUTC]
          ,[WorkspaceArtifactID]
          ,[DocumentIdentifier]
          ,[FileOrder]
          ,[QueueStatus]
          ,[AgentID]
          ,[ImportJobArtifactID]
          ,[JobType]
          ,[X]
          ,[Y]
          ,[Width]
          ,[Height]
          ,[MarkupSetArtifactID]
          ,[MarkupType]
          ,[FillA]
          ,[FillR]
          ,[FillG]
          ,[FillB]
          ,[BorderSize]
          ,[BorderA]
          ,[BorderR]
          ,[BorderG]
          ,[BorderB]
          ,[BorderStyle]
          ,[FontName]
          ,[FontA]
          ,[FontR]
          ,[FontG]
          ,[FontB]
          ,[FontSize]
          ,[FontStyle]
          ,[Text]
          ,[ZOrder]
          ,[DrawCrossLines]
          ,[MarkupSubType]
          ,[ResourceGroupID]
				  ,[SkipDuplicateRedactions]
          ,[X_d]
          ,[Y_d]
          ,[Width_d]
          ,[Height_d]
				)
				SELECT 
					[TimeStampUTC]
					,[WorkspaceArtifactID]
					,[DocumentIdentifier]
					,[FileOrder]
					,[QueueStatus]
					,[AgentID]
					,[ImportJobArtifactID]
					,[JobType]
					,[X]
					,[Y]
					,[Width]
					,[Height]
					,[MarkupSetArtifactID]
					,[MarkupType]
					,[FillA]
					,[FillR]
					,[FillG]
					,[FillB]
					,[BorderSize]
					,[BorderA]
					,[BorderR]
					,[BorderG]
					,[BorderB]
					,[BorderStyle]
					,[FontName]
					,[FontA]
					,[FontR]
					,[FontG]
					,[FontB]
					,[FontSize]
					,[FontStyle]
					,[Text]
					,[ZOrder]
					,[DrawCrossLines]
					,[MarkupSubType]
					,[ResourceGroupID]
					,[SkipDuplicateRedactions]
					,[X_d]
					,[Y_d]
					,[Width_d]
					,[Height_d]
				FROM [EDDSDBO].[{importManagerHoldingTableName}]";

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql));
    }

    public async Task<int> InsertRowIntoRedactionTableAsync(IDBContext workspaceDbContext, string fileGuid, int markupSetArtifactId, ImportWorkerQueueRecord importWorkerQueueRecord)
    {
      const string sql = @"
				BEGIN TRAN 
					INSERT INTO [EDDSDBO].[Redaction]
					(
						[FileGuid]
						,[X]
						,[Y]
						,[Width]
						,[Height]
						,[MarkupSetArtifactID]
						,[MarkupType]
						,[FillA]
						,[FillR]
						,[FillG]
						,[FillB]
						,[BorderSize]
						,[BorderA]
						,[BorderR]
						,[BorderG]
						,[BorderB]
						,[BorderStyle]
						,[FontName]
						,[FontA]
						,[FontR]
						,[FontG]
						,[FontB]
						,[FontSize]
						,[FontStyle]
						,[Text]
						,[ZOrder]
						,[DrawCrossLines]
						,[MarkupSubType]
						,[X_d]
						,[Y_d]
						,[Width_d]
						,[Height_d]
					)
					OUTPUT inserted.ID
					VALUES
					(
					  @fileGuid					
					  ,@x
					  ,@y
					  ,@width
					  ,@height
					  ,@markupSetArtifactId
					  ,@markupType
					  ,@fillA
					  ,@fillR
					  ,@fillG
					  ,@fillB
					  ,@borderSize
					  ,@borderA
					  ,@borderR
					  ,@borderG
					  ,@borderB
					  ,@borderStyle
					  ,@fontName
					  ,@fontA
					  ,@fontR
					  ,@fontG
					  ,@fontB
					  ,@fontSize
					  ,@fontStyle
					  ,@text
					  ,@zOrder
					  ,@drawCrossLines
					  ,@markupSubType
					  ,@xD
					  ,@yD
					  ,@widthD
					  ,@heightD
					)
				  COMMIT ";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@fileGuid", SqlDbType.VarChar) {Value = fileGuid},
        new SqlParameter("@x", SqlDbType.Int) {Value = importWorkerQueueRecord.X},
        new SqlParameter("@y", SqlDbType.Int) {Value = importWorkerQueueRecord.Y},
        new SqlParameter("@width", SqlDbType.Int) {Value = importWorkerQueueRecord.Width},
        new SqlParameter("@height", SqlDbType.Int) {Value = importWorkerQueueRecord.Height},
        new SqlParameter("@markupSetArtifactId", SqlDbType.Int) {Value = markupSetArtifactId},
        new SqlParameter("@markupType", SqlDbType.SmallInt) {Value = importWorkerQueueRecord.MarkupType},
        new SqlParameter("@fillA", SqlDbType.SmallInt) {Value = (object) importWorkerQueueRecord.FillA ?? DBNull.Value},
        new SqlParameter("@fillR", SqlDbType.SmallInt) {Value = (object) importWorkerQueueRecord.FillR ?? DBNull.Value},
        new SqlParameter("@fillG", SqlDbType.SmallInt) {Value = (object) importWorkerQueueRecord.FillG ?? DBNull.Value},
        new SqlParameter("@fillB", SqlDbType.SmallInt) {Value = (object) importWorkerQueueRecord.FillB ?? DBNull.Value},
        new SqlParameter("@borderSize", SqlDbType.Int) {Value = (object) importWorkerQueueRecord.BorderSize ?? DBNull.Value},
        new SqlParameter("@borderA", SqlDbType.SmallInt) {Value = (object) importWorkerQueueRecord.BorderA ?? DBNull.Value},
        new SqlParameter("@borderR", SqlDbType.SmallInt) {Value = (object) importWorkerQueueRecord.BorderR ?? DBNull.Value},
        new SqlParameter("@borderG", SqlDbType.SmallInt) {Value = (object) importWorkerQueueRecord.BorderG ?? DBNull.Value},
        new SqlParameter("@borderB", SqlDbType.SmallInt) {Value = (object) importWorkerQueueRecord.BorderB ?? DBNull.Value},
        new SqlParameter("@borderStyle", SqlDbType.SmallInt) {Value = (object) importWorkerQueueRecord.BorderStyle ?? DBNull.Value},
        new SqlParameter("@fontName", SqlDbType.NVarChar) {Value = importWorkerQueueRecord.FontName},
        new SqlParameter("@fontA", SqlDbType.SmallInt) {Value = (object) importWorkerQueueRecord.FontA ?? DBNull.Value},
        new SqlParameter("@fontR", SqlDbType.SmallInt) {Value = (object) importWorkerQueueRecord.FontR ?? DBNull.Value},
        new SqlParameter("@fontG", SqlDbType.SmallInt) {Value = (object) importWorkerQueueRecord.FontG ?? DBNull.Value},
        new SqlParameter("@fontB", SqlDbType.SmallInt) {Value = (object) importWorkerQueueRecord.FontB ?? DBNull.Value},
        new SqlParameter("@fontSize", SqlDbType.Int) {Value = (object) importWorkerQueueRecord.FontSize ?? DBNull.Value},
        new SqlParameter("@fontStyle", SqlDbType.SmallInt) {Value = (object) importWorkerQueueRecord.FontStyle ?? DBNull.Value},
        new SqlParameter("@text", SqlDbType.NVarChar) {Value = importWorkerQueueRecord.Text},
        new SqlParameter("@zOrder", SqlDbType.Int) {Value = importWorkerQueueRecord.ZOrder},
        new SqlParameter("@drawCrossLines", SqlDbType.Bit) {Value = importWorkerQueueRecord.DrawCrossLines},
        new SqlParameter("@markupSubType", SqlDbType.SmallInt) {Value = importWorkerQueueRecord.MarkupSubType},
        new SqlParameter("@xD", SqlDbType.Decimal) {Value = (object) importWorkerQueueRecord.Xd ?? DBNull.Value},
        new SqlParameter("@yD", SqlDbType.Decimal) {Value = (object) importWorkerQueueRecord.Yd ?? DBNull.Value},
        new SqlParameter("@widthD", SqlDbType.Decimal) {Value = (object) importWorkerQueueRecord.WidthD ?? DBNull.Value},
        new SqlParameter("@heightD", SqlDbType.Decimal) {Value = (object) importWorkerQueueRecord.HeightD ?? DBNull.Value}
      };

      var dataTable = await Task.Run(() => workspaceDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));
      return (int)dataTable.Rows[0][0];
    }

    public async Task<bool> DoesRedactionExistAsync(IDBContext workspaceDbContext, string fileGuid, int markupSetArtifactId, ImportWorkerQueueRecord importWorkerQueueRecord)
    {
      var hasRedactions = false;

      const string sql = @"
				SELECT
					COUNT(0)
				FROM [EDDSDBO].[Redaction]
				WHERE 
					[FileGuid] = @fileGuid
					AND [X] = @x
					AND [Y] = @y
					AND [Width] = @width
					AND [Height] = @height
					AND [MarkupSetArtifactID] = @markupSetArtifactId
					AND [MarkupType] = @markupType
					AND ([FillA] IS NULL OR [FillA] = @fillA)
					AND ([FillR] IS NULL OR [FillR] = @fillR)
					AND ([FillG] IS NULL OR [FillG] = @fillG)
					AND ([FillB] IS NULL OR [FillB] = @fillB)
					AND ([BorderSize] IS NULL OR [BorderSize] = @borderSize)
					AND ([BorderA] IS NULL OR [BorderA] = @borderA)
					AND ([BorderR] IS NULL OR [BorderR] = @borderR)
					AND ([BorderG] IS NULL OR [BorderG] = @borderG)
					AND ([BorderB] IS NULL OR [BorderB] = @borderB)
					AND ([BorderStyle] IS NULL OR [BorderStyle] = @borderStyle)
					AND ([FontName] IS NULL OR [FontName] = @fontName)
					AND ([FontA] IS NULL OR [FontA] = @fontA)
					AND ([FontR] IS NULL OR [FontR] = @fontR)
					AND ([FontG] IS NULL OR [FontG] = @fontG)
					AND ([FontB] IS NULL OR [FontB] = @fontB)
					AND ([FontSize] IS NULL OR [FontSize] = @fontSize)
					AND ([FontStyle] IS NULL OR [FontStyle] = @fontStyle)
					AND ([Text] IS NULL OR [Text] = @text)
					AND [ZOrder] = @zOrder
					AND [DrawCrossLines] = @drawCrossLines
					AND [MarkupSubType] = @markupSubType
					AND [X_d] = @xD
					AND [Y_d] = @yD
					AND [Width_d] = @widthD
					AND [Height_d] = @heightD ";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@fileGuid", SqlDbType.VarChar) {Value = fileGuid},
        new SqlParameter("@x", SqlDbType.Int) {Value = importWorkerQueueRecord.X},
        new SqlParameter("@y", SqlDbType.Int) {Value = importWorkerQueueRecord.Y},
        new SqlParameter("@width", SqlDbType.Int) {Value = importWorkerQueueRecord.Width},
        new SqlParameter("@height", SqlDbType.Int) {Value = importWorkerQueueRecord.Height},
        new SqlParameter("@markupSetArtifactId", SqlDbType.Int) {Value = markupSetArtifactId},
        new SqlParameter("@markupType", SqlDbType.SmallInt) {Value = importWorkerQueueRecord.MarkupType},
        new SqlParameter("@fillA", SqlDbType.SmallInt) {Value = (object) importWorkerQueueRecord.FillA ?? DBNull.Value},
        new SqlParameter("@fillR", SqlDbType.SmallInt) {Value = (object) importWorkerQueueRecord.FillR ?? DBNull.Value},
        new SqlParameter("@fillG", SqlDbType.SmallInt) {Value = (object) importWorkerQueueRecord.FillG ?? DBNull.Value},
        new SqlParameter("@fillB", SqlDbType.SmallInt) {Value = (object) importWorkerQueueRecord.FillB ?? DBNull.Value},
        new SqlParameter("@borderSize", SqlDbType.Int) {Value = (object) importWorkerQueueRecord.BorderSize ?? DBNull.Value},
        new SqlParameter("@borderA", SqlDbType.SmallInt) {Value = (object) importWorkerQueueRecord.BorderA ?? DBNull.Value},
        new SqlParameter("@borderR", SqlDbType.SmallInt) {Value = (object) importWorkerQueueRecord.BorderR ?? DBNull.Value},
        new SqlParameter("@borderG", SqlDbType.SmallInt) {Value = (object) importWorkerQueueRecord.BorderG ?? DBNull.Value},
        new SqlParameter("@borderB", SqlDbType.SmallInt) {Value = (object) importWorkerQueueRecord.BorderB ?? DBNull.Value},
        new SqlParameter("@borderStyle", SqlDbType.SmallInt) {Value = (object) importWorkerQueueRecord.BorderStyle ?? DBNull.Value},
        new SqlParameter("@fontName", SqlDbType.NVarChar) {Value = importWorkerQueueRecord.FontName},
        new SqlParameter("@fontA", SqlDbType.SmallInt) {Value = (object) importWorkerQueueRecord.FontA ?? DBNull.Value},
        new SqlParameter("@fontR", SqlDbType.SmallInt) {Value = (object) importWorkerQueueRecord.FontR ?? DBNull.Value},
        new SqlParameter("@fontG", SqlDbType.SmallInt) {Value = (object) importWorkerQueueRecord.FontG ?? DBNull.Value},
        new SqlParameter("@fontB", SqlDbType.SmallInt) {Value = (object) importWorkerQueueRecord.FontB ?? DBNull.Value},
        new SqlParameter("@fontSize", SqlDbType.Int) {Value = (object) importWorkerQueueRecord.FontSize ?? DBNull.Value},
        new SqlParameter("@fontStyle", SqlDbType.SmallInt) {Value = (object) importWorkerQueueRecord.FontStyle ?? DBNull.Value},
        new SqlParameter("@text", SqlDbType.NVarChar) {Value = importWorkerQueueRecord.Text},
        new SqlParameter("@zOrder", SqlDbType.Int) {Value = importWorkerQueueRecord.ZOrder},
        new SqlParameter("@drawCrossLines", SqlDbType.Bit) {Value = importWorkerQueueRecord.DrawCrossLines},
        new SqlParameter("@markupSubType", SqlDbType.SmallInt) {Value = importWorkerQueueRecord.MarkupSubType},
        new SqlParameter("@xD", SqlDbType.Decimal) {Value = (object) importWorkerQueueRecord.Xd ?? DBNull.Value},
        new SqlParameter("@yD", SqlDbType.Decimal) {Value = (object) importWorkerQueueRecord.Yd ?? DBNull.Value},
        new SqlParameter("@widthD", SqlDbType.Decimal) {Value = (object) importWorkerQueueRecord.WidthD ?? DBNull.Value},
        new SqlParameter("@heightD", SqlDbType.Decimal) {Value = (object) importWorkerQueueRecord.HeightD ?? DBNull.Value}
      };

      var redactionCount = await Task.Run(() => workspaceDbContext.ExecuteSqlStatementAsScalar<int>(sql, sqlParams));

      if (redactionCount > 0)
      {
        hasRedactions = true;
      }

      return hasRedactions;
    }

    public async Task UpdateStatusInJobManagerQueueAsync(IDBContext eddsDbContext, int statusId, int id, string jobManagerQueue)
    {
      var sql = $@"UPDATE [EDDSDBO].[{jobManagerQueue}] SET QueueStatus = @statusId WHERE ID = @id";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@statusId", SqlDbType.Int) {Value = statusId},
        new SqlParameter("@id", SqlDbType.Int) {Value = id}
      };

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
    }

    public async Task UpdateStatusInImportManagerQueueAsync(IDBContext eddsDbContext, int statusId, int id)
    {
      var sql = $@"UPDATE [EDDSDBO].[{Constant.Tables.ImportManagerQueue}] SET QueueStatus = @statusId WHERE ID = @id";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@statusId", SqlDbType.Int) {Value = statusId},
        new SqlParameter("@id", SqlDbType.Int) {Value = id}
      };

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
    }

    public async Task UpdateStatusInExportWorkerQueueAsync(IDBContext eddsDbContext, int statusId, string uniqueTableName)
    {
      var sql = string.Format(@" 
				UPDATE S 
					SET QueueStatus = @statusId
				FROM [EDDSDBO].[{1}] B
					INNER JOIN [EDDSDBO].[{0}] S ON B.ID = S.ID", Constant.Tables.ExportWorkerQueue, uniqueTableName);

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@statusId", SqlDbType.Int) {Value = statusId}
      };

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
    }


    public async Task UpdateStatusInReproduceWorkerQueueAsync(IDBContext eddsDbContext, int statusId, string id)
    {
      var sql = $@" 
				UPDATE S 
				SET QueueStatus = @statusId
				INNER JOIN [EDDSDBO].[{Constant.Tables.ReproduceWorkerQueue}] S WHERE ID=@id";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@statusId", SqlDbType.Int) {Value = statusId},
        new SqlParameter("@id", SqlDbType.Int) {Value = id}
      };

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
    }

    public async Task UpdateStatusInImportWorkerQueueAsync(IDBContext eddsDbContext, int statusId, string uniqueTableName)
    {
      var sql = string.Format(@" 
			  UPDATE S 
				  SET QueueStatus = @statusId
			  FROM [EDDSDBO].[{1}] B
				  INNER JOIN [EDDSDBO].[{0}] S ON B.ID = S.ID", Constant.Tables.ImportWorkerQueue, uniqueTableName);

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@statusId", SqlDbType.Int) {Value = statusId}
      };

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
    }

    public async Task CreateHoldingTableAsync(IDBContext eddsDbContext, string tableName)
    {
      var sql = string.Format(@" 
			  IF NOT OBJECT_ID('[EDDSDBO].[{0}]') IS NULL BEGIN
				  DROP TABLE [EDDSDBO].[{0}]
			  END
			  CREATE TABLE [EDDSDBO].[{0}](DocumentArtifactID INT)
			  ", tableName);

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql));
    }

    public async Task CreateSavedSearchHoldingTableAsync(IDBContext eddsDbContext, string tableName, bool includeRelationalGroup)
    {
      var relationalGroup = "";

      if (includeRelationalGroup)
      {
        relationalGroup = ", RelationalGroup NVARCHAR(MAX)";
      }

      var sql = string.Format(@" 
				IF NOT OBJECT_ID('[EDDSDBO].[{0}]') IS NULL BEGIN
					DROP TABLE [EDDSDBO].[{0}]
				END
				CREATE TABLE [EDDSDBO].[{0}](ID INT NOT NULL IDENTITY(1,1) PRIMARY KEY, DocumentArtifactID INT{1})", tableName, relationalGroup);

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql));
    }

    public async Task CreateRedactionsHoldingTableAsync(IDBContext eddsDbContext, string tableName, bool includeRelationalGroup)
    {
      var relationalGroup = "";
      if (includeRelationalGroup)
      {
        relationalGroup = ", RelationalGroup NVARCHAR(MAX)";
      }
      var sql = string.Format(@" 
				IF NOT OBJECT_ID('[EDDSDBO].[{0}]') IS NULL BEGIN
					DROP TABLE [EDDSDBO].[{0}]
				END
				CREATE TABLE [EDDSDBO].[{0}](
	        [PageNumber] INT,
	        X INT, 
	        Y INT, 
	        Width INT, 
	        Height INT, 
	        MarkupSetArtifactID INT, 
	        MarkupType SmallInt, 
	        FillA SmallInt, 
	        FillR SmallInt, 
	        FillG SmallInt, 
	        FillB SmallInt, 
	        BorderSize INT, 
	        BorderA SmallInt, 
	        BorderR SmallInt, 
	        BorderG SmallInt, 
	        BorderB SmallInt, 
	        BorderStyle SmallInt, 
	        FontName NVARCHAR(500), 
	        FontA SmallInt, 
	        FontR SmallInt, 
	        FontG SmallInt, 
	        FontB SmallInt , 
	        FontSize INT, 
	        FontStyle SmallInt, 
	        [Text] NVARCHAR(MAX), 
	        ZOrder INT, 
	        DrawCrossLines BIT, 
	        MarkupSubType SmallInt{1},
            X_d decimal(14, 4) NULL,
	        Y_d decimal(14, 4) NULL,
	        Width_d decimal(14, 4) NULL,
	        Height_d decimal(14, 4) NULL
        )", tableName, relationalGroup);

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql));
    }

    public async Task<DataTable> RetrieveNextBatchInExportWorkerQueueAsync(IDBContext eddsDbContext, int agentId, int batchSize, string uniqueTableName, string commaDelimitedResourceAgentIds)
    {
      var sql = string.Format(@"
				BEGIN TRAN
					IF NOT OBJECT_ID('[EDDSDBO].[{1}]') IS NULL BEGIN
						DROP TABLE [EDDSDBO].[{1}]
					END
					CREATE TABLE [EDDSDBO].[{1}](ID INT)
					
					DECLARE @exportJobArtifactId INT 
					SET @exportJobArtifactId = 
					(
						SELECT TOP 1 ExportJobArtifactID
						FROM [EDDSDBO].[{0}]
						WHERE QueueStatus = @notStartedQueueStatus 
							AND ResourceGroupID IN ({2})
						ORDER BY 
							[TimeStampUTC] ASC
					)

					UPDATE [EDDSDBO].[{0}]
					SET AgentID = @agentId, 
						QueueStatus = @inProgressQueueStatus
					OUTPUT inserted.ID
					INTO [EDDSDBO].[{1}](ID) 
					FROM [EDDSDBO].[{0}] WITH(UPDLOCK,READPAST) 
					WHERE ID IN
							(
								SELECT TOP (@batchSize) ID
								FROM [EDDSDBO].[{0}] WITH(UPDLOCK,READPAST) 
								WHERE 
									ExportJobArtifactID = @exportJobArtifactId 
									AND QueueStatus = @notStartedQueueStatus
								ORDER BY 
									[TimeStampUTC] ASC 
							)		
				COMMIT

				SELECT
					S.ID ID
					,S.WorkspaceArtifactID
					,S.DocumentArtifactID
					,S.MarkupSetArtifactID
					,S.QueueStatus
					,S.AgentID
					,S.ExportJobArtifactID
					,S.MarkupSubType
					,S.ResourceGroupID
				FROM [EDDSDBO].[{1}] B
					INNER JOIN [EDDSDBO].[{0}] S ON B.ID = S.ID	", Constant.Tables.ExportWorkerQueue, uniqueTableName, commaDelimitedResourceAgentIds);

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@agentId", SqlDbType.Int) {Value = agentId},
        new SqlParameter("@batchSize", SqlDbType.Int) {Value = batchSize},
        new SqlParameter("@notStartedQueueStatus", SqlDbType.Int) {Value = Constant.Status.Queue.NotStarted},
        new SqlParameter("@inProgressQueueStatus", SqlDbType.Int) {Value = Constant.Status.Queue.InProgress}
      };

      return await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));
    }

    public async Task<DataTable> RetrieveRedactionInfoAsync(IDBContext dbContext, int redactionId)
    {
      const string sql = @"
				SELECT R.*, F.DocumentArtifactID, F.[Order], F.[Identifier]				
				FROM 
					[EDDSDBO].[File] F WITH(NOLOCK)
					INNER JOIN [EDDSDBO].[Redaction] R WITH(NOLOCK) ON F.Guid = R.FileGuid
				WHERE 
					R.ID = @redactionId 
					AND F.[Type] = 1";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@redactionId", SqlDbType.Int) {Value = redactionId},
      };

      return await Task.Run(() => dbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));
    }

    public async Task<DataTable> RetrieveRedactionsForDocumentAsync(IDBContext dbContext, int workspaceArtifactId, int exportJobArtifactId, int documentArtifactId, int markupSetArtifactId, string documentIdentifierColumnName, string markupSubTypes)
    {
      var sql = $@"
				SELECT 
					GETUTCDATE() [TimeStampUTC],
					@workspaceArtifactId [WorkspaceArtifactID],
					@exportJobArtifactId [ExportJobArtifactID], 
					D.[{documentIdentifierColumnName}] [DocumentIdentifier], 
					F.[Order] [FileOrder],
					R.X,
					R.Y, 
					R.Width,
					R.Height,
					R.MarkupSetArtifactID,
					R.MarkupType,
					R.FillA,
					R.FillR,
					R.FillG,
					R.FillB,
					R.BorderSize,
					R.BorderA,
					R.BorderR,
					R.BorderG,
					R.BorderB,
					R.BorderStyle,
					R.FontName,
					R.FontA,
					R.FontR,
					R.FontG,
					R.FontB,
					R.FontSize,
					R.FontStyle,
					R.[Text],
					R.ZOrder,
					R.DrawCrossLines,
					R.MarkupSubType,
					R.X_d,
					R.Y_d, 
					R.Width_d,
					R.Height_d
				FROM 
					[EDDSDBO].[File] F WITH(NOLOCK)
					INNER JOIN [EDDSDBO].[Redaction] R WITH(NOLOCK) ON F.Guid = R.FileGuid
					INNER JOIN [EDDSDBO].[Document] D WITH(NOLOCK) ON F.DocumentArtifactID = D.ArtifactID
				WHERE 
					F.DocumentArtifactID = @documentArtifactId 
					AND R.MarkupSetArtifactID = @markupSetArtifactId
					AND F.[Type] = 1
					AND R.MarkupSubType IN ({markupSubTypes})";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@workspaceArtifactId", SqlDbType.Int) {Value = workspaceArtifactId},
        new SqlParameter("@exportJobArtifactId", SqlDbType.Int) {Value = exportJobArtifactId},
        new SqlParameter("@documentIdentifierColumnName", SqlDbType.NVarChar) {Value = documentIdentifierColumnName},
        new SqlParameter("@documentArtifactId", SqlDbType.Int) {Value = documentArtifactId},
        new SqlParameter("@markupSetArtifactId", SqlDbType.Int) {Value = markupSetArtifactId},
        new SqlParameter("@markupSubTypes", SqlDbType.NVarChar) {Value = markupSubTypes}
      };

      return await Task.Run(() => dbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));
    }

    public async Task<DataTable> RetrieveNextBatchInImportWorkerQueueAsync(IDBContext eddsDbContext, int agentId, int batchSize, string uniqueTableName, string commaDelimitedResourceAgentIds)
    {
      string sql = $@" 
				BEGIN TRAN
					IF NOT OBJECT_ID('[EDDSDBO].[{uniqueTableName}]') IS NULL BEGIN
						DROP TABLE [EDDSDBO].[{uniqueTableName}]
					END
					CREATE TABLE [EDDSDBO].[{uniqueTableName}](ID INT)
					
					DECLARE @WorkspaceArtifactId INT 
					DECLARE @ImportJobArtifactId INT 

					SELECT TOP 1  
						@WorkspaceArtifactId = [WorkspaceArtifactId],
						@ImportJobArtifactId = [ImportJobArtifactId]
					FROM [EDDSDBO].[{Constant.Tables.ImportWorkerQueue}]
					WHERE [QueueStatus] = @notStartedQueueStatus 
						AND [ResourceGroupID] IN ({commaDelimitedResourceAgentIds})
					ORDER BY 
						[TimeStampUTC] ASC

					UPDATE [EDDSDBO].[{Constant.Tables.ImportWorkerQueue}]
					SET [AgentID] = @agentId, 
							[QueueStatus] = @inProgressQueueStatus
					OUTPUT inserted.[ID]
					INTO [EDDSDBO].[{uniqueTableName}]([ID]) 
					FROM [EDDSDBO].[{Constant.Tables.ImportWorkerQueue}] WITH(UPDLOCK,READPAST) 
					WHERE [ID] IN
							(
								SELECT TOP (@batchSize) [ID]
								FROM [EDDSDBO].[{Constant.Tables.ImportWorkerQueue}] WITH(UPDLOCK,READPAST) 
								WHERE 
									[WorkspaceArtifactId] = @WorkspaceArtifactId AND [ImportJobArtifactId] = @ImportJobArtifactId AND QueueStatus = @notStartedQueueStatus
								ORDER BY 
									[TimeStampUTC] ASC 
							)		
				COMMIT

				SELECT
					S.[ID]
					,S.[TimeStampUTC]
					,S.[WorkspaceArtifactID]
					,S.[DocumentIdentifier]
					,S.[FileOrder]
					,S.[QueueStatus]
					,S.[AgentID]
					,S.[ImportJobArtifactID]
					,S.[JobType]
					,S.[X]
					,S.[Y]
					,S.[Width]
					,S.[Height]
					,S.[MarkupSetArtifactID]
					,S.[MarkupType]
					,S.[FillA]
					,S.[FillR]
					,S.[FillG]
					,S.[FillB]
					,S.[BorderSize]
					,S.[BorderA]
					,S.[BorderR]
					,S.[BorderG]
					,S.[BorderB]
					,S.[BorderStyle]
					,S.[FontName]
					,S.[FontA]
					,S.[FontR]
					,S.[FontG]
					,S.[FontB]
					,S.[FontSize]
					,S.[FontStyle]
					,S.[Text]
					,S.[ZOrder]
					,S.[DrawCrossLines]
					,S.[MarkupSubType]
					,S.[ResourceGroupID]
					,S.[SkipDuplicateRedactions]
					,S.[X_d]
					,S.[Y_d]
					,S.[Width_d]
					,S.[Height_d]
				FROM 
					[EDDSDBO].[{uniqueTableName}] B
					INNER JOIN [EDDSDBO].[{Constant.Tables.ImportWorkerQueue}] S ON B.ID = S.ID
				";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@agentId", SqlDbType.Int) {Value = agentId},
        new SqlParameter("@batchSize", SqlDbType.Int) {Value = batchSize},
        new SqlParameter("@notStartedQueueStatus", SqlDbType.Int) {Value = Constant.Status.Queue.NotStarted},
        new SqlParameter("@inProgressQueueStatus", SqlDbType.Int) {Value = Constant.Status.Queue.InProgress}
      };

      return await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));
    }

    public async Task RemoveRecordFromReproduceWorkerQueueAsync(IDBContext eddsDbContext, int id)
    {
      var sql = $@"DELETE [EDDSDBO].[{Constant.Tables.ReproduceWorkerQueue}] WHERE ID = @id";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@id", SqlDbType.Int) {Value = id},
      };

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
    }

    public async Task RemoveBatchFromExportWorkerQueueAsync(IDBContext eddsDbContext, string uniqueTableName)
    {
      var sql = string.Format(@"  
				DELETE [EDDSDBO].[{0}]
				FROM [EDDSDBO].[{0}] S
					INNER JOIN [EDDSDBO].[{1}] B ON B.ID = S.ID	", Constant.Tables.ExportWorkerQueue, uniqueTableName);

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql));
    }

    public async Task RemoveBatchFromImportWorkerQueueAsync(IDBContext eddsDbContext, string uniqueTableName)
    {
      var sql = string.Format(@"  
				DELETE [EDDSDBO].[{0}]
				FROM [EDDSDBO].[{0}] S
					INNER JOIN [EDDSDBO].[{1}] B ON B.ID = S.ID	", Constant.Tables.ImportWorkerQueue, uniqueTableName);

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql));
    }

    public async Task DropTableAsync(IDBContext dbContext, string tableName)
    {
      var sql = string.Format(@"  
				IF NOT OBJECT_ID('[EDDSDBO].[{0}]') IS NULL 
					BEGIN DROP TABLE [EDDSDBO].[{0}]
				END", tableName);

      await Task.Run(() => dbContext.ExecuteNonQuerySQLStatement(sql));
    }

    public async Task<DataTable> RetrieveMinMaxIdAsync(IDBContext dbContext, string tableName)
    {
      var sql = $@"
        SELECT 
          MIN(ID), 
          MAX(ID) 
        FROM [EDDSDBO].[{tableName}]";
      return await Task.Run(() => dbContext.ExecuteSqlStatementAsDataTable(sql));
    }


    public async Task<DataTable> RetrieveRelationalGroupsTask(IDBContext dbContext, string tableName)
    {
      var sql = $@"
        SELECT 
          DISTINCT D.[RelationalGroup] 
        FROM EDDSDBO.[{tableName}] D (NOLOCK) 
        WHERE [RelationalGroup] IS NOT NULL 
        GROUP BY D.[RelationalGroup] 
        HAVING COUNT(*)>1;";

      return await Task.Run(() => dbContext.ExecuteSqlStatementAsDataTable(sql));
    }

    public async Task<DataTable> RetrieveAllInExportManagerQueueAsync(IDBContext dbContext)
    {
      var sql = $@" 
				DECLARE @offset INT SET @offset = (SELECT DATEDIFF(HOUR,GetUTCDate(),GetDate()))

				SELECT 
					Q.[ID]
					,DATEADD(HOUR,@offset,Q.[TimeStampUTC]) [Added On]
					,Q.WorkspaceArtifactID [Workspace Artifact ID]
					,C.Name [Workspace Name]
					,CASE Q.[QueueStatus]	
						WHEN @notStartedStatusId THEN 'Waiting'
						WHEN @inProgressStatusId THEN 'In Progress'
						WHEN @errorStatusId THEN 'Error'
						END [Status]
					,Q.AgentID [Agent Artifact ID]
					,Q.[Priority]
					,U.LastName + ', ' + U.FirstName [Added By]
					,Q.RecordArtifactID [Record Artifact ID]
				FROM [EDDSDBO].[{Constant.Tables.ExportManagerQueue}] Q
					INNER JOIN EDDS.[EDDSDBO].ExtendedCase C ON Q.WorkspaceArtifactID = C.ArtifactID
					LEFT JOIN EDDS.[EDDSDBO].[User] U ON Q.UserID = U.ArtifactID
				ORDER BY 
					Q.[Priority] ASC
					,Q.[TimeStampUTC] ASC";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@notStartedStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.NotStarted},
        new SqlParameter("@inProgressStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.InProgress},
        new SqlParameter("@errorStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.Error}
      };

      return await Task.Run(() => dbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));
    }

    public async Task<DataTable> RetrieveAllInImportManagerQueueAsync(IDBContext dbContext)
    {
      var sql = $@" 
				DECLARE @offset INT SET @offset = (SELECT DATEDIFF(HOUR,GetUTCDate(),GetDate()))

				SELECT 
					Q.[ID]
					,DATEADD(HOUR,@offset,Q.[TimeStampUTC]) [Added On]
					,Q.WorkspaceArtifactID [Workspace Artifact ID]
					,C.Name [Workspace Name]
					,CASE Q.[QueueStatus]	
						WHEN @notStartedStatusId THEN 'Waiting'
						WHEN @inProgressStatusId THEN 'In Progress'
						WHEN @errorStatusId THEN 'Error'
						END [Status]
					,Q.AgentID [Agent Artifact ID]
					,Q.[Priority]
					,U.LastName + ', ' + U.FirstName [Added By]
					,Q.RecordArtifactID [Record Artifact ID]
				FROM [EDDSDBO].[{Constant.Tables.ImportManagerQueue}] Q
					INNER JOIN EDDS.[EDDSDBO].ExtendedCase C ON Q.WorkspaceArtifactID = C.ArtifactID
					LEFT JOIN EDDS.[EDDSDBO].[User] U ON Q.UserID = U.ArtifactID
				ORDER BY 
					Q.[Priority] ASC
					,Q.[TimeStampUTC] ASC";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@notStartedStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.NotStarted},
        new SqlParameter("@inProgressStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.InProgress},
        new SqlParameter("@errorStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.Error}
      };

      return await Task.Run(() => dbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));
    }

    public async Task<DataRow> RetrieveSingleInJobManagerQueueByArtifactIdAsync(IDBContext dbContext, int artifactId, int workspaceArtifactId, string jobManagerQueueTable)
    {
      var sql = $@" 
				DECLARE @offset INT SET @offset = (SELECT DATEDIFF(HOUR,GetUTCDate(),GetDate()))

				SELECT 
					Q.[ID]
					,DATEADD(HOUR,@offset,Q.[TimeStampUTC]) [Added On]
					,Q.WorkspaceArtifactID [Workspace Artifact ID]
					,C.Name [Workspace Name]
					,CASE Q.[QueueStatus]	
						WHEN @notStartedStatusId THEN 'Waiting'
						WHEN @inProgressStatusId THEN 'In Progress'
						WHEN @errorStatusId THEN 'Error'
						END [Status]
					,Q.AgentID [Agent Artifact ID]
					,U.LastName + ', ' + U.FirstName [Added By]
					,Q.ID [Record Artifact ID]
				FROM [EDDSDBO].[{jobManagerQueueTable}] Q
					INNER JOIN EDDS.[EDDSDBO].ExtendedCase C ON Q.WorkspaceArtifactID = C.ArtifactID
					LEFT JOIN EDDS.[EDDSDBO].[User] U ON Q.CreatedBy = U.ArtifactID
				WHERE Q.ID = @artifactId
					AND Q.WorkspaceArtifactID = @workspaceArtifactId";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@notStartedStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.NotStarted},
        new SqlParameter("@inProgressStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.InProgress},
        new SqlParameter("@errorStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.Error},
        new SqlParameter("@artifactId", SqlDbType.Int) {Value = artifactId},
        new SqlParameter("@workspaceArtifactId", SqlDbType.Int) {Value = workspaceArtifactId}
      };

      var dt = await Task.Run(() => dbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));
      return dt.Rows.Count > 0 ? dt.Rows[0] : null;
    }

    public async Task<DataRow> RetrieveSingleInImportManagerQueueByArtifactIdAsync(IDBContext dbContext, int artifactId, int workspaceArtifactId)
    {
      var sql = $@" 
				DECLARE @offset INT SET @offset = (SELECT DATEDIFF(HOUR,GetUTCDate(),GetDate()))

				SELECT 
					Q.[ID]
					,DATEADD(HOUR,@offset,Q.[TimeStampUTC]) [Added On]
					,Q.WorkspaceArtifactID [Workspace Artifact ID]
					,C.Name [Workspace Name]
					,CASE Q.[QueueStatus]	
						WHEN @notStartedStatusId THEN 'Waiting'
						WHEN @inProgressStatusId THEN 'In Progress'
						WHEN @errorStatusId THEN 'Error'
						END [Status]
					,Q.AgentID [Agent Artifact ID]
					,U.LastName + ', ' + U.FirstName [Added By]
					,Q.ImportJobArtifactID [Record Artifact ID]
				FROM [EDDSDBO].[{Constant.Tables.ImportManagerQueue}] Q
					INNER JOIN EDDS.[EDDSDBO].ExtendedCase C ON Q.WorkspaceArtifactID = C.ArtifactID
					LEFT JOIN EDDS.[EDDSDBO].[User] U ON Q.CreatedBy = U.ArtifactID
				WHERE Q.ID = @artifactId
					AND Q.WorkspaceArtifactID = @workspaceArtifactId";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@notStartedStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.NotStarted},
        new SqlParameter("@inProgressStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.InProgress},
        new SqlParameter("@errorStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.Error},
        new SqlParameter("@artifactId", SqlDbType.Int) {Value = artifactId},
        new SqlParameter("@workspaceArtifactId", SqlDbType.Int) {Value = workspaceArtifactId}
      };

      var dt = await Task.Run(() => dbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));
      return dt.Rows.Count > 0 ? dt.Rows[0] : null;
    }

    public async Task<DataTable> RetrieveAllInExportWorkerQueueAsync(IDBContext dbContext)
    {
      var sql = $@" 
				DECLARE @offset INT SET @offset = (SELECT DATEDIFF(HOUR,GetUTCDate(),GetDate()))

				SELECT 
					Q.JobID [ID]
					,DATEADD(HOUR,@offset,Q.[TimeStampUTC]) [Added On]
					,Q.WorkspaceArtifactID [Workspace Artifact ID]
					,C.Name [Workspace Name]
					,CASE Q.[QueueStatus]	
						WHEN @notStartedStatusId THEN 'Waiting'
						WHEN @inProgressStatusId THEN 'In Progress'
						WHEN @errorStatusId THEN 'Error'
						END [Status]
					,Q.AgentID [Agent Artifact ID]
					,Q.[Priority]
					,COUNT(Q.[ID]) [# Records Remaining]
					,Q.ParentRecordArtifactID [Parent Record Artifact ID]
				FROM [EDDSDBO].[{Constant.Tables.ExportWorkerQueue}] Q
					INNER JOIN EDDS.[EDDSDBO].ExtendedCase C ON Q.WorkspaceArtifactID = C.ArtifactID
				GROUP BY 
					Q.JobID
					,Q.[TimeStampUTC]
					,C.Name
					,Q.WorkspaceArtifactID
					,Q.[QueueStatus]
					,Q.AgentID
					,Q.[Priority]
					,Q.ParentRecordArtifactID
				ORDER BY 
					Q.[Priority] ASC
					,Q.[TimeStampUTC] ASC
					,Q.JobID ASC
					,Q.[QueueStatus] DESC";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@notStartedStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.NotStarted},
        new SqlParameter("@inProgressStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.InProgress},
        new SqlParameter("@errorStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.Error}
      };

      return await Task.Run(() => dbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));
    }

    public async Task<DataTable> RetrieveAllInImportWorkerQueueAsync(IDBContext dbContext)
    {
      var sql = $@" 
				DECLARE @offset INT SET @offset = (SELECT DATEDIFF(HOUR,GetUTCDate(),GetDate()))

				SELECT 
					Q.JobID [ID]
					,DATEADD(HOUR,@offset,Q.[TimeStampUTC]) [Added On]
					,Q.WorkspaceArtifactID [Workspace Artifact ID]
					,C.Name [Workspace Name]
					,CASE Q.[QueueStatus]	
						WHEN @notStartedStatusId THEN 'Waiting'
						WHEN @inProgressStatusId THEN 'In Progress'
						WHEN @errorStatusId THEN 'Error'
						END [Status]
					,Q.AgentID [Agent Artifact ID]
					,Q.[Priority]
					,COUNT(Q.[ID]) [# Records Remaining]
					,Q.ParentRecordArtifactID [Parent Record Artifact ID]
				FROM [EDDSDBO].[{Constant.Tables.ImportWorkerQueue}] Q
					INNER JOIN EDDS.[EDDSDBO].ExtendedCase C ON Q.WorkspaceArtifactID = C.ArtifactID
				GROUP BY 
					Q.JobID
					,Q.[TimeStampUTC]
					,C.Name
					,Q.WorkspaceArtifactID
					,Q.[QueueStatus]
					,Q.AgentID
					,Q.[Priority]
					,Q.ParentRecordArtifactID
				ORDER BY 
					Q.[Priority] ASC
					,Q.[TimeStampUTC] ASC
					,Q.JobID ASC
					,Q.[QueueStatus] DESC";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@notStartedStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.NotStarted},
        new SqlParameter("@inProgressStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.InProgress},
        new SqlParameter("@errorStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.Error}
      };

      return await Task.Run(() => dbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));
    }

    public async Task InsertRowIntoExportManagerQueueAsync(IDBContext eddsDbContext, int workspaceArtifactId, int priority, int userId, int artifactId, int resourceGroupId)
    {
      var sql = $@" 
			INSERT INTO [EDDSDBO].[{Constant.Tables.ExportManagerQueue}]
			(
				[TimeStampUTC]
				,WorkspaceArtifactID
				,QueueStatus
				,AgentID
				,[Priority]
				,[UserID]
				,RecordArtifactID
				,ResourceGroupID
			)
			VALUES 
			(
				GetUTCDate()
				,@workspaceArtifactId
				,@queueStatus
				,NULL
				,@priority
				,@userID
				,@artifactID
				,@resourceGroupID
			)";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@workspaceArtifactId", SqlDbType.Int) {Value = workspaceArtifactId},
        new SqlParameter("@queueStatus", SqlDbType.VarChar) {Value = Constant.Status.Queue.NotStarted},
        new SqlParameter("@priority", SqlDbType.Int) {Value = priority},
        new SqlParameter("@userID", SqlDbType.Int) {Value = userId},
        new SqlParameter("@artifactID", SqlDbType.Int) {Value = artifactId},
        new SqlParameter("@resourceGroupID", SqlDbType.Int) {Value = resourceGroupId}
      };

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
    }

    public async Task InsertRowIntoImportManagerQueueAsync(IDBContext eddsDbContext, int workspaceArtifactId, int priority, int userId, int artifactId, int resourceGroupId)
    {
      var sql = $@" 
			INSERT INTO [EDDSDBO].[{Constant.Tables.ImportManagerQueue}]
			(
				[TimeStampUTC]
				,WorkspaceArtifactID
				,QueueStatus
				,AgentID
				,[Priority]
				,[UserID]
				,RecordArtifactID
				,ResourceGroupID
			)
			VALUES 
			(
				GetUTCDate()
				,@workspaceArtifactId
				,@queueStatus
				,NULL
				,@priority
				,@userID
				,@artifactID
				,@resourceGroupID
			)";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@workspaceArtifactId", SqlDbType.Int) {Value = workspaceArtifactId},
        new SqlParameter("@queueStatus", SqlDbType.VarChar) {Value = Constant.Status.Queue.NotStarted},
        new SqlParameter("@priority", SqlDbType.Int) {Value = priority},
        new SqlParameter("@userID", SqlDbType.Int) {Value = userId},
        new SqlParameter("@artifactID", SqlDbType.Int) {Value = artifactId},
        new SqlParameter("@resourceGroupID", SqlDbType.Int) {Value = resourceGroupId}
      };

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
    }

    public async Task<DataTable> RetrieveOffHoursAsync(IDBContext eddsDbContext)
    {
      const string sql = @"
				DECLARE @OffHourStart VARCHAR(100)
				DECLARE @OffHourEndTime VARCHAR(100)

				SET @OffHourStart = (SELECT [VALUE] FROM [EDDS].[eddsdbo].[Configuration] WITH(NOLOCK) WHERE [SECTION] = 'kCura.EDDS.Agents' AND [NAME] = 'AgentOffHourStartTime')
				SET @OffHourEndTime = (SELECT [VALUE] FROM [EDDS].[eddsdbo].[Configuration] WITH(NOLOCK) WHERE [SECTION] = 'kCura.EDDS.Agents' AND [NAME] = 'AgentOffHourEndTime')

				SELECT
					@OffHourStart AS [AgentOffHourStartTime],
					@OffHourEndTime AS [AgentOffHourEndTime]
				";

      var dt = await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsDataTable(sql));
      return dt.Rows.Count > 0 ? dt : null;
    }

    public async Task InsertImportJobToImportManagerQueueAsync(IDBContext eddsDbContext, int workspaceArtifactId, int importJobArtifactId, int userId, int statusQueue, string importJobType, int resourceGroupId)
    {
      var sql = $@" 
			INSERT INTO [EDDSDBO].[{Constant.Tables.ImportManagerQueue}]
			(
				[TimeStampUTC]
				,WorkspaceArtifactID
				,QueueStatus
				,AgentID
				,ImportJobArtifactID
				,JobType
				,CreatedBy
				,CreatedOn
				,ResourceGroupID
			)
			VALUES 
			(
				GetUTCDate()
				,@workspaceArtifactId
				,@queueStatus
				,NULL
				,@artifactID
				,@jobType
				,@userID
				,GetUTCDate()
				,@resourceGroupID
			)";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@workspaceArtifactId", SqlDbType.Int) {Value = workspaceArtifactId},
        new SqlParameter("@queueStatus", SqlDbType.VarChar) {Value = statusQueue},
        new SqlParameter("@artifactID", SqlDbType.Int) {Value = importJobArtifactId},
        new SqlParameter("@jobType", SqlDbType.NVarChar) {Value = importJobType},
        new SqlParameter("@userID", SqlDbType.Int) {Value = userId},
        new SqlParameter("@resourceGroupID", SqlDbType.Int) {Value = resourceGroupId}
      };

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
    }

    public async Task InsertJobToManagerQueueAsync(IDBContext eddsDbContext, int workspaceArtifactId, int jobArtifactId, int userId, int statusQueue, int resourceGroupId, string jobManagerQueueTable, string jobArtifactIdColumnName)
    {
      var sql = $@" 
			  INSERT INTO [EDDSDBO].[{jobManagerQueueTable}]
			  (
				  [TimeStampUTC]
				  ,WorkspaceArtifactID
				  ,QueueStatus
				  ,AgentID
				  ,{jobArtifactIdColumnName}
				  ,CreatedBy
				  ,CreatedOn
				  ,ResourceGroupID
			  )
			  VALUES 
			  (
				  GetUTCDate()
				  ,@workspaceArtifactId
				  ,@queueStatus
				  ,NULL
				  ,@artifactID
				  ,@userID
				  ,GetUTCDate()
				  ,@resourceGroupID
			  )";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@workspaceArtifactId", SqlDbType.Int) {Value = workspaceArtifactId},
        new SqlParameter("@queueStatus", SqlDbType.VarChar) {Value = statusQueue},
        new SqlParameter("@artifactID", SqlDbType.Int) {Value = jobArtifactId},
        new SqlParameter("@userID", SqlDbType.Int) {Value = userId},
        new SqlParameter("@resourceGroupID", SqlDbType.Int) {Value = resourceGroupId}
      };

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
    }

    public async Task<DataTable> RetrieveMarkupTypesAsync(IDBContext workspaceDbContext)
    {
      const string sql = @"
				SELECT 
					[ID], 
					[Type] 
				FROM 
					[EDDSDBO].[RedactionMarkupType] WITH(NOLOCK)";

      var dataTable = await Task.Run(() => workspaceDbContext.ExecuteSqlStatementAsDataTable(sql));
      return dataTable.Rows.Count > 0 ? dataTable : null;
    }

    public async Task<DataTable> RetrieveMarkupSubTypesAsync(IDBContext workspaceDbContext)
    {
      const string sql = @"
				SELECT 
					[ID], 
					[SubType] 
				FROM 
					[EDDSDBO].[RedactionMarkupSubType] WITH(NOLOCK)";

      var dataTable = await Task.Run(() => workspaceDbContext.ExecuteSqlStatementAsDataTable(sql));
      return dataTable.Rows.Count > 0 ? dataTable : null;
    }

    public async Task<string> GetDocumentIdentifierFieldNameAsync(IDBContext workspaceDbContext)
    {
      const string sql = @"
				SELECT [TextIdentifier] FROM [EDDSDBO].[ExtendedField] WITH(NOLOCK)	
				WHERE [IsIdentifier] = 1 AND [FieldArtifactTypeID] = 10";

      var dataTable = await Task.Run(() => workspaceDbContext.ExecuteSqlStatementAsDataTable(sql));

      if (dataTable.Rows.Count != 1)
      {
        throw new MarkupUtilityException("An error occured when querying for document identifier field name.");
      }

      return dataTable.Rows[0][0].ToString().Trim();
    }

    public async Task<string> GetFileGuidForDocumentAsync(IDBContext workspaceDbContext, int documentArtifactId, int fileOrder)
    {
      const string sql = @"
				SELECT [Guid] FROM [EDDSDBO].[File] WITH(NOLOCK)	
				WHERE [Type] = @fileType AND [DocumentArtifactID] = @documentArtifactId AND [Order] = @fileOrder";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@fileType", SqlDbType.Int) {Value = 1}, // 1 is image file type
				new SqlParameter("@documentArtifactId", SqlDbType.Int) {Value = documentArtifactId},
        new SqlParameter("@fileOrder", SqlDbType.Int) {Value = fileOrder}
      };

      var dataTable = await Task.Run(() => workspaceDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));

      if (dataTable.Rows.Count != 1)
      {
        throw new MarkupUtilityException("An error occured when querying for the file guid for the document.");
      }

      return dataTable.Rows[0][0].ToString().Trim();
    }

    public async Task<bool> DoesDocumentHasImagesAsync(IDBContext workspaceDbContext, int documentArtifactId, int fileOrder)
    {
      var hasImages = false;

      const string sql = @"
				SELECT COUNT(0) FROM [EDDSDBO].[File] WITH(NOLOCK)	
				WHERE [Type] = @fileType AND [DocumentArtifactID] = @documentArtifactId AND [Order] = @fileOrder";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@fileType", SqlDbType.Int) {Value = 1}, // 1 is image file type
				new SqlParameter("@documentArtifactId", SqlDbType.Int) {Value = documentArtifactId},
        new SqlParameter("@fileOrder", SqlDbType.Int) {Value = fileOrder}
      };

      var imageCount = await Task.Run(() => workspaceDbContext.ExecuteSqlStatementAsScalar<int>(sql, sqlParams));

      if (imageCount == 1)
      {
        hasImages = true;
      }

      if (imageCount > 1)
      {
        throw new MarkupUtilityException("More than one image exists for the document.");
      }

      return hasImages;
    }

    public async Task CopyRecordsToExportWorkerQueueAsync(IDBContext eddsDbContext, string batchTableName, int workspaceArtifactId, int markupSetArtifactId, int exportJobArtifactId, string markupSubTypes, int resourceGroupArtifactId)
    {
      var sql = $@" 
			INSERT INTO [EDDSDBO].{Constant.Tables.ExportWorkerQueue}
			(
				[TimeStampUTC]
				,WorkspaceArtifactID
				,DocumentArtifactID
				,MarkupSetArtifactID
				,QueueStatus
				,AgentID
				,ExportJobArtifactID
				,MarkupSubType
				,ResourceGroupID
			)
			SELECT 
				GetUTCDate()
				,@workspaceArtifactId
				,DocumentArtifactID
				,@markupSetArtifactId
				,@queueStatus
				,NULL
				,@exportJobArtifactID
				,@markupSubType
				,@resourceGroupID
			FROM [EDDSDBO].[{batchTableName}]";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@workspaceArtifactId", SqlDbType.Int) {Value = workspaceArtifactId},
        new SqlParameter("@queueStatus", SqlDbType.VarChar) {Value = Constant.Status.Queue.NotStarted},
        new SqlParameter("@markupSetArtifactId", SqlDbType.VarChar) {Value = markupSetArtifactId},
        new SqlParameter("@exportJobArtifactID", SqlDbType.Int) {Value = exportJobArtifactId},
        new SqlParameter("@markupSubType", SqlDbType.NVarChar) {Value = markupSubTypes},
        new SqlParameter("@resourceGroupID", SqlDbType.Int) {Value = resourceGroupArtifactId}
      };

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
    }

    public async Task<DataTable> BulkInsertRedactionRecordsForRelationalGroup(IDBContext eddsDbContext, string savedSearchTableName, string redactionSetTableName, string relationalGroup)
    {
      var sql = string.Format(@"
        DECLARE @output TABLE (ID int)
 
			  INSERT INTO [EDDSDBO].Redaction
	      (
		      FileGuid, 
		      X, 
		      Y, 
		      Width, 
		      Height, 
		      MarkupSetArtifactID, 
		      MarkupType, 
		      FillA, 
		      FillR, 
		      FillG, 
		      FillB, 
		      BorderSize, 
		      BorderA, 
		      BorderR, 
		      BorderG, 
		      BorderB, 
		      BorderStyle, 
		      FontName, 
		      FontA, 
		      FontR, 
		      FontG, 
		      FontB, 
		      FontSize, 
		      FontStyle, 
		      [Text], 
		      ZOrder, 
		      DrawCrossLines, 
		      MarkupSubType,
		      X_d, 
		      Y_d, 
		      Width_d, 
		      Height_d
	      )

        OUTPUT INSERTED.ID INTO @output

	      SELECT DISTINCT
		      F.[Guid], 
		      R.X, 
		      R.Y, 
		      R.Width, 
		      R.Height, 
		      R.MarkupSetArtifactID,
		      R.MarkupType, 
		      R.FillA, 
		      R.FillR, 
		      R.FillG, 
		      R.FillB, 
		      R.BorderSize, 
		      R.BorderA, 
		      R.BorderR, 
		      R.BorderG, 
		      R.BorderB, 
		      R.BorderStyle, 
		      R.FontName, 
		      R.FontA, 
		      R.FontR, 
		      R.FontG, 
		      R.FontB, 
		      R.FontSize, 
		      R.FontStyle, 
		      R.[Text], 
		      R.ZOrder, 
		      R.DrawCrossLines, 
		      R.MarkupSubType,
		      R.X_d, 
		      R.Y_d, 
		      R.Width_d, 
		      R.Height_d
	      FROM 
          [EDDSDBO].[{1}] R,
          [EDDSDBO].[File] F (NOLOCK)
		        INNER JOIN [EDDSDBO].[{0}] K ON F.DocumentArtifactID = K.DocumentArtifactID
	      WHERE 
          F.[Type] = 1 
          AND K.RelationalGroup=@relationalGroup 
          AND R.RelationalGroup=@relationalGroup 

        SELECT * from @output", savedSearchTableName, redactionSetTableName);

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@relationalGroup", SqlDbType.NVarChar) {Value = relationalGroup},
      };

      return await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));
    }

    public async Task<DataTable> BulkUpdateHasAutoRedactionsFieldForRelationalGroup(IDBContext eddsDbContext, string savedSearchTableName, string redactionSetTableName, string relationalGroup, string hasAutoReddactionsColumn)
    {
      var sql = string.Format(@"        
			  UPDATE [EDDSDBO].[Document]
        SET [{2}]=1
        WHERE [ArtifactID] in 
	        (SELECT 
		        DISTINCT K.DocumentArtifactID
	        FROM 
            [EDDSDBO].[{1}] R (NOLOCK),
            [EDDSDBO].[File] F (NOLOCK)
		        INNER JOIN [EDDSDBO].[{0}] K ON F.DocumentArtifactID = K.DocumentArtifactID
	        WHERE
            F.[Type] = 1 
            AND K.RelationalGroup=@relationalGroup 
            AND R.RelationalGroup=@relationalGroup)", savedSearchTableName, redactionSetTableName, hasAutoReddactionsColumn);

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@relationalGroup", SqlDbType.NVarChar) {Value = relationalGroup},
      };

      return await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));
    }
    public async Task<DataTable> BulkUpdateHasAutoRedactionsForDocumentRange(IDBContext eddsDbContext, string savedSearchTableName, string redactionSetTableName, int startId, int endId, string hasAutoRedactionsColumn)
    {
      var sql = string.Format(@"
        UPDATE [EDDSDBO].[Document]
        SET [{2}]=1
        WHERE [ArtifactID] in (
	        SELECT 
		        DISTINCT K.DocumentArtifactID
	        FROM [EDDSDBO].[File] F (NOLOCK)
		        INNER JOIN [EDDSDBO].[{0}] K ON F.DocumentArtifactID = K.DocumentArtifactID
		        CROSS JOIN [EDDSDBO].[{1}] R
	        WHERE F.[Type] = 1 AND K.ID BETWEEN @Start AND @End )
            ", savedSearchTableName, redactionSetTableName, hasAutoRedactionsColumn);

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@Start", SqlDbType.Int) {Value = startId},
        new SqlParameter("@End", SqlDbType.Int) {Value = endId}
      };

      return await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));
    }

    public async Task<DataTable> BulkInsertRedactionRecordsForDocumentRange(IDBContext eddsDbContext, string savedSearchTableName, string redactionSetTableName, int startId, int endId)
    {
      var sql = $@"
        DECLARE @output TABLE (ID int)
 
			  INSERT INTO [EDDSDBO].Redaction
	      (
		      FileGuid, 
		      X, 
		      Y, 
		      Width, 
		      Height, 
		      MarkupSetArtifactID, 
		      MarkupType, 
		      FillA, 
		      FillR, 
		      FillG, 
		      FillB, 
		      BorderSize, 
		      BorderA, 
		      BorderR, 
		      BorderG, 
		      BorderB, 
		      BorderStyle, 
		      FontName, 
		      FontA, 
		      FontR, 
		      FontG, 
		      FontB, 
		      FontSize, 
		      FontStyle, 
		      [Text], 
		      ZOrder, 
		      DrawCrossLines, 
		      MarkupSubType,
		      X_d, 
		      Y_d, 
		      Width_d, 
		      Height_d
	      )

        OUTPUT INSERTED.ID INTO @output

	      SELECT DISTINCT
		      F.[Guid], 
		      R.X, 
		      R.Y, 
		      R.Width, 
		      R.Height, 
		      R.MarkupSetArtifactID,
		      R.MarkupType, 
		      R.FillA, 
		      R.FillR, 
		      R.FillG, 
		      R.FillB, 
		      R.BorderSize, 
		      R.BorderA, 
		      R.BorderR, 
		      R.BorderG, 
		      R.BorderB, 
		      R.BorderStyle, 
		      R.FontName, 
		      R.FontA, 
		      R.FontR, 
		      R.FontG, 
		      R.FontB, 
		      R.FontSize, 
		      R.FontStyle, 
		      R.[Text], 
		      R.ZOrder, 
		      R.DrawCrossLines, 
		      R.MarkupSubType,
		      R.X_d, 
		      R.Y_d, 
		      R.Width_d, 
		      R.Height_d
	      FROM [EDDSDBO].[File] F (NOLOCK)
		      INNER JOIN [EDDSDBO].[{savedSearchTableName}] K ON F.DocumentArtifactID = K.DocumentArtifactID
		      CROSS JOIN [EDDSDBO].[{redactionSetTableName}] R
	      WHERE 
          F.[Type] = 1 
          AND K.ID BETWEEN @Start AND @End 
               
        SELECT * from @output";


      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@Start", SqlDbType.Int) {Value = startId},
        new SqlParameter("@End", SqlDbType.Int) {Value = endId}
      };

      return await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));
    }

    public async Task<int> UpdateHasRedactionsOrHighlightsAsync(IDBContext eddsDbContext, int markupSetCodeTypeId, int redactionOrAnnotationCodeArtifactId, int markupType, string savedSearchHoldingTable, string redactionsHoldingTable, int markupSetArtifactId, int start, int end)
    {
      var sql = string.Format(@"
        INSERT INTO EDDSDBO.ZCodeArtifact_{0} (CodeArtifactID, AssociatedArtifactID)
	      SELECT
          DISTINCT {1},
          F.DocumentArtifactID
        FROM [EDDSDBO].[File] F (NOLOCK)
          INNER JOIN [EDDSDBO].[{2}] K ON F.DocumentArtifactID = K.DocumentArtifactID
          CROSS JOIN [EDDSDBO].[{3}] R
	      WHERE 
          F.[Type] = 1
          AND F.[Order] = 0 
          AND R.MarkupSetArtifactID = @MarkupSetArtifactID
          AND R.MarkupTYpe= @MarkupType
          AND K.ID BETWEEN @Start AND @End
          AND NOT EXISTS
			      (SELECT *
				      FROM EDDSDBO.ZCodeArtifact_{0}
				      WHERE 
                CodeArtifactID = {1}
					      AND AssociatedArtifactID = F.DocumentArtifactID )", markupSetCodeTypeId, redactionOrAnnotationCodeArtifactId, savedSearchHoldingTable, redactionsHoldingTable);

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@MarkupSetArtifactID", SqlDbType.Int) {Value = markupSetArtifactId},
        new SqlParameter("@MarkupType", SqlDbType.Int) {Value = markupType},
        new SqlParameter("@Start", SqlDbType.Int) {Value = start},
        new SqlParameter("@End", SqlDbType.Int) {Value = end},
      };

      return await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
    }

    public async Task<int> UpdateHasRedactionsOrHighlightsAsync(IDBContext eddsDbContext, int markupSetCodeTypeId, int redactionOrAnnotationCodeArtifactId, int markupType, string savedSearchHoldingTable, string redactionsHoldingTable, int markupSetArtifactId, string relationalGroup)
    {
      var sql = string.Format(@"
        INSERT INTO EDDSDBO.ZCodeArtifact_{0} (CodeArtifactID, AssociatedArtifactID)
	      SELECT
          DISTINCT {1},
          F.DocumentArtifactID
        FROM 
          [EDDSDBO].[File] F (NOLOCK)
          INNER JOIN [EDDSDBO].[{2}] K ON F.DocumentArtifactID = K.DocumentArtifactID
          CROSS JOIN [EDDSDBO].[{3}] R
	      WHERE 
          F.[Type] = 1
				  AND F.[Order] = 0 
				  AND R.MarkupSetArtifactID = @MarkupSetArtifactID
				  AND R.MarkupTYpe= @MarkupType
				  AND K.RelationalGroup = @relationalGroup
          AND R.RelationalGroup = @relationalGroup
				  AND NOT EXISTS
			      (SELECT *
				      FROM EDDSDBO.ZCodeArtifact_{0}
				      WHERE CodeArtifactID = {1}
					      AND AssociatedArtifactID = F.DocumentArtifactID )", markupSetCodeTypeId, redactionOrAnnotationCodeArtifactId, savedSearchHoldingTable, redactionsHoldingTable);

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@MarkupSetArtifactID", SqlDbType.Int) {Value = markupSetArtifactId},
        new SqlParameter("@MarkupType", SqlDbType.Int) {Value = markupType},
        new SqlParameter("@relationalGroup", SqlDbType.NVarChar) {Value = relationalGroup}
      };

      return await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
    }

    public async Task CopyRecordsToExportResultsAsync(IDBContext eddsDbContext, string holdingTableName)
    {
      var sql = $@" 
			  INSERT INTO [EDDSDBO].{Constant.Tables.ExportResults}
			  (
				  TimeStampUTC,
				  WorkspaceArtifactID,
				  ExportJobArtifactID,
				  DocumentIdentifier,
				  FileOrder,
				  X,
				  Y,
				  Width,
				  Height,
				  MarkupSetArtifactID,
				  MarkupType,
				  FillA,
				  FillR,
				  FillG,
				  FillB,
				  BorderSize,
				  BorderA,
				  BorderR,
				  BorderG,
				  BorderB,
				  BorderStyle,
				  FontName,
				  FontA,
				  FontR,
				  FontG,
				  FontB,
				  FontSize,
				  FontStyle,
				  Text,
				  ZOrder,
				  DrawCrossLines,
				  MarkupSubType,
				  X_d,
				  Y_d,
				  Width_d,
				  Height_d
			  )
			  SELECT 
				  TimeStampUTC,
				  WorkspaceArtifactID,
				  ExportJobArtifactID,
				  DocumentIdentifier,
				  FileOrder,
				  X,
				  Y,
				  Width,
				  Height,
				  MarkupSetArtifactID,
				  MarkupType,
				  FillA,
				  FillR,
				  FillG,
				  FillB,
				  BorderSize,
				  BorderA,
				  BorderR,
				  BorderG,
				  BorderB,
				  BorderStyle,
				  FontName,
				  FontA,
				  FontR,
				  FontG,
				  FontB,
				  FontSize,
				  FontStyle,
				  Text,
				  ZOrder,
				  DrawCrossLines,
				  MarkupSubType,
				  X_d,
				  Y_d,
				  Width_d,
				  Height_d
			  FROM [EDDSDBO].[{holdingTableName}]";

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql));
    }

    public async Task CreateAuditRecordAsync(IDBContext workspaceDbContext, RedactionAuditRecord redactionAuditRecord)
    {
      const string sql = @"
				BEGIN TRAN
					INSERT INTO [EDDSDBO].[AuditRecord](
					  [ArtifactID]
					  ,[Action]
					  ,[Details]
					  ,[UserID]
					  ,[TimeStamp]
					  ,[RequestOrigination]
					  ,[RecordOrigination]
					  ,[ExecutionTime]
					  ,[SessionIdentifier]
					)
					VALUES(
					  @artifactId
					  ,@action
					  ,@details
					  ,@userId
					  ,@timeStamp
					  ,@requestOrigination
					  ,@recordOrigination
					  ,@executionTime
					  ,@sessionIdentifier
					)
				COMMIT
				";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@artifactId", SqlDbType.Int) {Value = redactionAuditRecord.ArtifactId},
        new SqlParameter("@action", SqlDbType.Int) {Value = redactionAuditRecord.Action},
        new SqlParameter("@details", SqlDbType.NVarChar) {Value = redactionAuditRecord.Details},
        new SqlParameter("@userId", SqlDbType.Int) {Value = redactionAuditRecord.UserId},
        new SqlParameter("@timeStamp", SqlDbType.DateTime) {Value = redactionAuditRecord.TimeStamp},
        new SqlParameter("@requestOrigination", SqlDbType.NVarChar) {Value = redactionAuditRecord.RequestOrigination},
        new SqlParameter("@recordOrigination", SqlDbType.NVarChar) {Value = redactionAuditRecord.RecordOrigination},
        new SqlParameter("@executionTime", SqlDbType.Int) {Value = (object) redactionAuditRecord.ExecutionTime ?? DBNull.Value},
        new SqlParameter("@sessionIdentifier", SqlDbType.Int) {Value = (object) redactionAuditRecord.SessionIdentifier ?? DBNull.Value}
      };

      await Task.Run(() => workspaceDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
    }

    public async Task<bool> VerifyIfImportWorkerQueueContainsRecordsForJobAsync(IDBContext eddsDbContext, int workspaceArtifactId, int importJobArtifactId)
    {
      string sql = $@"
				SELECT 
					COUNT(0) 
				FROM 
					[EDDSDBO].[{Constant.Tables.ImportWorkerQueue}] WITH(NOLOCK) 
				WHERE 
					[WorkspaceArtifactID] = workspaceArtifactId AND [ImportJobArtifactID]= importJobArtifactId 
				";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@workspaceArtifactId", SqlDbType.Int) { Value = workspaceArtifactId },
        new SqlParameter("@importJobArtifactId", SqlDbType.Int) { Value = importJobArtifactId }
      };

      var rows = await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsScalar<int>(sql, sqlParams));
      return rows > 0;
    }

    public async Task<int> GetExportResultsRecordCountAsync(IDBContext eddsDbContext, int workspaceArtifactId, int exportJobArtifactId, int agentId)
    {
      string sql = $@"
				BEGIN TRAN
					DECLARE @workerTableCount BIGINT

					SELECT @workerTableCount = COUNT(0) 
					FROM 
						[EDDSDBO].[{Constant.Tables.ExportWorkerQueue}] WITH(NOLOCK) 
					WHERE 
						[WorkspaceArtifactID] = @workspaceArtifactId 
						AND [ExportJobArtifactID] = @exportJobArtifactId 

					IF (@workerTableCount = 0)
						BEGIN
							UPDATE [EDDSDBO].[{Constant.Tables.ExportResults}] 
							SET AgentID = @agentId
							WHERE 
								WorkspaceArtifactID = @workspaceArtifactId
								AND ExportJobArtifactID = @exportJobArtifactId
								AND AgentID IS NULL
						END

					DECLARE @finalCount BIGINT;
					SELECT @finalCount = COUNT(0) 
					FROM 
						[EDDSDBO].[{Constant.Tables.ExportResults}] WITH(NOLOCK) 
					WHERE 
						[AgentID] = @agentId 

						SELECT @finalCount
				COMMIT";

      List<SqlParameter> sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@workspaceArtifactId", SqlDbType.Int) { Value = workspaceArtifactId },
        new SqlParameter("@exportJobArtifactId", SqlDbType.Int) { Value = exportJobArtifactId },
        new SqlParameter("@notStarted", SqlDbType.Int) { Value = Constant.Status.Queue.NotStarted },
        new SqlParameter("@agentId", SqlDbType.Int) { Value = agentId }
      };

      return await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsScalar<int>(sql, sqlParams));
    }
    public async Task<int> GetJobWorkerRecordCountAsync(IDBContext eddsDbContext, int workspaceArtifactId, int jobArtifactId, string tableName, string jobColumn)
    {
      string sql = $@"
					SELECT COUNT(0) 
					FROM 
						[EDDSDBO].[{tableName}] WITH(NOLOCK) 
					WHERE 
						[WorkspaceArtifactID] = @workspaceArtifactId 
						AND [{jobColumn}] = @jJobArtifactId";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@workspaceArtifactId", SqlDbType.Int) { Value = workspaceArtifactId },
        new SqlParameter("@jJobArtifactId", SqlDbType.Int) { Value = jobArtifactId }
      };

      return await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsScalar<int>(sql, sqlParams));
    }

    public async Task<DataTable> GetExportResultsAsync(IDBContext eddsDbContext, int workspaceArtifactId, int exportJobArtifactId, int agentId)
    {
      string sql = $@"
				SELECT TOP {Constant.Sizes.ExportJobResultsBatchSize}
					[ID],
					TimeStampUTC,
					WorkspaceArtifactID,
					ExportJobArtifactID,
					DocumentIdentifier,
					FileOrder,
					X,
					Y,
					Width,
					Height,
					MarkupSetArtifactID,
					MarkupType,
					FillA,
					FillR,
					FillG,
					FillB,
					BorderSize,
					BorderA,
					BorderR,
					BorderG,
					BorderB,
					BorderStyle,
					FontName,
					FontA,
					FontR,
					FontG,
					FontB,
					FontSize,
					FontStyle,
					Text,
					ZOrder,
					DrawCrossLines,
					MarkupSubType,
					X_d,
					Y_d,
					Width_d,
					Height_d
				FROM 
					[EDDSDBO].[{Constant.Tables.ExportResults}] WITH(NOLOCK)
				WHERE
					WorkspaceArtifactID = {workspaceArtifactId}
					AND ExportJobArtifactID = {exportJobArtifactId}
					AND AgentID = {agentId}";

      var dataTable = await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsDataTable(sql));
      return dataTable.Rows.Count > 0 ? dataTable : null;
    }

    public async Task DeleteExportResultsAsync(IDBContext eddsDbContext, List<int> recordIdList)
    {
      var recordIds = string.Join(",", recordIdList);

      string sql = $@"
				DELETE FROM [EDDSDBO].[{Constant.Tables.ExportResults}]
				WHERE
					ID IN ({recordIds})";

      await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql));
    }

    public async Task<int> GetWorkspaceArtifactIdByGuidAsync(IDBContext workspaceDbContext, string fieldGuid)
    {
      var sql = @"
					SELECT ArtifactID
					FROM [EDDSDBO].[ArtifactGuid] WITH(NOLOCK)
					WHERE ArtifactGuid = @fieldGuid";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@fieldGuid", SqlDbType.NVarChar) {Value = fieldGuid}
      };

      var dataTable = await Task.Run(() => workspaceDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));
      return (int)dataTable.Rows[0][0];
    }

    public async Task UpdateMarkupSetMultipleChoiceFieldAsync(IDBContext workspaceDbContext, int documentArtifactId, int choiceTypeId, int choiceArtifactId)
    {
      var sql = string.Format($@"
				BEGIN TRAN

				  DECLARE @count INT

				  SELECT 
					  @count = COUNT(0) 
				  FROM 
					  [EDDSDBO].[ZCodeArtifact_{choiceTypeId}] WITH(NOLOCK)	
				  WHERE 
					  [CodeArtifactID] = @choiceArtifactId 
					  AND [AssociatedArtifactID] = @documentArtifactId

				  IF @count = 0
				  BEGIN
					  INSERT INTO [EDDSDBO].[ZCodeArtifact_{choiceTypeId}]([CodeArtifactID], [AssociatedArtifactID]) 
					  VALUES (@choiceArtifactId, @documentArtifactId)
				  END

				COMMIT
				");

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@choiceArtifactId", SqlDbType.Int) {Value = choiceArtifactId},
        new SqlParameter("@documentArtifactId", SqlDbType.Int) {Value = documentArtifactId}
      };

      await Task.Run(() => workspaceDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
    }

    public async Task<DataTable> RetrieveZCodesAsync(IDBContext workspaceDbContext, int destinationMarkupSetArtifactId)
    {
      const string sql = @"
				SELECT C.CodeTypeID, M.RedactionCodeArtifactID, M.AnnotationCodeArtifactID 
        FROM eddsdbo.MarkupSet M (NOLOCK), eddsdbo.[Code] (NOLOCK) C
        WHERE 
          M.ArtifactID = @destinationMarkupSetArtifactId 
          AND C.ArtifactID=M.RedactionCodeArtifactID";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@destinationMarkupSetArtifactId", SqlDbType.Int) {Value = destinationMarkupSetArtifactId}
      };

      return await Task.Run(() => workspaceDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));
    }
    
    public async Task<DataTable> RetrieveDocumentColumnAsync(IDBContext workspaceDbContext, int fieldArtifactId)
    {
      const string sql = @"
				SELECT 
          AVF.ColumnName 
        FROM 
          [EDDSDBO].[Field] F (nolock), 
          [EDDSDBO].[ArtifactViewField] AVF (nolock)
        WHERE 
          F.ArtifactViewFieldID = AVF.ArtifactViewFieldID
          AND F.ArtifactID = @artifactId;";

      var sqlParams = new List<SqlParameter>
      {
        new SqlParameter("@artifactId", SqlDbType.Int) {Value = fieldArtifactId}
      };

      return await Task.Run(() => workspaceDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));
    }
  }
}

