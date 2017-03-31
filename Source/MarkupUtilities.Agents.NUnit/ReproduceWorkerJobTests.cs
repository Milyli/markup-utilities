using System.Collections.Generic;
using System.Data;
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
  public class ReproduceWorkerJobTests
  {
    private ReproduceWorkerJob _reproduceWorkerJob;
    private Mock<IServicesMgr> _serviceMgr;
    private Mock<IAgentHelper> _agentHelper;
    private Mock<IQuery> _query;
    private Mock<IArtifactQueries> _artifactQueries;
    private Mock<IErrorQueries> _errorQueries;
    static readonly Task SuccessTask = Task.FromResult<object>(null);

    private readonly string[] _redactionFields = {
      "Order", "X", "Y", "Width", "Height", "MarkupType", "FillA", "FillR", "FillG", "FillB", "BorderSize",
      "BorderA", "BorderR",
      "BorderG", "BorderB", "BorderStyle", "FontName", "FontA", "FontR", "FontG", "FontB", "FontSize",
      "FontStyle", "Text", "ZOrder", "DrawCrossLines"
    };

    [SetUp]
    public void SetUp()
    {
      _serviceMgr = new Mock<IServicesMgr>();
      _agentHelper = new Mock<IAgentHelper>();
      _query = new Mock<IQuery>();
      _artifactQueries = new Mock<IArtifactQueries>();
      _errorQueries = new Mock<IErrorQueries>();
      _reproduceWorkerJob = new ReproduceWorkerJob(1, _agentHelper.Object, _query.Object, _artifactQueries.Object, new List<int>(), _errorQueries.Object);
      _agentHelper.Setup(x => x.GetDBContext(It.IsAny<int>())).Returns(new Mock<IDBContext>().Object);
      _agentHelper.Setup(x => x.GetServicesManager()).Returns(_serviceMgr.Object);
      _query.Setup(x => x.ResetUnfishedJobsAsync(It.IsAny<IDBContext>(), 1, It.IsAny<string>()))
        .Returns(SuccessTask)
        .Verifiable();
      var dataTable = new DataTable();

      dataTable.Columns.Add("WorkspaceArtifactID", typeof(int));
      dataTable.Columns.Add("ID", typeof(int));
      dataTable.Columns.Add("DocumentIDStart", typeof(int));
      dataTable.Columns.Add("DocumentIDEnd", typeof(int));
      dataTable.Columns.Add("SavedSearchHoldingTable", typeof(string));
      dataTable.Columns.Add("RedactionsHoldingTable", typeof(string));
      dataTable.Columns.Add("QueueStatus", typeof(int));
      dataTable.Columns.Add("ReproduceJobArtifactID", typeof(int));
      dataTable.Columns.Add("ResourceGroupID", typeof(int));
      dataTable.Columns.Add("RedactionCodeTypeID", typeof(int));
      dataTable.Columns.Add("MarkupSetRedactionCodeArtifactID", typeof(int));
      dataTable.Columns.Add("MarkupSetAnnotationCodeArtifactID", typeof(int));
      dataTable.Columns.Add("RelationalGroupColumn", typeof(string));
      dataTable.Columns.Add("HasAutoRedactionsColumn", typeof(string));
      dataTable.Columns.Add("RelationalGroup", typeof(string));
      dataTable.Rows.Add(1, 2, 3, 4, "5", "6", 7, 8, 9, 10, 11, 12, "13", "14", "15");

      _query.Setup(x => x.RetrieveNextInReproduceWorkerQueueAsync(It.IsAny<IDBContext>(), 1, It.IsAny<string>()))
        .Returns(Task.FromResult(dataTable))
        .Verifiable();
      _query.Setup(x => x.RemoveRecordFromReproduceWorkerQueueAsync(It.IsAny<IDBContext>(), It.IsAny<int>()))
        .Returns(SuccessTask)
        .Verifiable();
    }

    [Test]
    public async Task TestExecuteAsyncNoResourceIds()
    {
      await _reproduceWorkerJob.ExecuteAsync();
      _query.Verify(x => x.ResetUnfishedJobsAsync(It.IsAny<IDBContext>(), 1, It.IsAny<string>()), Times.Once);
      _query.Verify(x => x.RetrieveNextInReproduceWorkerQueueAsync(It.IsAny<IDBContext>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task TestExecuteAsyncCancelledJob()
    {
      _artifactQueries.Setup(x => x.RetrieveReproduceJobAsync(_serviceMgr.Object,
        ExecutionIdentity.CurrentUser,
        It.IsAny<int>(),
        It.IsAny<int>()))
        .Returns(Task.FromResult(new MarkupUtilityReproduceJob(1, "", 1, 1, 1, Constant.Status.Job.CANCELREQUESTED, "", 1, 1, -1, -1)))
        .Verifiable();
      _reproduceWorkerJob.AgentResourceGroupIds = new List<int>() { 1 };
      _query.Setup(x => x.BulkInsertRedactionRecordsForDocumentRange(It.IsAny<IDBContext>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
        .Returns(Task.FromResult(new DataTable()))
        .Verifiable();

      await _reproduceWorkerJob.ExecuteAsync();
      _query.Verify(x => x.ResetUnfishedJobsAsync(It.IsAny<IDBContext>(), 1, It.IsAny<string>()), Times.Once);
      _query.Verify(x => x.RetrieveNextInReproduceWorkerQueueAsync(It.IsAny<IDBContext>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
      _query.Verify(x => x.BulkInsertRedactionRecordsForDocumentRange(It.IsAny<IDBContext>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never());
      _query.Verify(x => x.RemoveRecordFromReproduceWorkerQueueAsync(It.IsAny<IDBContext>(), It.IsAny<int>()), Times.Once);
    }

    [Test]
    public async Task TestExecuteAsyncInProgressJobAcrossDocumentSet()
    {
      _artifactQueries.Setup(x => x.RetrieveReproduceJobAsync(_serviceMgr.Object, ExecutionIdentity.CurrentUser, It.IsAny<int>(), It.IsAny<int>()))
        .Returns(Task.FromResult(new MarkupUtilityReproduceJob(1, "", 1, 1, 1, Constant.Status.Job.IN_PROGRESS_WORKER, "", 1, 1, 1, -1)))
        .Verifiable();
      _reproduceWorkerJob.AgentResourceGroupIds = new List<int> { 1 };

      var redactions = new DataTable();
      redactions.Columns.Add("ID", typeof(int));
      redactions.Columns.Add("Identifier", typeof(string));
      redactions.Columns.Add("DocumentArtifactID", typeof(int));
      redactions.Columns.Add("FileGuid", typeof(string));
      redactions.Columns.Add("MarkupSetArtifactID", typeof(int));

      foreach (var field in _redactionFields)
      {
        redactions.Columns.Add(field, typeof(int));
      }
      redactions.Rows.Add(1, "id", 1, "guid", 1, 1);
      redactions.Rows.Add(1, "id", 1, "guid", 1, 1);
      redactions.Rows.Add(1, "id", 1, "guid", 1, 1);

      _query.Setup(x => x.BulkInsertRedactionRecordsForDocumentRange(It.IsAny<IDBContext>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
        .Returns(Task.FromResult(redactions))
        .Verifiable();

      _query.Setup(x => x.UpdateHasRedactionsOrHighlightsAsync(It.IsAny<IDBContext>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Returns(Task.FromResult(3)).Verifiable();
      _query.Setup(x => x.RetrieveRedactionInfoAsync(It.IsAny<IDBContext>(), It.IsAny<int>())).Returns(Task.FromResult(redactions)).Verifiable();
      _artifactQueries.Setup(x => x.CreateMarkupUtilityHistoryRecordAsync(It.IsAny<IServicesMgr>(), It.IsAny<ExecutionIdentity>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
        .Returns(SuccessTask)
        .Verifiable();

      await _reproduceWorkerJob.ExecuteAsync();
      _query.Verify(x => x.ResetUnfishedJobsAsync(It.IsAny<IDBContext>(), 1, It.IsAny<string>()), Times.Once);
      _query.Verify(x => x.RetrieveNextInReproduceWorkerQueueAsync(It.IsAny<IDBContext>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
      _query.Verify(x => x.BulkInsertRedactionRecordsForDocumentRange(It.IsAny<IDBContext>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
      _query.Verify(x => x.UpdateHasRedactionsOrHighlightsAsync(It.IsAny<IDBContext>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(2));

      _artifactQueries.Verify(x => x.CreateMarkupUtilityHistoryRecordAsync(It.IsAny<IServicesMgr>(), It.IsAny<ExecutionIdentity>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(redactions.Rows.Count));
      _query.Verify(x => x.RemoveRecordFromReproduceWorkerQueueAsync(It.IsAny<IDBContext>(), It.IsAny<int>()), Times.Once);
    }

    [Test]
    public async Task TestExecuteAsyncInProgressJobAcrossRelationalGroup()
    {
      _artifactQueries.Setup(x => x.RetrieveReproduceJobAsync(_serviceMgr.Object, ExecutionIdentity.CurrentUser, It.IsAny<int>(), It.IsAny<int>()))
        .Returns(Task.FromResult(new MarkupUtilityReproduceJob(1, "", 1, 1, 1, Constant.Status.Job.IN_PROGRESS_WORKER, "", 1, 1, 1, 1)))
        .Verifiable();
      _reproduceWorkerJob.AgentResourceGroupIds = new List<int> { 1 };

      var redactions = new DataTable();
      redactions.Columns.Add("ID", typeof(int));
      redactions.Columns.Add("Identifier", typeof(string));
      redactions.Columns.Add("DocumentArtifactID", typeof(int));
      redactions.Columns.Add("FileGuid", typeof(string));
      redactions.Columns.Add("MarkupSetArtifactID", typeof(int));

      foreach (var field in _redactionFields)
      {
        redactions.Columns.Add(field, typeof(int));
      }
      redactions.Rows.Add(1, "id", 1, "guid", 1, 1);
      redactions.Rows.Add(1, "id", 1, "guid", 1, 1);
      redactions.Rows.Add(1, "id", 1, "guid", 1, 1);

      _query.Setup(x => x.BulkInsertRedactionRecordsForRelationalGroup(It.IsAny<IDBContext>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        .Returns(Task.FromResult(redactions))
        .Verifiable();

      _query.Setup(x => x.UpdateHasRedactionsOrHighlightsAsync(It.IsAny<IDBContext>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Returns(Task.FromResult(3)).Verifiable();
      _query.Setup(x => x.RetrieveRedactionInfoAsync(It.IsAny<IDBContext>(), It.IsAny<int>())).Returns(Task.FromResult(redactions)).Verifiable();
      _artifactQueries.Setup(x => x.CreateMarkupUtilityHistoryRecordAsync(It.IsAny<IServicesMgr>(), It.IsAny<ExecutionIdentity>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
        .Returns(SuccessTask)
        .Verifiable();

      await _reproduceWorkerJob.ExecuteAsync();

      _query.Verify(x => x.ResetUnfishedJobsAsync(It.IsAny<IDBContext>(), 1, It.IsAny<string>()), Times.Once);
      _query.Verify(x => x.RetrieveNextInReproduceWorkerQueueAsync(It.IsAny<IDBContext>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
      _query.Verify(x => x.BulkInsertRedactionRecordsForRelationalGroup(It.IsAny<IDBContext>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

      _artifactQueries.Verify(x => x.CreateMarkupUtilityHistoryRecordAsync(It.IsAny<IServicesMgr>(), It.IsAny<ExecutionIdentity>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(redactions.Rows.Count));
      _query.Verify(x => x.RemoveRecordFromReproduceWorkerQueueAsync(It.IsAny<IDBContext>(), It.IsAny<int>()), Times.Once);
    }
  }
}