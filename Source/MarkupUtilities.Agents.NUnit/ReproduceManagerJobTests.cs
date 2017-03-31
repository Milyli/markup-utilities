using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using MarkupUtilities.Helpers;
using MarkupUtilities.Helpers.Models;
using MarkupUtilities.Helpers.Rsapi.Interfaces;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace MarkupUtilities.Agents.NUnit
{
  [TestFixture]
  public class ReproduceManagerJobTests
  {
    private ReproduceManagerJob _reproduceManagerJob;
    private Mock<IServicesMgr> _serviceMgr;
    private Mock<IAgentHelper> _agentHelper;
    private Mock<IQuery> _query;
    private Mock<IArtifactQueries> _artifactQueries;
    private Mock<Helpers.Utility.IQuery> _utilityQuery;
    private Mock<IErrorQueries> _errorQueries;
    private static readonly Task SuccessTask = Task.FromResult<object>(null);

    [SetUp]
    public void SetUp()
    {
      _serviceMgr = new Mock<IServicesMgr>();
      _agentHelper = new Mock<IAgentHelper>();
      _query = new Mock<IQuery>();
      _artifactQueries = new Mock<IArtifactQueries>();
      _utilityQuery = new Mock<Helpers.Utility.IQuery>();
      _errorQueries = new Mock<IErrorQueries>();

      _reproduceManagerJob = new ReproduceManagerJob(1, _agentHelper.Object, _query.Object, DateTime.Now, new List<int>(), _artifactQueries.Object, _utilityQuery.Object, _errorQueries.Object);

      _query.Setup(x => x.ResetUnfishedJobsAsync(It.IsAny<IDBContext>(), 1, It.IsAny<string>())).Returns(SuccessTask).Verifiable();
      _query.Setup(x => x.CreateSavedSearchHoldingTableAsync(It.IsAny<IDBContext>(), It.IsAny<string>(), false)).Returns(SuccessTask).Verifiable();
      _query.Setup(x => x.CreateRedactionsHoldingTableAsync(It.IsAny<IDBContext>(), It.IsAny<string>(), false)).Returns(SuccessTask).Verifiable();

      var dataTable = new DataTable();
      dataTable.Columns.Add("WorkspaceArtifactID", typeof(int));
      dataTable.Columns.Add("ID", typeof(int));
      dataTable.Columns.Add("ReproduceJobArtifactID", typeof(int));
      dataTable.Columns.Add("ResourceGroupID", typeof(int));
      dataTable.Rows.Add(1, 2, 3, 4);

      _query.Setup(x => x.RetrieveNextInJobManagerQueueAsync(It.IsAny<IDBContext>(), 1, It.IsAny<string>(), Constant.Tables.ReproduceManagerQueue, "ReproduceJobArtifactID")).Returns(Task.FromResult(dataTable)).Verifiable();
      _agentHelper.Setup(x => x.GetDBContext(It.IsAny<int>())).Returns(new Mock<IDBContext>().Object);
      _agentHelper.Setup(x => x.GetServicesManager()).Returns(_serviceMgr.Object);
      _artifactQueries.Setup(x => x.AddDocumentsToHoldingTableAsync(It.IsAny<IServicesMgr>(), It.IsAny<IDBContext>(), _utilityQuery.Object, It.IsAny<ExecutionIdentity>(), 1, It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<SqlBulkCopyColumnMapping>>())).Returns(Task.FromResult(true));
    }

    [Test]
    public void TestHoldingTableNames()
    {
      var savedSearchHoldingTable = _reproduceManagerJob.SavedSearchHoldingTable;
      Assert.AreSame(savedSearchHoldingTable, _reproduceManagerJob.SavedSearchHoldingTable);

      var reproduceManagerJob2 = new ReproduceManagerJob(1, _agentHelper.Object, _query.Object, DateTime.Now, new List<int>(), _artifactQueries.Object, _utilityQuery.Object, _errorQueries.Object);
      Assert.AreNotSame(savedSearchHoldingTable, reproduceManagerJob2.SavedSearchHoldingTable);

      var redactionsHoldingTable = _reproduceManagerJob.RedactionsHoldingTable;
      Assert.AreSame(redactionsHoldingTable, _reproduceManagerJob.RedactionsHoldingTable);
      Assert.AreNotSame(redactionsHoldingTable, reproduceManagerJob2.RedactionsHoldingTable);
    }

    [Test]
    public async Task TestExecuteAsyncNoResourceIds()
    {
      await _reproduceManagerJob.ExecuteAsync();
      _query.Verify(x => x.ResetUnfishedJobsAsync(It.IsAny<IDBContext>(), 1, It.IsAny<string>()), Times.Once);
      _query.Verify(x => x.RetrieveNextInJobManagerQueueAsync(It.IsAny<IDBContext>(), 1, It.IsAny<string>(), Constant.Tables.ReproduceManagerQueue, "ReproduceJobArtifactID"), Times.Never);
    }

    [Test]
    public async Task TestExecuteAsyncCancelledJob()
    {
      _artifactQueries.Setup(x => x.RetrieveReproduceJobAsync(_serviceMgr.Object, ExecutionIdentity.CurrentUser, 1, 3)).Returns(Task.FromResult(new MarkupUtilityReproduceJob(1, "", 1, 1, 1, "Cancel Requested", "", 1, 1, 1, 1))).Verifiable();
      _reproduceManagerJob.AgentResourceGroupIds = new List<int> { 1 };
      await _reproduceManagerJob.ExecuteAsync();
      _query.Verify(x => x.ResetUnfishedJobsAsync(It.IsAny<IDBContext>(), 1, It.IsAny<string>()), Times.Once);
      _query.Verify(x => x.RetrieveNextInJobManagerQueueAsync(It.IsAny<IDBContext>(), 1, It.IsAny<string>(), Constant.Tables.ReproduceManagerQueue, "ReproduceJobArtifactID"), Times.Once);
      _artifactQueries.Verify(x => x.RetrieveReproduceJobAsync(_serviceMgr.Object, ExecutionIdentity.CurrentUser, 1, 3), Times.Once);
      _query.Verify(x => x.CreateRedactionsHoldingTableAsync(It.IsAny<IDBContext>(), It.IsAny<string>(), false), Times.Never);
      _query.Verify(x => x.CreateSavedSearchHoldingTableAsync(It.IsAny<IDBContext>(), It.IsAny<string>(), false), Times.Never);
    }
    
    [Test]
    public async Task TestExecuteAsyncNullJob()
    {
      _artifactQueries.Setup(x => x.RetrieveReproduceJobAsync(_serviceMgr.Object, ExecutionIdentity.CurrentUser, 1, 3)).Returns(Task.FromResult((MarkupUtilityReproduceJob)null)).Verifiable();
      _reproduceManagerJob.AgentResourceGroupIds = new List<int> { 1 };
      await _reproduceManagerJob.ExecuteAsync();
      _query.Verify(x => x.ResetUnfishedJobsAsync(It.IsAny<IDBContext>(), 1, It.IsAny<string>()), Times.Once);
      _query.Verify(x => x.RetrieveNextInJobManagerQueueAsync(It.IsAny<IDBContext>(), 1, It.IsAny<string>(), Constant.Tables.ReproduceManagerQueue, "ReproduceJobArtifactID"), Times.Once);
      _artifactQueries.Verify(x => x.RetrieveReproduceJobAsync(_serviceMgr.Object, ExecutionIdentity.CurrentUser, 1, 3), Times.Once);
      _query.Verify(x => x.CreateRedactionsHoldingTableAsync(It.IsAny<IDBContext>(), It.IsAny<string>(), false), Times.Never);
      _query.Verify(x => x.CreateSavedSearchHoldingTableAsync(It.IsAny<IDBContext>(), It.IsAny<string>(), false), Times.Never);
    }

    [Test]
    public async Task TestExecuteAsyncSubmittedJobAcrossDocumentSet()
    {
      _artifactQueries.Setup(x => x.RetrieveReproduceJobAsync(_serviceMgr.Object, ExecutionIdentity.CurrentUser, 1, 3)).Returns(Task.FromResult(new MarkupUtilityReproduceJob(1, "", 1, 1, 1, "Submitted", "", 1, 1, 1, -1))).Verifiable();
      var documentColumn = new DataTable();
      documentColumn.Columns.Add("ColumnName", typeof(string));
      documentColumn.Rows.Add("columnname");
      _query.Setup(x => x.RetrieveDocumentColumnAsync(It.IsAny<IDBContext>(), It.IsAny<int>())).Returns(Task.FromResult(documentColumn));
      _reproduceManagerJob.AgentResourceGroupIds = new List<int> { 1 };
      await _reproduceManagerJob.ExecuteAsync();
      _query.Verify(x => x.ResetUnfishedJobsAsync(It.IsAny<IDBContext>(), 1, It.IsAny<string>()), Times.Once);
      _query.Verify(x => x.RetrieveNextInJobManagerQueueAsync(It.IsAny<IDBContext>(), 1, It.IsAny<string>(), Constant.Tables.ReproduceManagerQueue, "ReproduceJobArtifactID"), Times.Once);
      _artifactQueries.Verify(x => x.RetrieveReproduceJobAsync(_serviceMgr.Object, ExecutionIdentity.CurrentUser, 1, 3), Times.Once);
      _query.Verify(x => x.CreateRedactionsHoldingTableAsync(It.IsAny<IDBContext>(), It.IsAny<string>(), false), Times.Once);
      _query.Verify(x => x.CreateSavedSearchHoldingTableAsync(It.IsAny<IDBContext>(), It.IsAny<string>(), false), Times.Once);
      _artifactQueries.Verify(x => x.AddDocumentsToHoldingTableAsync(It.IsAny<IServicesMgr>(), It.IsAny<IDBContext>(), _utilityQuery.Object, It.IsAny<ExecutionIdentity>(), 1, It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<SqlBulkCopyColumnMapping>>()), Times.Once);
    }

    [Test]
    public async Task TestExecuteAsyncSubmittedJobAcrossRelationalGroup()
    {
      _artifactQueries.Setup(x => x.RetrieveReproduceJobAsync(_serviceMgr.Object, ExecutionIdentity.CurrentUser, 1, 3)).Returns(Task.FromResult(new MarkupUtilityReproduceJob(1, "", 1, 1, 1, "Submitted", "", 1, 1, 1, 1))).Verifiable();
      var documentColumn = new DataTable();
      documentColumn.Columns.Add("ColumnName", typeof(string));
      documentColumn.Rows.Add("columnname");
      _query.Setup(x => x.RetrieveDocumentColumnAsync(It.IsAny<IDBContext>(), It.IsAny<int>())).Returns(Task.FromResult(documentColumn));
      _reproduceManagerJob.AgentResourceGroupIds = new List<int> { 1 };
      await _reproduceManagerJob.ExecuteAsync();
      _query.Verify(x => x.ResetUnfishedJobsAsync(It.IsAny<IDBContext>(), 1, It.IsAny<string>()), Times.Once);
      _query.Verify(x => x.RetrieveNextInJobManagerQueueAsync(It.IsAny<IDBContext>(), 1, It.IsAny<string>(), Constant.Tables.ReproduceManagerQueue, "ReproduceJobArtifactID"), Times.Once);
      _artifactQueries.Verify(x => x.RetrieveReproduceJobAsync(_serviceMgr.Object, ExecutionIdentity.CurrentUser, 1, 3), Times.Once);
      _query.Verify(x => x.CreateRedactionsHoldingTableAsync(It.IsAny<IDBContext>(), It.IsAny<string>(), true), Times.Once);
      _query.Verify(x => x.CreateSavedSearchHoldingTableAsync(It.IsAny<IDBContext>(), It.IsAny<string>(), true), Times.Once);
      _query.Verify(x => x.RetrieveDocumentColumnAsync(It.IsAny<IDBContext>(), It.IsAny<int>()), Times.Exactly(2));
      _artifactQueries.Verify(x => x.AddDocumentsToHoldingTableAsync(It.IsAny<IServicesMgr>(), It.IsAny<IDBContext>(), _utilityQuery.Object, It.IsAny<ExecutionIdentity>(), 1, It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<SqlBulkCopyColumnMapping>>()), Times.Once);
    }

    [Test]
    public async Task TestExecuteAsyncBatchCreation()
    {
      _artifactQueries.Setup(x => x.RetrieveReproduceJobAsync(_serviceMgr.Object, ExecutionIdentity.CurrentUser, 1, 3)).Returns(Task.FromResult(new MarkupUtilityReproduceJob(1, "", 1, 1, 1, "Submitted", "", 1, 1, 0, 0))).Verifiable();
      _reproduceManagerJob.AgentResourceGroupIds = new List<int>() { 1 };

      var minMax = new DataTable();
      minMax.Columns.Add("MIN", typeof(int));
      minMax.Columns.Add("MAX", typeof(int));
      const int max = 10099;
      const int min = 5;
      minMax.Rows.Add(min, max);

      _query.Setup(x => x.RetrieveMinMaxIdAsync(It.IsAny<IDBContext>(), It.IsAny<string>())).Returns(Task.FromResult(minMax)).Verifiable();
      _query.Setup(x => x.InsertRowsIntoRedactionsHoldingTableAsync(It.IsAny<IDBContext>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), false)).Returns(Task.FromResult(2)).Verifiable();
      _query.Setup(x => x.InsertRowIntoReproduceWorkerQueueAsync(It.IsAny<IDBContext>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), null, null, null)).Returns(SuccessTask).Verifiable();

      var dt = new DataTable();
      dt.Columns.Add("CodeTypeID", typeof(int));
      dt.Columns.Add("RedactionCodeArtifactID", typeof(int));
      dt.Columns.Add("AnnotationCodeArtifactID", typeof(int));
      dt.Rows.Add(1, 1, 1);
      _query.Setup(x => x.RetrieveZCodesAsync(It.IsAny<IDBContext>(), It.IsAny<int>())).Returns(Task.FromResult(dt)).Verifiable();

      await _reproduceManagerJob.ExecuteAsync();

      _query.Verify(x => x.RetrieveMinMaxIdAsync(It.IsAny<IDBContext>(), It.IsAny<string>()), Times.Once);
      var timesCalled = (int)Math.Ceiling((max - min + 1.0) / Constant.Sizes.ReproduceJobBatchSize);
      _query.Verify(x => x.InsertRowsIntoRedactionsHoldingTableAsync(It.IsAny<IDBContext>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), false), Times.Exactly(timesCalled));
      var insertTimesCalled = (int)Math.Ceiling((max - min + 1.0) / Constant.Sizes.ReproduceJobInsertBatchSize);
      _query.Verify(x => x.InsertRowIntoReproduceWorkerQueueAsync(It.IsAny<IDBContext>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), null, null, null), Times.Exactly(insertTimesCalled));
    }
  }
}
