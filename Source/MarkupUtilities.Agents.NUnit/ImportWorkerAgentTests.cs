using System;
using System.Collections.Generic;
using System.Data;
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
  public class ImportWorkerAgentTests
  {
    public int AgentId;
    public Mock<IQuery> MockQuery;
    public Mock<IAgentHelper> MockAgentHelper;
    private List<int> _resourceGroupIdList;
    public Mock<IErrorQueries> MockErrorQueries;
    public Mock<IArtifactQueries> MockArtifactQueries;
    public Mock<IAuditRecordHelper> MockAuditRecordHelper;
    public Mock<IMarkupTypeHelper> MockMarkupTypeHelper;
    private ImportWorkerJob _workerJob;

    [SetUp]
    public void Setup()
    {
      AgentId = 1234567;
      MockQuery = new Mock<IQuery>();
      MockAgentHelper = new Mock<IAgentHelper>();
      _resourceGroupIdList = new List<int> { 10000, 20000 };
      MockErrorQueries = new Mock<IErrorQueries>();
      MockArtifactQueries = new Mock<IArtifactQueries>();
      MockAuditRecordHelper = new Mock<IAuditRecordHelper>();
      MockMarkupTypeHelper = new Mock<IMarkupTypeHelper>();
      _workerJob = GetImportWorkerJob();
    }

    [TearDown]
    public void Teardown()
    {
      MockQuery = null;
      MockAgentHelper = null;
      _resourceGroupIdList = null;
      MockErrorQueries = null;
      MockArtifactQueries = null;
      MockAuditRecordHelper = null;
      MockMarkupTypeHelper = null;
      _workerJob = null;
    }

    #region Tests

    [Description("This will test getting a comma delimited list of resource IDs from a list of integers.")]
    [Test]
    public void GetCommaDelimitedListOfResourceIds()
    {
      // Arrange
      var resourceIdsList = new List<int> { 1000001, 1000002, 1000003 };
      const string expectedResult = "1000001,1000002,1000003";
      var importManagerDataTable = GetImportWorkerQueueDataTableWithOneRecord(Constant.ImportJobType.IMPORT, Constant.MarkupType.Redaction.VALUE, false);
      MockRetrieveNextBatchInImportWorkerQueueAsync(importManagerDataTable);

      //Act
      var observedResult = _workerJob.GetCommaDelimitedListOfResourceIds(resourceIdsList);

      //Assert
      Assert.AreEqual(expectedResult, observedResult);
    }

    [Description("When no record is picked up by the agent, should not process")]
    [Test]
    public async Task ExecuteAsync_QueueHasNoRecord_DoNotExecute()
    {
      // Arrange
      //when import manager queue does not have any record(s)
      var emptyImportWorkerDataTable = GetEmptyImportWorkerQueueDataTable();
      MockMethodsForWhenNoRecordExistsInQueue(emptyImportWorkerDataTable);

      // Act
      await _workerJob.ExecuteAsync();

      // Assert
      AssertResetUnfishedJobsAsyncWasCalled(1);
      AssertRetrieveNextBatchInImportWorkerQueueAsyncWasCalled(1);
      AssertDropTableAsync(1);
    }

    [Description("When a record is picked up by the agent, should complete execution process with redaction insert")]
    [Test]
    [TestCase(Constant.MarkupType.Redaction.VALUE)]
    [TestCase(Constant.MarkupType.Highlight.VALUE)]
    public async Task ExecuteAsync_ImportJob_QueueHasOneRecord_DoesNotSkip(int markupType)
    {
      // Arrange
      const bool skipDuplicateRedactions = false;
      //when import manager queue has one record
      var importWorkerDataTable = GetImportWorkerQueueDataTableWithOneRecord(Constant.ImportJobType.IMPORT, markupType, skipDuplicateRedactions);
      var markupUtilityImportJob = GetMarkupUtilityImportJob(Constant.ImportJobType.IMPORT, skipDuplicateRedactions);
      MockMethodsForWhenRecordExistsInQueue_ImportJobType(importWorkerDataTable, markupUtilityImportJob);

      // Act
      await _workerJob.ExecuteAsync();

      // Assert
      AssertResetUnfishedJobsAsyncWasCalled(1);
      AssertRetrieveNextBatchInImportWorkerQueueAsyncWasCalled(1);
      AssertUpdateRdoJobTextFieldAsyncWasCalled(4);
      AssertRetrieveImportJobAsyncWasCalled(2);
      AssertRetreiveMarkupSetNameAsyncWasCalled(1);
      AssertRetreiveMarkupSetMultipleChoiceFieldTypeIdAsyncWasCalled(1);
      AssertQueryAllMarkupSetMultipleChoiceFieldValuesAsyncWasCalled(1);
      AssertGetDocumentIdentifierFieldNameAsyncWasCalled(1);
      AssertDoesDocumentExistAsyncWasCalled(1);
      AssertRetrieveDocumentArtifactIdAsyncWasCalled(1);
      AssertDoesDocumentHasImagesAsyncWasCalled(1);
      AssertGetFileGuidForDocumentAsyncWasCalled(1);
      AssertVerifyIfMarkupSetExistsAsyncWasCalled(1);
      AssertDoesRedactionExistAsyncWasCalled(1);
      AssertInsertRowIntoRedactionTableAsyncWasCalled(1);
      AssertUpdateMarkupSetMultipleChoiceFieldAsyncWasCalled(1);
      AssertCreateRedactionAuditRecordAsyncWasCalled(1);
      AssertGetMarkupSubTypeNameAsyncWascalled(1);
      AssertCreateMarkupUtilityHistoryRecordAsyncCompletedStatusWasCalled(1);
      AssertRetrieveImportJobRedactionCountFieldValueAsyncWasCalled(3);
      AssertUpdateImportJobRedactionCountFieldValueAsyncWasCalled(3);
      AssertRemoveBatchFromImportWorkerQueueAsyncWasCalled(1);
      AssertVerifyIfImportWorkerQueueContainsRecordsForJobAsyncWasCalled(1);
      AssertDropTableAsync(1);
    }

    [Description("When a record is picked up by the agent, should complete execution process with skip redaction insert")]
    [Test]
    [TestCase(Constant.MarkupType.Redaction.VALUE)]
    [TestCase(Constant.MarkupType.Highlight.VALUE)]
    public async Task ExecuteAsync_ImportJob_QueueHasOneRecord_DoesSkip(int markupType)
    {
      // Arrange
      const bool skipDuplicateRedactions = true;
      //when import manager queue has one record
      var importWorkerDataTable = GetImportWorkerQueueDataTableWithOneRecord(Constant.ImportJobType.IMPORT, markupType, skipDuplicateRedactions);
      var markupUtilityImportJob = GetMarkupUtilityImportJob(Constant.ImportJobType.IMPORT, skipDuplicateRedactions);
      MockMethodsForWhenRecordExistsInQueue_ImportJobType(importWorkerDataTable, markupUtilityImportJob);

      // Act
      await _workerJob.ExecuteAsync();

      // Assert
      AssertResetUnfishedJobsAsyncWasCalled(1);
      AssertRetrieveNextBatchInImportWorkerQueueAsyncWasCalled(1);
      AssertUpdateRdoJobTextFieldAsyncWasCalled(4);
      AssertRetrieveImportJobAsyncWasCalled(2);
      AssertRetreiveMarkupSetNameAsyncWasCalled(1);
      AssertRetreiveMarkupSetMultipleChoiceFieldTypeIdAsyncWasCalled(1);
      AssertQueryAllMarkupSetMultipleChoiceFieldValuesAsyncWasCalled(1);
      AssertGetDocumentIdentifierFieldNameAsyncWasCalled(1);
      AssertDoesDocumentExistAsyncWasCalled(1);
      AssertRetrieveDocumentArtifactIdAsyncWasCalled(1);
      AssertDoesDocumentHasImagesAsyncWasCalled(1);
      AssertGetFileGuidForDocumentAsyncWasCalled(1);
      AssertVerifyIfMarkupSetExistsAsyncWasCalled(1);
      AssertDoesRedactionExistAsyncWasCalled(1);
      AssertGetMarkupSubTypeNameAsyncWascalled(1);
      AssertCreateMarkupUtilityHistoryRecordAsyncSkippedStatusWasCalled(1);
      AssertRetrieveImportJobRedactionCountFieldValueAsyncWasCalled(3);
      AssertUpdateImportJobRedactionCountFieldValueAsyncWasCalled(3);
      AssertRemoveBatchFromImportWorkerQueueAsyncWasCalled(1);
      AssertVerifyIfImportWorkerQueueContainsRecordsForJobAsyncWasCalled(1);
      AssertDropTableAsync(1);

      //for skip, these should not be called
      AssertInsertRowIntoRedactionTableAsyncWasCalled(0);
      AssertUpdateMarkupSetMultipleChoiceFieldAsyncWasCalled(0);
      AssertCreateRedactionAuditRecordAsyncWasCalled(0);
    }

    [Description("When an error occurs when retrieving a record from the import worker queue, the exception has to be logged.")]
    [Test]
    public async Task ExecuteAsync_ImportJob_RetrieveNext_Throws_Exception()
    {
      // Arrange
      MockResetUnfishedJobsAsync_ThrowsException();
      MockUpdateRdoJobTextFieldAsync();
      MockInsertRowIntoImportErrorLogAsync();
      MockWriteError();

      // Act
      await _workerJob.ExecuteAsync();

      // Assert
      AssertResetUnfishedJobsAsyncWasCalled(1);
      AssertUpdateRdoJobTextFieldAsyncWasCalled(2);
      AssertInsertRowIntoImportErrorLogAsyncWasCalled(1);
      AssertkWriteErrorWasCalled(1);
      AssertDropTableAsync(1);
    }

    [Description("When an error occurs when inserting a redaction to the redaction table, a failure history record has to be created")]
    [Test]
    [TestCase(Constant.MarkupType.Redaction.VALUE)]
    [TestCase(Constant.MarkupType.Highlight.VALUE)]
    public async Task ExecuteAsync_ImportJob_InsertRedaction_Throws_Exception(int markupType)
    {
      // Arrange
      const bool skipDuplicateRedactions = false;
      //when import manager queue has one record
      var importWorkerDataTable = GetImportWorkerQueueDataTableWithOneRecord(Constant.ImportJobType.IMPORT, markupType, skipDuplicateRedactions);
      var markupUtilityImportJob = GetMarkupUtilityImportJob(Constant.ImportJobType.IMPORT, skipDuplicateRedactions);
      MockMethodsForWhenRecordExistsInQueue_ImportJobType(importWorkerDataTable, markupUtilityImportJob);

      //override previous mock for InsertRowIntoRedactionTableAsync to throw Exception
      MockInsertRowIntoRedactionTableAsync_Throws_Exception();

      // Act
      await _workerJob.ExecuteAsync();

      // Assert
      AssertMethodsForWhenRecordExistsInQueue_ErrorOccurs_WhenProcessingSingleRedaction();
    }

    [Description("When inserting a redaction to the redaction table if returned redaction id is negative, a failure history record has to be created")]
    [Test]
    [TestCase(Constant.MarkupType.Redaction.VALUE)]
    [TestCase(Constant.MarkupType.Highlight.VALUE)]
    public async Task ExecuteAsync_ImportJob_InsertRedaction_Returns_Negative_RedactionId(int markupType)
    {
      // Arrange
      const bool skipDuplicateRedactions = false;
      //when import manager queue has one record
      var importWorkerDataTable = GetImportWorkerQueueDataTableWithOneRecord(Constant.ImportJobType.IMPORT, markupType, skipDuplicateRedactions);
      var markupUtilityImportJob = GetMarkupUtilityImportJob(Constant.ImportJobType.IMPORT, skipDuplicateRedactions);
      MockMethodsForWhenRecordExistsInQueue_ImportJobType(importWorkerDataTable, markupUtilityImportJob);

      //override previous mock for InsertRowIntoRedactionTableAsync to return negative redaction id
      MockInsertRowIntoRedactionTableAsync_Returns_NegativeRedactionId();

      // Act
      await _workerJob.ExecuteAsync();

      // Assert
      AssertMethodsForWhenRecordExistsInQueue_ErrorOccurs_WhenProcessingSingleRedaction();
    }

    [Description("When the document doesn't exists, a failure history record has to be created")]
    [Test]
    [TestCase(Constant.MarkupType.Redaction.VALUE)]
    [TestCase(Constant.MarkupType.Highlight.VALUE)]
    public async Task ExecuteAsync_ImportJob_DocumentDoesNotExist(int markupType)
    {
      // Arrange
      const bool skipDuplicateRedactions = false;
      //when import manager queue has one record
      var importWorkerDataTable = GetImportWorkerQueueDataTableWithOneRecord(Constant.ImportJobType.IMPORT, markupType, skipDuplicateRedactions);
      var markupUtilityImportJob = GetMarkupUtilityImportJob(Constant.ImportJobType.IMPORT, skipDuplicateRedactions);
      MockMethodsForWhenRecordExistsInQueue_ImportJobType(importWorkerDataTable, markupUtilityImportJob);

      //override previous mock for MockDoesDocumentExistAsync to return false
      MockDoesDocumentExistAsync(false);

      // Act
      await _workerJob.ExecuteAsync();

      // Assert
      AssertMethodsForWhenRecordExistsInQueue_DocumentDoesNotExist();
    }

    [Description("When the document doesn't have an image, a failure history record has to be created")]
    [Test]
    [TestCase(Constant.MarkupType.Redaction.VALUE)]
    [TestCase(Constant.MarkupType.Highlight.VALUE)]
    public async Task ExecuteAsync_ImportJob_DocumentDoesNotHaveImage(int markupType)
    {
      // Arrange
      const bool skipDuplicateRedactions = false;
      //when import manager queue has one record
      var importWorkerDataTable = GetImportWorkerQueueDataTableWithOneRecord(Constant.ImportJobType.IMPORT, markupType, skipDuplicateRedactions);
      var markupUtilityImportJob = GetMarkupUtilityImportJob(Constant.ImportJobType.IMPORT, skipDuplicateRedactions);
      MockMethodsForWhenRecordExistsInQueue_ImportJobType(importWorkerDataTable, markupUtilityImportJob);

      //override previous mock for DoesDocumentHasImagesAsync to return false
      MockDoesDocumentHasImagesAsync(false);

      // Act
      await _workerJob.ExecuteAsync();

      // Assert
      AssertMethodsForWhenRecordExistsInQueue_DocumentDoesNotHaveImage();
    }

    [Description("When a record is picked up by the agent, should complete execution process with redaction delete")]
    [Test]
    [TestCase(Constant.MarkupType.Redaction.VALUE)]
    [TestCase(Constant.MarkupType.Highlight.VALUE)]
    public async Task ExecuteAsync_RevertJob_QueueHasOneRecord(int markupType)
    {
      // Arrange
      const bool skipDuplicateRedactions = false;
      //when import manager queue has one record
      var importWorkerDataTable = GetImportWorkerQueueDataTableWithOneRecord(Constant.ImportJobType.REVERT, markupType, skipDuplicateRedactions);
      MockResetUnfishedJobsAsync();
      MockRetrieveNextBatchInImportWorkerQueueAsync(importWorkerDataTable);
      MockDropTableAsync();

      // Act
      await _workerJob.ExecuteAsync();

      // Assert
      AssertResetUnfishedJobsAsyncWasCalled(1);
      AssertRetrieveNextBatchInImportWorkerQueueAsyncWasCalled(1);
      AssertDropTableAsync(1);
    }

    [Description("When a record is picked up by the agent, should complete execution process with redaction delete")]
    [Test]
    [TestCase(Constant.MarkupType.Redaction.VALUE)]
    [TestCase(Constant.MarkupType.Highlight.VALUE)]
    public async Task ExecuteAsync_InvalidJob_QueueHasOneRecord(int markupType)
    {
      // Arrange
      const bool skipDuplicateRedactions = false;
      //when import manager queue has one record
      var importWorkerDataTable = GetImportWorkerQueueDataTableWithOneRecord("invalid job", markupType, skipDuplicateRedactions);
      MockResetUnfishedJobsAsync();
      MockRetrieveNextBatchInImportWorkerQueueAsync(importWorkerDataTable);
      MockUpdateRdoJobTextFieldAsync();
      MockInsertRowIntoImportErrorLogAsync();
      MockWriteError();
      MockDropTableAsync();

      // Act
      await _workerJob.ExecuteAsync();

      // Assert
      AssertResetUnfishedJobsAsyncWasCalled(1);
      AssertRetrieveNextBatchInImportWorkerQueueAsyncWasCalled(1);
      AssertUpdateRdoJobTextFieldAsyncWasCalled(2);
      AssertInsertRowIntoImportErrorLogAsyncWasCalled(1);
      AssertkWriteErrorWasCalled(1);
      AssertDropTableAsync(1);
    }

    #endregion Tests

    #region Test Helpers

    private static List<ChoiceModel> GetMarkupSetMultipleChoiceValues()
    {
      var choices = new List<ChoiceModel>
      {
       new ChoiceModel(1, Constant.MarkupSet.MarkupSetMultiChoiceValues.HAS_REDACTIONS),
       new ChoiceModel(2, Constant.MarkupSet.MarkupSetMultiChoiceValues.HAS_HIGHLIGHTS)
      };

      return choices;
    }

    private MarkupUtilityImportJob GetMarkupUtilityImportJob(string jobType, bool skipDuplicateRedactions)
    {
      var retVal = new MarkupUtilityImportJob(
        123,
        string.Empty,
        123,
        Constant.MarkupSubTypeCategory.SupportedMarkupUtilityTypes,
        skipDuplicateRedactions,
        123,
        string.Empty,
        string.Empty,
        123,
        123,
        jobType,
        123);
      return retVal;
    }

    private static DataTable GetEmptyImportWorkerQueueDataTable()
    {
      return null;
    }

    private static DataTable GetImportWorkerQueueDataTableWithOneRecord(string jobType, int markupType, bool skipDuplicateRedactions)
    {
      DataTable dataTable = new DataTable("Test Import Worker Table");

      dataTable.Columns.Add(new DataColumn("ID", typeof(Int32)) { AllowDBNull = false });
      dataTable.Columns.Add(new DataColumn("TimeStampUTC", typeof(DateTime)) { AllowDBNull = false });
      dataTable.Columns.Add(new DataColumn("WorkspaceArtifactID", typeof(Int32)) { AllowDBNull = false });
      dataTable.Columns.Add(new DataColumn("DocumentIdentifier", typeof(String)) { AllowDBNull = false });
      dataTable.Columns.Add(new DataColumn("FileOrder", typeof(Int32)) { AllowDBNull = false });
      dataTable.Columns.Add(new DataColumn("QueueStatus", typeof(Int32)) { AllowDBNull = false });
      dataTable.Columns.Add(new DataColumn("AgentID", typeof(Int32)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("ImportJobArtifactID", typeof(Int32)) { AllowDBNull = false });
      dataTable.Columns.Add(new DataColumn("JobType", typeof(String)) { AllowDBNull = false });
      dataTable.Columns.Add(new DataColumn("X", typeof(Int32)) { AllowDBNull = false });
      dataTable.Columns.Add(new DataColumn("Y", typeof(Int32)) { AllowDBNull = false });
      dataTable.Columns.Add(new DataColumn("Width", typeof(Int32)) { AllowDBNull = false });
      dataTable.Columns.Add(new DataColumn("Height", typeof(Int32)) { AllowDBNull = false });
      dataTable.Columns.Add(new DataColumn("MarkupSetArtifactID", typeof(Int32)) { AllowDBNull = false });
      dataTable.Columns.Add(new DataColumn("MarkupType", typeof(Int16)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("FillA", typeof(Int16)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("FillR", typeof(Int16)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("FillG", typeof(Int16)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("FillB", typeof(Int16)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("BorderSize", typeof(Int32)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("BorderA", typeof(Int16)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("BorderR", typeof(Int16)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("BorderG", typeof(Int16)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("BorderB", typeof(Int16)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("BorderStyle", typeof(Int16)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("FontName", typeof(String)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("FontA", typeof(Int16)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("FontR", typeof(Int16)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("FontG", typeof(Int16)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("FontB", typeof(Int16)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("FontSize", typeof(Int32)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("FontStyle", typeof(Int16)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("Text", typeof(String)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("ZOrder", typeof(Int32)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("DrawCrossLines", typeof(Boolean)) { AllowDBNull = false });
      dataTable.Columns.Add(new DataColumn("MarkupSubType", typeof(Int16)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("ResourceGroupID", typeof(Int32)) { AllowDBNull = false });
      dataTable.Columns.Add(new DataColumn("SkipDuplicateRedactions", typeof(Boolean)) { AllowDBNull = false });
      dataTable.Columns.Add(new DataColumn("X_d", typeof(decimal)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("Y_d", typeof(decimal)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("Width_d", typeof(decimal)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("Height_d", typeof(decimal)) { AllowDBNull = true });

      dataTable.Rows.Add(
        123, //ID
        DateTime.UtcNow, //TimeStampUTC
        123, //WorkspaceArtifactID
        "documentIdentifier", //DocumentIdentifier
        123, //FileOrder
        0, //QueueStatus
        (int?)null, //AgentID
        123, //ImportJobArtifactID
        jobType, //JobType
        123, // X
        123, // Y
        123, //Width
        123, //Height
        123, //MarkupSetArtifactID
        markupType, //MarkupType
        123, //FillA
        123, //FillR
        123, //FillG
        123, //FillB
        123, //BorderSize
        123, //BorderA
        123, //BorderR
        123, //BorderG
        123, //BorderB
        123, //BorderStyle
        "fontName", //FontName
        123, //FontA
        123, //FontR
        123, //FontG
        123, //FontB
        123, //FontSize
        123, //FontStyle
        "text", //Text
        123, //ZOrder
        false, //DrawCrossLines
        123, //MarkupSubType
        123, //ResourceGroupID
        skipDuplicateRedactions,  //SkipDuplicateRedactions
        123.1, // X_d
        123.1, // Y_d
        123.1, //Width_d
        123.1 //Height_d
      );

      return dataTable;
    }

    private ImportWorkerJob GetImportWorkerJob()
    {
      var importWorkerJob = new ImportWorkerJob(
        AgentId,
        MockAgentHelper.Object,
        MockQuery.Object,
        new DateTime(2016, 01, 25, 01, 00, 00),
        _resourceGroupIdList,
        MockErrorQueries.Object,
        MockArtifactQueries.Object,
        MockAuditRecordHelper.Object,
        MockMarkupTypeHelper.Object);

      return importWorkerJob;
    }

    #endregion Test Helpers

    #region Mocks

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

    private void MockMethodsForWhenRecordExistsInQueue_ImportJobType(DataTable importWorkerDataTable, MarkupUtilityImportJob markupUtilityImportJob)
    {
      MockResetUnfishedJobsAsync();
      MockRetrieveNextBatchInImportWorkerQueueAsync(importWorkerDataTable);
      MockUpdateRdoJobTextFieldAsync();
      MockRetrieveImportJobAsync(markupUtilityImportJob);
      MockRetreiveMarkupSetNameAsync();
      MockRetreiveMarkupSetMultipleChoiceFieldTypeIdAsync();
      MockQueryAllMarkupSetMultipleChoiceFieldValuesAsync();
      MockGetDocumentIdentifierFieldNameAsync();
      MockDoesDocumentExistAsync(true);
      MockRetrieveDocumentArtifactIdAsync();
      MockDoesDocumentHasImagesAsync(true);
      MockGetFileGuidForDocumentAsync();
      MockVerifyIfMarkupSetExistsAsync(true);
      MockDoesRedactionExistAsync(true);
      MockInsertRowIntoRedactionTableAsync();
      MockUpdateMarkupSetMultipleChoiceFieldAsync();
      MockCreateRedactionAuditRecordAsync();
      MockGetMarkupSubTypeNameAsync();
      MockCreateMarkupUtilityHistoryRecordAsync();
      const int redactionCount = 123;
      MockRetrieveImportJobRedactionCountFieldValueAsync(redactionCount);
      MockUpdateImportJobRedactionCountFieldValueAsync();
      MockRemoveBatchFromImportWorkerQueueAsync();
      MockVerifyIfImportWorkerQueueContainsRecordsForJobAsync(false);
      MockDropTableAsync();
    }

    private void MockMethodsForWhenNoRecordExistsInQueue(DataTable emptryImportWorkerDataTable)
    {
      MockResetUnfishedJobsAsync();
      MockRetrieveNextBatchInImportWorkerQueueAsync(emptryImportWorkerDataTable);
      MockDropTableAsync();
    }

    private void MockVerifyIfImportWorkerQueueContainsRecordsForJobAsync(bool hasResults)
    {
      MockQuery
        .Setup(x => x.VerifyIfImportWorkerQueueContainsRecordsForJobAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<int>()))
        .Returns(Task.FromResult(hasResults))
        .Verifiable();
    }

    private void MockUpdateImportJobRedactionCountFieldValueAsync()
    {
      MockArtifactQueries
        .Setup(x => x.UpdateImportJobRedactionCountFieldValueAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<Guid>(),
          It.IsAny<int>()))
        .Returns(Task.FromResult(default(Task)))
        .Verifiable();
    }

    private void MockRetrieveImportJobRedactionCountFieldValueAsync(int count)
    {
      MockArtifactQueries
        .Setup(x => x.RetrieveImportJobRedactionCountFieldValueAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<Guid>()))
        .Returns(Task.FromResult(count))
        .Verifiable();
    }

    private void MockUpdateMarkupSetMultipleChoiceFieldAsync()
    {
      MockQuery
        .Setup(x => x.UpdateMarkupSetMultipleChoiceFieldAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<int>()))
        .Returns(Task.FromResult(default(Task)))
        .Verifiable();
    }

    private void MockCreateMarkupUtilityHistoryRecordAsync()
    {
      MockArtifactQueries
        .Setup(x => x.CreateMarkupUtilityHistoryRecordAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<string>(),
          It.IsAny<string>(),
          It.IsAny<string>(),
          It.IsAny<string>(),
          It.IsAny<int?>(), -1))
        .Returns(Task.FromResult(default(Task)))
        .Verifiable();
    }

    private void MockGetMarkupSubTypeNameAsync()
    {
      MockMarkupTypeHelper
        .Setup(x => x.GetMarkupSubTypeNameAsync(
          It.IsAny<int>()))
        .Returns(Task.FromResult("markupTypeName"))
        .Verifiable();
    }

    private void MockCreateRedactionAuditRecordAsync()
    {
      MockAuditRecordHelper
        .Setup(x => x.CreateRedactionAuditRecordAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<ImportWorkerQueueRecord>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<string>()))
        .Returns(Task.FromResult(default(Task)))
        .Verifiable();
    }

    private void MockInsertRowIntoRedactionTableAsync()
    {
      MockQuery
        .Setup(x => x.InsertRowIntoRedactionTableAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<string>(),
          It.IsAny<int>(),
          It.IsAny<ImportWorkerQueueRecord>()))
        .Returns(Task.FromResult(123))
        .Verifiable();
    }

    private void MockInsertRowIntoRedactionTableAsync_Throws_Exception()
    {
      MockQuery
        .Setup(x => x.InsertRowIntoRedactionTableAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<string>(),
          It.IsAny<int>(),
          It.IsAny<ImportWorkerQueueRecord>()))
        .Throws<MarkupUtilityException>()
        .Verifiable();
    }

    private void MockInsertRowIntoRedactionTableAsync_Returns_NegativeRedactionId()
    {
      MockQuery
        .Setup(x => x.InsertRowIntoRedactionTableAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<string>(),
          It.IsAny<int>(),
          It.IsAny<ImportWorkerQueueRecord>()))
        .Returns(Task.FromResult(-1))
        .Verifiable();
    }

    private void MockDoesRedactionExistAsync(bool doesRedactionExist)
    {
      MockQuery
        .Setup(x => x.DoesRedactionExistAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<string>(),
          It.IsAny<int>(),
          It.IsAny<ImportWorkerQueueRecord>()))
        .Returns(Task.FromResult(doesRedactionExist))
        .Verifiable();
    }

    private void MockVerifyIfMarkupSetExistsAsync(bool doesMarkupSetExists)
    {
      MockArtifactQueries
        .Setup(x => x.VerifyIfMarkupSetExistsAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<int>()))
        .Returns(Task.FromResult(doesMarkupSetExists))
        .Verifiable();
    }

    private void MockGetFileGuidForDocumentAsync()
    {
      MockQuery
        .Setup(x => x.GetFileGuidForDocumentAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<int>()))
        .Returns(Task.FromResult("fileGuid"))
        .Verifiable();
    }

    private void MockDoesDocumentHasImagesAsync(bool hasImages)
    {
      MockQuery
        .Setup(x => x.DoesDocumentHasImagesAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<int>()))
        .Returns(Task.FromResult(hasImages))
        .Verifiable();
    }

    private void MockRetrieveDocumentArtifactIdAsync()
    {
      MockArtifactQueries
        .Setup(x => x.RetrieveDocumentArtifactIdAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<string>()))
        .Returns(Task.FromResult(123))
        .Verifiable();
    }

    private void MockDoesDocumentExistAsync(bool exists)
    {
      MockArtifactQueries
        .Setup(x => x.DoesDocumentExistAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<string>()))
        .Returns(Task.FromResult(exists))
        .Verifiable();
    }

    private void MockGetDocumentIdentifierFieldNameAsync()
    {
      MockQuery
        .Setup(x => x.GetDocumentIdentifierFieldNameAsync(
          It.IsAny<IDBContext>()))
        .Returns(Task.FromResult("documentIdentifierFieldName"))
        .Verifiable();
    }

    private void MockQueryAllMarkupSetMultipleChoiceFieldValuesAsync()
    {
      var choices = GetMarkupSetMultipleChoiceValues();

      MockArtifactQueries
        .Setup(x => x.QueryAllMarkupSetMultipleChoiceFieldValuesAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<string>()))
        .Returns(Task.FromResult(choices));
    }

    private void MockRetreiveMarkupSetMultipleChoiceFieldTypeIdAsync()
    {
      MockArtifactQueries
        .Setup(x => x.RetreiveMarkupSetMultipleChoiceFieldTypeIdAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<string>()))
        .Returns(Task.FromResult(123))
        .Verifiable();
    }

    private void MockRetreiveMarkupSetNameAsync()
    {
      MockArtifactQueries
        .Setup(x => x.RetreiveMarkupSetNameAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<int>()))
        .Returns(Task.FromResult("markupSetName"))
        .Verifiable();
    }

    private void MockRetrieveImportJobAsync(MarkupUtilityImportJob markupUtilityImportJob)
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

    private void MockRetrieveNextBatchInImportWorkerQueueAsync(DataTable dataTable)
    {
      MockQuery
        .Setup(x => x.RetrieveNextBatchInImportWorkerQueueAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<string>()))
        .ReturnsAsync(dataTable);
    }

    private void MockDropTableAsync()
    {
      MockQuery
        .Setup(x => x.DropTableAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<string>()))
        .Returns(Task.FromResult(false))
        .Verifiable();
    }

    private void MockRemoveBatchFromImportWorkerQueueAsync()
    {
      MockQuery
        .Setup(x => x.RemoveBatchFromImportWorkerQueueAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<string>()))
        .Returns(Task.FromResult(false))
        .Verifiable();
    }

    private void MockResetUnfishedJobsAsync()
    {
      MockQuery
        .Setup(x => x.ResetUnfishedJobsAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<string>()))
        .Returns(Task.FromResult(default(Task)))
        .Verifiable();
    }

    private void MockResetUnfishedJobsAsync_ThrowsException()
    {
      MockQuery
        .Setup(x => x.ResetUnfishedJobsAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<string>()))
        .Throws<MarkupUtilityException>()
        .Verifiable();
    }

    #endregion

    #region Asserts

    private void AssertMethodsForWhenRecordExistsInQueue_DocumentDoesNotHaveImage()
    {
      AssertResetUnfishedJobsAsyncWasCalled(1);
      AssertRetrieveNextBatchInImportWorkerQueueAsyncWasCalled(1);
      AssertUpdateRdoJobTextFieldAsyncWasCalled(4);
      AssertRetrieveImportJobAsyncWasCalled(2);
      AssertRetreiveMarkupSetNameAsyncWasCalled(1);
      AssertRetreiveMarkupSetMultipleChoiceFieldTypeIdAsyncWasCalled(1);
      AssertQueryAllMarkupSetMultipleChoiceFieldValuesAsyncWasCalled(1);
      AssertGetDocumentIdentifierFieldNameAsyncWasCalled(1);
      AssertDoesDocumentExistAsyncWasCalled(1);
      AssertRetrieveDocumentArtifactIdAsyncWasCalled(1);
      AssertDoesDocumentHasImagesAsyncWasCalled(1);
      AssertGetMarkupSubTypeNameAsyncWascalled(1);
      AssertCreateMarkupUtilityHistoryRecordAsyncErrorStatusWasCalled(1);
      AssertRetrieveImportJobRedactionCountFieldValueAsyncWasCalled(3);
      AssertUpdateImportJobRedactionCountFieldValueAsyncWasCalled(3);
      AssertRemoveBatchFromImportWorkerQueueAsyncWasCalled(1);
      AssertVerifyIfImportWorkerQueueContainsRecordsForJobAsyncWasCalled(1);
      AssertDropTableAsync(1);

      //these should not be called
      AssertGetFileGuidForDocumentAsyncWasCalled(0);
      AssertVerifyIfMarkupSetExistsAsyncWasCalled(0);
      AssertDoesRedactionExistAsyncWasCalled(0);
      AssertInsertRowIntoRedactionTableAsyncWasCalled(0);
      AssertUpdateMarkupSetMultipleChoiceFieldAsyncWasCalled(0);
      AssertCreateRedactionAuditRecordAsyncWasCalled(0);
    }

    private void AssertMethodsForWhenRecordExistsInQueue_DocumentDoesNotExist()
    {
      AssertResetUnfishedJobsAsyncWasCalled(1);
      AssertRetrieveNextBatchInImportWorkerQueueAsyncWasCalled(1);
      AssertUpdateRdoJobTextFieldAsyncWasCalled(4);
      AssertRetrieveImportJobAsyncWasCalled(2);
      AssertRetreiveMarkupSetNameAsyncWasCalled(1);
      AssertRetreiveMarkupSetMultipleChoiceFieldTypeIdAsyncWasCalled(1);
      AssertQueryAllMarkupSetMultipleChoiceFieldValuesAsyncWasCalled(1);
      AssertGetDocumentIdentifierFieldNameAsyncWasCalled(1);
      AssertDoesDocumentExistAsyncWasCalled(1);
      AssertGetMarkupSubTypeNameAsyncWascalled(1);
      AssertCreateMarkupUtilityHistoryRecordAsyncErrorStatusWasCalled(1);
      AssertRetrieveImportJobRedactionCountFieldValueAsyncWasCalled(3);
      AssertUpdateImportJobRedactionCountFieldValueAsyncWasCalled(3);
      AssertRemoveBatchFromImportWorkerQueueAsyncWasCalled(1);
      AssertVerifyIfImportWorkerQueueContainsRecordsForJobAsyncWasCalled(1);
      AssertDropTableAsync(1);

      //these should not be called
      AssertRetrieveDocumentArtifactIdAsyncWasCalled(0);
      AssertDoesDocumentHasImagesAsyncWasCalled(0);
      AssertGetFileGuidForDocumentAsyncWasCalled(0);
      AssertVerifyIfMarkupSetExistsAsyncWasCalled(0);
      AssertDoesRedactionExistAsyncWasCalled(0);
      AssertInsertRowIntoRedactionTableAsyncWasCalled(0);
      AssertUpdateMarkupSetMultipleChoiceFieldAsyncWasCalled(0);
      AssertCreateRedactionAuditRecordAsyncWasCalled(0);
    }

    private void AssertMethodsForWhenRecordExistsInQueue_ErrorOccurs_WhenProcessingSingleRedaction()
    {
      AssertResetUnfishedJobsAsyncWasCalled(1);
      AssertRetrieveNextBatchInImportWorkerQueueAsyncWasCalled(1);
      AssertUpdateRdoJobTextFieldAsyncWasCalled(4);
      AssertRetrieveImportJobAsyncWasCalled(2);
      AssertRetreiveMarkupSetNameAsyncWasCalled(1);
      AssertRetreiveMarkupSetMultipleChoiceFieldTypeIdAsyncWasCalled(1);
      AssertQueryAllMarkupSetMultipleChoiceFieldValuesAsyncWasCalled(1);
      AssertGetDocumentIdentifierFieldNameAsyncWasCalled(1);
      AssertDoesDocumentExistAsyncWasCalled(1);
      AssertRetrieveDocumentArtifactIdAsyncWasCalled(1);
      AssertDoesDocumentHasImagesAsyncWasCalled(1);
      AssertGetFileGuidForDocumentAsyncWasCalled(1);
      AssertVerifyIfMarkupSetExistsAsyncWasCalled(1);
      AssertDoesRedactionExistAsyncWasCalled(1);
      AssertInsertRowIntoRedactionTableAsyncWasCalled(1);
      AssertGetMarkupSubTypeNameAsyncWascalled(1);
      AssertCreateMarkupUtilityHistoryRecordAsyncErrorStatusWasCalled(1);
      AssertRetrieveImportJobRedactionCountFieldValueAsyncWasCalled(3);
      AssertUpdateImportJobRedactionCountFieldValueAsyncWasCalled(3);
      AssertRemoveBatchFromImportWorkerQueueAsyncWasCalled(1);
      AssertVerifyIfImportWorkerQueueContainsRecordsForJobAsyncWasCalled(1);
      AssertDropTableAsync(1);

      //these should not be called
      AssertUpdateMarkupSetMultipleChoiceFieldAsyncWasCalled(0);
      AssertCreateRedactionAuditRecordAsyncWasCalled(0);
    }

    private void AssertkWriteErrorWasCalled(int timesCalled)
    {
      MockErrorQueries
        .Verify(x => x.WriteError(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<Exception>()),
          Times.Exactly(timesCalled));
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

    private void AssertVerifyIfImportWorkerQueueContainsRecordsForJobAsyncWasCalled(int timesCalled)
    {
      MockQuery
        .Verify(x => x.VerifyIfImportWorkerQueueContainsRecordsForJobAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<int>()),
          Times.Exactly(timesCalled));
    }

    private void AssertUpdateImportJobRedactionCountFieldValueAsyncWasCalled(int timesCalled)
    {
      MockArtifactQueries
        .Verify(x => x.UpdateImportJobRedactionCountFieldValueAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<Guid>(),
          It.IsAny<int>()),
          Times.Exactly(timesCalled));
    }

    private void AssertRetrieveImportJobRedactionCountFieldValueAsyncWasCalled(int timesCalled)
    {
      MockArtifactQueries
        .Verify(x => x.RetrieveImportJobRedactionCountFieldValueAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<Guid>()),
          Times.Exactly(timesCalled));
    }

    private void AssertUpdateMarkupSetMultipleChoiceFieldAsyncWasCalled(int timesCalled)
    {
      MockQuery
        .Verify(x => x.UpdateMarkupSetMultipleChoiceFieldAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<int>()),
          Times.Exactly(timesCalled));
    }

    private void AssertCreateMarkupUtilityHistoryRecordAsyncErrorStatusWasCalled(int timesCalled)
    {
      MockArtifactQueries
        .Verify(x => x.CreateMarkupUtilityHistoryRecordAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<string>(),
          Constant.Status.History.ERROR,
          It.IsAny<string>(),
          It.IsAny<string>(),
          It.IsAny<int?>(), -1),
          Times.Exactly(timesCalled));
    }

    private void AssertCreateMarkupUtilityHistoryRecordAsyncCompletedStatusWasCalled(int timesCalled)
    {
      MockArtifactQueries
        .Verify(x => x.CreateMarkupUtilityHistoryRecordAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<string>(),
          Constant.Status.History.COMPLETED,
          It.IsAny<string>(),
          It.IsAny<string>(),
          It.IsAny<int?>(), -1),
          Times.Exactly(timesCalled));
    }

    private void AssertCreateMarkupUtilityHistoryRecordAsyncSkippedStatusWasCalled(int timesCalled)
    {
      MockArtifactQueries
        .Verify(x => x.CreateMarkupUtilityHistoryRecordAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<string>(),
          Constant.Status.History.SKIPPED,
          It.IsAny<string>(),
          It.IsAny<string>(),
          It.IsAny<int?>(), -1),
          Times.Exactly(timesCalled));
    }

    private void AssertGetMarkupSubTypeNameAsyncWascalled(int timesCalled)
    {
      MockMarkupTypeHelper
        .Verify(x => x.GetMarkupSubTypeNameAsync(
          It.IsAny<int>()),
          Times.Exactly(timesCalled));
    }

    private void AssertCreateRedactionAuditRecordAsyncWasCalled(int timesCalled)
    {
      MockAuditRecordHelper
        .Verify(x => x.CreateRedactionAuditRecordAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<ImportWorkerQueueRecord>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<string>()),
          Times.Exactly(timesCalled));
    }

    private void AssertInsertRowIntoRedactionTableAsyncWasCalled(int timesCalled)
    {
      MockQuery
        .Verify(x => x.InsertRowIntoRedactionTableAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<string>(),
          It.IsAny<int>(),
          It.IsAny<ImportWorkerQueueRecord>()),
          Times.Exactly(timesCalled));
    }

    private void AssertDoesRedactionExistAsyncWasCalled(int timesCalled)
    {
      MockQuery
        .Verify(x => x.DoesRedactionExistAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<string>(),
          It.IsAny<int>(),
          It.IsAny<ImportWorkerQueueRecord>()),
          Times.Exactly(timesCalled));
    }

    private void AssertVerifyIfMarkupSetExistsAsyncWasCalled(int timesCalled)
    {
      MockArtifactQueries
        .Verify(x => x.VerifyIfMarkupSetExistsAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<int>()),
          Times.Exactly(timesCalled));
    }

    private void AssertGetFileGuidForDocumentAsyncWasCalled(int timesCalled)
    {
      MockQuery
        .Verify(x => x.GetFileGuidForDocumentAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<int>()),
          Times.Exactly(timesCalled));
    }

    private void AssertDoesDocumentHasImagesAsyncWasCalled(int timesCalled)
    {
      MockQuery
        .Verify(x => x.DoesDocumentHasImagesAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<int>()),
          Times.Exactly(timesCalled));
    }

    private void AssertRetrieveDocumentArtifactIdAsyncWasCalled(int timesCalled)
    {
      MockArtifactQueries
        .Verify(x => x.RetrieveDocumentArtifactIdAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<string>()),
          Times.Exactly(timesCalled));
    }

    private void AssertDoesDocumentExistAsyncWasCalled(int timesCalled)
    {
      MockArtifactQueries
        .Verify(x => x.DoesDocumentExistAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<string>()),
          Times.Exactly(timesCalled));
    }

    private void AssertGetDocumentIdentifierFieldNameAsyncWasCalled(int timesCalled)
    {
      MockQuery
        .Verify(x => x.GetDocumentIdentifierFieldNameAsync(
          It.IsAny<IDBContext>()),
          Times.Exactly(timesCalled));
    }

    private void AssertQueryAllMarkupSetMultipleChoiceFieldValuesAsyncWasCalled(int timesCalled)
    {
      MockArtifactQueries
        .Verify(x => x.QueryAllMarkupSetMultipleChoiceFieldValuesAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<string>()),
          Times.Exactly(timesCalled));
    }

    private void AssertRetreiveMarkupSetMultipleChoiceFieldTypeIdAsyncWasCalled(int timesCalled)
    {
      MockArtifactQueries
        .Verify(x => x.RetreiveMarkupSetMultipleChoiceFieldTypeIdAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<string>()),
          Times.Exactly(timesCalled));
    }

    private void AssertRetreiveMarkupSetNameAsyncWasCalled(int timesCalled)
    {
      MockArtifactQueries
        .Verify(x => x.RetreiveMarkupSetNameAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<int>()),
          Times.Exactly(timesCalled));
    }

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

    private void AssertRetrieveNextBatchInImportWorkerQueueAsyncWasCalled(int timesCalled)
    {
      MockQuery
        .Verify(x => x.RetrieveNextBatchInImportWorkerQueueAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<string>()
          ),
          Times.Exactly(timesCalled));
    }

    private void AssertRemoveBatchFromImportWorkerQueueAsyncWasCalled(int timesCalled)
    {
      MockQuery
        .Verify(x => x.RemoveBatchFromImportWorkerQueueAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<string>()),
          Times.Exactly(timesCalled));
    }

    private void AssertResetUnfishedJobsAsyncWasCalled(int timesCalled)
    {
      MockQuery
        .Verify(x => x.ResetUnfishedJobsAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<string>()),
          Times.Exactly(timesCalled));
    }

    private void AssertDropTableAsync(int timesCalled)
    {
      MockQuery
        .Verify(x => x.DropTableAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<string>()),
          Times.Exactly(timesCalled));
    }

    #endregion

  }
}
