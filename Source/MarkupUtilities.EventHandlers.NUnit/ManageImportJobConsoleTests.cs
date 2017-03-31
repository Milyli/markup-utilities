using System;
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
  public class ManageImportJobConsoleTests
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
    public void GetConsole_ReturnsConsoleWithButtons()
    {
      // Arrange 
      WhenARecordExists();
      var console = GetManageJobConsole();

      // Act 
      var actual = console.GetConsole(PageEvent.Load);

      // Assert 
      Assert.IsNotNull(actual);
      Assert.IsNotNull(actual.Items);
      AssertHasButtons(actual);
    }

    [Test]
    public void TestSetButtonState_NewJob()
    {
      // Arrange 
      WhenARecordExists();
      var console = GetManageJobConsole();

      // Act 
      var actual = console.GetConsole(PageEvent.Load);
      var validate = (ConsoleButton)actual.Items.Find(b => ((ConsoleButton)b).Name == "Validate");
      var submit = (ConsoleButton)actual.Items.Find(b => ((ConsoleButton)b).Name == "Submit");
      //TODO - Complete Revert functionality
      //var revert = (ConsoleButton)actual.Items.Find(b => ((ConsoleButton)b).Name == "Revert");
      var cancel = (ConsoleButton)actual.Items.Find(b => ((ConsoleButton)b).Name == "Cancel");

      //TODO - Complete Revert functionality
      //console.SetButtonState(Constant.Status.Job.NEW, validate, submit, cancel, revert);
      console.SetButtonState(Constant.Status.Job.NEW, validate, submit, cancel);

      // Assert 
      Assert.IsTrue(validate.Enabled);
      Assert.IsFalse(cancel.Enabled);
      Assert.IsFalse(submit.Enabled);
      //TODO - Complete Revert functionality
      //Assert.IsFalse(revert.Enabled);

      //TODO - Complete Revert functionality
      //console.SetButtonState(Constant.Status.Job.VALIDATED, validate, submit, cancel, revert);
      console.SetButtonState(Constant.Status.Job.VALIDATED, validate, submit, cancel);

      // Assert 
      Assert.IsFalse(validate.Enabled);
      Assert.IsFalse(cancel.Enabled);
      Assert.IsTrue(submit.Enabled);
      //TODO - Complete Revert functionality
      //Assert.IsFalse(revert.Enabled);

      //TODO - Complete Revert functionality
      //console.SetButtonState(Constant.Status.Job.SUBMITTED, validate, submit, cancel, revert);
      console.SetButtonState(Constant.Status.Job.SUBMITTED, validate, submit, cancel);

      // Assert 
      Assert.IsFalse(validate.Enabled);
      Assert.IsTrue(cancel.Enabled);
      Assert.IsFalse(submit.Enabled);
      //TODO - Complete Revert functionality
      //Assert.IsFalse(revert.Enabled);

      //TODO - Complete Revert functionality
      //console.SetButtonState(Constant.Status.Job.CANCELLED, validate, submit, cancel, revert);
      console.SetButtonState(Constant.Status.Job.CANCELLED, validate, submit, cancel);

      // Assert 
      Assert.IsFalse(validate.Enabled);
      Assert.IsFalse(cancel.Enabled);
      Assert.IsFalse(submit.Enabled);
      //TODO - Complete Revert functionality
      //Assert.IsFalse(revert.Enabled);
    }

    #region Helper Methods

    private ManageImportJobConsole GetManageJobConsole()
    {
      var console = new ManageImportJobConsole();
      console = AddActiveArtifacts(console);
      console = AddMockHelper(console);
      console = AddMockQuery(console);
      console = AddMockWorkspaceQuery(console);
      console.Application = new Application(1, "", "");
      console.ArtifactQueries = _artifactQueries.Object;

      return console;
    }

    private static ManageImportJobConsole AddActiveArtifacts(ManageImportJobConsole console)
    {
      console.ActiveArtifact = new Artifact(1234567, 2345678, 16, "Console", true, new FieldCollection() { new Field(3456789) });
      console.ActiveLayout = new Layout(1234567, "Test Console Layout");
      return console;
    }

    private ManageImportJobConsole AddMockHelper(ManageImportJobConsole console)
    {
      var authMgr = new Mock<IAuthenticationMgr>();

      authMgr.Setup(x => x.UserInfo)
        .Returns(new Mock<IUserInfo>().Object);

      MockEhHelper.Setup(x => x.GetAuthenticationManager())
        .Returns(authMgr.Object);

      console.Helper = MockEhHelper.Object;
      return console;
    }

    private ManageImportJobConsole AddMockQuery(ManageImportJobConsole console)
    {
      // Makes these two calls verifiable 
      MockQuery.Setup(x => x.InsertRowIntoImportManagerQueueAsync(It.IsAny<IDBContext>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
        .Returns(Task.FromResult(false))
        .Verifiable();

      MockQuery.Setup(x => x.RemoveRecordFromTableByIdAsync(It.IsAny<IDBContext>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(Task.FromResult(false))
        .Verifiable();

      console.QueryHelper = MockQuery.Object;

      return console;
    }

    private ManageImportJobConsole AddMockWorkspaceQuery(ManageImportJobConsole console)
    {
      MockWorkspaceQuery.Setup(x => x.GetResourcePoolAsync(It.IsAny<IServicesMgr>(), It.IsAny<ExecutionIdentity>(), It.IsAny<int>()))
        .ReturnsAsync(It.IsAny<int>())
        .Verifiable();

      console.WorkspaceQueryHelper = MockWorkspaceQuery.Object;

      return console;
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
      MockQuery.Setup(x => x.RetrieveSingleInImportManagerQueueByArtifactIdAsync(It.IsAny<IDBContext>(), It.IsAny<int>(), It.IsAny<int>()))
        .ReturnsAsync(GetDataRow());
    }

    #endregion When Methods

    #region Assert Methods

    private static void AssertHasButtons(kCura.EventHandler.Console console)
    {
      if (console == null) throw new ArgumentNullException(nameof(console));
      Assert.IsTrue(console.Items.Exists(b => ((ConsoleButton)b).Name == "Submit"));
      Assert.IsTrue(console.Items.Exists(b => ((ConsoleButton)b).Name == "Validate"));
      Assert.IsTrue(console.Items.Exists(b => ((ConsoleButton)b).Name == "Cancel"));
      //TODO - Complete Revert functionality
      //Assert.IsTrue(console.Items.Exists(b =>( (ConsoleButton)b).Name == "Revert" ));
    }

    #endregion Assert Methods
  }
}
