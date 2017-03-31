using System;
using System.Threading.Tasks;
using MarkupUtilities.Helpers;
using MarkupUtilities.Helpers.Rsapi;
using MarkupUtilities.Helpers.Rsapi.Interfaces;
using Relativity.API;

namespace MarkupUtilities.Agents
{
  // Change the name and guid for your application
  [kCura.Agent.CustomAttributes.Name("Markup Utilities - Import Worker")]
  [System.Runtime.InteropServices.Guid("C9918055-82F4-4CD7-B721-93A1A2D22BA6")]
  public class ImportWorker : kCura.Agent.AgentBase
  {
    public override void Execute()
    {
      ExecuteAsync().Wait();
    }

    public async Task ExecuteAsync()
    {
      IQuery queryHelper = new Query();
      var resourceGroupIds = GetResourceGroupIDs();
      IErrorQueries errorQueries = new ErrorQueries();
      IArtifactQueries artifactQueries = new ArtifactQueries();
      IAuditRecordHelper auditRecordHelper = new AuditRecordHelper(queryHelper);
      IMarkupTypeHelper markupTypeHelper = new MarkupTypeHelper();
      var job = new ImportWorkerJob(AgentID, Helper, queryHelper, DateTime.Now, resourceGroupIds, errorQueries, artifactQueries, auditRecordHelper, markupTypeHelper);
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
        queryHelper.InsertRowIntoImportErrorLogAsync(Helper.GetDBContext(-1), job.WorkspaceArtifactId, Constant.Tables.ImportWorkerQueue, job.RecordId, job.AgentId, ex.ToString()).Wait();

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
        queryHelper.UpdateStatusInImportWorkerQueueAsync(Helper.GetDBContext(-1), Constant.Status.Queue.Error, job.BatchTableName).Wait();
      }
    }

    public override string Name
    {
      get { return "Markup Utilities - Import Worker"; }
    }

    private void MessageRaised(Object sender, string message)
    {
      RaiseMessage(message, 10);
    }
  }
}
