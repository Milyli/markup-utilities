using System.Threading.Tasks;
using System.Web.Mvc;
using MarkupUtilities.CustomPages.Models;
using Relativity.CustomPages;

namespace MarkupUtilities.CustomPages.Controllers
{
  [MyManagerQueueAuthorize]
  public class ManagerAgentController : Controller
  {
    [HttpGet]
    public async Task<ActionResult> Index()
    {
      var model = new ManagerAgentModel();
      await model.GetAllAsync(ConnectionHelper.Helper().GetDBContext(-1));
      return View(model);
    }
  }
}
