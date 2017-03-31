using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Relativity.API;
using MarkupUtilities.Helpers.Rsapi;
using Relativity.CustomPages;

namespace MarkupUtilities.CustomPages
{
  public class MyWorkerQueueAuthorizeAttribute : AuthorizeAttribute
  {
    protected override bool AuthorizeCore(HttpContextBase httpContext)
    {
      var isAuthorized = false;

      if (httpContext.Session != null)
      {
        int caseArtifactId;
        int.TryParse(httpContext.Request.QueryString["appid"], out caseArtifactId);

        var query = new ArtifactQueries();
        var res = query.DoesUserHaveAccessToArtifact(
        ConnectionHelper.Helper().GetServicesManager(),
        ExecutionIdentity.CurrentUser,
        caseArtifactId,
        Helpers.Constant.Guids.Tabs.ExportWorkerQueueTab,
        "Tab");
        isAuthorized = res;
      }

      return isAuthorized;
    }

    protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
    {
      filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
      {
        {"action", "AccessDenied"},
        {"controller", "Error"}
      });
    }
  }
}
