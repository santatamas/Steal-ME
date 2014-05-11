namespace StealME.Server.Core.BLL
{
    using System;
    using System.Linq;
    using System.Data.Entity;

    using StealME.Server.Data.DAL;

    public static class LicenceLogic
    {
        public static Licence[] GetLicencesForUser(string userName, bool loadTrackers)
        {
            var user = UserLogic.GetUser(userName);
            return user != null ? GetLicencesForUser(user, loadTrackers) : null;     
        }

        public static Licence GetLicenceByTrackerId(string trackerId)
        {
            // Create licence query
            var trackerQuery = DataHandler.GetContext().Licence.Where(l => l.TrackerId == new Guid(trackerId)).Include(l => l.Tracker);
            return trackerQuery.First();
        }

        public static Licence[] GetLicencesForUser(User user, bool loadTrackers)
        {
            // Create result variable
            Licence[] result = null;

            // Create licence query
            var trackerQuery = DataHandler.GetContext().Licence.Where(l => l.UserId == user.Id).Include(l => l.Tracker);
            

            // Run query
            if (trackerQuery.Any()) result = trackerQuery.ToArray();


            //Return result
            return result;
        }

        public static void CreateLicence(User currentUser, Tracker newTracker, DateTime dateTime)
        {
            Licence l = new Licence
                {
                    UserId = currentUser.Id,
                    TrackerId = newTracker.Id,
                    CreationDate = DateTime.Now,
                    ValidFrom = DateTime.Now,
                    ValidUntil = dateTime,
                    Id = Guid.NewGuid()
                };
            var context = DataHandler.GetContext();
            context.Licence.AddObject(l);
            context.SaveChanges();
        }
    }
}