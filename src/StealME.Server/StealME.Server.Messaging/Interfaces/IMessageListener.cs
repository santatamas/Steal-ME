using StealME.Server.Messaging.Enums;

namespace StealME.Server.Messaging.Interfaces
{
    public interface IMessageListener
    {
        void onCommandReceived(CommandType signal);

        void onSetRequest(SetRequestType interval, object o);

        void onMessage(MessageType status, object o);

        void onGetRequest(GetRequestType status);
    }
}
