using System.Web.Mvc;

namespace SurveilAI.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult Error()
        {
            return View();
        }
        public ActionResult Error505()
        {
            return View();
        }
        public ActionResult ParViewError()
        {
            return View();
        }
    }
}