using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.EventHandler;
using MarkupUtilities.Helpers;
using MarkupUtilities.Helpers.Rsapi;
using MarkupUtilities.Helpers.Rsapi.Interfaces;
using Relativity.API;
using Console = kCura.EventHandler.Console;

namespace MarkupUtilities.EventHandlers
{
  [kCura.EventHandler.CustomAttributes.Description("Allows a user to submit a job or remove a job from the Export Manager queue.")]
  [System.Runtime.InteropServices.Guid("6F9E75E8-A8AF-471A-BD7D-E58AC0E5411C")]
  public class ManageExportJobConsole : ConsoleEventHandler
  {
    public IQuery QueryHelper = new Query();
    public IArtifactQueries ArtifactQueries = new ArtifactQueries();
    public IWorkspaceQueries WorkspaceQueryHelper = new WorkspaceQueries();
    public IServicesMgr SvcManager;
    public int WorkspaceArtifactId;
    public ExecutionIdentity Identity;
    public int CurrentArtifactId;
    public int CurrentUserArtifactId;
    public IDBContext DbContextWorkspace;
    public IDBContext DbContextEdds;

    public override Console GetConsole(PageEvent pageEvent)
    {
      var console = GetConsoleAsync(pageEvent).Result;
      return console;
    }

    public async Task<Console> GetConsoleAsync(PageEvent pageEvent)
    {
      SvcManager = Helper.GetServicesManager();
      WorkspaceArtifactId = Application.ArtifactID;
      Identity = ExecutionIdentity.CurrentUser;
      CurrentArtifactId = ActiveArtifact.ArtifactID;
      CurrentUserArtifactId = Helper.GetAuthenticationManager().UserInfo.ArtifactID;
      DbContextWorkspace = Helper.GetDBContext(WorkspaceArtifactId);
      DbContextEdds = Helper.GetDBContext(-1);

      var console = new Console { Items = new List<IConsoleItem>(), Title = "Manage Redaction Export Job" };

      var submitButton = new ConsoleButton
      {
        Name = Constant.Buttons.SUBMIT,
        DisplayText = "Submit",
        ToolTip = "Click here to add this Import job to the queue.",
        RaisesPostBack = true
      };

      var cancelButton = new ConsoleButton
      {
        Name = Constant.Buttons.CANCEL,
        DisplayText = "Cancel",
        ToolTip = "Click here to remove this Import job from the queue.",
        RaisesPostBack = true
      };

      if (pageEvent == PageEvent.PreRender)
      {
        var jobStatus = await RetrieveJobStatusAsync(SvcManager, WorkspaceArtifactId, Identity, CurrentArtifactId);

        switch (jobStatus)
        {
          case Constant.Status.Job.COMPLETED:
          case Constant.Status.Job.COMPLETED_MANAGER:
          case Constant.Status.Job.COMPLETED_WORKER:
          case Constant.Status.Job.COMPLETED_WITH_ERRORS:
          case Constant.Status.Job.ERROR:
          case Constant.Status.Job.VALIDATING:
          case Constant.Status.Job.CANCELLED:
          case Constant.Status.Job.CANCELREQUESTED:
            submitButton.Enabled = false;
            cancelButton.Enabled = false;
            break;
          case Constant.Status.Job.NEW:
          case Constant.Status.Job.VALIDATED:
            submitButton.Enabled = true;
            cancelButton.Enabled = false;
            break;
          case Constant.Status.Job.SUBMITTED:
          case Constant.Status.Job.IN_PROGRESS_MANAGER:
          case Constant.Status.Job.IN_PROGRESS_WORKER:
            submitButton.Enabled = false;
            cancelButton.Enabled = true;
            break;
          default:
            submitButton.Enabled = false;
            cancelButton.Enabled = false;
            break;
        }
      }

      console.Items.Add(submitButton);
      console.Items.Add(cancelButton);
      console.AddRefreshLinkToConsole().Enabled = true;

      return console;
    }

    public override void OnButtonClick(ConsoleButton consoleButton)
    {
      OnButtonClickAsync(consoleButton).Wait();
    }

    public async Task OnButtonClickAsync(ConsoleButton consoleButton)
    {
      var recordExists = await DoesRecordExistAsync();
      var resourceGroupId = 0;

      //If no db record exists, get the Resource Pool fpor the agent and add it to the Import Job record
      if (recordExists == false)
      {
        resourceGroupId = await WorkspaceQueryHelper.GetResourcePoolAsync(SvcManager, ExecutionIdentity.System, Helper.GetActiveCaseID());
      }

      switch (consoleButton.Name)
      {
        case Constant.Buttons.SUBMIT:
          if (recordExists == false && resourceGroupId > 0)
          {
            //Add the record to the Export Manager queue table
            await InsertExportJobToExportManagerQueueAsync(DbContextEdds, WorkspaceArtifactId, CurrentArtifactId, CurrentUserArtifactId, Constant.Status.Queue.NotStarted, resourceGroupId);

            //Set the Export Job RDO status to Validating
            await UpdateJobStatusAsync(SvcManager, WorkspaceArtifactId, Identity, Constant.Guids.ObjectType.MarkupUtilityExportJob, CurrentArtifactId, Constant.Guids.Field.MarkupUtilityExportJob.Status, Constant.Status.Job.SUBMITTED);
          }
          break;
        case Constant.Buttons.CANCEL:
          if (recordExists == false && resourceGroupId > 0)
          {
            //Set the Export Job RDO status to Cancel Requested
            await UpdateJobStatusAsync(SvcManager, WorkspaceArtifactId, Identity, Constant.Guids.ObjectType.MarkupUtilityExportJob, CurrentArtifactId, Constant.Guids.Field.MarkupUtilityExportJob.Status, Constant.Status.Job.CANCELREQUESTED);
          }
          break;
      }
    }

    public override FieldCollection RequiredFields
    {
      get
      {
        var retVal = new FieldCollection();
        return retVal;
      }
    }

    private async Task<bool> DoesRecordExistAsync()
    {
      var dataRow = await QueryHelper.RetrieveSingleInJobManagerQueueByArtifactIdAsync(Helper.GetDBContext(-1), ActiveArtifact.ArtifactID, Helper.GetActiveCaseID(), Constant.Tables.ExportManagerQueue);
      return dataRow != null;
    }

    private async Task<string> RetrieveJobStatusAsync(IServicesMgr svcManager, int workspaceArtifactId, ExecutionIdentity identity, int artifactId)
    {
      var status = await ArtifactQueries.RetrieveRdoJobStatusAsync(svcManager, workspaceArtifactId, identity, artifactId);
      return status;
    }

    private async Task UpdateJobStatusAsync(IServicesMgr svcManager, int workspaceArtifactId, ExecutionIdentity identity, Guid objectTypeGuid, int artifactId, Guid fieldGuid, string status)
    {
      await ArtifactQueries.UpdateRdoJobTextFieldAsync(svcManager, workspaceArtifactId, identity, objectTypeGuid, artifactId, fieldGuid, status);
    }

    private async Task InsertExportJobToExportManagerQueueAsync(IDBContext dbContext, int workspaceArtifactId, int exportJobArtifactId, int userArtifactId, int statusQueue, int resourceGroupArtifactId)
    {
      await QueryHelper.InsertJobToManagerQueueAsync(dbContext, workspaceArtifactId, exportJobArtifactId, userArtifactId, statusQueue, resourceGroupArtifactId, Constant.Tables.ExportManagerQueue, "ExportJobArtifactID");
    }
  }
}
