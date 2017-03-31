using System;
using System.Threading.Tasks;
using MarkupUtilities.Helpers;
using MarkupUtilities.Helpers.Rsapi;
using MarkupUtilities.Helpers.Rsapi.Interfaces;
using Relativity.API;

namespace MarkupUtilities.Agents
{
  [kCura.Agent.CustomAttributes.Name("Markup Utilities - Reproduce Worker")]
  [System.Runtime.InteropServices.Guid("296e8a64-f6de-4442-b496-85fb1d4417b5")]
  public class ReproduceWorker : kCura.Agent.AgentBase
  {
    public override void Execute()
    {
      ExecuteAsync().Wait();
    }

    public async Task ExecuteAsync()
    {
      var queryHelper = new Query();
      var resourceGroupIds = GetResourceGroupIDs();
      IArtifactQueries artifactQueries = new ArtifactQueries();
      IErrorQueries errorQueries = new ErrorQueries();

      var job = new ReproduceWorkerJob(AgentID, Helper, queryHelper, artifactQueries, resourceGroupIds, errorQueries);
      job.OnMessage += MessageRaised;

      try
      {
        RaiseMessage("Enter Agent", 10);
        await job.ExecuteAsync();
        RaiseMessage("Exit Agent", 10);
      }
      catch (Exception ex)
      {
        //Raise an error on the agents tab and event viewer
        RaiseError(ex.ToString(), ex.ToString());

        //Add the error to our custom Errors table
        queryHelper.InsertRowIntoJobErrorLogAsync(Helper.GetDBContext(-1), job.WorkspaceArtifactId, Constant.Tables.ReproduceWorkerQueue, job.RecordId, job.AgentId, ex.ToString(), Constant.Tables.ReproduceErrorLog).Wait();

        //Add the error to the Relativity Errors tab
        //this second try catch is in case we have a problem connecting to the RSAPI
        try
        {
          errorQueries.WriteError(Helper.GetServicesManager(), ExecutionIdentity.System, job.WorkspaceArtifactId, ex);
        }
        catch (Exception rsapiException)
        {
          RaiseError(rsapiException.ToString(), rsapiException.ToString());
        }
      }
    }

    public override string Name
    {
      get { return "Markup Utilities - Reproduce Worker"; }
    }

    private void MessageRaised(object sender, string message)
    {
      RaiseMessage(message, 10);
    }
  }
}
