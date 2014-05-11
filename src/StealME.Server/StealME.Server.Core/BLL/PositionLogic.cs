namespace StealME.Server.Core.BLL
{
    using System;
    using System.Diagnostics;
    using System.Linq;

    using StealME.Server.Data.DAL;

    public static class PositionLogic
    {
        public static void InsertPosition(string imei, Position position)
        {
            try
            {
                var tracker = TrackerLogic.GetTracker(imei);
                if (tracker == null) return;

                InsertPosition(tracker, position);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("StealME.MessageType:" + ex.Message);
            }
        }
        public static void InsertPosition(Tracker tracker, Position position)
        {
            try
            {
                position.TrackerId = tracker.Id;
                position.Id = Guid.NewGuid();
                position.CreationDate = DateTime.Now;

                var context = DataHandler.GetContext();
                context.Position.AddObject(position);
                context.SaveChanges();

                SMLogger.LogThis("Position inserted.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("StealME.MessageType:" + ex.Message);
            }
        }
        public static Position[] GetPositions(string imei)
        {
            Tracker t = TrackerLogic.GetTracker(imei);
            return PositionLogic.GetPositions(t);
        }
        public static Position[] GetPositions(Tracker tracker)
        {
            return DataHandler.GetContext().Position.Where(p => p.TrackerId == tracker.Id).ToArray();
        }
        public static Position[] GetPositions(Tracker tracker, int? maxNumberOfPositions, DateTime? validFrom, DateTime? validUntil)
        {
            var positionsQuery = DataHandler.GetContext().Position.OrderByDescending(p => p.CreationDate).Where(p => p.TrackerId == tracker.Id);

            if(validFrom.HasValue) positionsQuery = positionsQuery.Where(p => p.CreationDate >= validFrom);
            if(validUntil.HasValue) positionsQuery = positionsQuery.Where(p => p.CreationDate <= validUntil);

            if(maxNumberOfPositions.HasValue) positionsQuery = positionsQuery.Take(maxNumberOfPositions.Value);

            return positionsQuery.ToArray();
        }
    }
}
