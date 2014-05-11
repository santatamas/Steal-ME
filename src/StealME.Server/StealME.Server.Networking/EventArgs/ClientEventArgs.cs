namespace StealME.Server.Networking.EventArgs
{
    using System;

    using StealME.Server.Networking.Tcp;

    public class ClientEventArgs : EventArgs
    {
        public ConnectionHandler Connection { get; set; }
    }
}
