using System.Collections.Generic;
using StealME.Server.Core.Communication.Thrift;

namespace StealME.Server.Core.Communication
{
    public class NodeServiceImplementation : NodeService.Iface
    {
        private List<Session> _activeSessions;
        public List<Session> ActiveSessions
        {
            get { return _activeSessions; }
        }

        public NodeServiceImplementation(List<Session> sessions)
        {
            this._activeSessions = sessions;
        }

        #region NodeService Interface Implementation
        public List<int> getConnectedTrackerIds()
        {
            return new List<int>();
        }

        public bool isTrackerConnected(int trackerId)
        {
            return true;
        }

        public string getNodeId()
        {
            return "";
        }

        public void sendCommand(int trackerId, tracker_command command)
        {

        }
        #endregion
    }
}
