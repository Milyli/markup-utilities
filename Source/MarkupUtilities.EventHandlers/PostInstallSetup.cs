using System;
using System.Threading.Tasks;
using MarkupUtilities.Helpers;
using MarkupUtilities.Helpers.Rsapi;
using MarkupUtilities.Helpers.Rsapi.Interfaces;
using Relativity.API;

namespace MarkupUtilities.EventHandlers
{
  [kCura.EventHandler.CustomAttributes.RunOnce(false)]
  [kCura.EventHandler.CustomAttributes.Description("Creates the underlying tables for the application.")]
  [System.Runtime.InteropServices.Guid("e2a36fae-10ef-4353-83e8-84b78e6a062f")]
  public class PostInstallSetup : kCura.EventHandler.PostInstallEventHandler
  {
    public override kCura.EventHandler.Response Execute()
    {
      return ExecuteAsync().Result;
    }

    public async Task<kCura.EventHandler.Response> ExecuteAsync()
    {
      IQuery query = new Query();
      IArtifactQueries artifactQueries = new ArtifactQueries();
      IPostInstallSetupHelper postInstallSetupHelper = new PostInstallSetupHelper(query, artifactQueries);

      //Create Tables
      var returnResponse = await Task.Run(() =>
      {
        var response = new kCura.EventHandler.Response
        {
          Success = true,
          Message = string.Empty
        };

        //Create Export Manager Queue table if it doesn't already exist
        var exportManagerTableTask = query.CreateExportManagerQueueTableAsync(Helper.GetDBContext(-1));

        //Create Export Worker Queue table if it doesn't already exist
        var exportWorkerTableTask = query.CreateExportWorkerQueueTableAsync(Helper.GetDBContext(-1));

        var reproduceWorkerTableTask = query.CreateReproduceWorkerQueueTableAsync(Helper.GetDBContext(-1));

        //Create Export Error Log table if it doesn't already exist
        var exportErrorLogTableTask = query.CreateExportErrorLogTableAsync(Helper.GetDBContext(-1));

        //Create Import Manager Queue table if it doesn't already exist
        var importManagerTableTask = query.CreateImportManagerQueueTableAsync(Helper.GetDBContext(-1));

        //Create Import Worker Queue table if it doesn't already exist
        var importWorkerTableTask = query.CreateImportWorkerQueueTableAsync(Helper.GetDBContext(-1));

        //Create Import Error Log table if it doesn't already exist
        var importErrorLogTableTask = query.CreateImportErrorLogTableAsync(Helper.GetDBContext(-1));

        //Create Export Results table if it doesn't already exist
        var importExportResultsTableTask = query.CreateExportResultsTableAsync(Helper.GetDBContext(-1));

        //Create Reproduce Manager Queue table if it doesn't already exist
        var reproduceManagerTableTask = query.CreateReproduceManagerQueueTableAsync(Helper.GetDBContext(-1));

        //Create Reproduce Error Log table if it doesn't already exist
        var reproduceErrorLogTableTask = query.CreateReproduceErrorLogTableAsync(Helper.GetDBContext(-1));

        try
        {
          //Waits for all tasks, otherwise exceptions would be lost
          Task.WaitAll(exportManagerTableTask, exportWorkerTableTask, exportErrorLogTableTask, importManagerTableTask, importWorkerTableTask, importErrorLogTableTask, importExportResultsTableTask, reproduceManagerTableTask, reproduceErrorLogTableTask, reproduceWorkerTableTask);
        }
        catch (AggregateException aex)
        {
          var ex = aex.Flatten();
          var message = ex.Message + " : " + (ex.InnerException?.Message ?? "None");
          response.Success = false;
          response.Message = "Post-Install queue and error log table creation failed with message: " + message;
        }

        ////If table(s) creation fails return error response
        if (!response.Success) return response;
        {
          //Create records for MarkupUtilityType Rdo
          var workspaceArtifactId = Helper.GetActiveCaseID();
          var servicesManager = Helper.GetServicesManager();
          var workspaceDbContext = Helper.GetDBContext(workspaceArtifactId);

          var createRecordsForMarkupUtilityTypeRdoTask = Task.Run(async () =>
    {
      await postInstallSetupHelper.CreateRecordsForMarkupUtilityTypeRdoAsync(servicesManager, ExecutionIdentity.CurrentUser, workspaceArtifactId, workspaceDbContext);
    });

          try
          {
            createRecordsForMarkupUtilityTypeRdoTask.Wait();
          }
          catch (Exception ex)
          {
            var message = ex.Message + " : " + (ex.InnerException?.Message ?? "None");
            response.Success = false;
            response.Message = "Post-Install queue and error log table creation failed with message: " + message;
          }
        }

        return response;
      });

      return returnResponse;
    }
  }
}
