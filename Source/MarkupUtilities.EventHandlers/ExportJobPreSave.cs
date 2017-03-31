using System;
using kCura.EventHandler;
using MarkupUtilities.Helpers;

namespace MarkupUtilities.EventHandlers
{
  [kCura.EventHandler.CustomAttributes.Description("Sets fields on the Markup Utility Export Job")]
  [System.Runtime.InteropServices.Guid("B56D1ADD-0707-4D7B-AA50-A1CEAF62D8D3")]
  public class ExportJobPreSave : JobPreSave
  {
    public override Guid GetJobStatusGuid()
    {
      return Constant.Guids.Field.MarkupUtilityExportJob.Status;
    }

    public override FieldCollection RequiredFields
    {
      get
      {
        var fieldCollection = new FieldCollection { new Field(Constant.Guids.Field.MarkupUtilityExportJob.Status) };
        return fieldCollection;
      }
    }
  }
}
