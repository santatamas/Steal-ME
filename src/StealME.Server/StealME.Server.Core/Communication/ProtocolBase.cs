using StealME.Server.Core.BLL;
using StealME.Server.Data.DAL;
using StealME.Server.Messaging.Exceptions;
using StealME.Server.Messaging.Interfaces;
using StealME.Server.Messaging.Messages;
using StealME.Server.Messaging.Requests;
using StealME.Server.Messaging.Responses;

namespace StealME.Server.Core.Communication
{
    public class ProtocolBase : IProtocol
    {
        internal Session _session;
        internal int _version;

        public ProtocolBase(Session session)
        {
            _session = session;
            _version = 1;
        }

        public virtual void ProcessMessage(object message)
        {
            // not really optimal, better use a hashtable here - with integer keys
            switch (message.GetType().Name)
            {
                case "AuthRequest":
                    var authReq = (AuthRequest)message;
                    var tracker = TrackerLogic.GetTracker(authReq.IMEI);
                    // todo: license validation
                    if (tracker != null)
                    {
                        _session.SendMessage(new AuthResponse {Result = true});
                    }
                    else
                    {
                        _session.SendMessage(new AuthResponse { Result = false, ErrorCode = 1});
                        _session.Terminate();
                    }
                    break;
                case "LocationMessage":
                    var location = (LocationMessage) message;
                    PositionLogic.InsertPosition(_session.Tracker, new Position
                                                                       {
                                                                           Latitude = location.Latitude.ToString(),
                                                                           Longtitude = location.Longitude.ToString(),
                                                                           CellId = location.CellID.ToString(),
                                                                           Mnc = location.CellID.ToString(),
                                                                           Mcc = location.MCC.ToString(),
                                                                           Rssi = location.LatestRssi.ToString()
                                                                       });
                    break;
                case "StatusResponse":
                    // todo: push information to DB if authenticated
                    break;
                case "PingRequest":
                    // todo: refresh lastSeenDate
                    _session.SendMessage(new PingResponse());
                    break;
                case "TrackerStateMessage":
                    
                    break;
                default:
                    throw new UnknownMessageException();
            }
        }

        public virtual bool IsKnownMessageType(object message)
        {
            return true;
        }

        public int Version
        {
            get { return _version; }
        }
    }
}
