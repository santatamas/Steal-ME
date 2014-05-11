using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StealME.Server.Core;
using StealME.Server.Networking.Tcp;
using StealME.Server.Networking.EventArgs;
using System.Threading;

namespace StealME.Server.Service.Console
{
    class Program
    {
        private static SynchronousSocketListener _socketListener;

        private static readonly List<ClientHandler> _clients = new List<ClientHandler>();

        static void Main(string[] args)
        {
            try
            {
                SMLogger.LogThis("StealME Service Starting...");

                _socketListener = new SynchronousSocketListener();
                _socketListener.ClientConnected +=
                    new System.EventHandler<ClientEventArgs>(socketListener_ClientConnected);
                _socketListener.ClientLeft += new System.EventHandler<ClientEventArgs>(socketListener_ClientLeft);

                Thread listenerThread = new Thread(new ThreadStart(_socketListener.StartListening));
                listenerThread.IsBackground = true;
                listenerThread.Start();

                MessageQueue.StartPolling();

                SMLogger.LogThis("StealME Service Started.");
                System.Console.ReadLine();
            }
            catch (Exception ex)
            {
                SMLogger.LogThis(ex.ToString());
            }
        }

        private static void socketListener_ClientLeft(object sender, ClientEventArgs e)
        {
            var client = _clients.FirstOrDefault(c => Equals(c.Connection, e.Connection));
            if (client != null)
            {
                _clients.Remove(client);
                SMLogger.LogThis("Client left");
            }
        }

        private static void socketListener_ClientConnected(object sender, ClientEventArgs e)
        {
            try
            {
                var client = new ClientHandler(e.Connection);
                _clients.Add(client);
                SMLogger.LogThis("Client connected");
            }
            catch (Exception ex)
            {
                SMLogger.LogThis(ex.ToString());
            }
        }
    }
}
