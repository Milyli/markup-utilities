using System;
using kCura.EventHandler;
using MarkupUtilities.Helpers;

namespace MarkupUtilities.EventHandlers
{
  public abstract class JobPreSave : PreSaveEventHandler
  {
    public override Response Execute()
    {
      var response = new Response()
      {
        Success = true,
        Message = string.Empty
      };

      if (!ActiveArtifact.IsNew) return response;
      //Update the Status field
      var statusFieldGuid = GetArtifactIdByGuid(GetJobStatusGuid());
      ActiveArtifact.Fields[statusFieldGuid].Value.Value = Constant.Status.Job.NEW;

      return response;
    }

    public abstract Guid GetJobStatusGuid();
  }
}
