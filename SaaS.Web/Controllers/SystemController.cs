using System.Web.Mvc;
using SaaS.Client.Projections.Releases;

namespace SaaS.Web.Controllers
{
    public sealed class SystemController : Controller
    {
        public ActionResult Releases()
        {
            var view = Global.Client.GetSingleton<ReleasesView>();
            return Json(view.List, JsonRequestBehavior.AllowGet);
        }
    }
}