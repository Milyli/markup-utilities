using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
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
  public class ImportManagerAgentTests
  {
    public int AgentId;
    public Mock<IQuery> MockQuery;
    public Mock<IAgentHelper> MockAgentHelper;
    public Mock<IArtifactQueries> MockArtifactQueries;
    public Mock<IImportFileParser> MockImportFileParser;
    private List<int> _resourceGroupIdList;
    public Mock<IWorkspaceQueries> MockWorkspaceQueries;
    public Mock<IErrorQueries> MockErrorQueries;
    public Mock<IMarkupTypeHelper> MockMarkupTypeHelper;
    private const string DummyFileData = "dummy file data";
    private const string GoodImportFile = "TestImportFile-GoodData.csv";
    private const string BadImportFile = "TestImportFile-BadData.csv";

    [SetUp]
    public void Setup()
    {
      AgentId = 123;
      MockQuery = new Mock<IQuery>();
      MockAgentHelper = new Mock<IAgentHelper>();
      MockArtifactQueries = new Mock<IArtifactQueries>();
      MockImportFileParser = new Mock<IImportFileParser>();
      _resourceGroupIdList = new List<int> { 10000, 20000 };
      MockWorkspaceQueries = new Mock<IWorkspaceQueries>();
      MockErrorQueries = new Mock<IErrorQueries>();
      MockMarkupTypeHelper = new Mock<IMarkupTypeHelper>();
    }

    [TearDown]
    public void TearDown()
    {
      MockQuery = null;
      MockAgentHelper = null;
      MockArtifactQueries = null;
      MockImportFileParser = null;
      _resourceGroupIdList = null;
      MockWorkspaceQueries = null;
      MockErrorQueries = null;
      MockMarkupTypeHelper = null;
    }

    #region Tests

    [Description("This will test getting a comma delimited list of resource IDs from a list of integers.")]
    [Test]
    public void GetCommaDelimitedListOfResourceIds()
    {
      // Arrange
      var resourceIdsList = new List<int> { 1000001, 1000002, 1000003 };
      const string expectedResult = "1000001,1000002,1000003";
      var importManagerDataTable = GetImportManagerDataTable(Constant.ImportJobType.IMPORT);
      MockRetrieveNextInImportManagerQueueAsync(importManagerDataTable);

      var managerJob = GetImportManagerJob();

      //Act
      var observedResult = managerJob.GetCommaDelimitedListOfResourceIds(resourceIdsList);

      //Assert
      Assert.AreEqual(expectedResult, observedResult);
    }

    [Description("When no record is picked up by the agent, should not process")]
    [Test]
    public async Task ExecuteAsync_QueueHasNoRecord_DoNotExecute()
    {
      // Arrange
      //when there is no record(s) in the queue
      var emptyImportManagerDataTable = GetEmptyImportManagerDataTable();
      MockRetrieveNextInImportManagerQueueAsync(emptyImportManagerDataTable);

      var importManagerJob = GetImportManagerJob();

      // Act
      await importManagerJob.ExecuteAsync();

      // Assert
      AssertRetrieveNextInImportManagerQueueAsyncWasCalled(1);
      AssertRetrieveImportJobAsyncWasCalled(0);
    }

    [Description("When a record is picked up by the agent, should complete validation process")]
    [Test]
    public async Task ExecuteAsync_QueueHasARecord_Validate_Job_Type_Valid_Test()
    {
      // Arrange
      //when import manager queue has record(s)
      var importManagerDataTable = GetImportManagerDataTable(Constant.ImportJobType.VALIDATE);
      MockRetrieveNextInImportManagerQueueAsync(importManagerDataTable);

      //retrieve import job
      var markupUtilityImportJob = GetMarkupUtilityImportJob(Constant.ImportJobType.IMPORT);
      MockRetrieveImportJob(markupUtilityImportJob);

      //update status of the import job to validating
      //update status of the import job to validated
      //update details of the import job
      MockUpdateRdoJobTextFieldAsync();

      //read contents of the import job file
      var streamReader = GetDummyImportFileDataFileStreamReader();
      MockGetFileFieldContentsAsync(streamReader);

      //validate contents of the import job file
      MockValidateFileContentsAsync();

      //deletes the completed job from the Manager Queue
      MockRemoveRecordFromTableByIdAsync();

      var importManagerJob = GetImportManagerJob();

      // Act
      await importManagerJob.ExecuteAsync();

      // Assert
      AssertRetrieveNextInImportManagerQueueAsyncWasCalled(1);
      AssertRetrieveImportJobAsyncWasCalled(1);
      AssertUpdateRdoJobTextFieldAsyncWasCalled(4);
      AssertGetFileFieldContentsAsyncWasCalled(1);
      AssertValidateFileContentsAsyncWasCalled(1);
      AssertRemoveRecordFromTableByIdAsyncWasCalled(1);
    }

    [Description("When a record is picked up by the agent with good import file data, should complete validation process")]
    [Test]
    public async Task ExecuteAsync_QueueHasARecord_Validate_Job_Type_Good_Import_File_Data_Valid_Test()
    {
      // Arrange
      //when import manager queue has a record(s)
      var dataTable = GetImportManagerDataTable(Constant.ImportJobType.VALIDATE);
      MockRetrieveNextInImportManagerQueueAsync(dataTable);

      //retrieve import job
      var markupUtilityImportJob = GetMarkupUtilityImportJob(Constant.ImportJobType.IMPORT);
      MockRetrieveImportJob(markupUtilityImportJob);

      //update status of the import job to validating
      //update status of the import job to validated
      //update details of the import job
      MockUpdateRdoJobTextFieldAsync();

      //read contents of the import job file
      var streamReader = GetGoodImportFileDataFileStreamReader();
      MockGetFileFieldContentsAsync(streamReader);

      //create an instance of ImportFileParser to parse the actual import file
      IImportFileParser importFileParser = new ImportFileParser();

      //deletes the completed job from the Manager Queue
      MockRemoveRecordFromTableByIdAsync();

      //pass created importFileParser
      var importManagerJob = GetImportManagerJob(importFileParser);

      // Act
      await importManagerJob.ExecuteAsync();

      // Assert
      AssertRetrieveNextInImportManagerQueueAsyncWasCalled(1);
      AssertRetrieveImportJobAsyncWasCalled(1);
      AssertUpdateRdoJobTextFieldAsyncWasCalled(4);
      AssertGetFileFieldContentsAsyncWasCalled(1);
      AssertInsertRowIntoImportErrorLogAsyncWasNeverCalled();
      AssertWriteErrorWasNeverCalled();
      AssertRemoveRecordFromTableByIdAsyncWasCalled(1);
    }

    [Description("When a record is picked up by the agent with bad import file data, should fail validation process")]
    [Test]
    public async Task ExecuteAsync_QueueHasARecord_Validate_Job_Type_Bad_Import_File_Data_Valid_Test()
    {
      // Arrange

      //when import manager queue has a record(s)
      var dataTable = GetImportManagerDataTable(Constant.ImportJobType.VALIDATE);
      MockRetrieveNextInImportManagerQueueAsync(dataTable);

      //retrieve import job
      var markupUtilityImportJob = GetMarkupUtilityImportJob(Constant.ImportJobType.IMPORT);
      MockRetrieveImportJob(markupUtilityImportJob);

      //update status of the import job to validating
      //update status of the import job to validated
      //update details of the import job
      MockUpdateRdoJobTextFieldAsync();

      //read contents of the import job file
      var streamReader = GetBadImportFileDataFileStreamReader();
      MockGetFileFieldContentsAsync(streamReader);

      //create an instance of ImportFileParser to parse the actual import file
      IImportFileParser importFileParser = new ImportFileParser();

      //deletes the completed job from the Manager Queue
      MockRemoveRecordFromTableByIdAsync();

      //pass created importFileParser
      var importManagerJob = GetImportManagerJob(importFileParser);

      // Act
      await importManagerJob.ExecuteAsync();

      // Assert
      AssertRetrieveNextInImportManagerQueueAsyncWasCalled(1);
      AssertRetrieveImportJobAsyncWasCalled(1);
      AssertUpdateRdoJobTextFieldAsyncWasCalled(4);
      AssertGetFileFieldContentsAsyncWasCalled(1);
      AssertInsertRowIntoImportErrorLogAsyncWasCalled(1);
      AssertWriteErrorWasCalled(1);
      AssertRemoveRecordFromTableByIdAsyncWasCalled(1);
    }

    [Description("When a record is picked up by the agent, should fail validation process")]
    [Test]
    public async Task ExecuteAsync_QueueHasARecord_Validate_Job_Type_Validation_Error_Test()
    {
      // Arrange
      //when import manager queue has a record(s)
      var dataTable = GetImportManagerDataTable(Constant.ImportJobType.VALIDATE);
      MockRetrieveNextInImportManagerQueueAsync(dataTable);

      //retrieve import job
      var markupUtilityImportJob = GetMarkupUtilityImportJob(Constant.ImportJobType.IMPORT);
      MockRetrieveImportJob(markupUtilityImportJob);

      //update status of the import job to validating
      //update status of the import job to validated
      //update details of the import job
      MockUpdateRdoJobTextFieldAsync();

      //read contents of the import job file
      var streamReader = GetDummyImportFileDataFileStreamReader();
      MockGetFileFieldContentsAsync(streamReader);

      //validate contents of the import job file
      MockValidateFileContentsAsync_ThrowsError();

      //log error to Error table
      MockInsertRowIntoImportErrorLogAsync();

      //write error to Errors tab
      MockWriteError();

      //deletes the completed job from the Manager Queue
      MockRemoveRecordFromTableByIdAsync();

      var importManagerJob = GetImportManagerJob();

      // Act
      await importManagerJob.ExecuteAsync();

      // Assert
      AssertRetrieveNextInImportManagerQueueAsyncWasCalled(1);
      AssertRetrieveImportJobAsyncWasCalled(1);
      AssertUpdateRdoJobTextFieldAsyncWasCalled(4);
      AssertGetFileFieldContentsAsyncWasCalled(1);
      AssertValidateFileContentsAsyncWasCalled(1);
      AssertInsertRowIntoImportErrorLogAsyncWasCalled(1);
      AssertWriteErrorWasCalled(1);
      AssertRemoveRecordFromTableByIdAsyncWasCalled(1);
    }

    [Description("When a record is picked up by the agent, should complete import process")]
    [Test]
    public async Task ExecuteAsync_QueueHasARecord_Import_Job_Type_Valid_Test()
    {
      // Arrange
      //when import manager queue has a record(s)
      var dataTable = GetImportManagerDataTable(Constant.ImportJobType.IMPORT);
      MockRetrieveNextInImportManagerQueueAsync(dataTable);

      //retrieve import job
      var markupUtilityImportJob = GetMarkupUtilityImportJob(Constant.ImportJobType.IMPORT);
      MockRetrieveImportJob(markupUtilityImportJob);

      //update status of the import job to validating
      //update status of the import job to validated
      //update details of the import job
      MockUpdateRdoJobTextFieldAsync();

      //read contents of the import job file
      var streamReader = GetDummyImportFileDataFileStreamReader();
      MockGetFileFieldContentsAsync(streamReader);

      //parse contents of the import job file
      MockParseFileContentsAsync();

      //deletes the completed job from the Manager Queue
      MockRemoveRecordFromTableByIdAsync();

      var importManagerJob = GetImportManagerJob();

      // Act
      await importManagerJob.ExecuteAsync();

      // Assert
      AssertRetrieveNextInImportManagerQueueAsyncWasCalled(1);
      AssertRetrieveImportJobAsyncWasCalled(1);
      AssertUpdateRdoJobTextFieldAsyncWasCalled(4);
      AssertGetFileFieldContentsAsyncWasCalled(1);
      AssertParseFileContentsAsyncWasCalled(1);
      AssertRemoveRecordFromTableByIdAsyncWasCalled(1);
    }

    [Description("When a record is picked up by the agent with good import file data, should complete import process")]
    [Test]
    public async Task ExecuteAsync_QueueHasARecord_Import_Job_Type_Good_Import_File_Data_Valid_Test()
    {
      // Arrange
      //when import manager queue has a record(s)
      var dataTable = GetImportManagerDataTable(Constant.ImportJobType.IMPORT);
      MockRetrieveNextInImportManagerQueueAsync(dataTable);

      //Create import manager holding table
      MockCreateImportManagerHoldingTableAsync();

      //retrieve import job
      var markupUtilityImportJob = GetMarkupUtilityImportJob(Constant.ImportJobType.IMPORT);
      MockRetrieveImportJob(markupUtilityImportJob);

      //update status of the import job to validating
      //update status of the import job to validated
      //update details of the import job
      MockUpdateRdoJobTextFieldAsync();

      //read contents of the import job file
      var streamReader = GetGoodImportFileDataFileStreamReader();
      MockGetFileFieldContentsAsync(streamReader);

      //create an instance of ImportFileParser to parse the actual import file
      IImportFileParser importFileParser = new ImportFileParser();

      //create an instance of IMarkupTypeHelper to parse the actual import file
      IMarkupTypeHelper markupTypeHelper = new MarkupTypeHelper();

      //get resource group id to insert into worker queue
      MockGetResourcePoolAsync();

      //insert import file redaction record into import manager holding table
      MockInsertRowsIntoImportManagerHoldingTableAsync();

      //copy data from import manager holding table to import worker queue table
      MockCopyDataFromImportManagerHoldingTableIntoImportWorkerQueueTableAsync();

      //drop import manager holding table
      MockDropTableAsync();

      //deletes the completed job from the Manager Queue
      MockRemoveRecordFromTableByIdAsync();

      //pass created importFileParser and markupTypeHelper
      var importManagerJob = GetImportManagerJob(importFileParser, markupTypeHelper);

      // Act
      await importManagerJob.ExecuteAsync();

      // Assert
      AssertRetrieveNextInImportManagerQueueAsyncWasCalled(1);
      AssertCreateImportManagerHoldingTableAsyncWasCalled(1);
      AssertRetrieveImportJobAsyncWasCalled(1);
      AssertUpdateRdoJobTextFieldAsyncWasCalled(4);
      AssertGetFileFieldContentsAsyncWasCalled(1);
      AssertCopyDataFromImportManagerHoldingTableIntoImportWorkerQueueTableAsyncWasCalled(1);
      AssertGetResourcePoolAsyncWasCalled(1);
      AssertInsertRowsIntoImportManagerHoldingTableAsyncWasCalled(1);
      AssertInsertRowIntoImportErrorLogAsyncWasNeverCalled();
      AssertWriteErrorWasNeverCalled();
      AssertDropTableAsyncWasCalled(1);
      AssertRemoveRecordFromTableByIdAsyncWasCalled(1);
    }

    [Description("When a record is picked up by the agent with bad import file data, should fail import process")]
    [Test]
    public async Task ExecuteAsync_QueueHasARecord_Import_Job_Type_Bad_Import_File_Data_Valid_Test()
    {
      // Arrange
      //when import manager queue has a record(s)
      var dataTable = GetImportManagerDataTable(Constant.ImportJobType.IMPORT);
      MockRetrieveNextInImportManagerQueueAsync(dataTable);

      //Create import manager holding table
      MockCreateImportManagerHoldingTableAsync();

      //retrieve import job
      var markupUtilityImportJob = GetMarkupUtilityImportJob(Constant.ImportJobType.IMPORT);
      MockRetrieveImportJob(markupUtilityImportJob);

      //update status of the import job to validating
      //update status of the import job to validated
      //update details of the import job
      MockUpdateRdoJobTextFieldAsync();

      //read contents of the import job file
      var streamReader = GetBadImportFileDataFileStreamReader();
      MockGetFileFieldContentsAsync(streamReader);

      //create an instance of ImportFileParser to parse the actual import file
      IImportFileParser importFileParser = new ImportFileParser();

      //create an instance of IMarkupTypeHelper to parse the actual import file
      IMarkupTypeHelper markupTypeHelper = new MarkupTypeHelper();

      //get resource group id to insert into worker queue
      MockGetResourcePoolAsync();

      //insert import file redaction record into import manager holding table
      MockInsertRowsIntoImportManagerHoldingTableAsync();

      //copy data from import manager holding table to import worker queue table
      MockCopyDataFromImportManagerHoldingTableIntoImportWorkerQueueTableAsync();

      //drop import manager holding table
      MockDropTableAsync();

      //deletes the completed job from the Manager Queue
      MockRemoveRecordFromTableByIdAsync();

      //pass created importFileParser and markupTypeHelper
      var importManagerJob = GetImportManagerJob(importFileParser, markupTypeHelper);

      // Act
      await importManagerJob.ExecuteAsync();

      // Assert
      AssertRetrieveNextInImportManagerQueueAsyncWasCalled(1);
      AssertCreateImportManagerHoldingTableAsyncWasCalled(1);
      AssertRetrieveImportJobAsyncWasCalled(1);
      AssertUpdateRdoJobTextFieldAsyncWasCalled(4);
      AssertGetFileFieldContentsAsyncWasCalled(1);
      AssertCopyDataFromImportManagerHoldingTableIntoImportWorkerQueueTableAsyncWasNeverCalled();
      AssertGetResourcePoolAsyncWasNeverCalled();
      AssertInsertRowsIntoImportManagerHoldingTableAsyncWasNeverCalled();
      AssertInsertRowIntoImportErrorLogAsyncWasCalled(1);
      AssertWriteErrorWasCalled(1);
      AssertDropTableAsyncWasCalled(1);
      AssertRemoveRecordFromTableByIdAsyncWasCalled(1);
    }

    [Description("When a record is picked up by the agent, should fail import process")]
    [Test]
    public async Task ExecuteAsync_QueueHasARecord_Import_Job_Type_Parse_Error_Test()
    {
      // Arrange
      //when import manager queue has a record(s)
      var dataTable = GetImportManagerDataTable(Constant.ImportJobType.IMPORT);
      MockRetrieveNextInImportManagerQueueAsync(dataTable);

      //retrieve import job
      var markupUtilityImportJob = GetMarkupUtilityImportJob(Constant.ImportJobType.IMPORT);
      MockRetrieveImportJob(markupUtilityImportJob);

      //update status of the import job to validating
      //update status of the import job to validated
      //update details of the import job
      MockUpdateRdoJobTextFieldAsync();

      //read contents of the import job file
      var streamReader = GetDummyImportFileDataFileStreamReader();
      MockGetFileFieldContentsAsync(streamReader);

      //parse contents of the import job file
      MockParseFileContentsAsync_ThrowsError();

      //log error to Error table
      MockInsertRowIntoImportErrorLogAsync();

      //write error to Errors tab
      MockWriteError();

      //deletes the completed job from the Manager Queue
      MockRemoveRecordFromTableByIdAsync();

      var importManagerJob = GetImportManagerJob();

      // Act
      await importManagerJob.ExecuteAsync();

      // Assert
      AssertRetrieveNextInImportManagerQueueAsyncWasCalled(1);
      AssertRetrieveImportJobAsyncWasCalled(1);
      AssertUpdateRdoJobTextFieldAsyncWasCalled(4);
      AssertGetFileFieldContentsAsyncWasCalled(1);
      AssertParseFileContentsAsyncWasCalled(1);
      AssertInsertRowIntoImportErrorLogAsyncWasCalled(1);
      AssertWriteErrorWasCalled(1);
      AssertRemoveRecordFromTableByIdAsyncWasCalled(1);
    }

    #endregion Tests

    #region Test Helpers

    private ImportManagerJob GetImportManagerJob(IImportFileParser importFileParser = null, IMarkupTypeHelper markupTypeHelper = null)
    {
      var importManagerJob = new ImportManagerJob(
        AgentId,
        MockAgentHelper.Object,
        MockQuery.Object,
        new DateTime(2016, 01, 25, 01, 00, 00),
        _resourceGroupIdList,
        MockArtifactQueries.Object,
        importFileParser ?? MockImportFileParser.Object,
        MockWorkspaceQueries.Object,
        MockErrorQueries.Object,
        markupTypeHelper ?? MockMarkupTypeHelper.Object);

      return importManagerJob;
    }

    private static DataTable GetEmptyImportManagerDataTable()
    {
      return null;
    }

    private DataTable GetImportManagerDataTable(string jobType)
    {
      var dataTable = new DataTable("Test Import Manager Table");
      dataTable.Columns.Add(new DataColumn("ID", typeof(int)) { AllowDBNull = false });
      dataTable.Columns.Add(new DataColumn("TimeStampUTC", typeof(DateTime)) { AllowDBNull = false });
      dataTable.Columns.Add(new DataColumn("WorkspaceArtifactID", typeof(int)) { AllowDBNull = false });
      dataTable.Columns.Add(new DataColumn("QueueStatus", typeof(int)) { AllowDBNull = false });
      dataTable.Columns.Add(new DataColumn("AgentID", typeof(int)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("ImportJobArtifactID", typeof(int)) { AllowDBNull = false });
      dataTable.Columns.Add(new DataColumn("JobType", typeof(string)) { AllowDBNull = false });
      dataTable.Columns.Add(new DataColumn("ResourceGroupID", typeof(int)) { AllowDBNull = false });

      dataTable.Rows.Add(123, DateTime.UtcNow, 123, 0, null as int?, 123, jobType, 123);

      return dataTable;
    }

    private static MarkupUtilityImportJob GetMarkupUtilityImportJob(string jobType)
    {
      var retVal = new MarkupUtilityImportJob(
        123,
        string.Empty,
        123,
        Constant.MarkupSubTypeCategory.SupportedMarkupUtilityTypes,
        false,
        123,
        string.Empty,
        string.Empty,
        123,
        123,
        jobType,
        123);
      return retVal;
    }

    private static StreamReader GetDummyImportFileDataFileStreamReader(string fileData = DummyFileData)
    {
      var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(fileData));
      var streamReader = new StreamReader(memoryStream);
      return streamReader;
    }

    private static StreamReader GetGoodImportFileDataFileStreamReader()
    {
      var streamReader = new StreamReader(GetFilePath(GoodImportFile));
      return streamReader;
    }

    private static StreamReader GetBadImportFileDataFileStreamReader()
    {
      var streamReader = new StreamReader(GetFilePath(BadImportFile));
      return streamReader;
    }

    private static string GetFilePath(string fileName)
    {
      var directoryName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.RelativeSearchPath ?? "");
      {
        var rootDirectory = Path.Combine(directoryName, "TestData");
        return Path.Combine(rootDirectory, fileName);
      }
    }

    #endregion Test Helpers

    #region Mocks

    private void MockRemoveRecordFromTableByIdAsync()
    {
      MockQuery
        .Setup(x => x.RemoveRecordFromTableByIdAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<string>(),
          It.IsAny<int>()))
        .Returns(Task.FromResult(default(Task)))
        .Verifiable();
    }

    private void MockValidateFileContentsAsync()
    {
      MockImportFileParser
        .Setup(x => x.ValidateFileContentsAsync(
          It.IsAny<StreamReader>()))
        .Returns(Task.FromResult(default(Task)))
        .Verifiable();
    }

    private void MockValidateFileContentsAsync_ThrowsError()
    {
      MockImportFileParser
        .Setup(x => x.ValidateFileContentsAsync(
          It.IsAny<StreamReader>()))
        .Throws<MarkupUtilityException>()
        .Verifiable();
    }

    private void MockGetFileFieldContentsAsync(StreamReader streamReader)
    {
      MockArtifactQueries
        .Setup(x => x.GetFileFieldContentsAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<string>()))
        .Returns(Task.FromResult(streamReader))
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

    private void MockRetrieveImportJob(MarkupUtilityImportJob markupUtilityImportJob)
    {
      MockArtifactQueries
        .Setup(x => x.RetrieveImportJobAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<int>()))
        .Returns(Task.FromResult(markupUtilityImportJob))
        .Verifiable();
    }

    private void MockRetrieveNextInImportManagerQueueAsync(DataTable dataTable)
    {
      MockQuery
        .Setup(x => x.RetrieveNextInImportManagerQueueAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<string>()))
        .Returns(Task.FromResult(dataTable))
        .Verifiable();
    }

    private void MockWriteError()
    {
      MockErrorQueries
        .Setup(x => x.WriteError(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<Exception>()))
        .Verifiable();
    }

    private void MockInsertRowIntoImportErrorLogAsync()
    {
      MockQuery
        .Setup(x => x.InsertRowIntoImportErrorLogAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<string>()))
        .Returns(Task.FromResult(default(Task)))
        .Verifiable();
    }

    private void MockParseFileContentsAsync()
    {
      MockImportFileParser
        .Setup(x => x.ParseFileContentsAsync(
          It.IsAny<StreamReader>(),
          It.IsAny<Func<ImportFileRecord, Task<Boolean>>>(),
          It.IsAny<Func<Task<Boolean>>>()))
        .Returns(Task.FromResult(default(Task)))
        .Verifiable();
    }

    private void MockParseFileContentsAsync_ThrowsError()
    {
      MockImportFileParser
        .Setup(x => x.ParseFileContentsAsync(
          It.IsAny<StreamReader>(),
          It.IsAny<Func<ImportFileRecord, Task<Boolean>>>(),
          It.IsAny<Func<Task<Boolean>>>()))
        .Throws<MarkupUtilityException>()
        .Verifiable();
    }

    private void MockGetResourcePoolAsync()
    {
      MockWorkspaceQueries
        .Setup(x => x.GetResourcePoolAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>()))
        .Returns(Task.FromResult(123))
        .Verifiable();
    }

    private void MockInsertRowsIntoImportManagerHoldingTableAsync()
    {
      MockQuery
        .Setup(x => x.InsertRowIntoImportManagerHoldingTableAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<ImportFileRecord>(),
          It.IsAny<Boolean>(),
          It.IsAny<string>()))
        .Returns(Task.FromResult(default(Task)))
        .Verifiable();
    }

    private void MockCreateImportManagerHoldingTableAsync()
    {
      MockQuery
        .Setup(x => x.CreateImportManagerHoldingTableAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<string>()))
        .Returns(Task.FromResult(default(Task)))
        .Verifiable();
    }

    private void MockCopyDataFromImportManagerHoldingTableIntoImportWorkerQueueTableAsync()
    {
      MockQuery
        .Setup(x => x.BulkCopyDataFromImportManagerHoldingTableIntoImportWorkerQueueTableAsync(
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

    #endregion Mocks

    #region Asserts

    private void AssertRetrieveImportJobAsyncWasCalled(int timesCalled)
    {
      MockArtifactQueries
        .Verify(x => x.RetrieveImportJobAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<int>()),
          Times.Exactly(timesCalled));
    }

    private void AssertRetrieveNextInImportManagerQueueAsyncWasCalled(int timesCalled)
    {
      MockQuery
        .Verify(x => x.RetrieveNextInImportManagerQueueAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<string>()),
          Times.Exactly(timesCalled));
    }

    private void AssertRemoveRecordFromTableByIdAsyncWasCalled(int timesCalled)
    {
      MockQuery
        .Verify(x => x.RemoveRecordFromTableByIdAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<string>(),
          It.IsAny<int>()),
          Times.Exactly(timesCalled));
    }

    private void AssertValidateFileContentsAsyncWasCalled(int timesCalled)
    {
      MockImportFileParser
        .Verify(x => x.ValidateFileContentsAsync(
          It.IsAny<StreamReader>()),
          Times.Exactly(timesCalled));
    }

    private void AssertGetFileFieldContentsAsyncWasCalled(int timesCalled)
    {
      MockArtifactQueries
        .Verify(x => x.GetFileFieldContentsAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
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

    private void AssertParseFileContentsAsyncWasCalled(int timesCalled)
    {
      MockImportFileParser
        .Verify(x => x.ParseFileContentsAsync(
          It.IsAny<StreamReader>(),
          It.IsAny<Func<ImportFileRecord, Task<bool>>>(),
          It.IsAny<Func<Task<bool>>>()),
          Times.Exactly(timesCalled));
    }

    private void AssertWriteErrorWasCalled(int timesCalled)
    {
      MockErrorQueries
        .Verify(x => x.WriteError(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<Exception>()),
          Times.Exactly(timesCalled));
    }

    private void AssertWriteErrorWasNeverCalled()
    {
      MockErrorQueries
        .Verify(x => x.WriteError(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<Exception>()),
          Times.Never);
    }

    private void AssertInsertRowIntoImportErrorLogAsyncWasCalled(int timesCalled)
    {
      MockQuery
        .Verify(x => x.InsertRowIntoImportErrorLogAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<string>()),
          Times.Exactly(timesCalled));
    }

    private void AssertInsertRowIntoImportErrorLogAsyncWasNeverCalled()
    {
      MockQuery
        .Verify(x => x.InsertRowIntoImportErrorLogAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<string>()),
          Times.Never);
    }

    private void AssertGetResourcePoolAsyncWasCalled(int timesCalled)
    {
      MockWorkspaceQueries
        .Verify(x => x.GetResourcePoolAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>()),
          Times.AtLeast(timesCalled));
    }

    private void AssertGetResourcePoolAsyncWasNeverCalled()
    {
      MockWorkspaceQueries
        .Verify(x => x.GetResourcePoolAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>()),
          Times.Never);
    }

    private void AssertInsertRowsIntoImportManagerHoldingTableAsyncWasCalled(int timesCalled)
    {
      MockQuery
        .Verify(x => x.InsertRowIntoImportManagerHoldingTableAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<ImportFileRecord>(),
          It.IsAny<bool>(),
          It.IsAny<string>()),
          Times.AtLeast(timesCalled));
    }

    private void AssertInsertRowsIntoImportManagerHoldingTableAsyncWasNeverCalled()
    {
      MockQuery
        .Verify(x => x.InsertRowIntoImportManagerHoldingTableAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<ImportFileRecord>(),
          It.IsAny<bool>(),
          It.IsAny<string>()),
          Times.Never);
    }

    private void AssertCreateImportManagerHoldingTableAsyncWasCalled(int timesCalled)
    {
      MockQuery
        .Verify(x => x.CreateImportManagerHoldingTableAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<string>()),
          Times.Exactly(timesCalled));
    }

    private void AssertCopyDataFromImportManagerHoldingTableIntoImportWorkerQueueTableAsyncWasCalled(int timesCalled)
    {
      MockQuery
        .Verify(x => x.BulkCopyDataFromImportManagerHoldingTableIntoImportWorkerQueueTableAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<string>()),
          Times.Exactly(timesCalled));
    }

    private void AssertCopyDataFromImportManagerHoldingTableIntoImportWorkerQueueTableAsyncWasNeverCalled()
    {
      MockQuery
        .Verify(x => x.BulkCopyDataFromImportManagerHoldingTableIntoImportWorkerQueueTableAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<string>()),
          Times.Never);
    }

    private void AssertDropTableAsyncWasCalled(int timesCalled)
    {
      MockQuery
        .Verify(x => x.DropTableAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<string>()),
          Times.Exactly(timesCalled));
    }

    #endregion Asserts
  }
}
