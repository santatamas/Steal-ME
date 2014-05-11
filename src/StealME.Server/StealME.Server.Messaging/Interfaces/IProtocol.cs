namespace StealME.Server.Messaging.Interfaces
{
    public interface IProtocol
    {
        int Version { get; }
        void ProcessMessage(object message);
        bool IsKnownMessageType(object message);
    }
}
