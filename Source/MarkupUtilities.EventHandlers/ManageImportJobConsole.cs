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
  [kCura.EventHandler.CustomAttributes.Description("Allows a user to submit a job or remove a job from the Import Manager queue.")]
  [System.Runtime.InteropServices.Guid("0BD89E26-72CF-470B-9F49-0EF162E5CED1")]
  public class ManageImportJobConsole : ConsoleEventHandler
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
      SvcManager = Helper.GetServicesManager();
      WorkspaceArtifactId = Application.ArtifactID;
      Identity = ExecutionIdentity.CurrentUser;
      CurrentArtifactId = ActiveArtifact.ArtifactID;
      CurrentUserArtifactId = Helper.GetAuthenticationManager().UserInfo.ArtifactID;
      DbContextWorkspace = Helper.GetDBContext(WorkspaceArtifactId);
      DbContextEdds = Helper.GetDBContext(-1);

      var console = new Console { Items = new List<IConsoleItem>(), Title = "Manage Redaction Import Job" };

      var validateButton = new ConsoleButton
      {
        Name = Constant.Buttons.VALIDATE,
        DisplayText = "Validate",
        ToolTip = "Click here to validate that redactions and documents exist before importing.",
        RaisesPostBack = true
      };

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
      //	Name = Constant.Buttons.REVERT,
      //	DisplayText = "Revert",
      //	ToolTip = "Click here to remove redactions created by this job.",
      //	RaisesPostBack = true
      //};

      if (pageEvent == PageEvent.PreRender)
      {
        var jobStatus = ActiveArtifact.Fields[GetArtifactIdByGuid(Constant.Guids.Field.MarkupUtilityImportJob.Status)].Value.Value.ToString();
        SetButtonState(jobStatus, validateButton, submitButton, cancelButton);
      }

      console.Items.Add(validateButton);
      console.Items.Add(submitButton);
      console.Items.Add(cancelButton);
      //TODO - Complete Revert functionality
      //console.Items.Add(revertButton);
      console.AddRefreshLinkToConsole().Enabled = true;

      return console;
    }

    //TODO - Complete functionality
    //public void SetButtonState(string jobStatus, ConsoleButton validateButton, ConsoleButton submitButton, ConsoleButton cancelButton, ConsoleButton revertButton)
    public void SetButtonState(string jobStatus, ConsoleButton validateButton, ConsoleButton submitButton, ConsoleButton cancelButton)
    {
      switch (jobStatus)
      {
        case Constant.Status.Job.NEW:
        case Constant.Status.Job.VALIDATION_FAILED:
        case Constant.Status.Job.ERROR:
          validateButton.Enabled = true;
          submitButton.Enabled = false;
          cancelButton.Enabled = false;
          //TODO - Complete Revert fucntionality
          //revertButton.Enabled = false;
          break;
        case Constant.Status.Job.VALIDATED:
          validateButton.Enabled = false;
          submitButton.Enabled = true;
          cancelButton.Enabled = false;
          //TODO - Complete Revert fucntionality
          //revertButton.Enabled = false;
          break;
        case Constant.Status.Job.SUBMITTED:
          validateButton.Enabled = false;
          submitButton.Enabled = false;
          cancelButton.Enabled = true;
          //TODO - Complete Revert fucntionality
          //revertButton.Enabled = false;
          break;
        case Constant.Status.Job.IN_PROGRESS_MANAGER:
        case Constant.Status.Job.IN_PROGRESS_WORKER:
          validateButton.Enabled = false;
          submitButton.Enabled = false;
          //TODO - Complete Revert fucntionality
          //revertButton.Enabled = false;

          //Verfiy the Job Type, if Revert, do not allow Cancellation
          var jobType = ActiveArtifact.Fields[GetArtifactIdByGuid(Constant.Guids.Field.MarkupUtilityImportJob.JobType)].Value.Value.ToString();

          if (jobType == Constant.ImportJobType.IMPORT)
            cancelButton.Enabled = true;
          else if (jobType == Constant.ImportJobType.REVERT)
          {
            cancelButton.Enabled = false;
          }
          break;
        case Constant.Status.Job.COMPLETED:
        case Constant.Status.Job.COMPLETED_WITH_ERRORS:
        case Constant.Status.Job.COMPLETED_WITH_SKIPPED_DOCUMENTS:
        case Constant.Status.Job.COMPLETED_WITH_ERRORS_AND_SKIPPED_DOCUMENTS:
          validateButton.Enabled = false;
          submitButton.Enabled = false;
          cancelButton.Enabled = false;
          //TODO - Complete Revert fucntionality
          //revertButton.Enabled = true;
          break;
        case Constant.Status.Job.VALIDATING:
        case Constant.Status.Job.CANCELLED:
        case Constant.Status.Job.CANCELREQUESTED:
          validateButton.Enabled = false;
          submitButton.Enabled = false;
          cancelButton.Enabled = false;
          //TODO - Complete Revert fucntionality
          //revertButton.Enabled = false;
          break;
        default:
          validateButton.Enabled = false;
          submitButton.Enabled = false;
          cancelButton.Enabled = false;
          //TODO - Complete Revert fucntionality
          //revertButton.Enabled = false;
          break;
      }
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

      //Flush the Import Job Details
      await UpdateJobDetailsAsync(SvcManager, WorkspaceArtifactId, Identity, Constant.Guids.ObjectType.MarkupUtilityImportJob, CurrentArtifactId, Constant.Guids.Field.MarkupUtilityImportJob.Details, string.Empty);

      switch (consoleButton.Name)
      {
        case Constant.Buttons.VALIDATE:
          recordExists = await DoesRecordExistAsync();
          if (recordExists == false && resourceGroupId > 0)
          {
            //Add the record to the Import Manager queue table
            await InsertImportJobToImportManagerQueueAsync(DbContextEdds, WorkspaceArtifactId, CurrentArtifactId, CurrentUserArtifactId, Constant.Status.Queue.NotStarted, Constant.ImportJobType.VALIDATE, resourceGroupId);

            //Set the Import Job RDO status to Validating
            await UpdateJobStatusAsync(SvcManager, WorkspaceArtifactId, Identity, Constant.Guids.ObjectType.MarkupUtilityImportJob, CurrentArtifactId, Constant.Guids.Field.MarkupUtilityImportJob.Status, Constant.Status.Job.VALIDATING);
          }
          break;
        case Constant.Buttons.SUBMIT:
          if (recordExists == false && resourceGroupId > 0)
          {
            //Add the record to the Import Manager queue table
            await InsertImportJobToImportManagerQueueAsync(DbContextEdds, WorkspaceArtifactId, CurrentArtifactId, CurrentUserArtifactId, Constant.Status.Queue.NotStarted, Constant.ImportJobType.IMPORT, resourceGroupId);

            //Set the Import Job RDO status to Validating
            await UpdateJobStatusAsync(SvcManager, WorkspaceArtifactId, Identity, Constant.Guids.ObjectType.MarkupUtilityImportJob, CurrentArtifactId, Constant.Guids.Field.MarkupUtilityImportJob.Status, Constant.Status.Job.SUBMITTED);
          }
          break;
        case Constant.Buttons.CANCEL:
          if (recordExists == false && resourceGroupId > 0)
          {
            //Set the Import Job RDO status to Cancel Requested
            await UpdateJobStatusAsync(SvcManager, WorkspaceArtifactId, Identity, Constant.Guids.ObjectType.MarkupUtilityImportJob, CurrentArtifactId, Constant.Guids.Field.MarkupUtilityImportJob.Status, Constant.Status.Job.CANCELREQUESTED);
          }
          break;
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
          //case Constant.Buttons.REVERT:
          //if (recordExists == false && resourceGroupId > 0)
          //{
          //	//Add the record to the Import Manager queue table
          //	await InsertImportJobToImportManagerQueueAsync(DbContextEdds, WorkspaceArtifactId, CurrentArtifactId, CurrentUserArtifactId, Constant.Status.Queue.NotStarted, Constant.ImportJobType.REVERT, resourceGroupId);

          //	//Set the Import Job RDO status to reverting
          //	await UpdateJobStatusAsync(SvcManager, WorkspaceArtifactId, Identity, Constant.Guids.ObjectType.MarkupUtilityImportJob, CurrentArtifactId, Constant.Guids.Field.MarkupUtilityImportJob.Status, Constant.Status.Job.REVERTING);

          //	//Set the Import Job RDO Job Type to Revert
          //	await UpdateJobJobTypeAsync(SvcManager, WorkspaceArtifactId, Identity, Constant.Guids.ObjectType.MarkupUtilityImportJob, CurrentArtifactId, Constant.Guids.Field.MarkupUtilityImportJob.JobType, Constant.Guids.Choices.ImportJobType.Revert);
          //}
          //break;
      }
    }

    public override FieldCollection RequiredFields
    {
      get
      {
        var retVal = new FieldCollection
        {
          new Field(Constant.Guids.Field.MarkupUtilityImportJob.JobType),
          new Field(Constant.Guids.Field.MarkupUtilityImportJob.Status),
          new Field(Constant.Guids.Field.MarkupUtilityImportJob.Details)
        };

        return retVal;
      }
    }

    private async Task<bool> DoesRecordExistAsync()
    {
      var dataRow = await QueryHelper.RetrieveSingleInImportManagerQueueByArtifactIdAsync(DbContextEdds, ActiveArtifact.ArtifactID, Helper.GetActiveCaseID());
      return dataRow != null;
    }


    private async Task UpdateJobStatusAsync(IServicesMgr svcManager, int workspaceArtifactId, ExecutionIdentity identity, Guid objectTypeGuid, int artifactId, Guid fieldGuid, string status)
    {
      await ArtifactQueries.UpdateRdoJobTextFieldAsync(svcManager, workspaceArtifactId, identity, objectTypeGuid, artifactId, fieldGuid, status);
    }

    private async Task UpdateJobDetailsAsync(IServicesMgr svcManager, int workspaceArtifactId, ExecutionIdentity identity, Guid objectTypeGuid, int artifactId, Guid fieldGuid, string details)
    {
      await ArtifactQueries.UpdateRdoJobTextFieldAsync(svcManager, workspaceArtifactId, identity, objectTypeGuid, artifactId, fieldGuid, details);
    }

    //TODO - Complete functionality, used in Revert
    //private async Task UpdateJobJobTypeAsync(IServicesMgr svcManager, int workspaceArtifactId, ExecutionIdentity identity, Guid objectTypeGuid, int artifactId, Guid fieldGuid, Guid choiceGuid)
    //{
    //	await ArtifactQueries.UpdateRdoJobJobTypeAsync(svcManager, workspaceArtifactId, identity, objectTypeGuid, artifactId, fieldGuid, choiceGuid);
    //}

    private async Task InsertImportJobToImportManagerQueueAsync(IDBContext dbContext, int workspaceArtifactId, int importJobArtifactId, int userArtifactId, int statusQueue, string jobTypeImport, int resourceGroupArtifactId)
    {
      await QueryHelper.InsertImportJobToImportManagerQueueAsync(dbContext, workspaceArtifactId, importJobArtifactId, userArtifactId, statusQueue, jobTypeImport, resourceGroupArtifactId);
    }
  }
}
