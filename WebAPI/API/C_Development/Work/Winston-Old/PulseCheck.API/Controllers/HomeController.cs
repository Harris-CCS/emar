using System.Web.Mvc;

namespace PulseCheck.API.Controllers
{
    /// <summary>
    /// Home Controller for PulseCheck API
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Index ActionResult
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }
    }
}
