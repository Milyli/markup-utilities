using MarkupUtilities.Helpers;

namespace MarkupUtilities.EventHandlers
{
  [kCura.EventHandler.CustomAttributes.Description("Pre Cascade Delete EventHandler")]
  [System.Runtime.InteropServices.Guid("f3f6ca8d-042b-4c0f-8d19-59dc605bc909")]
  public class ExportJobPreCascadeDelete : JobPreCascadeDelete
  {
    public override string GetJobType()
    {
      return Constant.Tables.ExportJob;
    }
  }
}
