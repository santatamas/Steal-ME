namespace StealME.Server.Networking.Tcp
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Sockets;
    using System.Threading;

    using StealME.Server.Networking.EventArgs;
    using StealME.Server.Networking.Helpers;

    public class SynchronousSocketListener
    {
        public const int portNum = 8888;
        public static SharedState SharedStateObj;

        public event EventHandler<ClientEventArgs> ClientConnected;
        public event EventHandler<ClientEventArgs> ClientLeft;

        private List<ConnectionHandler> _clients = new List<ConnectionHandler>();

        public List<ConnectionHandler> Clients
        {
            get
            {
                return this._clients;
            }
        }

        public void StartListening()
        {
            SharedStateObj = new SharedState();
            SharedStateObj.ContinueProcess = true;
            SharedStateObj.NumberOfClients = 0;
            SharedStateObj.Ev = new AutoResetEvent(false);

            TcpListener listener = new TcpListener(portNum);
            try
            {
                listener.Start();

                int ClientNbr = 0;

                // Start listening for connections.
                Debug.WriteLine("Waiting for a connection...");
                while (SharedStateObj.ContinueProcess)
                {
                    TcpClient handler = listener.AcceptTcpClient();
                    SharedStateObj.NumberOfClients++;
                    Debug.WriteLine("Client#{0} accepted!", ClientNbr);

                    ConnectionHandler _connection = new ConnectionHandler(handler);
                    _connection.Disconnected += new EventHandler(this.client_Disconnected);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(_connection.Process), SharedStateObj);
                    this.Clients.Add(_connection);

                    this.RaiseClientConnected(_connection);
                }

                listener.Stop();

            }
            catch (Exception ex)
            {
                Debug.WriteLine("StealME.Exception:" + ex.ToString());
            }

            // Stop and wait all Client connections to end
            SharedStateObj.ContinueProcess = false;
            SharedStateObj.Ev.WaitOne();
        }

        private void RaiseClientConnected(ConnectionHandler _connection)
        {
            if (this.ClientConnected != null)
                this.ClientConnected(this, new ClientEventArgs { Connection = _connection });
        }

        private void RaiseClientLeft(ConnectionHandler _connection)
        {
            if (this.ClientLeft != null)
                this.ClientLeft(this, new ClientEventArgs { Connection = _connection });
        }

        void client_Disconnected(object sender, EventArgs e)
        {
            ConnectionHandler _connection = (ConnectionHandler)sender;
            _connection.Disconnected -= this.client_Disconnected;
            this.RaiseClientLeft(_connection);
            SharedStateObj.NumberOfClients--;
            Debug.WriteLine("A Client left");
        }
    }
}