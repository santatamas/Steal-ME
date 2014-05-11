namespace StealME.Server.Frontend.Web.Controllers
{
    using System.Linq;
    using System.Web.Mvc;

    using StealME.Server.Core.BLL;

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Your device's last known location:";     
            ViewBag.Positions = PositionLogic.GetPositions("359234040316475").Take(20).ToArray();

            return View();
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
