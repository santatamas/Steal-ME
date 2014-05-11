using StealME.Networking.EventArgs;

namespace StealME.Server.Networking.Tcp
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Sockets;
    using System.Text;

    using StealME.Server.Networking.EventArgs;
    using StealME.Server.Networking.Helpers;
    using StealME.Server.Messaging.Protocol;

    public class ConnectionHandler
    {
        #region Construction
        public ConnectionHandler(TcpClient clientSocket)
        {
            this._clientSocket = clientSocket;
        }
        #endregion

        #region Properties
        public TcpClient ClientSocket
        {
            get
            {
                return this._clientSocket;
            }
        }

        public Queue<string> MessageQueue
        {
            get
            {
                return this._messageQueue;
            }
        }

        public DateTime LastPing { get; set; }
        #endregion

        #region Events
        public event EventHandler<MessageEventArgs> MessageReceived;
        public event EventHandler Disconnected;
        #endregion

        #region Private
        private void RaiseMessageReceive(string message)
        {
            if (this.MessageReceived != null) this.MessageReceived(this, new MessageEventArgs { Message = message });
        }

        private void RaiseDisconnected()
        {
            if (this.Disconnected != null) this.Disconnected(this, new EventArgs());
        }

        #region Private State

        private TcpClient _clientSocket;
        private SharedState _sharedState;
        private Queue<string> _messageQueue = new Queue<string>();

        #endregion
        #endregion

        #region Public

        public void Process(Object o)
        {
            this._sharedState = (SharedState)o;

            // Incoming data from the Client.
            string data = null;

            // Data buffer for incoming data.
            byte[] bytes;
            byte[] messageBytes;
            this.LastPing = DateTime.Now;

            if (this.ClientSocket != null)
            {
                NetworkStream networkStream = this.ClientSocket.GetStream();
                this.ClientSocket.ReceiveTimeout = 10000;
                networkStream.ReadTimeout = 10000;
                StreamReader sr = new StreamReader(networkStream);
                StreamWriter sw = new StreamWriter(networkStream);
                sw.AutoFlush = true;

                while (this._sharedState.ContinueProcess)
                {
                    if ((DateTime.Now - this.LastPing).TotalMilliseconds > 30000)
                    {
                        break;
                    }

                    //if we have pending message in the queue, send it here
                    if (this._messageQueue.Count != 0)
                    {
                        var message = this._messageQueue.Dequeue();
                        sw.WriteLine(message);
                    }

                    bytes = new byte[this.ClientSocket.ReceiveBufferSize];
                    try
                    {
                        data = sr.ReadLine();

                        if (data != null)
                        {
                            // Show the data on the console.
                            Debug.WriteLine("Text received : {0}", data);

                            //if we got a PING message, reply with PONG. This is the heartbeat function.
                            if (data == "PING")
                            {
                                sw.WriteLine("PONG");
                                this.LastPing = DateTime.Now;
                            }
                            else
                            {
                                this.RaiseMessageReceive(data);
                            }
                        }
                    }
                    catch (IOException ex)
                    {
                        Debug.WriteLine("StealME.Message: Timeout");
                    } // Timeout
                    catch (SocketException)
                    {
                        Debug.WriteLine("Connection is broken!");
                        break;
                    }
                }
                networkStream.Close();
                this.ClientSocket.Close();
            }

            this.RaiseDisconnected();

            // Signal main process if this is the last Client connections main thread requested to stop.
            if (!this._sharedState.ContinueProcess && this._sharedState.NumberOfClients == 0) this._sharedState.Ev.Set();
        }

        public void EnqueueMessage(string message)
        {
            this._messageQueue.Enqueue(message);
        }

        #endregion
    } 
}