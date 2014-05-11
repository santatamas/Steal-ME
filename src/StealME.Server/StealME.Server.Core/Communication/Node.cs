using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using StealME.Server.Core.Communication.Thrift;
using StealME.Server.Networking.Async;
using StealME.Server.Networking.Helpers;
using Thrift.Server;
using Thrift.Transport;

namespace StealME.Server.Core.Communication
{
    public class Node
    {
        private static List<Session> _currentSessions;

        public Node()
        {
            // todo: use NodeId here
        }

        public void Start()
        {
            try
            {
                _currentSessions = new List<Session>();

                // Initialize and start server which accepts tracker connections
                var socketListener = new SocketListener(NetworkSettings.GetFromAppConfig());
                socketListener.ActiveChannels.CollectionChanged += ActiveSessions_CollectionChanged;

                // Initialize and start Node interface (used by other services to access devices realtime)
                // Processor
                var implementation = new NodeServiceImplementation(_currentSessions);
                var testProcessor = new NodeService.Processor(implementation);

                // Transport
                // todo: define port in AppConfig!
                var tServerSocket = new TServerSocket(4445, 0, true);

                // ThreadPool Server
                TServer serverEngine = new TThreadPoolServer(testProcessor, tServerSocket);

                // Run it
                serverEngine.Serve();

                Console.WriteLine("Server is running...press any key to exit...");

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        void ActiveSessions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var channel in e.NewItems)
                    {
                        var session = new Session((ITransportChannel)channel);
                        _currentSessions.Add(session);
                    }
                    Console.WriteLine("Client connected");
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var channel in e.OldItems)
                    {
                        var session = _currentSessions.FirstOrDefault(s => s.Channel.Equals(channel));
                        if (session != null)
                        {
                            _currentSessions.Remove(session);
                        }
                    }
                    Console.WriteLine("Client disconnected");
                    break;

            }
        }
    }
}