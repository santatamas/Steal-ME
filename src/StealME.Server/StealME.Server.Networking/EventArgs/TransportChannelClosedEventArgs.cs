using System.Net.Sockets;

namespace StealME.Server.Networking.EventArgs
{
    public class TransportChannelClosedEventArgs : System.EventArgs
    {
        public SocketAsyncEventArgs ReceiveEventArgs { get; set; }
        public SocketAsyncEventArgs SendEventArgs { get; set; }
    }
}
