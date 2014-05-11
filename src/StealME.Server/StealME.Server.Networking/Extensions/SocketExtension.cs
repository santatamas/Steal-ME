namespace StealME.Server.Networking.Extensions
{
    using System.Net.Sockets;

    public static class SocketExtension
    {
        public static bool IsConnected(this TcpClient client)
        {
            try
            {
                bool connected = !(client.Client.Poll(1, SelectMode.SelectRead) && client.Client.Available == 0);

                return connected;
            }
            catch
            {
                return false;
            }
        }
    }
}
