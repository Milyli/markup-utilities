using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MarkupUtilities.Helpers.Exceptions;
using MarkupUtilities.Helpers.Models;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace MarkupUtilities.Helpers.NUnit
{
  [TestFixture]
  public class AuditRecordHelperTests
  {
    private IAuditRecordHelper _auditRecordHelper;
    private Mock<IQuery> _mockQuery;
    private Mock<IDBContext> _mockDbContext;

    [SetUp]
    public void Setup()
    {
      _mockQuery = new Mock<IQuery>();
      _mockDbContext = new Mock<IDBContext>();
      _auditRecordHelper = new AuditRecordHelper(_mockQuery.Object);
    }

    [TearDown]
    public void Teardown()
    {
      _mockQuery = null;
      _auditRecordHelper = null;
    }


    #region Tests

    [Description("When redaction data is passed, should successfully create redaction audit record")]
    [Test]
    public void CreateRedactionAuditRecordSuccessTest()
    {
      //Arrange
      MockCreateAuditRecordAsync();
      var createRedactionAuditRecordTask = _auditRecordHelper.CreateRedactionAuditRecordAsync(_mockDbContext.Object, 123, 123, 123, GetImportWorkerQueueRecord(), 123, 123, "fileGuid");

      //Act
      createRedactionAuditRecordTask.Wait();

      //Assert
      Assert.That(createRedactionAuditRecordTask.Exception, Is.EqualTo(null));
      AssertCreateAuditRecordAsyncWasCalled(1);
    }

    [Description("When redaction data is passed and fails to create redaction audit record, an exception has to be throws")]
    [Test]
    public void CreateRedactionAuditRecordFailTest()
    {
      //Arrange
      MockCreateAuditRecordAsync_ThrowsException();
      var createRedactionAuditRecordTask = _auditRecordHelper.CreateRedactionAuditRecordAsync(_mockDbContext.Object, 123, 123, 123, GetImportWorkerQueueRecord(), 123, 123, "fileGuid");

      //Act
      var aggregateException = Assert.Throws<AggregateException>(() => { createRedactionAuditRecordTask.Wait(); });

      //Assert
      var actualException = aggregateException.InnerExceptions.First(x => x.GetType() == typeof(MarkupUtilityException)) as MarkupUtilityException;

      Assert.That(actualException, Is.Not.Null);

      if (actualException != null)
      {
        StringAssert.Contains(Constant.ErrorMessages.CREATE_REDACTION_AUDIT_RECORD_ERROR, actualException.Message);
      }

      AssertCreateAuditRecordAsyncWasCalled(1);
    }

    [Test]
    public async Task CreateRedactionAuditRecordAsyncTestSuccess()
    {
      var mock = new Mock<IQuery>();
      mock
        .Setup(x => x.CreateAuditRecordAsync(It.IsAny<IDBContext>(), It.IsAny<RedactionAuditRecord>()))
        .Returns(Task.FromResult(default(Task)))
        .Verifiable();
      var auditRecordHelper = new AuditRecordHelper(mock.Object);
      await auditRecordHelper.CreateRedactionAuditRecordAsync(_mockDbContext.Object, 1, 2, 3, "4", 5, 6, 7);

      mock.Verify(x => x.CreateAuditRecordAsync(It.IsAny<IDBContext>(),
        It.Is<RedactionAuditRecord>(arg => arg.ArtifactId == 2
                                    && arg.Action == 1 && arg.UserId == 3 &&
                                    arg.Details ==
                                    $@"<auditElement><imageMarkup id=""5"" pageNumber=""7"" markupSetArtifactID=""6"" /></auditElement>"
                                    && arg.RequestOrigination == $@"<auditElement><RequestOrigination><IP /><Prefix /><Page>{Constant.AuditRecord.APPLICATION_NAME}</Page></RequestOrigination></auditElement>"
                                    && arg.RecordOrigination == $@"<auditElement><RecordOrigination><MAC /><IP /><Server /></RecordOrigination></auditElement>")),
        Times.Exactly(1));
    }
    
    [Test]
    public void CreateRedactionAuditRecordAsyncTestFailiure()
    {
      var mock = new Mock<IQuery>();
      mock
        .Setup(x => x.CreateAuditRecordAsync(It.IsAny<IDBContext>(), It.IsAny<RedactionAuditRecord>()))
        .Throws<MarkupUtilityException>()
        .Verifiable();
      var auditRecordHelper = new AuditRecordHelper(mock.Object);
      var task = auditRecordHelper.CreateRedactionAuditRecordAsync(_mockDbContext.Object, 1, 2, 3, "4", 5, 6, 7);
      var aggregateException = Assert.Throws<AggregateException>(() => { task.Wait(); });
      string errorContext = $"{Constant.ErrorMessages.CREATE_REDACTION_AUDIT_RECORD_ERROR} [AuditActionId = {1}, ArtifactId = {2}, UserId = {3}, MarkupSetArtifactId = {6}, RedactionId = {5}, FileGuid = {"4"}]";
      var actualException = aggregateException.InnerExceptions.First(x => x.GetType() == typeof(MarkupUtilityException)) as MarkupUtilityException;

      Assert.That(actualException, Is.Not.Null);
      if (actualException != null) Assert.AreEqual(errorContext, actualException.Message);
    }

    #endregion

    #region Test Helpers

    private ImportWorkerQueueRecord GetImportWorkerQueueRecord()
    {
      var dataTable = GetImportWorkerQueueDataTableWithOneRecord();
      var dataRow = dataTable.Rows[0];
      return new ImportWorkerQueueRecord(dataRow);
    }

    private DataTable GetImportWorkerQueueDataTableWithOneRecord()
    {
      var dataTable = new DataTable("Test Import Worker Table");

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
        "jobType", //JobType
        123, // X
        123, // Y
        123, //Width
        123, //Height
        123, //MarkupSetArtifactID
        1, //MarkupType
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
        false,  //SkipDuplicateRedactions
        123.1, // X
        123.1, // Y
        123.1, //Width
        123.1 //Height
      );

      return dataTable;
    }

    #endregion

    #region Mocks

    private void MockCreateAuditRecordAsync_ThrowsException()
    {
      _mockQuery
        .Setup(x => x.CreateAuditRecordAsync(It.IsAny<IDBContext>(), It.IsAny<RedactionAuditRecord>()))
        .Throws<MarkupUtilityException>()
        .Verifiable();
    }

    private void MockCreateAuditRecordAsync()
    {
      _mockQuery
        .Setup(x => x.CreateAuditRecordAsync(It.IsAny<IDBContext>(), It.IsAny<RedactionAuditRecord>()))
        .Returns(Task.FromResult(default(Task)))
        .Verifiable();
    }

    #endregion

    #region Asserts

    private void AssertCreateAuditRecordAsyncWasCalled(int timesCalled)
    {
      _mockQuery
        .Verify(x => x.CreateAuditRecordAsync(It.IsAny<IDBContext>(), It.IsAny<RedactionAuditRecord>()), Times.Exactly(timesCalled));
    }

    #endregion
  }
}
