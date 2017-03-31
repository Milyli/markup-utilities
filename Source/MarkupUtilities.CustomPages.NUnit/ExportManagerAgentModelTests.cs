using System;
using System.Linq;
using System.Threading.Tasks;
using MarkupUtilities.CustomPages.Models;
using MarkupUtilities.Helpers;
using Moq;

using NUnit.Framework;
using Relativity.API;

namespace MarkupUtilities.CustomPages.NUnit
{
  [TestFixture]
  public class ExportManagerAgentModelTests
  {
    #region Tests

    [Test]
    public async Task GetAllExportManagerRecordsTestAsync()
    {
      //Arrange
      var queryMock = new Mock<IQuery>();
      var managerAgentModel = new ManagerAgentModel(queryMock.Object);
      var dbContextMock = new Mock<IDBContext>();
      var dt = await DataHelpers.ManagerAgentData.BuildDataTableAsync();

      const int workspaceArtifactId = 12345;
      const string workspaceName = "Test Workspace";

      const int row1Id = 1;
      var row1AddedOn = DateTime.UtcNow.AddTicks(-(DateTime.UtcNow.Ticks % TimeSpan.TicksPerSecond));
      const string row1Status = "Waiting";
      const int row1Priority = 10;
      const string row1AddedBy = "Doe, Jane";
      const int row1ArtifactId = 88888;

      const int row2Id = 2;
      var row2AddedOn = DateTime.UtcNow.AddTicks(-(DateTime.UtcNow.Ticks % TimeSpan.TicksPerSecond));
      const string row2Status = "In Progress";
      const int row2AgentId = 11111;
      const int row2Priority = 5;
      const string row2AddedBy = "Doe, John";
      const int row2ArtifactId = 9999999;

      var row1 = await DataHelpers.ManagerAgentData.BuildDataRowAsync(dt, row1Id, row1AddedOn, workspaceArtifactId, workspaceName, row1Status, null, row1Priority, row1AddedBy, row1ArtifactId);
      var row2 = await DataHelpers.ManagerAgentData.BuildDataRowAsync(dt, row2Id, row2AddedOn, workspaceArtifactId, workspaceName, row2Status, row2AgentId, row2Priority, row2AddedBy, row2ArtifactId);
      dt.Rows.Add(row1);
      dt.Rows.Add(row2);
      queryMock.Setup(x => x.RetrieveAllInExportManagerQueueAsync(It.IsAny<IDBContext>())).ReturnsAsync(dt);

      //Act
      await managerAgentModel.GetAllAsync(dbContextMock.Object);

      //Assert
      //There are 2 records
      Assert.AreEqual(2, managerAgentModel.Records.Count);

      //The first row is set correctly
      Assert.AreEqual(workspaceArtifactId, managerAgentModel.Records.ElementAt(0).WorkspaceArtifactId);
      Assert.AreEqual(workspaceName, managerAgentModel.Records.ElementAt(0).WorkspaceName);
      Assert.AreEqual(row1Id, managerAgentModel.Records.ElementAt(0).Id);
      Assert.AreEqual(row1AddedOn, managerAgentModel.Records.ElementAt(0).AddedOn);
      Assert.AreEqual(row1Status, managerAgentModel.Records.ElementAt(0).Status);
      Assert.AreEqual(null, managerAgentModel.Records.ElementAt(0).AgentId);
      Assert.AreEqual(row1Priority, managerAgentModel.Records.ElementAt(0).Priority);
      Assert.AreEqual(row1AddedBy, managerAgentModel.Records.ElementAt(0).AddedBy);
      Assert.AreEqual(row1ArtifactId, managerAgentModel.Records.ElementAt(0).RecordArtifactId);

      //The second row is set correctly
      Assert.AreEqual(workspaceArtifactId, managerAgentModel.Records.ElementAt(1).WorkspaceArtifactId);
      Assert.AreEqual(workspaceName, managerAgentModel.Records.ElementAt(1).WorkspaceName);
      Assert.AreEqual(row2Id, managerAgentModel.Records.ElementAt(1).Id);
      Assert.AreEqual(row2AddedOn, managerAgentModel.Records.ElementAt(1).AddedOn);
      Assert.AreEqual(row2Status, managerAgentModel.Records.ElementAt(1).Status);
      Assert.AreEqual(row2AgentId, managerAgentModel.Records.ElementAt(1).AgentId);
      Assert.AreEqual(row2Priority, managerAgentModel.Records.ElementAt(1).Priority);
      Assert.AreEqual(row2AddedBy, managerAgentModel.Records.ElementAt(1).AddedBy);
      Assert.AreEqual(row2ArtifactId, managerAgentModel.Records.ElementAt(1).RecordArtifactId);
    }

    #endregion
  }
}
