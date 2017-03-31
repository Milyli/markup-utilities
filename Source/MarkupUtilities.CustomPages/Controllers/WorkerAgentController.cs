using System.Threading.Tasks;
using System.Web.Mvc;
using MarkupUtilities.CustomPages.Models;
using Relativity.CustomPages;

namespace MarkupUtilities.CustomPages.Controllers
{
  [MyWorkerQueueAuthorize]
  public class WorkerAgentController : Controller
  {
    [HttpGet]
    public async Task<ActionResult> Index()
    {
      var model = new WorkerAgentModel();
      await model.GetAllAsync(ConnectionHelper.Helper().GetDBContext(-1));
      return View(model);
    }
  }
}
