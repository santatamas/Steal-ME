namespace StealME.Server.Core.BLL
{
    using System;
    using System.Diagnostics;
    using System.Linq;

    using StealME.Server.Data.DAL;

    public static class TrackerLogic
    {
        public static Tracker GetTracker(string imei)
        {
            return DataHandler.GetContext().Tracker.FirstOrDefault(t => t.IMEI == imei);
        }
        public static Tracker GetTracker(Guid id)
        {
            return DataHandler.GetContext().Tracker.First(t => t.Id == id);
        }

        public static Tracker CreateTracker(string imei, string name, string description)
        {
            var context = DataHandler.GetContext();
            Tracker result = new Tracker
                {
                    Id = Guid.NewGuid(),
                    CreationDate = DateTime.Now,
                    IMEI = imei,
                    Name = name,
                    Description = description
                };
            context.Tracker.AddObject(result);
            context.SaveChanges();

            return result;
        }    

        public static bool DeleteTracker(string imei)
        {
            if (!DataHandler.GetContext().Tracker.Any(t => t.IMEI == imei))
            {
                return false;
            }

            Tracker trackerToRemove = DataHandler.GetContext().Tracker.First(t => t.IMEI == imei);
            return DeleteTracker(trackerToRemove);
        }
        public static bool DeleteTracker(Guid id)
        {
            if (!DataHandler.GetContext().Tracker.Any(t => t.Id == id))
            {
                return false;
            }

            Tracker trackerToRemove = DataHandler.GetContext().Tracker.First(t => t.Id == id);
            return DeleteTracker(trackerToRemove);
        }
        public static bool DeleteTracker(Tracker t)
        {
            try
            {
                var context = DataHandler.GetContext();
                context.Tracker.DeleteObject(t);
                context.SaveChanges();
            }
            catch (Exception ex)
            { return false; }

            return true;
        }

        public static void SetTrackerOnLine(Tracker tracker)
        {
            try
            {
                tracker.IsOnline = true;
                DataHandler.GetContext().SaveChanges();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("StealME.Exception:" + ex.Message);
            }
        }
        public static void SetTrackerOffLine(Tracker tracker)
        {
            try
            {
                tracker.IsOnline = false;
                DataHandler.GetContext().SaveChanges();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("StealME.Exception:" + ex.Message);
            }
        }
    }
}
