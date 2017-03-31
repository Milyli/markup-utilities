using System.Data;
using System.Linq;
using System.Threading.Tasks;
using kCura.EventHandler;
using MarkupUtilities.Helpers;
using MarkupUtilities.Helpers.Rsapi.Interfaces;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Artifact = kCura.EventHandler.Artifact;

namespace MarkupUtilities.EventHandlers.NUnit
{
  [TestFixture]
  public class ManageReproduceJobConsoleTests
  {
    public Mock<IQuery> MockQuery;
    private ManageReproduceJobConsole _console;

    [SetUp]
    public void SetUp()
    {
      _console = new ManageReproduceJobConsole
      {
        ActiveArtifact = new Artifact(1, 1, 1, "", true, new FieldCollection()),
        Application = new Application(1, "", "")
      };

      var mockEhHelper = new Mock<IEHHelper>();
      var authMgr = new Mock<IAuthenticationMgr>();
      authMgr.Setup(x => x.UserInfo).Returns(new Mock<IUserInfo>().Object);
      mockEhHelper.Setup(x => x.GetAuthenticationManager()).Returns(authMgr.Object);
      mockEhHelper.Setup(x => x.GetActiveCaseID()).Returns(1);
      mockEhHelper.Setup(x => x.GetDBContext(It.IsAny<int>())).Returns(new Mock<IDBContext>().Object);
      _console.Helper = mockEhHelper.Object;

      var mockWorkspaceQuery = new Mock<IWorkspaceQueries>();
      mockWorkspaceQuery.Setup(x => x.GetResourcePoolAsync(It.IsAny<IServicesMgr>(), It.IsAny<ExecutionIdentity>(), It.IsAny<int>()))
        .ReturnsAsync(1)
        .Verifiable();
      _console.WorkspaceQueryHelper = mockWorkspaceQuery.Object;

      MockQuery = new Mock<IQuery>();
      MockQuery.Setup(x => x.RetrieveSingleInJobManagerQueueByArtifactIdAsync(It.IsAny<IDBContext>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
        .Returns(Task.Factory.StartNew(() => (DataRow)null)).Verifiable();
      MockQuery.Setup(x => x.InsertJobToManagerQueueAsync(It.IsAny<IDBContext>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), Constant.Tables.ReproduceManagerQueue, "ReproduceJobArtifactID"))
        .Returns(Task.Factory.StartNew(() => (DataRow)null)).Verifiable();
      _console.ArtifactQueries = new Mock<IArtifactQueries>().Object;
      _console.QueryHelper = MockQuery.Object;
    }

    [Test]
    public void GetConsoleTestTitleAndButtons()
    {
      var console = _console.GetConsole(ConsoleEventHandler.PageEvent.Load);

      Assert.AreEqual("Manage Redaction Reproduce Job", console.Title);
      Assert.IsNotEmpty(console.Items);
      Assert.That(console.Items.Any(i => i.GetType() == typeof(ConsoleButton) && ((ConsoleButton)i).DisplayText == "Submit"));
      Assert.That(console.Items.Any(i => i.GetType() == typeof(ConsoleButton) && ((ConsoleButton)i).DisplayText == "Cancel"));
      //TODO - Complete Revert functionality
      //Assert.That(console.Items.Any(i => i.GetType() == typeof(ConsoleButton) && ((ConsoleButton)i).DisplayText == "Revert"));
    }

    [Test]
    public void SetButtonState()
    {
      var console = _console.GetConsole(ConsoleEventHandler.PageEvent.Load);
      var submit = (ConsoleButton)console.Items.Find(b => ((ConsoleButton)b).Name == "Submit");
      //TODO - Complete Revert functionality
      //var revert = (ConsoleButton)console.Items.Find(b => ((ConsoleButton)b).Name == "Revert");
      var cancel = (ConsoleButton)console.Items.Find(b => ((ConsoleButton)b).Name == "Cancel");

      //TODO - Complete Revert functionality
      //_console.SetButtonState(Constant.Status.Job.NEW, submit,cancel, revert);
      _console.SetButtonState(Constant.Status.Job.NEW, submit, cancel);
      Assert.IsTrue(submit.Enabled);
      Assert.IsFalse(cancel.Enabled);

      //TODO - Complete Revert functionality
      //_console.SetButtonState(Constant.Status.Job.CANCELLED, submit, cancel, revert);
      _console.SetButtonState(Constant.Status.Job.CANCELLED, submit, cancel);
      Assert.IsFalse(submit.Enabled);
      Assert.IsFalse(cancel.Enabled);

      //TODO - Complete Revert functionality
      //_console.SetButtonState(Constant.Status.Job.SUBMITTED, submit, cancel, revert);
      _console.SetButtonState(Constant.Status.Job.SUBMITTED, submit, cancel);
      Assert.IsFalse(submit.Enabled);
      Assert.IsTrue(cancel.Enabled);

      //TODO - Complete Revert functionality
      //_console.SetButtonState(Constant.Status.Job.IN_PROGRESS_WORKER, submit, cancel, revert);
      _console.SetButtonState(Constant.Status.Job.IN_PROGRESS_WORKER, submit, cancel);
      Assert.IsFalse(submit.Enabled);
      Assert.IsFalse(cancel.Enabled);

      //TODO - Complete Revert functionality
      //_console.SetButtonState(Constant.Status.Job.IN_PROGRESS_MANAGER, submit, cancel, revert);
      _console.SetButtonState(Constant.Status.Job.IN_PROGRESS_MANAGER, submit, cancel);
      Assert.IsFalse(submit.Enabled);
      Assert.IsFalse(cancel.Enabled);

      //TODO - Complete Revert functionality
      //_console.SetButtonState(Constant.Status.Job.COMPLETED_MANAGER, submit, cancel, revert);
      _console.SetButtonState(Constant.Status.Job.COMPLETED_MANAGER, submit, cancel);
      Assert.IsFalse(submit.Enabled);
      Assert.IsFalse(cancel.Enabled);
    }

    [Test]
    public void OnButtonClickTestSubmit()
    {
      var consoleButton = new ConsoleButton { Name = "Submit" };
      _console.OnButtonClick(consoleButton);
      MockQuery.Verify(x => x.RetrieveSingleInJobManagerQueueByArtifactIdAsync(It.IsAny<IDBContext>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
      MockQuery.Verify(x => x.InsertJobToManagerQueueAsync(It.IsAny<IDBContext>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), Constant.Tables.ReproduceManagerQueue, "ReproduceJobArtifactID"), Times.Once);
    }

    [Test]
    public void OnButtonClickTestCancel()
    {
      var consoleButton = new ConsoleButton { Name = "Cancel" };
      _console.OnButtonClick(consoleButton);

      MockQuery.Verify(x => x.RetrieveSingleInJobManagerQueueByArtifactIdAsync(It.IsAny<IDBContext>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
      MockQuery.Verify(x => x.InsertJobToManagerQueueAsync(It.IsAny<IDBContext>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), Constant.Tables.ReproduceManagerQueue, "ReproduceJobArtifactID"), Times.Never);
    }

    //TODO - Complete Revert functionality
    //[Test]
    //public void OnButtonClickTestRevert()
    //{
    //    var consoleButton = new ConsoleButton { Name = "Revert" };
    //    _console.OnButtonClick(consoleButton);

    //    MockQuery.Verify(x => x.RetrieveSingleInJobManagerQueueByArtifactIdAsync(It.IsAny<IDBContext>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
    //    MockQuery.Verify(x => x.InsertJobToManagerQueueAsync(It.IsAny<IDBContext>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), Constant.Tables.ReproduceManagerQueue, "ReproduceJobArtifactID"), Times.Never);
    //}
  }
}