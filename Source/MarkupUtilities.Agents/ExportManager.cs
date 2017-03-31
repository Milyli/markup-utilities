using System;
using System.Threading.Tasks;
using MarkupUtilities.Helpers;
using MarkupUtilities.Helpers.Rsapi;
using MarkupUtilities.Helpers.Rsapi.Interfaces;
using Relativity.API;
using IQuery = MarkupUtilities.Helpers.Utility.IQuery;

namespace MarkupUtilities.Agents
{
  // Change the name and guid for your application
  [kCura.Agent.CustomAttributes.Name("Markup Utilities - Export Manager")]
  [System.Runtime.InteropServices.Guid("f2751fd0-7af0-4fb2-a560-d1b442aec3ff")]
  public class ExportManager : kCura.Agent.AgentBase
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
      IMarkupTypeHelper markuptypeHelper = new MarkupTypeHelper();
      IQuery utilityQueryHelper = new Helpers.Utility.Query();
      var job = new ExportManagerJob(AgentID, Helper.GetServicesManager(), Helper, queryHelper, DateTime.Now, resourceGroupIds, artifactQueries, utilityQueryHelper, errorQueries, markuptypeHelper);
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
        queryHelper.InsertRowIntoJobErrorLogAsync(Helper.GetDBContext(-1), job.WorkspaceArtifactId, Constant.Tables.ExportManagerQueue, job.RecordId, job.AgentId, ex.ToString(), Constant.Tables.ExportErrorLog).Wait();

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
        queryHelper.UpdateStatusInJobManagerQueueAsync(Helper.GetDBContext(-1), Constant.Status.Queue.Error, job.RecordId, Constant.Tables.ExportManagerQueue).Wait();
      }
    }

    public override string Name
    {
      get { return "Markup Utilities - Export Manager"; }
    }

    private void MessageRaised(object sender, string message)
    {
      RaiseMessage(message, 10);
    }
  }
}
