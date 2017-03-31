using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using MarkupUtilities.Helpers;
using MarkupUtilities.Helpers.Models;
using MarkupUtilities.Helpers.Rsapi.Interfaces;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Constant = MarkupUtilities.Helpers.Constant;

namespace MarkupUtilities.Agents.NUnit
{
  [TestFixture]
  public class ExportManagerAgentTests
  {
    public int AgentId;
    public Mock<IQuery> MockQuery;
    public Mock<IAgentHelper> MockAgentHelper;
    private List<int> _resourceGroupIdList;
    protected List<int> EmptyResourceGroupIdList;
    public Mock<IArtifactQueries> MockArtifactQueries;
    public Mock<IServicesMgr> MockServiceManager;
    public Mock<Helpers.Utility.IQuery> MockUtilityQueryHelper;
    public Mock<IErrorQueries> MockErrorQueries;
    public Mock<IMarkupTypeHelper> MockMarkupTypeHelper;

    [SetUp]
    public void Setup()
    {
      AgentId = 1234567;
      MockQuery = new Mock<IQuery>();
      MockAgentHelper = new Mock<IAgentHelper>();
      MockArtifactQueries = new Mock<IArtifactQueries>();
      MockServiceManager = new Mock<IServicesMgr>();
      MockErrorQueries = new Mock<IErrorQueries>();
      MockUtilityQueryHelper = new Mock<Helpers.Utility.IQuery>();
      _resourceGroupIdList = new List<int> { 10000, 20000 };
      EmptyResourceGroupIdList = new List<int>();
      MockMarkupTypeHelper = new Mock<IMarkupTypeHelper>();
    }


    #region Tests

    [Description("When a record is picked up by the agent, agent should complete execution process")]
    [Test]
    public async Task ExecuteAsync_QueueHasARecord_ExecuteAll()
    {
      //Arrange
      MockRetrieveNextInExportManagerQueueAsync();
      var markupUtilityExportJob = GetMigrationRedactionExportJob(Constant.Status.Job.NEW);
      MockRetrieveRdoJobStatusAsync(Constant.Status.Job.NEW);
      MockRetrieveExportJobAsync();
      MockRetrieveExportJobAsync(markupUtilityExportJob);
      MockGetMappingsForWorkerQueue();
      MockAddDocumentsToHoldingTableAsync();

      //Act
      var exportmanagerJob = GetExportManagerJob();
      await exportmanagerJob.ExecuteAsync();

      //Assert
      AssertRetrieveRdoJobStatusAsync(1);
      AssertRetrieveNextInExportManagerQueueAsync(1);
      AssertRetrieveExportJobAsync(1);
      AssertCopyRecordsToExportWorkerQueueAsync(1);
      AssertRemoveRecordFromTableByIdAsync(2);
    }

    [Description("No records in queue, agent should not process")]
    [Test]
    public async Task ExecuteAsync_QueueHasNoRecord_DoNotExecute()
    {
      //Arrange
      MockRetrieveNextInExportManagerQueueAsyncEmptytable();
      var markupUtilityExportJob = GetMigrationRedactionExportJob(Constant.Status.Job.NEW);
      MockRetrieveRdoJobStatusAsync(Constant.Status.Job.NEW);
      MockRetrieveExportJobAsync();
      MockRetrieveExportJobAsync(markupUtilityExportJob);
      MockGetMappingsForWorkerQueue();
      MockAddDocumentsToHoldingTableAsync();
      var exportmanagerJob = GetExportManagerJob();
      var wasCalled = false;
      var message = string.Empty;

      exportmanagerJob.OnMessage += (sender, args) => { wasCalled = true; message = args; };

      //Act
      await exportmanagerJob.ExecuteAsync();

      //Assert
      Assert.IsTrue(wasCalled);
      Assert.AreEqual(message, Constant.AgentRaiseMessages.NO_RECORDS_IN_QUEUE_FOR_THIS_RESOURCE_POOL);
    }

