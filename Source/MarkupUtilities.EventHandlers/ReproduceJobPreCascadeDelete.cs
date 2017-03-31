using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using kCura.EventHandler;
using MarkupUtilities.Helpers;

namespace MarkupUtilities.EventHandlers
{
  [kCura.EventHandler.CustomAttributes.Description("Pre Cascade Delete EventHandler")]
  [System.Runtime.InteropServices.Guid("4d51019c-5ca1-4718-a96a-df94930c0e2f")]
  public class ReproduceJobPreCascadeDelete : JobPreCascadeDelete
  {
    public IQuery QueryHelper = new Query();

    public override Response Execute()
    {
      var executeAsync = ExecuteAsync();
      return executeAsync.Result;
    }

    public async Task<Response> ExecuteAsync()
    {
      var retVal = base.Execute();

      if (!retVal.Success) return retVal;

      try
      {
        var next = await RetrieveArtifactIDs();
        foreach (DataRow row in next.Rows)
        {
          await CleanupHoldingTables((int)row["ArtifactID"]);
        }

      }
      catch (Exception ex)
      {
        //Change the response Success property to false to let the user know an error occurred
        retVal.Success = false;
        retVal.Message = ex.ToString();
        retVal.Exception = ex;
      }

      return retVal;
    }


    public override string GetJobType()
    {
      return Constant.Tables.ReproduceJob;
    }

    public async Task<DataTable> RetrieveArtifactIDs()
    {
      var dataTable = await QueryHelper.RetrieveArtifactIDsAsync(Helper.GetDBContext(-1), TempTableNameWithParentArtifactsToDelete);
      return dataTable;
    }

    public async Task CleanupHoldingTables(int artifactId)
    {
      var resulTable = await QueryHelper.RetrieveReproduceWorkerQueueAsync(Helper.GetDBContext(-1), artifactId);
      var rows = resulTable.Rows;
      var tasks = new List<Task>();

      foreach (DataRow row in rows)
      {
        var dropSearchTable = QueryHelper.DropTableAsync(Helper.GetDBContext((int)row["WorkspaceArtifactID"]), (string)row["SavedSearchHoldingTable"]);
        var dropRedactionTable = QueryHelper.DropTableAsync(Helper.GetDBContext((int)row["WorkspaceArtifactID"]), (string)row["RedactionsHoldingTable"]);

        tasks.Add(dropSearchTable);
        tasks.Add(dropRedactionTable);
      }

      await Task.WhenAll(tasks);
    }
  }
}