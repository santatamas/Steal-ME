using System;
using StealME.Server.Networking.EventArgs;

namespace StealME.Server.Networking.Async
{
    public interface ITransportChannel
    {
        void Send(byte[] data);
        void TerminateChannel();
        event EventHandler<TransportChannelClosedEventArgs> Closed;
        event EventHandler Opened;
        event EventHandler<RawMessageEventArgs> MessageReceived;
        event EventHandler MessageSent;
    }
}
