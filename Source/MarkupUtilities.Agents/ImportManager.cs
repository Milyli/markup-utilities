using System;
using System.Threading.Tasks;
using MarkupUtilities.Helpers;
using MarkupUtilities.Helpers.Rsapi;
using MarkupUtilities.Helpers.Rsapi.Interfaces;
using Relativity.API;

namespace MarkupUtilities.Agents
{
  // Change the name and guid for your application
  [kCura.Agent.CustomAttributes.Name("Markup Utilities - Import Manager")]
  [System.Runtime.InteropServices.Guid("13E95CDF-6A2B-4A21-86F8-9F21E85FB902")]
  public class ImportManager : kCura.Agent.AgentBase
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
      IImportFileParser importFileParser = new ImportFileParser();
      IWorkspaceQueries workspaceQueries = new WorkspaceQueries();
      IErrorQueries errorQueries = new ErrorQueries();
      IMarkupTypeHelper markupTypeHelper = new MarkupTypeHelper();
      var job = new ImportManagerJob(
        AgentID,
        Helper,
        queryHelper,
        DateTime.Now,
        resourceGroupIds,
        artifactQueries,
        importFileParser,
        workspaceQueries,
        errorQueries,
        markupTypeHelper);
      job.OnMessage += MessageRaised;

      try
      {
        RaiseMessage(string.Empty, 10);
        RaiseMessage("Enter Agent", 10);
        await job.ExecuteAsync();
        RaiseMessage("Exit Agent", 10);
        RaiseMessage(string.Empty, 10);
      }
      catch (Exception ex)
      {
        //Raise an error on the agents tab and event viewer
        RaiseError(ex.ToString(), ex.ToString());

        //Add the error to our custom Errors table
        queryHelper.InsertRowIntoImportErrorLogAsync(Helper.GetDBContext(-1), job.WorkspaceArtifactId, Constant.Tables.ImportManagerQueue, job.RecordId, job.AgentId, ex.ToString()).Wait();

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
        await queryHelper.UpdateStatusInImportManagerQueueAsync(Helper.GetDBContext(-1), Constant.Status.Queue.Error, job.RecordId);
      }
    }

    public override string Name
    {
      get { return "Markup Utilities - Import Manager"; }
    }

    private void MessageRaised(object sender, string message)
    {
      RaiseMessage(message, 10);
    }
  }
}
