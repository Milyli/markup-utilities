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
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace MarkupUtilities.Agents.NUnit
{
  [TestFixture]
  public class ExportWorkerAgentTests
  {
    public int AgentId;
    public Mock<IQuery> MockQuery;
    public Mock<IAgentHelper> MockAgentHelper;
    public Mock<IServicesMgr> MockServiceManager;
    public Mock<IArtifactQueries> MockArtifactQueries;
    public Mock<Helpers.Utility.IQuery> MockUtilityQueryHelper;
    public Mock<IExportFileCreator> MockExportFileCreator;
    public Mock<IErrorQueries> MockErrorQueries;
    private List<int> _resourceGroupIdList;
    private ExportWorkerJob _exportWorkerJob;

    [SetUp]
    public void Setup()
    {
      AgentId = 1234567;
      MockQuery = new Mock<IQuery>();
      MockAgentHelper = new Mock<IAgentHelper>();
      MockServiceManager = new Mock<IServicesMgr>();
      MockArtifactQueries = new Mock<IArtifactQueries>();
      MockUtilityQueryHelper = new Mock<Helpers.Utility.IQuery>();
      MockExportFileCreator = new Mock<IExportFileCreator>();
      MockErrorQueries = new Mock<IErrorQueries>();
      _resourceGroupIdList = new List<int> { 10000, 20000 };
      _exportWorkerJob = GetExportWorkerJob();
    }

    [TearDown]
    public void TearDown()
    {
      MockQuery = null;
      MockAgentHelper = null;
      MockServiceManager = null;
      MockArtifactQueries = null;
      MockUtilityQueryHelper = null;
      MockErrorQueries = null;
      MockExportFileCreator = null;
      MockErrorQueries = null;
      _resourceGroupIdList = null;
      _exportWorkerJob = null;
    }

    #region Tests

    [Test]
    [Description("This will test getting a comma delimited list of resource IDs from a list of integers.")]
    public void GetCommaDelimitedListOfResourceIds()
    {
      // Arrange
      var resourceIdsList = new List<int> { 1000001, 1000002, 1000003 };
      const string expectedResult = "1000001,1000002,1000003";
      var dataTable = GetExportWorkerTable();
      MockRetrieveNextInExportWorkerQueueAsync(dataTable);

      //Act
      var observedResult = _exportWorkerJob.GetCommaDelimitedListOfResourceIds(resourceIdsList);

      //Assert
      Assert.AreEqual(expectedResult, observedResult);
    }


    [Description("When no record is picked up by the agent, should not process")]
    [Test]
    public async Task ExecuteAsync_QueueHasNoRecord_DoNotExecute()
    {
      // Arrange

      //when there is no record(s) in the queue
      MockRetrieveNextInExportWorkerQueueAsync(null);

      // Act
      await _exportWorkerJob.ExecuteAsync();

      // Assert
      AssertResetUnfishedJobsAsyncWasCalled(1);
      AssertRetrieveNextBatchInExportWorkerQueueAsyncWasCalled(1);
      AssertDropTableAsyncWasCalled(2);
    }

    [Description("When a record is picked up by the agent, should complete execution process")]
    [Test]
    public async Task ExecuteAsync_QueueHasARecord_ExecuteAll()
    {
      // Arrange
      ArrangeGoldenFlowMocks();

      // Act
      await _exportWorkerJob.ExecuteAsync();

      // Assert
      AssertResetUnfishedJobsAsyncWasCalled(1);
      AssertRetrieveExportJobAsyncWasCalled(1);
      AssertCreateExportWorkerHoldingTableAsyncWasCalled(1);
      AssertUpdateRdoJobTextFieldAsyncWasCalled(3); // Export Job Status: twice, Export Redaction Total Count: once
      AssertVerifyIfDocumentExistsAsyncWasCalled(1);
      AssertGetDocumentIdentifierFieldColumnNameAsyncWasCalled(1);
      AssertRetrieveRedactionsForDocumentAsyncWasCalled(1);
      AssertAddRedactionsToTableAsyncWasCalled(1);
      AssertCopyRecordsToExportResultsAsyncWasCalled(1);
      AssertDropTableAsyncWasCalled(4); // Dropping the Batch table and the Holding table
      AssertRemoveBatchFromExportWorkerQueueAsyncWasCalled(1);
      AssertGetExportResultsRecordCountAsyncWasCalled(1);
    }

    [Description("When a record is picked up by the agent, and no results exist in the Results table and no Redactions found.")]
    [Test]
    public async Task ExecuteAsync_QueueHasARecord_Execute_No_Records_In_Results_Table_No_Redactions_Found()
    {
      // Arrange
      ArrangeGoldenFlowMocks();
      MockGetExportResultsRecordCountAsync(0);
      MockGetExportWorkerRecordCountAsync(0);

      // Act
      await _exportWorkerJob.ExecuteAsync();

      // Assert
      AssertResetUnfishedJobsAsyncWasCalled(1);
      AssertRetrieveExportJobAsyncWasCalled(1);
      AssertCreateExportWorkerHoldingTableAsyncWasCalled(1);
      AssertUpdateRdoJobTextFieldAsyncWasCalled(3); // Export Job Status: twice, Export Redaction Total Count: once
      AssertVerifyIfDocumentExistsAsyncWasCalled(1);
      AssertGetDocumentIdentifierFieldColumnNameAsyncWasCalled(1);
      AssertRetrieveRedactionsForDocumentAsyncWasCalled(1);
      AssertAddRedactionsToTableAsyncWasCalled(1);
      AssertCopyRecordsToExportResultsAsyncWasCalled(1);
      AssertDropTableAsyncWasCalled(3); // Dropping the Batch table and the Holding table
      AssertRemoveBatchFromExportWorkerQueueAsyncWasCalled(1);
      AssertGetExportResultsRecordCountAsyncWasCalled(1);
      AssertGetExportWorkerRecordCountAsyncWasCalled(1);
    }

    [Description("When the agent throws an exception.")]
    [Test]
    public async Task ExecuteAsync_Exception_Thrown()
    {
      // Arrange
      MockResetUnfishedJobsAsync_ThrowsException();
      MockInsertRowIntoExportErrorLogAsync();
      MockWriteError();
      MockDropTableAsync();

      // Act
      await _exportWorkerJob.ExecuteAsync();

      // Assert
      AssertResetUnfishedJobsAsyncWasCalled(1);
      AssertInsertRowIntoExportErrorLogAsyncWasCalled(1);
      AssertRetrieveExportJobAsyncWasCalled(0);
      AssertCreateExportWorkerHoldingTableAsyncWasCalled(0);
      AssertUpdateRdoJobTextFieldAsyncWasCalled(0);
      AssertVerifyIfDocumentExistsAsyncWasCalled(0);
      AssertGetDocumentIdentifierFieldColumnNameAsyncWasCalled(0);
      AssertRetrieveRedactionsForDocumentAsyncWasCalled(0);
      AssertAddRedactionsToTableAsyncWasCalled(0);
      AssertCopyRecordsToExportResultsAsyncWasCalled(0);
      AssertDropTableAsyncWasCalled(2);
      AssertRemoveBatchFromExportWorkerQueueAsyncWasCalled(0);
      AssertGetExportResultsRecordCountAsyncWasCalled(0);
      AssertGetExportWorkerRecordCountAsyncWasCalled(0);
      AssertWriteErrorWasCalled(1);
    }

    #endregion Tests

    #region Test Helpers

    private ExportWorkerJob GetExportWorkerJob()
    {
      var exportWorkerJob = new ExportWorkerJob(
        AgentId,
        MockServiceManager.Object,
        MockAgentHelper.Object,
        MockQuery.Object,
        MockArtifactQueries.Object,
        MockUtilityQueryHelper.Object,
        new DateTime(2016, 01, 25, 01, 00, 00),
        _resourceGroupIdList,
        MockExportFileCreator.Object,
        MockErrorQueries.Object);

      return exportWorkerJob;
    }

    private static DataTable GetExportWorkerTable()
    {
      var table = new DataTable("Test Worker Table");
      table.Columns.Add(new DataColumn("ID", typeof(int)));
      table.Columns.Add(new DataColumn("TimeStampUTC", typeof(DateTime)));
      table.Columns.Add(new DataColumn("WorkspaceArtifactID", typeof(int)));
      table.Columns.Add(new DataColumn("DocumentArtifactID", typeof(int)));
      table.Columns.Add(new DataColumn("MarkupSetArtifactID", typeof(int)));
      table.Columns.Add(new DataColumn("QueueStatus", typeof(int)));
      table.Columns.Add(new DataColumn("AgentID", typeof(int)) { AllowDBNull = true });
      table.Columns.Add(new DataColumn("ExportJobArtifactID", typeof(int)));
      table.Columns.Add(new DataColumn("MarkupSubType", typeof(string)));
      table.Columns.Add(new DataColumn("ResourceGroupID", typeof(int)));

      //Create test rows
      table.Rows.Add(1, DateTime.Now, 100, 200, 300, 0, 123, 1234, "1,2,3,4", 12345);
      return table;
    }

    private static DataTable GetExportWorkerRedactionsTable()
    {
      var table = new DataTable("Test Worker Redaction Table");
      table.Columns.Add(new DataColumn("TimeStampUTC", typeof(DateTime)));
      table.Columns.Add(new DataColumn("WorkspaceArtifactID", typeof(int)));
      table.Columns.Add(new DataColumn("ExportJobArtifactID", typeof(int)));
      table.Columns.Add(new DataColumn("DocumentIdentifier", typeof(string)));
      table.Columns.Add(new DataColumn("FileOrder", typeof(int)));
      table.Columns.Add(new DataColumn("X", typeof(int)));
      table.Columns.Add(new DataColumn("Y", typeof(int)));
      table.Columns.Add(new DataColumn("Width", typeof(int)));
      table.Columns.Add(new DataColumn("Height", typeof(int)));
      table.Columns.Add(new DataColumn("MarkupSetArtifactID", typeof(int)));
      table.Columns.Add(new DataColumn("MarkupType", typeof(int)));
      table.Columns.Add(new DataColumn("FillA", typeof(int)));
      table.Columns.Add(new DataColumn("FillR", typeof(int)));
      table.Columns.Add(new DataColumn("FillG", typeof(int)));
      table.Columns.Add(new DataColumn("FillB", typeof(int)));
      table.Columns.Add(new DataColumn("BorderSize", typeof(int)) { AllowDBNull = true });
      table.Columns.Add(new DataColumn("BorderA", typeof(int)) { AllowDBNull = true });
      table.Columns.Add(new DataColumn("BorderR", typeof(int)) { AllowDBNull = true });
      table.Columns.Add(new DataColumn("BorderG", typeof(int)) { AllowDBNull = true });
      table.Columns.Add(new DataColumn("BorderB", typeof(int)) { AllowDBNull = true });
      table.Columns.Add(new DataColumn("BorderStyle", typeof(int)) { AllowDBNull = true });
      table.Columns.Add(new DataColumn("FontName", typeof(string)) { AllowDBNull = true });
      table.Columns.Add(new DataColumn("FontA", typeof(int)) { AllowDBNull = true });
      table.Columns.Add(new DataColumn("FontR", typeof(int)) { AllowDBNull = true });
      table.Columns.Add(new DataColumn("FontG", typeof(int)) { AllowDBNull = true });
      table.Columns.Add(new DataColumn("FontB", typeof(int)) { AllowDBNull = true });
      table.Columns.Add(new DataColumn("FontSize", typeof(int)) { AllowDBNull = true });
      table.Columns.Add(new DataColumn("FontStyle", typeof(int)) { AllowDBNull = true });
      table.Columns.Add(new DataColumn("Text", typeof(int)) { AllowDBNull = true });
      table.Columns.Add(new DataColumn("ZOrder", typeof(int)));
      table.Columns.Add(new DataColumn("DrawCrossLines", typeof(bool)));
      table.Columns.Add(new DataColumn("MarkupSubType", typeof(int)));
      table.Columns.Add(new DataColumn("X_d", typeof(decimal)) { AllowDBNull = true });
      table.Columns.Add(new DataColumn("Y_d", typeof(decimal)) { AllowDBNull = true });
      table.Columns.Add(new DataColumn("Width_d", typeof(decimal)) { AllowDBNull = true });
      table.Columns.Add(new DataColumn("Height_d", typeof(decimal)) { AllowDBNull = true });

      //Create test rows
      table.Rows.Add(DateTime.Now, 1, 1, "ABC", 0, 0, 0, 300, 300, 12345, 1, 255, 0, 0, 0, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, 0, 0, 1, 0.0000, 0.0000, 300.0000, 300.0000);
      return table;
    }

    private static DataTable GetExportResults()
    {
      var table = new DataTable("Test Worker Redaction Table");
      table.Columns.Add(new DataColumn("ID", typeof(int)));
      table.Columns.Add(new DataColumn("TimeStampUTC", typeof(DateTime)));
      table.Columns.Add(new DataColumn("WorkspaceArtifactID", typeof(int)));
      table.Columns.Add(new DataColumn("ExportJobArtifactID", typeof(int)));
      table.Columns.Add(new DataColumn("DocumentIdentifier", typeof(string)));
      table.Columns.Add(new DataColumn("FileOrder", typeof(int)));
      table.Columns.Add(new DataColumn("X", typeof(int)));
      table.Columns.Add(new DataColumn("Y", typeof(int)));
      table.Columns.Add(new DataColumn("Width", typeof(int)));
      table.Columns.Add(new DataColumn("Height", typeof(int)));
      table.Columns.Add(new DataColumn("MarkupSetArtifactID", typeof(int)));
      table.Columns.Add(new DataColumn("MarkupType", typeof(int)));
      table.Columns.Add(new DataColumn("FillA", typeof(int)));
      table.Columns.Add(new DataColumn("FillR", typeof(int)));
      table.Columns.Add(new DataColumn("FillG", typeof(int)));
      table.Columns.Add(new DataColumn("FillB", typeof(int)));
      table.Columns.Add(new DataColumn("BorderSize", typeof(int)) { AllowDBNull = true });
      table.Columns.Add(new DataColumn("BorderA", typeof(int)) { AllowDBNull = true });
      table.Columns.Add(new DataColumn("BorderR", typeof(int)) { AllowDBNull = true });
      table.Columns.Add(new DataColumn("BorderG", typeof(int)) { AllowDBNull = true });
      table.Columns.Add(new DataColumn("BorderB", typeof(int)) { AllowDBNull = true });
      table.Columns.Add(new DataColumn("BorderStyle", typeof(int)) { AllowDBNull = true });
      table.Columns.Add(new DataColumn("FontName", typeof(string)) { AllowDBNull = true });
      table.Columns.Add(new DataColumn("FontA", typeof(int)) { AllowDBNull = true });
      table.Columns.Add(new DataColumn("FontR", typeof(int)) { AllowDBNull = true });
      table.Columns.Add(new DataColumn("FontG", typeof(int)) { AllowDBNull = true });
      table.Columns.Add(new DataColumn("FontB", typeof(int)) { AllowDBNull = true });
      table.Columns.Add(new DataColumn("FontSize", typeof(int)) { AllowDBNull = true });
      table.Columns.Add(new DataColumn("FontStyle", typeof(int)) { AllowDBNull = true });
      table.Columns.Add(new DataColumn("Text", typeof(int)) { AllowDBNull = true });
      table.Columns.Add(new DataColumn("ZOrder", typeof(int)));
      table.Columns.Add(new DataColumn("DrawCrossLines", typeof(bool)));
      table.Columns.Add(new DataColumn("MarkupSubType", typeof(int)));
      table.Columns.Add(new DataColumn("X_d", typeof(decimal)) { AllowDBNull = true });
      table.Columns.Add(new DataColumn("Y_d", typeof(decimal)) { AllowDBNull = true });
      table.Columns.Add(new DataColumn("Width_d", typeof(decimal)) { AllowDBNull = true });
      table.Columns.Add(new DataColumn("Height_d", typeof(decimal)) { AllowDBNull = true });

      //Create test rows
      table.Rows.Add(1, DateTime.Now, 1, 1, "ABC", 0, 0, 0, 300, 300, 12345, 1, 255, 0, 0, 0, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, 0, 0, 1, 0.0000, 0.0000, 300.0000, 300.0000);
      return table;
    }

    private static MarkupUtilityExportJob GetMarkupUtilityExportJob()
    {
      var retVal = new MarkupUtilityExportJob(
        123,
        string.Empty,
        123,
        Constant.MarkupSubTypeCategory.SupportedMarkupUtilityTypes,
        123,
        123,
        string.Empty,
        string.Empty,
        123);
      return retVal;
    }

    private static List<SqlBulkCopyColumnMapping> GetColumnNames()
    {
      var columns = new List<string>
      {
        "ABC",
        "DEF",
        "GHI"
      };
      return columns.Select(column => new SqlBulkCopyColumnMapping(column, column)).ToList();
    }

    #endregion Test Helpers

    #region Mocks

    private void ArrangeGoldenFlowMocks()
    {
      // When Export Worker queue has a record(s)
      var dataTable = GetExportWorkerTable();
      MockRetrieveNextInExportWorkerQueueAsync(dataTable);

      //Retrieve Export Job
      var markupUtilityExportJob = GetMarkupUtilityExportJob();
      MockRetrieveExportJobAsync(markupUtilityExportJob);

      //Verify the Document Exists
      MockVerifyIfDocumentExistsAsync();

      //Retrieve a valid column name for the Document Identifier
      MockGetDocumentIdentifierFieldColumnNameAsync();

      //Create redaction table
      var dtRedactions = GetExportWorkerRedactionsTable();
      MockRetrieveRedactionsForDocumentAsync(dtRedactions);

      //Create the Column names
      var columns = GetColumnNames();
      MockGetMappingsForWorkerQueue(columns);

      //Mock Add Redactions
      MockAddRedactionsToTableAsync();

      //Mock the Bulk Insert of Redactions
      MockBulkInsertIntoTable();

      //Copy Records to Export Results table
      MockCopyRecordsToExportResultsAsync();

      //Drop Holding Table
      MockDropTableAsync();

      //Remove the Batch from the Worker Queue
      MockRemoveBatchFromExportWorkerQueueAsync();

      //Retrieve count of records from Results Table
      MockGetExportResultsRecordCountAsync(1);

      //Create a temp CSV file
      MockCreateExportFileAsync();

      //Get the results from the Export Results table
      var dtExportResults = GetExportResults();
      MockGetExportResultsAsync(dtExportResults);

      //Write the results to the Export CSV file
      MockProcessExportResultsAsync();

      //Delete results from the Export Results table
      MockDeleteExportResultsAsync();

      //Create new File RDO record
      const int rdoArtifactId = 123456;
      MockCreateMarkupUtilityFileRdoRecordAsync(rdoArtifactId);

      //Get Artifact ID of the File Field on the Migration File RDO
      const int rdoFileFieldArtifactId = 123456;
      MockGetWorkspaceArtifactIdByGuidAsync(rdoFileFieldArtifactId);

      //Attach CSV file to RDO record
      MockAttachFileToMarkupUtilityFileRecord();

      //Attach File RDO record to the Export Job
      MockAttachRedactionFileToExportJob();

      //Update Exported Redaction Count
      MockUpdateRdoJobTextFieldAsync();

      //Delete Temp CSV file
      MockDeleteExportFileAsync();

      //Update Status field on Export Job
      MockUpdateRdoJobTextFieldAsync();

      //Drop Batch Table
      MockDropTableAsync();
    }

    private void MockResetUnfishedJobsAsync_ThrowsException()
    {
      MockQuery
        .Setup(x => x.ResetUnfishedJobsAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<string>()
          ))
        .Throws<MarkupUtilityException>()
        .Verifiable();
    }
    private void MockRetrieveNextInExportWorkerQueueAsync(DataTable dataTable)
    {
      MockQuery
        .Setup(x => x.RetrieveNextBatchInExportWorkerQueueAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<string>()
          ))
        .Returns(Task.FromResult(dataTable))
        .Verifiable();
    }

    private void MockRetrieveExportJobAsync(MarkupUtilityExportJob markupUtilityExportJob)
    {
      MockArtifactQueries
        .Setup(x => x.RetrieveExportJobAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<int>()
          ))
        .Returns(Task.FromResult(markupUtilityExportJob))
        .Verifiable();
    }

    private void MockRetrieveRedactionsForDocumentAsync(DataTable dataTable)
    {
      MockQuery
        .Setup(x => x.RetrieveRedactionsForDocumentAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<string>()))
        .Returns(Task.FromResult(dataTable))
        .Verifiable();
    }

    private void MockVerifyIfDocumentExistsAsync()
    {
      MockArtifactQueries
        .Setup(x => x.VerifyIfDocumentExistsAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<int>()))
        .Returns(Task.FromResult(true))
        .Verifiable();
    }

    private void MockGetDocumentIdentifierFieldColumnNameAsync()
    {
      MockQuery
        .Setup(x => x.GetDocumentIdentifierFieldColumnNameAsync(
          It.IsAny<IDBContext>()))
        .Returns(Task.FromResult("AZIPPER_1000001"))
        .Verifiable();
    }

    private void MockBulkInsertIntoTable()
    {
      MockUtilityQueryHelper
        .Setup(x => x.BulkInsertIntoTable(
          It.IsAny<IDBContext>(),
          It.IsAny<DataTable>(),
          It.IsAny<List<SqlBulkCopyColumnMapping>>(),
          It.IsAny<string>()))
        .Verifiable();
    }

    private void MockAddRedactionsToTableAsync()
    {
      MockArtifactQueries
        .Setup(x => x.AddRedactionsToTableAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<Helpers.Utility.IQuery>(),
          It.IsAny<string>(),
          It.IsAny<DataTable>(),
          It.IsAny<List<SqlBulkCopyColumnMapping>>()))
        .Returns(Task.FromResult(default(Task)))
        .Verifiable();
    }

    private void MockGetMappingsForWorkerQueue(List<SqlBulkCopyColumnMapping> columns)
    {
      MockUtilityQueryHelper
        .Setup(x => x.GetMappingsForWorkerQueue(
          It.IsAny<List<string>>()))
        .Returns(columns)
        .Verifiable();
    }

    private void MockCopyRecordsToExportResultsAsync()
    {
      MockQuery
        .Setup(x => x.CopyRecordsToExportResultsAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<string>()))
        .Returns(Task.FromResult(default(Task)))
        .Verifiable();
    }

    private void MockDropTableAsync()
    {
      MockQuery
        .Setup(x => x.DropTableAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<string>()))
        .Returns(Task.FromResult(default(Task)))
        .Verifiable();
    }

    private void MockRemoveBatchFromExportWorkerQueueAsync()
    {
      MockQuery
        .Setup(x => x.RemoveBatchFromExportWorkerQueueAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<string>()))
        .Returns(Task.FromResult(default(Task)))
        .Verifiable();
    }

    private void MockGetExportResultsRecordCountAsync(int remainingRedactionCount)
    {
      MockQuery
        .Setup(x => x.GetExportResultsRecordCountAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<int>()))
        .Returns(Task.FromResult(remainingRedactionCount))
        .Verifiable();
    }

    private void MockCreateExportFileAsync()
    {
      const string exportFileName = "abc";
      string exportFullFilePath = $@"C:\{exportFileName}.csv";

      MockExportFileCreator
        .Setup(x => x.CreateExportFileAsync(
          It.IsAny<string>()))
        .Returns(Task.FromResult(exportFullFilePath))
        .Verifiable();

      MockExportFileCreator
        .Setup(x => x.ExportFileName)
        .Returns(exportFileName);

      MockExportFileCreator
        .Setup(x => x.ExportFullFilePath)
        .Returns(exportFullFilePath);
    }

    private void MockGetExportResultsAsync(DataTable dtExportResults)
    {
      MockQuery
        .SetupSequence(x => x.GetExportResultsAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<int>()))
        .Returns(Task.FromResult(dtExportResults))
        .Returns(Task.FromResult((DataTable)null));
    }

    private void MockProcessExportResultsAsync()
    {
      MockExportFileCreator
        .Setup(x => x.WriteToExportFileAsync(
          It.IsAny<List<ExportResultsRecord>>()))
        .Returns(Task.FromResult(default(Task)))
        .Verifiable();
    }

    private void MockDeleteExportResultsAsync()
    {
      MockQuery
        .Setup(x => x.DeleteExportResultsAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<List<int>>()))
        .Returns(Task.FromResult(default(Task)))
        .Verifiable();
    }

    private void MockCreateMarkupUtilityFileRdoRecordAsync(int rdoArtifactId)
    {
      MockArtifactQueries
        .Setup(x => x.CreateMarkupUtilityFileRdoRecordAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<string>()))
        .Returns(Task.FromResult(rdoArtifactId))
        .Verifiable();
    }

    private void MockGetWorkspaceArtifactIdByGuidAsync(int rdoFileFieldArtifactId)
    {
      MockQuery
        .Setup(x => x.GetArtifactIdByGuidAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<Guid>()))
        .Returns(Task.FromResult(rdoFileFieldArtifactId))
        .Verifiable();
    }

    private void MockAttachFileToMarkupUtilityFileRecord()
    {
      MockArtifactQueries
        .Setup(x => x.AttachFileToMarkupUtilityFileRecord(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<int>()
          ))
        .Returns(Task.FromResult(default(Task)))
        .Verifiable();
    }

    private void MockAttachRedactionFileToExportJob()
    {
      MockArtifactQueries
        .Setup(x => x.AttachRedactionFileToExportJob(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<int>()))
        .Returns(Task.FromResult(default(Task)))
        .Verifiable();
    }

    private void MockUpdateRdoJobTextFieldAsync()
    {
      MockArtifactQueries
        .Setup(x => x.UpdateRdoJobTextFieldAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<int>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<Guid>(),
          It.IsAny<int>(),
          It.IsAny<Guid>(),
          It.IsAny<string>()))
        .Returns(Task.FromResult(default(Task)))
        .Verifiable();
    }

    private void MockDeleteExportFileAsync()
    {
      MockExportFileCreator
        .Setup(x => x.DeleteExportFileAsync())
        .Returns(Task.FromResult(default(Task)))
        .Verifiable();
    }

    private void MockGetExportWorkerRecordCountAsync(int retCount)
    {
      MockQuery
        .Setup(x => x.GetJobWorkerRecordCountAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<int>(), Constant.Tables.ExportWorkerQueue, "ExportJobArtifactID"))
        .Returns(Task.FromResult(retCount))
        .Verifiable();
    }

    private void MockInsertRowIntoExportErrorLogAsync()
    {
      MockQuery
        .Setup(x => x.InsertRowIntoJobErrorLogAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<string>(), Constant.Tables.ExportErrorLog))
        .Returns(Task.FromResult(default(Task)))
        .Verifiable();
    }

    private void MockWriteError()
    {
      MockErrorQueries
        .Setup(x => x.WriteError(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<Exception>()
          ))
        .Verifiable();
    }

    private void AssertWriteErrorWasCalled(int timesCalled)
    {
      MockErrorQueries
        .Verify(x => x.WriteError(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<Exception>()
          ),
          Times.Exactly(timesCalled));
    }

    #endregion Mocks

    #region Asserts

    private void AssertResetUnfishedJobsAsyncWasCalled(int timesCalled)
    {
      MockQuery
        .Verify(x => x.ResetUnfishedJobsAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<string>()),
          Times.Exactly(timesCalled));
    }

    private void AssertRetrieveNextBatchInExportWorkerQueueAsyncWasCalled(int timesCalled)
    {
      MockQuery
        .Verify(x => x.RetrieveNextBatchInExportWorkerQueueAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<string>()),
          Times.Exactly(timesCalled));
    }

    private void AssertRetrieveExportJobAsyncWasCalled(int timesCalled)
    {
      MockArtifactQueries
        .Verify(x => x.RetrieveExportJobAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<int>()),
          Times.Exactly(timesCalled));
    }

    private void AssertCreateExportWorkerHoldingTableAsyncWasCalled(int timesCalled)
    {
      MockQuery
        .Verify(x => x.CreateExportWorkerHoldingTableAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<string>()),
          Times.Exactly(timesCalled));
    }

    private void AssertUpdateRdoJobTextFieldAsyncWasCalled(int timesCalled)
    {
      MockArtifactQueries
        .Verify(x => x.UpdateRdoJobTextFieldAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<int>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<Guid>(),
          It.IsAny<int>(),
          It.IsAny<Guid>(),
          It.IsAny<string>()),
          Times.Exactly(timesCalled));
    }

    private void AssertVerifyIfDocumentExistsAsyncWasCalled(int timesCalled)
    {
      MockArtifactQueries
        .Verify(x => x.VerifyIfDocumentExistsAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<int>()),
          Times.Exactly(timesCalled));
    }

    private void AssertGetDocumentIdentifierFieldColumnNameAsyncWasCalled(int timesCalled)
    {
      MockQuery
        .Verify(x => x.GetDocumentIdentifierFieldColumnNameAsync(
          It.IsAny<IDBContext>()),
          Times.Exactly(timesCalled));
    }

    private void AssertRetrieveRedactionsForDocumentAsyncWasCalled(int timesCalled)
    {
      MockQuery
        .Verify(x => x.RetrieveRedactionsForDocumentAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<string>()),
          Times.Exactly(timesCalled));
    }

    private void AssertAddRedactionsToTableAsyncWasCalled(int timesCalled)
    {
      MockArtifactQueries
        .Verify(x => x.AddRedactionsToTableAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<Helpers.Utility.IQuery>(),
          It.IsAny<string>(),
          It.IsAny<DataTable>(),
          It.IsAny<List<SqlBulkCopyColumnMapping>>()),
          Times.Exactly(timesCalled));
    }

    private void AssertCopyRecordsToExportResultsAsyncWasCalled(int timesCalled)
    {
      MockQuery
        .Verify(x => x.CopyRecordsToExportResultsAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<string>()),
          Times.Exactly(timesCalled));
    }

    private void AssertDropTableAsyncWasCalled(int timesCalled)
    {
      MockQuery
        .Verify(x => x.DropTableAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<string>()),
          Times.Exactly(timesCalled));
    }

    private void AssertRemoveBatchFromExportWorkerQueueAsyncWasCalled(int timesCalled)
    {
      MockQuery
        .Verify(x => x.RemoveBatchFromExportWorkerQueueAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<string>()),
          Times.Exactly(timesCalled));
    }

    private void AssertGetExportResultsRecordCountAsyncWasCalled(int timesCalled)
    {
      MockQuery
        .Verify(x => x.GetExportResultsRecordCountAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<int>()),
          Times.Exactly(timesCalled));
    }

    private void AssertGetExportWorkerRecordCountAsyncWasCalled(int timesCalled)
    {
      MockQuery
        .Verify(x => x.GetJobWorkerRecordCountAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<int>(), Constant.Tables.ExportWorkerQueue, "ExportJobArtifactID"),
          Times.Exactly(timesCalled));
    }

    private void AssertInsertRowIntoExportErrorLogAsyncWasCalled(int timesCalled)
    {
      MockQuery
        .Verify(x => x.InsertRowIntoJobErrorLogAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<string>(), Constant.Tables.ExportErrorLog),
          Times.Exactly(timesCalled));
    }

    #endregion Asserts
  }
}
