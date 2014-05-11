namespace StealME.Server.Core.Tests
{
    using System;

    using NUnit.Framework;

    using StealME.Server.Core.BLL;
    using StealME.Server.Data.DAL;
    using System.Linq;

    [TestFixture]
    public class BLLTests
    {
        private User _testUser;
        private Tracker _testTracker;

        private const string DEMO_TRACKER_IMEI = "123456789101112";

        [Test]
        public void TestUserCRUD()
        {
            Assert.DoesNotThrow(DeleteUser);
            Assert.DoesNotThrow(CreateUser);
            Assert.DoesNotThrow(GetUser);
            Assert.DoesNotThrow(ChangePassword);
        }

        [Test]
        public void TestTrackerCRUD()
        {
            Assert.DoesNotThrow(DeleteTracker);
            Assert.DoesNotThrow(CreateTracker);
            Assert.DoesNotThrow(GetTracker);
            Assert.DoesNotThrow(InsertRandomPositions);
        }

        [Test]
        public void TestCommandCRUD()
        {
            CommandLogic.AddCommand(TrackerLogic.GetTracker(DEMO_TRACKER_IMEI).Id, "CMD.ACTIVATE");
            CommandLogic.AddCommand(TrackerLogic.GetTracker(DEMO_TRACKER_IMEI).Id, "CMD.DEACTIVATE");
            CommandLogic.AddCommand(TrackerLogic.GetTracker(DEMO_TRACKER_IMEI).Id, "CMD.ACTIVATE");
            CommandLogic.AddCommand(TrackerLogic.GetTracker(DEMO_TRACKER_IMEI).Id, "CMD.DEACTIVATE");
            CommandLogic.AddCommand(TrackerLogic.GetTracker(DEMO_TRACKER_IMEI).Id, "CMD.ACTIVATE");
            CommandLogic.AddCommand(TrackerLogic.GetTracker(DEMO_TRACKER_IMEI).Id, "CMD.DEACTIVATE");

            var cmdList = CommandLogic.GetCommands().Where(c => c.TrackerId == TrackerLogic.GetTracker(DEMO_TRACKER_IMEI).Id).ToList();
            foreach (var cmd in cmdList)
            {
                Assert.DoesNotThrow(delegate { CommandLogic.DeleteCommand(cmd); });
            }
        }

        #region User CRUD Tests

        public void DeleteUser()
        {
            UserLogic.DeleteUser("Test");
        }

        public void CreateUser()
        {
            _testUser = UserLogic.CreateUser("Test", "test", "test@test.com", true);
            Assert.IsNotNull(_testUser);
        }

        public void GetUser()
        {
            _testUser = null;
            _testUser = UserLogic.GetUser("Test");
            Assert.IsNotNull(_testUser);
        }

        public void ChangePassword()
        {
            Assert.IsTrue(UserLogic.ChangePassword("Test", "test", "test1"));
        }

        #endregion

        #region Tracker CRUD Tests

        public void DeleteTracker()
        {
            TrackerLogic.DeleteTracker(DEMO_TRACKER_IMEI);
        }

        public void CreateTracker()
        {
            _testTracker = TrackerLogic.CreateTracker(DEMO_TRACKER_IMEI, "Demo tracker", "demo description");
            Assert.IsNotNull(_testTracker);
        }

        public void GetTracker()
        {
            _testTracker = null;
            _testTracker = TrackerLogic.GetTracker(DEMO_TRACKER_IMEI);
            Assert.IsNotNull(_testTracker);
        }

        public void InsertRandomPositions()
        {
            Random rnd = new Random();
            for (int i = 0; i < 100; i++)
            {
                PositionLogic.InsertPosition(DEMO_TRACKER_IMEI, new Position
                    {
                        Latitude = (rnd.NextDouble() * 70).ToString(),
                        Longtitude = (rnd.NextDouble() * 70).ToString(),
                        Acc = "01",
                        Mcc = "01",
                        Mnc = "01",
                        Rssi = "01",
                        CellId = "007",
                        CreationDate = DateTime.Now
                    });
            }
        }


        #endregion
    }
}
