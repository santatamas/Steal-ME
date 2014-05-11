namespace StealME.Server.Frontend.Web.Controllers
{
    using System;
    using System.Web.Mvc;
    using System.Web.Security;
    using System.Web.UI;

    using StealME.Server.Core.BLL;
    using StealME.Server.Data.DAL;
    using StealME.Server.Frontend.Web.Models;

    public class DevicesController : Controller
    {
        public ActionResult Index()
        {
            // If not authenticated, redirect to login page
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("LogOn", "Account");
            }

            // Get user's licences
            var licences = LicenceLogic.GetLicencesForUser(User.Identity.Name, true);

            // Set View parameter
            ViewBag.Licences = licences;

            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Add(DeviceRegistrationModel model)
        {
            if (ModelState.IsValid)
            {
                Tracker newTracker = TrackerLogic.CreateTracker(model.IMEI, model.DeviceName, model.Description);
                User currentUser = UserLogic.GetUser(User.Identity.Name);
                LicenceLogic.CreateLicence(currentUser, newTracker, DateTime.Now.AddYears(100));

                return RedirectToAction("Index", "Devices");
                //ModelState.AddModelError("", "The user name or password provided is incorrect.");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        [OutputCache(Location = OutputCacheLocation.None, NoStore = true)]
        public ActionResult UpdateDevice(string DeviceId)
        {
            return PartialView("DeviceStatusControl", LicenceLogic.GetLicenceByTrackerId(DeviceId));
        }
    }
}
