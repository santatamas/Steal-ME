namespace StealME.Server.Frontend.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.UI;

    using StealME.Server.Core.BLL;
    using StealME.Server.Data.DAL;
    using StealME.Server.Frontend.Web.Models;

    public class TrackingController : Controller
    {
        //
        // GET: /Tracking/

        public ActionResult Index(string DeviceId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("LogOn", "Account");
            }

            if (string.IsNullOrEmpty(DeviceId)) return Content("This page cannot be viewed without provided DeviceId parameter.");

            // Get user's licences
            var licences = LicenceLogic.GetLicencesForUser(User.Identity.Name, true);
            ViewBag.Trackers = licences.Select(l => l.Tracker).ToList();

            Tracker tracker = TrackerLogic.GetTracker(new Guid(DeviceId));
            //Position[] positions = PositionLogic.GetPositions(tracker);

            ViewBag.SelectedTracker = tracker;

            ViewBag.Message = "Your device's last known location:";
            //ViewBag.Positions = positions;
            //ViewBag.Tracker = tracker;

            return View(new TrackerNavigationModel { SelectedTracker = tracker, Trackers = licences.Select(l => l.Tracker).ToList() });
        }

        [HttpPost]
        [OutputCache(Location = OutputCacheLocation.None, NoStore = true)]
        public JsonResult UpdateNavigation(string DeviceId)
        {
            // Get Post variables
            var noPos = Request.Form["No. Positions"];
            var posFrom = Request.Form["positions-from"];
            var posTo = Request.Form["positions-to"];

            // Validate/Parse Post Variables
            int numberOfPositions;
            var isNumPosValid = int.TryParse(noPos, out numberOfPositions);
            if (numberOfPositions == -1) numberOfPositions = 10000;

            DateTime positionsFromDate;
            var isPositionsFromDateValid = DateTime.TryParse(posFrom, out positionsFromDate);

            DateTime positionsToDate;
            var ispositionsToDateValid = DateTime.TryParse(posTo, out positionsToDate);

            // Get Tracker
            Tracker tracker = TrackerLogic.GetTracker(new Guid(DeviceId));

            // Create result model
            var result = new NavigationUpdateResultModel
                {
                    TrackerStatus = tracker.LastKnownState,
                    TrackerAvaliability = (tracker.IsOnline != null && tracker.IsOnline.Value) ? "Online" : "Offline",
                    CreationDate = DateTime.Now
                };

            // Get Positions based on the provided filter parameters
            var positions = PositionLogic.GetPositions(tracker, numberOfPositions, positionsFromDate, positionsToDate);
            result.PositionResult = positions.Select(pos => new JsonPosition
                {
                    CreationDate = pos.CreationDate, 
                    Latitude = pos.Latitude, 
                    Longtitude = pos.Longtitude
                }).ToArray();

            // Return the serialized result model
            return Json(result);
        }

        public ActionResult SendActivateCommand(string DeviceId)
        {
            CommandLogic.AddCommand(new Guid(DeviceId), "CMD.ACTIVATE");
            return Content("Command Sent");
        }

        public ActionResult SendDeactivateCommand(string DeviceId)
        {
            CommandLogic.AddCommand(new Guid(DeviceId), "CMD.DEACTIVATE");
            return Content("Command Sent");
        }

        public ActionResult SendSignalCommand(string DeviceId)
        {
            CommandLogic.AddCommand(new Guid(DeviceId), "CMD.SIGNAL");
            return Content("Command Sent");
        }

        public ActionResult SendEnableSmsCommand(string DeviceId)
        {
            //CommandLogic.AddCommand(new Guid(DeviceId), "CMD.DEACTIVATE");
            return Content("Command not implemented.");
        }

        public ActionResult SendDisableSmsCommand(string DeviceId)
        {
            //CommandLogic.AddCommand(new Guid(DeviceId), "CMD.DEACTIVATE");
            return Content("Command not implemented.");
        }
    }

    public class NavigationUpdateResultModel
    {
        public string TrackerAvaliability { get; set; }
        public string TrackerStatus { get; set; }
        public DateTime CreationDate { get; set; }

        public JsonPosition[] PositionResult { get; set; }
    }

    public struct JsonPosition
    {
        public string Latitude { get; set; }
        public string Longtitude { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
