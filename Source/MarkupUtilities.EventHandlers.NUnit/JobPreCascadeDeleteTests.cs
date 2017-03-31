using System.Data;
using MarkupUtilities.Helpers;
using NUnit.Framework;
using Moq;
using Relativity.API;

namespace MarkupUtilities.EventHandlers.NUnit
{
  [TestFixture]
  public class JobPreCascadeDeleteTests
  {
    private ImportJobPreCascadeDelete _importJob;
    private ExportJobPreCascadeDelete _exportJob;

    [SetUp]
    public void SetUp()
    {
      _importJob = new ImportJobPreCascadeDelete();
      _exportJob = new ExportJobPreCascadeDelete();
    }

    [Test]
    public void TestNoRecords()
    {
      // Arrange 
      var dbMock = new Mock<IDBContext>();
      dbMock.Setup(x => x.ExecuteSqlStatementAsDataTable(It.IsAny<string>())).Returns(new DataTable("result"))
        .Verifiable();

      // Act 
      var result = _importJob.IsOkToDelete(dbMock.Object, "test", "test");

      // Assert 
      Assert.IsTrue(result);
      dbMock.VerifyAll();
    }

    [Test]
    public void TestSingleRecordInProgress()
    {
      // Arrange 
      var dbMock = new Mock<IDBContext>();
      var table = CreateDataTable("Validating");
      dbMock.Setup(x => x.ExecuteSqlStatementAsDataTable(It.IsAny<string>()))
        .Returns(table)
        .Verifiable();

      // Act 
      var result = _importJob.IsOkToDelete(dbMock.Object, "test", "test");

      // Assert 
      Assert.IsFalse(result);
      dbMock.VerifyAll();
    }

    [Test]
    public void TestSingleRecordComplete()
    {
      // Arrange 
      var dbMock = new Mock<IDBContext>();
      var table = CreateDataTable("Completed");
      dbMock.Setup(x => x.ExecuteSqlStatementAsDataTable(It.IsAny<string>()))
        .Returns(table)
        .Verifiable();

      // Act 
      var result = _importJob.IsOkToDelete(dbMock.Object, "test", "test");

      // Assert 
      Assert.IsTrue(result);
      dbMock.VerifyAll();
    }

    [Test]
    public void TestMultipleRecordsInProgress()
    {
      // Arrange 
      var dbMock = new Mock<IDBContext>();
      var table = CreateDataTable("Validating", "Completed");
      dbMock.Setup(x => x.ExecuteSqlStatementAsDataTable(It.IsAny<string>()))
        .Returns(table)
        .Verifiable();

      // Act 
      var result = _importJob.IsOkToDelete(dbMock.Object, "test", "test");

      // Assert 
      Assert.IsFalse(result);
    }

    [Test]
    public void TestMultipleRecordsComplete()
    {
      // Arrange 
      var dbMock = new Mock<IDBContext>();
      var table = CreateDataTable("Error", "Completed");
      dbMock.Setup(x => x.ExecuteSqlStatementAsDataTable(It.IsAny<string>()))
        .Returns(table)
        .Verifiable();

      // Act 
      var result = _importJob.IsOkToDelete(dbMock.Object, "test", "test");

      // Assert 
      Assert.IsTrue(result);
      dbMock.VerifyAll();
    }

    [Test]
    public void TestExecuteSuccess()
    {
      // Arrange 
      var dbMock = new Mock<IEHHelper>();
      var dbContext = new Mock<IDBContext>();
      var table = CreateDataTable(Constant.Status.Job.ERROR, Constant.Status.Job.COMPLETED);

      dbContext.Setup(x => x.ExecuteSqlStatementAsDataTable(It.IsAny<string>()))
        .Returns(table)
        .Verifiable();

      dbMock.Setup(x => x.GetActiveCaseID())
        .Returns(1)
        .Verifiable();

      dbMock.Setup(x => x.GetDBContext(It.IsAny<int>()))
        .Returns(dbContext.Object)
        .Verifiable();

      _importJob.Helper = dbMock.Object;
      _exportJob.Helper = dbMock.Object;

      var response = _importJob.Execute();
      Assert.IsTrue(response.Success);

      response = _exportJob.Execute();
      Assert.IsTrue(response.Success);

      dbMock.VerifyAll();
    }

    [Test]
    public void TestExecuteFail()
    {
      // Arrange 
      var dbMock = new Mock<IEHHelper>();
      var dbContext = new Mock<IDBContext>();

      var table = CreateDataTable(Constant.Status.Job.VALIDATING, Constant.Status.Job.COMPLETED);
      dbContext.Setup(x => x.ExecuteSqlStatementAsDataTable(It.IsAny<string>()))
        .Returns(table)
        .Verifiable();

      dbMock.Setup(x => x.GetActiveCaseID())
        .Returns(1)
        .Verifiable();

      dbMock.Setup(x => x.GetDBContext(It.IsAny<int>()))
        .Returns(dbContext.Object)
        .Verifiable();

      _importJob.Helper = dbMock.Object;
      _exportJob.Helper = dbMock.Object;

      var response = _importJob.Execute();
      Assert.IsFalse(response.Success);
      Assert.NotNull(response.Exception);

      response = _exportJob.Execute();
      Assert.IsFalse(response.Success);
      Assert.NotNull(response.Exception);

      dbMock.VerifyAll();
    }

    private static DataTable CreateDataTable(params string[] statusStrings)
    {
      var table = new DataTable();
      var columnSpec = new DataColumn { DataType = typeof(string), ColumnName = "Status" };
      table.Columns.Add(columnSpec);

      foreach (var statusString in statusStrings)
      {
        var mydatarow = table.NewRow();
        mydatarow["Status"] = statusString;
        table.Rows.Add(mydatarow);
      }

      return table;
    }
  }
}