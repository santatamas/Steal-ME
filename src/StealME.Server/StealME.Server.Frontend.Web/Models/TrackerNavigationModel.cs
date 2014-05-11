namespace StealME.Server.Frontend.Web.Models
{
    using System.Collections.Generic;
    using StealME.Server.Data.DAL;

    public class TrackerNavigationModel
    {
        public Tracker SelectedTracker { get; set; }

        public List<Tracker> Trackers { get; set; }
    }
}
