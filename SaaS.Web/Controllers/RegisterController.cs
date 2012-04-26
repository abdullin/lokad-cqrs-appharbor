using System.Web.Mvc;

namespace SaaS.Web.Controllers
{
    public sealed class RegisterController : Controller
    {
        public ActionResult Index()
        {
            return Content("Ready");
        }
    }
}