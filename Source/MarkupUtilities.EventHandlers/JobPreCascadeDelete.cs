using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MarkupUtilities.Helpers;
using Relativity.API;

namespace MarkupUtilities.EventHandlers
{
  public abstract class JobPreCascadeDelete : kCura.EventHandler.PreCascadeDeleteEventHandler
  {
    private IDBContext _workspaceContext;

    public abstract string GetJobType();

    public override kCura.EventHandler.Response Execute()
    {
      //Construct a response object with default values.
      var retVal = new kCura.EventHandler.Response
      {
        Success = true,
        Message = string.Empty
      };

      try
      {
        var currentWorkspaceArtifactId = Helper.GetActiveCaseID();

        //ActiveArtifact is null so this is how we get the IDs of the objects to delete
        var tempTableNameWithParentArtifactsToDelete = TempTableNameWithParentArtifactsToDelete;
        _workspaceContext = Helper.GetDBContext(currentWorkspaceArtifactId);
        var eddsDbContext = Helper.GetDBContext(-1);
        var isOkToDelete = IsOkToDelete(eddsDbContext, tempTableNameWithParentArtifactsToDelete, _workspaceContext.Database);

        if (!isOkToDelete) { throw new SystemException("Unable to delete job(s) in progress."); }
      }
      catch (Exception ex)
      {
        //Change the response Success property to false to let the user know an error occurred
        retVal.Success = false;
        retVal.Message = ex.ToString();
        retVal.Exception = ex;
        retVal.Exception = ex;
      }

      return retVal;
    }

    public override kCura.EventHandler.FieldCollection RequiredFields => new kCura.EventHandler.FieldCollection();

    public override void Rollback() { }

    public override void Commit() { }

    public bool IsOkToDelete(IDBContext eddsDbContext, string tempTable, string database)
    {
      string sql = $@"
				SELECT j.Status
				FROM [EDDSResource].[EDDSDBO].[{tempTable}] AS t WITH(NOLOCK),[{database}].[EDDSDBO].[{GetJobType()}] AS j WITH(NOLOCK) 
                WHERE t.ArtifactID = j.ArtifactID ";

      var exportJobs = eddsDbContext.ExecuteSqlStatementAsDataTable(sql);

      var statusOkToDelete = new List<string>
      {
        Constant.Status.Job.NEW,
        Constant.Status.Job.CANCELLED,
        Constant.Status.Job.COMPLETED,
        Constant.Status.Job.COMPLETED_WITH_ERRORS,
        Constant.Status.Job.COMPLETED_WITH_ERRORS_AND_SKIPPED_DOCUMENTS,
        Constant.Status.Job.COMPLETED_WITH_SKIPPED_DOCUMENTS,
        Constant.Status.Job.ERROR,
        Constant.Status.Job.REVERTED,
        Constant.Status.Job.VALIDATED,
      };

      return exportJobs?.Rows == null || (from DataRow dataRow in exportJobs.Rows select (string)dataRow[0]).All(status => statusOkToDelete.Contains(status));
    }
  }
}
