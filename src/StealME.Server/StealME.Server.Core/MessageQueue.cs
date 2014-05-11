namespace StealME.Server.Core
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading;

    using StealME.Server.Core.BLL;
    using StealME.Server.Data.DAL;

    public static class MessageQueue
    {
        private const int POLL_INTERVAL = 1000;

        static MessageQueue()
        {

        }

        public static void StartPolling()
        {
            Thread commandPollThread = new Thread(new ThreadStart(CheckServerForPendingCommands));
            commandPollThread.Name = "StealME.Server.CommandPollThread";
            commandPollThread.IsBackground = true;
            commandPollThread.Start();
        }

        private static Object _syncRoot = new Object();
        private static void CheckServerForPendingCommands()
        {
            while (true)
            {
                if (!IsCurrentCommandListValid())
                {
                    lock (_syncRoot)
                    {
                        _pendingCommands = CommandLogic.GetCommands();
                    }
                }

                Thread.Sleep(POLL_INTERVAL);
            }
        }

        private static List<Command> _pendingCommands = new List<Command>();

        public static string[] GetPendingCommands(Guid trackerId)
        {
            IEnumerable<Command> tComs;
            lock (_syncRoot)
            {
                tComs = _pendingCommands.Where(c => c.TrackerId == trackerId).ToArray();
            }

            foreach (Command c in tComs)
            {
                CommandLogic.DeleteCommand(c);

                lock (_syncRoot)
                {
                    _pendingCommands.Remove(c);
                }
            }

            return tComs.Select(c => c.CommandText).ToArray();
        }

        private static bool IsCurrentCommandListValid()
        {
            if (_pendingCommands == null)
                return true;

            DateTime? latestCommandDate = CommandLogic.GetLatestCommandDate();
            if (latestCommandDate == null)
                return true;

            DateTime currentCommandDate;
            lock (_syncRoot)
            {
                if (_pendingCommands.Any())
                {
                    currentCommandDate = _pendingCommands.OrderByDescending(c => c.CreationDate).First().CreationDate;
                }
                else
                {
                    return false;
                }
            }

            return currentCommandDate >= latestCommandDate.Value;
        }
    }
}
