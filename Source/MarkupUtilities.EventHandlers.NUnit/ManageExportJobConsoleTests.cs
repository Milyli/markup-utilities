using System.Data;
using System.Threading.Tasks;
using kCura.EventHandler;
using MarkupUtilities.Helpers;
using MarkupUtilities.Helpers.Rsapi.Interfaces;
using Moq;
using NUnit.Framework;
using Relativity.API;
using PageEvent = kCura.EventHandler.ConsoleEventHandler.PageEvent;

namespace MarkupUtilities.EventHandlers.NUnit
{
  [TestFixture]
  public class ManageExportJobConsoleTests
  {
    public Mock<IEHHelper> MockEhHelper;
    public Mock<IQuery> MockQuery;
    public Mock<IWorkspaceQueries> MockWorkspaceQuery;
    private Mock<IArtifactQueries> _artifactQueries;

    [SetUp]
    public void SetUp()
    {
      MockEhHelper = new Mock<IEHHelper>();
      MockQuery = new Mock<IQuery>();
      MockWorkspaceQuery = new Mock<IWorkspaceQueries>();
      _artifactQueries = new Mock<IArtifactQueries>();
    }

    [Test]
    public async Task GetConsoleAsync_RecordExists_ReturnsConsoleWithRemoveNotAdd()
    {
      // Arrange 
      WhenARecordExists();
      var console = GetManageJobConsole();

      // Act 
      var actual = await console.GetConsoleAsync(PageEvent.PreRender);

      // Assert 
      Assert.IsNotNull(actual);
      Assert.IsNotNull(actual.Items);
      AssertHasButtons(actual);
    }

    [Test]
    public async Task GetConsoleAsync_NoRecordExists_ReturnsConsoleWithAddNotRemove()
    {
      // Arrange 
      WhenNoRecordExists();
      var console = GetManageJobConsole();

      // Act 
      var actual = await console.GetConsoleAsync(PageEvent.PreRender);

      // Assert 
      Assert.IsNotNull(actual);
      Assert.IsNotNull(actual.Items);
      AssertHasButtons(actual);
    }

    #region Helper Methods

    private ManageExportJobConsole GetManageJobConsole()
    {
      var console = new ManageExportJobConsole();
      console = AddActiveArtifacts(console);
      console = AddMockHelper(console);
      console = AddMockQuery(console);
      console = AddMockWorkspaceQuery(console);
      console.Application = new Application(1, "", "");
      console.ArtifactQueries = _artifactQueries.Object;

      return console;
    }

    private static ManageExportJobConsole AddActiveArtifacts(ManageExportJobConsole console)
    {
      console.ActiveArtifact = new Artifact(1234567, 2345678, 16, "Console", true, new FieldCollection() { new Field(3456789) });
      console.ActiveLayout = new Layout(1234567, "Test Console Layout");

      return console;
    }

    private ManageExportJobConsole AddMockHelper(ManageExportJobConsole console)
    {
      var authMgr = new Mock<IAuthenticationMgr>();
      authMgr.Setup(x => x.UserInfo)
        .Returns(new Mock<IUserInfo>().Object);

      MockEhHelper.Setup(x => x.GetAuthenticationManager())
        .Returns(authMgr.Object);

      console.Helper = MockEhHelper.Object;

      return console;
    }

    private ManageExportJobConsole AddMockQuery(ManageExportJobConsole console)
    {
      // Makes these two calls verifiable 
      MockQuery.Setup(x => x.InsertRowIntoExportManagerQueueAsync(It.IsAny<IDBContext>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
        .Returns(Task.FromResult(false))
        .Verifiable();

      MockQuery.Setup(
        x => x.RemoveRecordFromTableByIdAsync(It.IsAny<IDBContext>(), It.IsAny<string>(), It.IsAny<int>()))
          .Returns(Task.FromResult(false))
          .Verifiable();

      console.QueryHelper = MockQuery.Object;

      return console;
    }

    private ManageExportJobConsole AddMockWorkspaceQuery(ManageExportJobConsole console)
    {
      MockWorkspaceQuery.Setup(x => x.GetResourcePoolAsync(It.IsAny<IServicesMgr>(), It.IsAny<ExecutionIdentity>(), It.IsAny<int>()))
        .ReturnsAsync(It.IsAny<int>())
        .Verifiable();

      console.WorkspaceQueryHelper = MockWorkspaceQuery.Object;

      return console;
    }

    private static bool HasAddButton(IConsoleItem item)
    {
      if (item.GetType() != typeof(ConsoleButton)) return false;
      var button = (ConsoleButton)item;

      return (button.Name == "Submit");
    }


    private static bool HasRemoveButton(IConsoleItem item)
    {
      if (item.GetType() != typeof(ConsoleButton)) return false;
      var button = (ConsoleButton)item;

      return (button.Name == "Cancel");
    }

    private static DataRow GetDataRow()
    {
      var table = new DataTable("Test Table");
      table.Columns.Add("ID");
      table.Rows.Add(1);

      return table.Rows[0];
    }

    #endregion Helper Methods

    #region When Methods

    private void WhenARecordExists()
    {
      MockQuery.Setup(x => x.RetrieveSingleInJobManagerQueueByArtifactIdAsync(It.IsAny<IDBContext>(), It.IsAny<int>(), It.IsAny<int>(), Constant.Tables.ExportManagerQueue))
        .ReturnsAsync(GetDataRow());
    }

    private void WhenNoRecordExists()
    {
      MockQuery.Setup(x => x.RetrieveSingleInJobManagerQueueByArtifactIdAsync(It.IsAny<IDBContext>(), It.IsAny<int>(), It.IsAny<int>(), Constant.Tables.ExportManagerQueue))
        .ReturnsAsync(null);
    }

    #endregion When Methods

    #region Assert Methods

    private static void AssertHasButtons(Console console)
    {
      Assert.IsTrue(console.Items.Exists(HasAddButton));
      Assert.IsTrue(console.Items.Exists(HasRemoveButton));
    }

    #endregion Assert Methods
  }
}