    [Description("When there are no resource group ids, record is not processed")]
    [Test]
    public async Task ExecuteAsync_QueueHasARecord_NotDuringOffHours()
    {
      //Arrange
      MockRetrieveNextInExportManagerQueueAsyncEmptytable();
      var markupUtilityExportJob = GetMigrationRedactionExportJob(Constant.Status.Job.NEW);
      MockRetrieveRdoJobStatusAsync(Constant.Status.Job.NEW);
      MockRetrieveExportJobAsync();
      MockRetrieveExportJobAsync(markupUtilityExportJob);
      MockGetMappingsForWorkerQueue();
      MockAddDocumentsToHoldingTableAsync();
      var exportmanagerJob = GetExportManagerJobWithNoResourceGroupId();
      var wasCalled = false;
      var message = string.Empty;

      exportmanagerJob.OnMessage += (sender, args) => { wasCalled = true; message = args; };

      //Act
      await exportmanagerJob.ExecuteAsync();

      //Assert
      Assert.IsTrue(wasCalled);
      Assert.AreEqual(message, Constant.AgentRaiseMessages.AGENT_SERVER_NOT_PART_OF_ANY_RESOURCE_POOL);
    }

    [Test]
    [Description("When a job is cancelled, the manager should fail")]
    public async Task ExecuteAsync_CancelJob()
    {
      //Arrange
      MockRetrieveNextInExportManagerQueueAsync();
      var markupUtilityExportJob = GetMigrationRedactionExportJob(Constant.Status.Job.CANCELLED);
      MockRetrieveRdoJobStatusAsync(Constant.Status.Job.CANCELLED);
      MockRetrieveExportJobAsync();
      MockRetrieveExportJobAsync(markupUtilityExportJob);
      MockGetMappingsForWorkerQueue();
      MockAddDocumentsToHoldingTableAsync();
      var exportmanagerJob = GetExportManagerJob();
      var wasCalled = false;

      exportmanagerJob.OnMessage += (sender, args) => { wasCalled = true; };

      //Act
      await exportmanagerJob.ExecuteAsync();

      //Assert
      Assert.IsTrue(wasCalled);
    }

    #endregion Tests

    #region Test Helpers

    private MarkupUtilityExportJob GetMigrationRedactionExportJob(string jobstatus)
    {
      var exportManagerJob = new MarkupUtilityExportJob(
        3456789, // artifact
        "Test Name",   // Name
        5677, // markupsetartifactid
        Constant.MarkupSubTypeCategory.SupportedMarkupUtilityTypes,  // redaction type
        6776, // saved search
        345345,  // file artifact id
        jobstatus,
        It.IsAny<string>(),
        It.IsAny<int>())
      ;
      return exportManagerJob;
    }

    private ExportManagerJob GetExportManagerJob()
    {
      var exportManagerJob = new ExportManagerJob(
        123445,
        MockServiceManager.Object,
        MockAgentHelper.Object,
        MockQuery.Object,
        new DateTime(2016, 01, 25, 01, 00, 00),
        _resourceGroupIdList,
        MockArtifactQueries.Object,
        MockUtilityQueryHelper.Object,
        MockErrorQueries.Object,
        MockMarkupTypeHelper.Object)
      ;
      return exportManagerJob;
    }

    private ExportManagerJob GetExportManagerJobWithNoResourceGroupId()
    {
      var exportManagerJob = new ExportManagerJob(
        AgentId,
        MockServiceManager.Object,
        MockAgentHelper.Object,
        MockQuery.Object,
        new DateTime(2016, 01, 25, 01, 00, 00),
        EmptyResourceGroupIdList,
        MockArtifactQueries.Object,
        MockUtilityQueryHelper.Object,
        MockErrorQueries.Object,
        MockMarkupTypeHelper.Object)
      ;
      return exportManagerJob;
    }

    private void MockRetrieveNextInExportManagerQueueAsync()
    {
      var table = GetExportManagerTable();
      MockQuery.Setup(x => x.RetrieveNextInJobManagerQueueAsync(It.IsAny<IDBContext>(), It.IsAny<int>(), It.IsAny<string>(), Constant.Tables.ExportManagerQueue, "ExportJobArtifactID")).ReturnsAsync(table);
    }

