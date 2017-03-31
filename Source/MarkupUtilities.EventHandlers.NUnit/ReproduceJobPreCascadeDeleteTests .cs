using System.Data;
using System.Threading.Tasks;
using MarkupUtilities.Helpers;
using NUnit.Framework;
using Moq;
using Relativity.API;

namespace MarkupUtilities.EventHandlers.NUnit
{
  [TestFixture]
  public class ReproduceJobPreCascadeDeleteTests
  {
    private ReproduceJobPreCascadeDelete _job;
    public Mock<IQuery> MockQuery;

    [SetUp]
    public void SetUp()
    {
      _job = new ReproduceJobPreCascadeDelete();
      MockQuery = new Mock<IQuery>();
      var dataTable = new DataTable();
      dataTable.Columns.Add(new DataColumn("ArtifactID", typeof(int)));
      dataTable.Rows.Add(1);
      MockQuery.Setup(x => x.RetrieveArtifactIDsAsync(It.IsAny<IDBContext>(), It.IsAny<string>()))
        .Returns(Task.Factory.StartNew(() => dataTable)).Verifiable();
      _job.QueryHelper = MockQuery.Object;
    }

    [Test]
    public void TestExecuteSuccess()
    {
      // Arrange 
      var dbMock = new Mock<IEHHelper>();
      var dbContext = new Mock<IDBContext>();
      var table = CreateDataTable(Constant.Status.Job.ERROR, Constant.Status.Job.COMPLETED);

      dbContext.Setup(x => x.ExecuteSqlStatementAsDataTable(It.IsAny<string>()))
        .Returns(table).Verifiable();
      dbMock.Setup(x => x.GetActiveCaseID())
        .Returns(1);
      dbMock.Setup(x => x.GetDBContext(It.IsAny<int>()))
        .Returns(dbContext.Object);
      _job.Helper = dbMock.Object;

      MockQuery.Setup(x => x.RetrieveReproduceWorkerQueueAsync(It.IsAny<IDBContext>(), It.IsAny<int>()))
        .Returns(Task.Factory.StartNew(() => new DataTable())).Verifiable();

      //Act
      var response = _job.Execute();

      //Assert
      Assert.IsTrue(response.Success);
      MockQuery.Verify(x => x.RetrieveArtifactIDsAsync(It.IsAny<IDBContext>(), It.IsAny<string>()), Times.Once);
      MockQuery.Verify(x => x.RetrieveReproduceWorkerQueueAsync(It.IsAny<IDBContext>(), It.IsAny<int>()), Times.Once);
    }

    [Test]
    public void TestCleanup()
    {
      // Arrange 
      var dbMock = new Mock<IEHHelper>();
      var dbContext = new Mock<IDBContext>();
      var table = CreateDataTable(Constant.Status.Job.ERROR, Constant.Status.Job.COMPLETED);

      dbContext.Setup(x => x.ExecuteSqlStatementAsDataTable(It.IsAny<string>())).Returns(table);
      dbMock.Setup(x => x.GetActiveCaseID()).Returns(1);
      dbMock.Setup(x => x.GetDBContext(It.IsAny<int>())).Returns(dbContext.Object);

      _job.Helper = dbMock.Object;

      var dataTable = new DataTable();
      dataTable.Columns.Add(new DataColumn("WorkspaceArtifactID", typeof(int)));
      dataTable.Columns.Add(new DataColumn("SavedSearchHoldingTable", typeof(string)));
      dataTable.Columns.Add(new DataColumn("RedactionsHoldingTable", typeof(string)));
      dataTable.Rows.Add(1, "SavedSearchHoldingTable", "RedactionsHoldingTable");

      MockQuery.Setup(x => x.RetrieveReproduceWorkerQueueAsync(It.IsAny<IDBContext>(), It.IsAny<int>()))
        .Returns(Task.Factory.StartNew(() => dataTable)).Verifiable();
      MockQuery.Setup(x => x.DropTableAsync(It.IsAny<IDBContext>(), It.IsAny<string>()))
        .Returns(Task.Factory.StartNew(() => dataTable)).Verifiable();

      var response = _job.Execute();
      Assert.IsTrue(response.Success);

      MockQuery.Verify(x => x.RetrieveArtifactIDsAsync(It.IsAny<IDBContext>(), It.IsAny<string>()), Times.Once);
      MockQuery.Verify(x => x.RetrieveReproduceWorkerQueueAsync(It.IsAny<IDBContext>(), It.IsAny<int>()), Times.Once);
      MockQuery.Verify(x => x.DropTableAsync(It.IsAny<IDBContext>(), "SavedSearchHoldingTable"), Times.Once);
      MockQuery.Verify(x => x.DropTableAsync(It.IsAny<IDBContext>(), "RedactionsHoldingTable"), Times.Once);
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

      _job.Helper = dbMock.Object;

      MockQuery.Setup(x => x.RetrieveReproduceWorkerQueueAsync(It.IsAny<IDBContext>(), It.IsAny<int>()))
        .Returns(Task.Factory.StartNew(() => new DataTable()))
        .Verifiable();

      var response = _job.Execute();
      Assert.IsFalse(response.Success);
      Assert.NotNull(response.Exception);

      MockQuery.Verify(x => x.RetrieveArtifactIDsAsync(It.IsAny<IDBContext>(), It.IsAny<string>()), Times.Never);
      MockQuery.Verify(x => x.RetrieveReproduceWorkerQueueAsync(It.IsAny<IDBContext>(), It.IsAny<int>()), Times.Never);
    }

    private DataTable CreateDataTable(params string[] statusStrings)
    {
      var table = new DataTable();
      var columnSpec = new DataColumn
      {
        DataType = typeof(string),
        ColumnName = "Status"
      };
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