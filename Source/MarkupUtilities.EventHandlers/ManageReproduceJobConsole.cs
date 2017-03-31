using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using kCura.EventHandler;
using MarkupUtilities.Helpers;
using MarkupUtilities.Helpers.Rsapi;
using MarkupUtilities.Helpers.Rsapi.Interfaces;
using Relativity.API;
using Console = kCura.EventHandler.Console;

namespace MarkupUtilities.EventHandlers
{
  [kCura.EventHandler.CustomAttributes.Description("Console EventHandler")]
  [System.Runtime.InteropServices.Guid("d887cb95-f11c-4103-a88d-f268bece5ab1")]
  public class ManageReproduceJobConsole : ConsoleEventHandler
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
    private static readonly HashSet<string> SubmitEnabled = new HashSet<string>() { Constant.Status.Job.NEW, Constant.Status.Job.ERROR };
    private static readonly HashSet<string> CancelEnabled = new HashSet<string>() { Constant.Status.Job.SUBMITTED };

    public override Console GetConsole(PageEvent pageEvent)
    {
      SvcManager = Helper.GetServicesManager();
      WorkspaceArtifactId = Application.ArtifactID;
      Identity = ExecutionIdentity.CurrentUser;
      CurrentArtifactId = ActiveArtifact.ArtifactID;
      CurrentUserArtifactId = Helper.GetAuthenticationManager().UserInfo.ArtifactID;
      DbContextWorkspace = Helper.GetDBContext(WorkspaceArtifactId);
      DbContextEdds = Helper.GetDBContext(-1);

      var console = new Console { Items = new List<IConsoleItem>(), Title = "Manage Redaction Reproduce Job" };

      var submitButton = new ConsoleButton
      {
        Name = Constant.Buttons.SUBMIT,
        DisplayText = "Submit",
        ToolTip = "Click here to add this Reproduce job to the queue.",
        RaisesPostBack = true
      };

      var cancelButton = new ConsoleButton
      {
        Name = Constant.Buttons.CANCEL,
        DisplayText = "Cancel",
        ToolTip = "Click here to remove this Reproduce job from the queue.",
        RaisesPostBack = true
      };

      // *******************************************************************************************************
      //
      //       *********
      //       *       *
      //  *********    *
      //  *    *  *    *
      //  *    *********
      //  *       *
      //  *********
      //
      //  This functionality currently needs to be completed.
      //
      //  If you are interested in building this functionality, please make sure to write associated unit tests.
      //  The basic concept is that the Import Worker Agent will retrieve all the redactrions that were imported
      //  for the selected job and delete them from the Redaction table.
      // *******************************************************************************************************
      //var revertButton = new ConsoleButton
      //{
      //    Name = Constant.Buttons.REVERT,
      //    DisplayText = "Revert",
      //    ToolTip = "Click here to remove redactions created by this job.",
      //    RaisesPostBack = true
      //};

      if (pageEvent == PageEvent.PreRender)
      {
        var artifactIdByGuid = GetArtifactIdByGuid(Constant.Guids.Field.MarkupUtilityReproduceJob.Status);
        var jobStatus = ActiveArtifact.Fields[artifactIdByGuid].Value.Value.ToString();

        //TODO - Complete Revert functionality
        //SetButtonState(jobStatus, submitButton, cancelButton, revertButton);
        SetButtonState(jobStatus, submitButton, cancelButton);
      }

      console.Items.Add(submitButton);
      console.Items.Add(cancelButton);
      //TODO - Complete Revert functionality
      //console.Items.Add(revertButton);
      console.AddRefreshLinkToConsole().Enabled = true;

      return console;
    }

    //TODO - Complete Revert functionality
    //public void SetButtonState(string jobStatus, ConsoleButton submitButton, ConsoleButton cancelButton, ConsoleButton revertButton)
    public void SetButtonState(string jobStatus, ConsoleButton submitButton, ConsoleButton cancelButton)
    {
      submitButton.Enabled = false;
      //TODO - Complete Revert functionality
      //revertButton.Enabled = false;
      cancelButton.Enabled = false;

      if (SubmitEnabled.Contains(jobStatus))
      {
        submitButton.Enabled = true;
      }

      if (CancelEnabled.Contains(jobStatus))
      {
        cancelButton.Enabled = true;
      }

      //TODO - Complete Revert functionality
      //if (RevertEnabled.Contains(jobStatus))
      //{
      //    revertButton.Enabled = true;
      //}
    }

