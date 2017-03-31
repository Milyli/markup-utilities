using System;


namespace MarkupUtilities.EventHandlers
{
  [kCura.EventHandler.CustomAttributes.Description("Pre Cascade Delete EventHandler")]
  [System.Runtime.InteropServices.Guid("b563a7a2-6fc8-4500-b76a-2aec05c37914")]
  public class FilePreCascadeDelete : kCura.EventHandler.PreCascadeDeleteEventHandler
  {
    public override kCura.EventHandler.Response Execute()
    {
      //Executed ONLY if the file object is associated with jobs, no need to query the db to check.
      var retVal = new kCura.EventHandler.Response()
      {
        Success = false,
        Message = "The file is associated with jobs.",
        Exception = new SystemException("Unable to delete: the file is associated with one or more Import or Export Jobs.")

      };

      return retVal;
    }

    public override kCura.EventHandler.FieldCollection RequiredFields => new kCura.EventHandler.FieldCollection();

    public override void Rollback() { }

    public override void Commit() { }
  }
}
