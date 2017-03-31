using System;
using System.Collections.Generic;
using kCura.EventHandler;
using MarkupUtilities.Helpers;
using Field = kCura.EventHandler.Field;

namespace MarkupUtilities.EventHandlers
{
  [kCura.EventHandler.CustomAttributes.Description("Pre Save EventHandler")]
  [System.Runtime.InteropServices.Guid("78ea7df2-b3da-44ac-9d9e-cd80d2d9f3ea")]
  public class ReproduceJobPreSave : JobPreSave
  {
    public override Response Execute()
    {
      var response = base.Execute();
      if (!response.Success) return response;
      if (!ActiveArtifact.IsNew)
      {
        var status = ActiveArtifact.Fields[GetArtifactIdByGuid(Constant.Guids.Field.MarkupUtilityReproduceJob.Status)].Value.Value;
        var okToEditStatuses = new HashSet<string> { Constant.Status.Job.NEW, Constant.Status.Job.ERROR };
        var jobInProgress = !okToEditStatuses.Contains((string)status);

        if (jobInProgress)
        {
          response.Message = "Unable to edit fields of completed jobs or jobs in progress";
          response.Success = false;
          return response;
        }
      }

      //validate fields according to job type
      var jobTypeArtifactId = GetArtifactIdByGuid(Constant.Guids.Field.MarkupUtilityReproduceJob.ReproduceJobType);
      var acrossDocumentSetArtifactId = GetArtifactIdByGuid(Constant.Guids.Choices.ReproduceJobType.AcrossDocumentSet);
      var acrossRelatinalGroupartifactId = GetArtifactIdByGuid(Constant.Guids.Choices.ReproduceJobType.AcrossRelationalGroup);
      var rationalFieldArtifactId = GetArtifactIdByGuid(Constant.Guids.Field.MarkupUtilityReproduceJob.RelationalField);
      var value = (ChoiceCollection)ActiveArtifact.Fields[jobTypeArtifactId].Value.Value;

      if (value[acrossDocumentSetArtifactId] != null && value[acrossDocumentSetArtifactId].IsSelected)
      {
        ActiveArtifact.Fields[rationalFieldArtifactId].Value.Value = null;
      }

      if (value[acrossRelatinalGroupartifactId] == null || !value[acrossRelatinalGroupartifactId].IsSelected) return response;

      // check that relational group field is set
      if (ActiveArtifact.Fields[rationalFieldArtifactId].Value.Value != null) return response;

      response.Success = false;
      response.Message = "Please select Relational Field for Job Type: Reproduce Across Relational Group.";
      return response;
    }

    public override Guid GetJobStatusGuid()
    {
      return Constant.Guids.Field.MarkupUtilityReproduceJob.Status;
    }

    public override FieldCollection RequiredFields
    {
      get
      {
        var fieldCollection = new FieldCollection
        {
           new Field(Constant.Guids.Field.MarkupUtilityReproduceJob.Status)
        };

        return fieldCollection;
      }
    }
  }
}