    private void MockRetrieveNextInExportManagerQueueAsyncEmptytable()
    {
      MockQuery.Setup(x => x.RetrieveNextInJobManagerQueueAsync(It.IsAny<IDBContext>(), It.IsAny<int>(), It.IsAny<string>(), Constant.Tables.ExportManagerQueue, "ExportJobArtifactID")).ReturnsAsync(null);
    }

    private void MockRetrieveRdoJobStatusAsync(string jobType)
    {
      MockArtifactQueries.Setup(x => x.RetrieveRdoJobStatusAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<int>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>()
          ))
        .Returns(Task.FromResult(jobType))
        .Verifiable();
    }

    private void MockRetrieveExportJobAsync()
    {
      MockArtifactQueries.Setup(x => x.RetrieveRdoJobStatusAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<int>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>()
          ))
        .Returns(Task.FromResult(Constant.Status.Job.NEW))
        .Verifiable();
    }

    private void MockRetrieveExportJobAsync(MarkupUtilityExportJob markupUtilityExportJob)
    {
      MockArtifactQueries.Setup(x => x.RetrieveExportJobAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<int>()
          ))
        .Returns(Task.FromResult(markupUtilityExportJob))
        .Verifiable();
    }
    private void MockAddDocumentsToHoldingTableAsync()
    {
      MockArtifactQueries.Setup(x => x.AddDocumentsToHoldingTableAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<IDBContext>(),
          It.IsAny<Helpers.Utility.IQuery>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<List<SqlBulkCopyColumnMapping>>()
          )).Returns(Task.FromResult(true))
        .Verifiable();
    }

    private void MockGetMappingsForWorkerQueue()
    {
      var columns = new List<string> { "DocumentArtifactID" };
      const string columnName = "DocumentArtifactID";
      var test = columns.Select(column => new SqlBulkCopyColumnMapping(columnName, columnName)).ToList();

      MockUtilityQueryHelper.Setup(x => x.GetMappingsForWorkerQueue(It.IsAny<List<string>>())).Returns(test).Verifiable();
    }

    private static DataTable GetExportManagerTable()
    {
      var table = new DataTable("Test Manager Table");
      table.Columns.Add("WorkspaceArtifactID", typeof(int));
      table.Columns.Add("ID", typeof(int));
      table.Columns.Add("ArtifactID", typeof(int));
      table.Columns.Add("Priority", typeof(int));
      table.Columns.Add("ResourceGroupID", typeof(int));
      table.Columns.Add("ExportJobArtifactID", typeof(int));
      table.Rows.Add(2345678, 1, 3456789, 3, 1000001, 100);
      return table;
    }

    #endregion Test Helpers

    #region Asserts

    private void AssertRetrieveRdoJobStatusAsync(int timesCalled)
    {
      MockArtifactQueries.Verify(x => x.RetrieveRdoJobStatusAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<int>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>()
          ),
        Times.Exactly(timesCalled));
    }

    private void AssertRetrieveNextInExportManagerQueueAsync(int timesCalled)
    {
      MockQuery.Verify(x => x.RetrieveNextInJobManagerQueueAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<int>(),
          It.IsAny<string>(), Constant.Tables.ExportManagerQueue, "ExportJobArtifactID"),
        Times.Exactly(timesCalled));
    }

    private void AssertRetrieveExportJobAsync(int timesCalled)
    {
      MockArtifactQueries.Verify(x => x.RetrieveExportJobAsync(
          It.IsAny<IServicesMgr>(),
          It.IsAny<ExecutionIdentity>(),
          It.IsAny<int>(),
          It.IsAny<int>()
          ),
        Times.Exactly(timesCalled));
    }

    private void AssertCopyRecordsToExportWorkerQueueAsync(int timesCalled)
    {
      MockQuery.Verify(x => x.CopyRecordsToExportWorkerQueueAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<string>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<int>()
          ),
        Times.Exactly(timesCalled));
    }

    private void AssertRemoveRecordFromTableByIdAsync(int timesCalled)
    {
      MockQuery.Verify(x => x.RemoveRecordFromTableByIdAsync(
          It.IsAny<IDBContext>(),
          It.IsAny<string>(),
          It.IsAny<int>()
          ),
        Times.Exactly(timesCalled));
    }
    #endregion
  }
}
