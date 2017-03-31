using System;
using kCura.EventHandler;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using MarkupUtilities.Helpers;
using Relativity.API;
using Choice = kCura.EventHandler.Choice;
using Field = kCura.EventHandler.Field;

namespace MarkupUtilities.EventHandlers
{
  [kCura.EventHandler.CustomAttributes.Description("Sets fields on the Markup Utility Import Job")]
  [System.Runtime.InteropServices.Guid("8C3041D9-96AB-4CB3-9183-05768005FBD0")]
  public class ImportJobPreSave : PreSaveEventHandler
  {
    public override Response Execute()
    {
      var response = new Response()
      {
        Success = true,
        Message = string.Empty
      };

      var statusFieldGuid = GetArtifactIdByGuid(Constant.Guids.Field.MarkupUtilityImportJob.Status);

      if (ActiveArtifact.IsNew)
      {
        //Update the Status field
        ActiveArtifact.Fields[statusFieldGuid].Value.Value = Constant.Status.Job.NEW;

        //Update the Job Type field
        var jobTypeFieldGuid = GetArtifactIdByGuid(Constant.Guids.Field.MarkupUtilityImportJob.JobType);
        var jobTypeChoiceImportGuid = GetArtifactIdByGuid(Constant.Guids.Choices.ImportJobType.Import);
        var collection = new ChoiceCollection { new Choice(jobTypeChoiceImportGuid, "") };
        var cfv = (ChoiceFieldValue)ActiveArtifact.Fields[jobTypeFieldGuid].Value;
        cfv.Choices = collection;
      }
      else
      {
        var status = ActiveArtifact.Fields[statusFieldGuid].Value.Value;
        var statusArray = new[] { Constant.Status.Job.NEW, Constant.Status.Job.ERROR, Constant.Status.Job.VALIDATION_FAILED };
        var jobInProgress = Array.IndexOf(statusArray, status) > -1;

        if (!jobInProgress) return response;
        var currentWorkspaceArtifactId = Helper.GetActiveCaseID();
        var artifactId = ActiveArtifact.ArtifactID;

        using (var proxy = Helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.System))
        {
          proxy.APIOptions.WorkspaceID = currentWorkspaceArtifactId;
          var importJob = proxy.Repositories.RDO.ReadSingle(artifactId);
          var value = importJob[Constant.Guids.Field.MarkupUtilityImportJob.RedactionFile].ValueAsSingleObject;
          var oldRedactionFileId = value.ArtifactID;
          var redactionFileGuid = GetArtifactIdByGuid(Constant.Guids.Field.MarkupUtilityImportJob.RedactionFile);
          var newRedactionFileId = ActiveArtifact.Fields[redactionFileGuid].Value.Value;

          if (newRedactionFileId.Equals(oldRedactionFileId)) return response;
          response.Message = "Unable to change Import Redaction File for completed jobs or jobs in progress";
          response.Success = false;
        }
      }

      return response;
    }

    public override FieldCollection RequiredFields
    {
      get
      {
        var fieldCollection = new FieldCollection
        {
          new Field(Constant.Guids.Field.MarkupUtilityImportJob.Status),
          new Field(Constant.Guids.Field.MarkupUtilityImportJob.JobType)
        };

        return fieldCollection;
      }
    }
  }
}
