using System;
using System.Threading.Tasks;
using MarkupUtilities.Helpers;
using MarkupUtilities.Helpers.Rsapi;
using MarkupUtilities.Helpers.Rsapi.Interfaces;
using Relativity.API;
using IQuery = MarkupUtilities.Helpers.Utility.IQuery;

namespace MarkupUtilities.Agents
{
  [kCura.Agent.CustomAttributes.Name("Markup Utilities - Reproduce Manager")]
  [System.Runtime.InteropServices.Guid("a35145c2-d0e8-436d-bf9e-bb73c67d4d60")]
  public class ReproduceManager : kCura.Agent.AgentBase
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
      IQuery utilityQueryHelper = new Helpers.Utility.Query();
      var job = new ReproduceManagerJob(AgentID, Helper, queryHelper, DateTime.Now, resourceGroupIds, artifactQueries, utilityQueryHelper, errorQueries);
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
        queryHelper.InsertRowIntoJobErrorLogAsync(Helper.GetDBContext(-1), job.WorkspaceArtifactId, Constant.Tables.ExportManagerQueue, job.RecordId, job.AgentId, ex.ToString(), Constant.Tables.ReproduceErrorLog).Wait();

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

        //Set the status in the queue to error
        queryHelper.UpdateStatusInJobManagerQueueAsync(Helper.GetDBContext(-1), Constant.Status.Queue.Error, job.RecordId, Constant.Tables.ReproduceManagerQueue).Wait();
      }
    }

    public override string Name
    {
      get { return "Markup Utilities - Reproduce Manager"; }
    }

    private void MessageRaised(object sender, string message)
    {
      RaiseMessage(message, 10);
    }
  }
}