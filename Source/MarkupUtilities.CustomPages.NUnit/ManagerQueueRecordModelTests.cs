using System;
using System.Threading.Tasks;
using MarkupUtilities.CustomPages.Models;
using MarkupUtilities.Helpers;
using Moq;
using NUnit.Framework;

namespace MarkupUtilities.CustomPages.NUnit
{
  [TestFixture]
  class ManagerQueueRecordModelTests
  {

    #region Tests

    [Test]
    public async Task NewManagerQueueRecordTest_NoAgentIdAsync()
    {
      //Arrange
      var queryMock = new Mock<IQuery>();
      var dt = await DataHelpers.ManagerAgentData.BuildDataTableAsync();

      const int workspaceArtifactId = 12345;
      const string workspaceName = "Test Workspace";

      const int row1Id = 1;
      var row1AddedOn = DateTime.UtcNow.AddTicks(-(DateTime.UtcNow.Ticks % TimeSpan.TicksPerSecond));
      const string row1Status = "Waiting";
      const int row1Priority = 10;
      const string row1AddedBy = "Doe, Jane";
      const int row1ArtifactId = 88888;

      var dataRow = await DataHelpers.ManagerAgentData.BuildDataRowAsync(dt, row1Id, row1AddedOn, workspaceArtifactId, workspaceName, row1Status, null, row1Priority, row1AddedBy, row1ArtifactId);

      //Act 
      var record = new ManagerQueueRecordModel(dataRow, queryMock.Object);

      //Assert
      Assert.AreEqual(row1Id, record.Id);
      Assert.AreEqual(row1AddedOn, record.AddedOn);
      Assert.AreEqual(workspaceArtifactId, record.WorkspaceArtifactId);
      Assert.AreEqual(workspaceName, record.WorkspaceName);
      Assert.AreEqual(row1Status, record.Status);
      Assert.AreEqual(null, record.AgentId);
      Assert.AreEqual(row1Priority, record.Priority);
      Assert.AreEqual(row1AddedBy, record.AddedBy);
      Assert.AreEqual(row1ArtifactId, record.RecordArtifactId);
    }

    [Test]
    public async Task NewManagerQueueRecordTest_AgentIdAsync()
    {
      //Arrange
      var queryMock = new Mock<IQuery>();
      var dt = await DataHelpers.ManagerAgentData.BuildDataTableAsync();

      const int workspaceArtifactId = 12345;
      const string workspaceName = "Test Workspace";

      const int row1Id = 1;
      var row1AddedOn = DateTime.UtcNow.AddTicks(-(DateTime.UtcNow.Ticks % TimeSpan.TicksPerSecond));
      const string row1Status = "Waiting";
      const int row1Priority = 10;
      const string row1AddedBy = "Doe, Jane";
      const int row1ArtifactId = 88888;
      const int row1AgentId = 999999;

      var dataRow = await DataHelpers.ManagerAgentData.BuildDataRowAsync(dt, row1Id, row1AddedOn, workspaceArtifactId, workspaceName, row1Status, row1AgentId, row1Priority, row1AddedBy, row1ArtifactId);

      //Act 
      var record = new ManagerQueueRecordModel(dataRow, queryMock.Object);

      //Assert
      Assert.AreEqual(row1Id, record.Id);
      Assert.AreEqual(row1AddedOn, record.AddedOn);
      Assert.AreEqual(workspaceArtifactId, record.WorkspaceArtifactId);
      Assert.AreEqual(workspaceName, record.WorkspaceName);
      Assert.AreEqual(row1Status, record.Status);
      Assert.AreEqual(row1AgentId, record.AgentId);
      Assert.AreEqual(row1Priority, record.Priority);
      Assert.AreEqual(row1AddedBy, record.AddedBy);
      Assert.AreEqual(row1ArtifactId, record.RecordArtifactId);
    }

    [Test]
    public async Task NewManagerQueueRecordTest_NoValuesAsync()
    {
      //Arrange
      var queryMock = new Mock<IQuery>();
      var dt = await DataHelpers.ManagerAgentData.BuildDataTableAsync();
      var dataRow = await DataHelpers.ManagerAgentData.BuildEmptyDataRowAsync(dt);

      //Act 
      var record = new ManagerQueueRecordModel(dataRow, queryMock.Object);

      //Assert
      Assert.AreEqual(0, record.Id);
      Assert.AreEqual(new DateTime(), record.AddedOn);
      Assert.AreEqual(0, record.WorkspaceArtifactId);
      Assert.AreEqual(string.Empty, record.WorkspaceName);
      Assert.AreEqual(string.Empty, record.Status);
      Assert.AreEqual(null, record.AgentId);
      Assert.AreEqual(0, record.Priority);
      Assert.AreEqual(string.Empty, record.AddedBy);
      Assert.AreEqual(0, record.RecordArtifactId);
    }

    #endregion
  }
}
