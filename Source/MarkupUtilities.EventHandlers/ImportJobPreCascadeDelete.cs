using MarkupUtilities.Helpers;

namespace MarkupUtilities.EventHandlers
{
  [kCura.EventHandler.CustomAttributes.Description("Pre Cascade Delete EventHandler")]
  [System.Runtime.InteropServices.Guid("7bec50ac-6391-42df-a46b-732b136543f1")]
  public class ImportJobPreCascadeDelete : JobPreCascadeDelete
  {
    public override string GetJobType()
    {
      return Constant.Tables.ImportJob;
    }
  }
}
