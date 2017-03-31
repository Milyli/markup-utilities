using System.Web.Mvc;
using Relativity.API;
using Relativity.CustomPages;
using MarkupUtilities.Helpers;
using MarkupUtilities.Helpers.Rsapi;
using MarkupUtilities.Helpers.Rsapi.Interfaces;

namespace MarkupUtilities.CustomPages
{
  public class MyCustomErrorHandler : HandleErrorAttribute
  {
    public override void OnException(ExceptionContext filterContext)
    {
      base.OnException(filterContext);
      int caseArtifactId;
      int.TryParse(filterContext.HttpContext.Request.QueryString["appid"], out caseArtifactId);

      var queryHelper = new Query();
      IErrorQueries errorQueries = new ErrorQueries();

      if (filterContext.Exception != null)
      {
        try
        {
          //try to log the error to the errors tab in Relativity
          errorQueries.WriteError(ConnectionHelper.Helper().GetServicesManager(),
          ExecutionIdentity.CurrentUser, caseArtifactId, filterContext.Exception);
        }
        catch
        {
          //if the error cannot be logged, add the error to our custom Errors table
          queryHelper.InsertRowIntoJobErrorLogAsync(ConnectionHelper.Helper().GetDBContext(-1), caseArtifactId, Constant.Tables.ExportWorkerQueue, 0, 0, filterContext.Exception.ToString(), Constant.Tables.ExportErrorLog).Wait();
        }
      }
    }
  }
}