    public override void OnButtonClick(ConsoleButton consoleButton)
    {
      OnButtonClickAsync(consoleButton).Wait();
    }

    public async Task OnButtonClickAsync(ConsoleButton consoleButton)
    {
      var record = await RetrieveRecordAsync();
      var resourceGroupId = 0;

      //If no db record exists, get the Resource Pool for the agent and add it to the Job record
      if (record == null)
      {
        resourceGroupId = await WorkspaceQueryHelper.GetResourcePoolAsync(SvcManager, ExecutionIdentity.System, Helper.GetActiveCaseID());
      }

      if (consoleButton.Name == Constant.Buttons.SUBMIT)
      {
        if (record == null && resourceGroupId > 0)
        {
          await InsertExportJobToManagerQueueAsync(DbContextEdds, WorkspaceArtifactId, CurrentArtifactId, CurrentUserArtifactId, Constant.Status.Queue.NotStarted, resourceGroupId);
          await UpdateJobStatusAsync(SvcManager, WorkspaceArtifactId, Identity, Constant.Guids.ObjectType.MarkupUtilityReproduceJob, CurrentArtifactId, Constant.Guids.Field.MarkupUtilityReproduceJob.Status, Constant.Status.Job.SUBMITTED);
        }
      }
      else if (consoleButton.Name == Constant.Buttons.CANCEL)
      {
        if (record == null && resourceGroupId > 0)
        {
          await UpdateJobStatusAsync(SvcManager, WorkspaceArtifactId, Identity, Constant.Guids.ObjectType.MarkupUtilityReproduceJob, CurrentArtifactId, Constant.Guids.Field.MarkupUtilityReproduceJob.Status, Constant.Status.Job.CANCELREQUESTED);
        }
      }
      // *******************************************************************************************************
      //
      //       *********
      //       *       *
      //  *********    *
      //  *    *  *    *
      //  *    *********
      //  *       *
      //  *********
      //
      //  This functionality currently needs to be completed.
      //
      //  If you are interested in building this functionality, please make sure to write associated unit tests.
      //  The basic concept is that the Import Worker Agent will retrieve all the redactrions that were imported
      //  for the selected job and delete them from the Redaction table.
      // *******************************************************************************************************
      //else if (consoleButton.Name == Constant.Buttons.REVERT)
      //{
      //    
      //}
    }

    public override FieldCollection RequiredFields
    {
      get
      {
        var retVal = new FieldCollection();
        return retVal;
      }
    }


    private async Task<DataRow> RetrieveRecordAsync()
    {
      var dataRow = await QueryHelper.RetrieveSingleInJobManagerQueueByArtifactIdAsync(Helper.GetDBContext(-1), ActiveArtifact.ArtifactID, Helper.GetActiveCaseID(), Constant.Tables.ReproduceManagerQueue);
      return dataRow;
    }

    private async Task InsertExportJobToManagerQueueAsync(IDBContext dbContext, int workspaceArtifactId, int jobArtifactId, int userArtifactId, int statusQueue, int resourceGroupArtifactId)
    {
      await QueryHelper.InsertJobToManagerQueueAsync(dbContext, workspaceArtifactId, jobArtifactId, userArtifactId, statusQueue, resourceGroupArtifactId, Constant.Tables.ReproduceManagerQueue, "ReproduceJobArtifactID");
    }

    private async Task UpdateJobStatusAsync(IServicesMgr svcManager, int workspaceArtifactId, ExecutionIdentity identity, Guid objectTypeGuid, int artifactId, Guid fieldGuid, string status)
    {
      await ArtifactQueries.UpdateRdoJobTextFieldAsync(svcManager, workspaceArtifactId, identity, objectTypeGuid, artifactId, fieldGuid, status);
    }
  }
}
