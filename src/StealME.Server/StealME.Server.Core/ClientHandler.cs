using StealME.Networking.EventArgs;

namespace StealME.Server.Core
{
    using System;
    using System.ComponentModel;
    using System.Threading;

    using StealME.Server.Core.BLL;
    using StealME.Server.Data.DAL;
    using StealME.Server.Messaging.Protocol;
    using StealME.Server.Networking.EventArgs;
    using StealME.Server.Networking.Tcp;

    public class ClientHandler : IMessageListener, INotifyPropertyChanged
    {
        #region Construction
        public ClientHandler(ConnectionHandler connHandler)
        {
            this._connHandler = connHandler;
            this.Connection.Disconnected += new EventHandler(this.connHandler_Disconnected);
            this.Connection.MessageReceived += new EventHandler<Networking.EventArgs.MessageEventArgs>(this.connHandler_MessageReceived);

            this._messageParser = new MessageParser(this);

            this.Connection.EnqueueMessage(PC.GET_PREFIX + "." + GetRequest.IMEI);
            SMLogger.LogThis("ClientHandler Initialized...requesting IMEI.");
        }
        #endregion

        #region Properties
        public Tracker Tracker
        {
            get
            {
                return this._tracker;
            }
            private set
            {
                this._tracker = value;
                this.OnPropertyChanged("Tracker");
            }
        }

        public ConnectionHandler Connection
        {
            get
            {
                return this._connHandler;
            }
        }

        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler Authenticated;
        public event EventHandler Disconnected;
        public event EventHandler<MessageEventArgs> MessageReceived;
        #endregion

        #region Private

        private void InsertPosition(TcpLoc location)
        {
            if (!this.IsAuthenticated())
                return;

            PositionLogic.InsertPosition(this._tracker,
                    new Position
                    {
                        Latitude = location.Latitude,
                        Longtitude = location.Longitude,
                        CellId = location.CellID,
                        Mcc = location.MCC,
                        Mnc = location.MNC,
                        Rssi = location.LatestRssi
                    });
        }

        private void connHandler_MessageReceived(object sender, MessageEventArgs e)
        {
            SMLogger.LogThis("Message received:" + e.Message);
            //this._messageParser.processMessage(e.Message);
            this.OnMessageReceived(e);
        }
        private void connHandler_Disconnected(object sender, EventArgs e)
        {
            this.OnDisconnected(new EventArgs());
            TrackerLogic.SetTrackerOffLine(this.Tracker);
            this._processMessages = false;
        }

        private void OnMessageReceived(MessageEventArgs e)
        {
            EventHandler<MessageEventArgs> handler = this.MessageReceived;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        private void OnDisconnected(EventArgs e)
        {
            EventHandler handler = this.Disconnected;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        private void OnAuthenticated(EventArgs e)
        {
            EventHandler handler = this.Authenticated;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void StartMessagePolling()
        {
            Thread pollingThread = new Thread(new ThreadStart(this.ProcessMessages));
            pollingThread.Name = "PollingThread:" + this.Tracker.Name;
            pollingThread.Start();
        }

        private void ProcessMessages()
        {
            while (this._processMessages)
            {
                var pendingCommands = MessageQueue.GetPendingCommands(this.Tracker.Id);
                foreach (string command in pendingCommands)
                {
                    this._connHandler.EnqueueMessage(command);
                }
                SMLogger.LogThis("Processed " + pendingCommands.Length + " messages.");
                Thread.Sleep(1000);
            }
        }

        #region Private State
        private Tracker _tracker;
        private MessageParser _messageParser;
        private ConnectionHandler _connHandler;
        private bool _processMessages = true;
        #endregion
        #endregion

        #region Protected
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region Public

        #region IMessageListener implementation
        public void onCommandReceived(Messaging.Protocol.Command signal)
        {

        }
        public void onSetRequest(SetRequest interval, object o)
        {

        }
        public void onMessage(Networking.Protocol.Message status, object o)
        {
            switch (status)
            {
                case Networking.Protocol.Message.IMEI:
                    this.Tracker = TrackerLogic.GetTracker((string)o);
                    if (this.IsAuthenticated())
                    {
                        this.OnAuthenticated(new EventArgs());
                        SMLogger.LogThis("Client Authenticated.");
                        TrackerLogic.SetTrackerOnLine(this.Tracker);
                        this.StartMessagePolling();
                    }
                    break;
                case Networking.Protocol.Message.TcpLoc:
                    this.InsertPosition((TcpLoc)o);
                    break;
            }
        }
        public void onGetRequest(GetRequest status)
        {

        }
        #endregion

        public string GetIPAddress()
        {
            return this.Connection.ClientSocket.Client.RemoteEndPoint.ToString();
        }

        public void EnqueueMessage(string message)
        {
            this.Connection.EnqueueMessage(message);
        }

        public bool IsAuthenticated()
        {
            return this._tracker != null;
        }

        #endregion
    }
}