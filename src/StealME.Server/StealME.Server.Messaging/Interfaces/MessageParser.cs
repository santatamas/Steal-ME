namespace StealME.Server.Messaging.Protocol
{
    using System;
    using System.Diagnostics;

    using StealME.Server.Networking.Protocol;

    public class MessageParser
    {
        IMessageListener _listener;

        public MessageParser(IMessageListener listener)
        {
            this._listener = listener;
        }

        public void processMessage(String message)
        {
            try
            {
                //TODO:regex validation
                String prefix = message.Substring(0, 3).ToUpper();
                String instruction = message.Substring(4, message.Length - 4).ToUpper();

                switch (prefix)
                {
                    case PC.COMMAND_PREFIX:
                        this.DispatchCommand(instruction);
                        break;
                    case PC.SET_PREFIX:
                        this.DispatchSetRequest(instruction);
                        break;
                    case PC.GET_PREFIX:
                        this.DispatchGetRequest(instruction);
                        break;
                    case PC.MESSAGE_PREFIX:
                        this.DispatchMessage(instruction);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Malformed message:" + message);
                Debug.WriteLine("StealME.Exception" + ex.ToString());
            }
        }

        public static bool IsTcpLoc(string message)
        {
            return message.Split(';').Length == 8;
        }

        public static TcpLoc ParseTcpLoc(string message)
        {
            var splittedMessage = message.Split(';');
            return new TcpLoc
                {
                    Latitude = splittedMessage[0],
                    Longitude = splittedMessage[1],
                    MNC = splittedMessage[2],
                    MCC = splittedMessage[3],
                    LAC = splittedMessage[4],
                    CellID = splittedMessage[5],
                    LatestRssi = splittedMessage[6]
                };
        }

        private void DispatchMessage(String instruction)
        {

            string[] param = instruction.Split('=');

            switch(param[0])
            {
                case PC.MESSAGE_STATUS:
                    this._listener.onMessage(Message.Status, param[1]);
                    break;
                case PC.MESSAGE_LOCATION:
                    this._listener.onMessage(Message.Location, param[1]);
                    break;
                case PC.MESSAGE_STATE:
                    this._listener.onMessage(Message.State, param[1]);
                    break;
                case PC.MESSAGE_TCP_LOCATION:
                    this._listener.onMessage(Message.TcpLoc, ParseTcpLoc(param[1]));
                    break;
                case PC.MESSAGE_IMEI:
                    this._listener.onMessage(Message.IMEI, param[1]);
                    break;
            }
        }
        private void DispatchGetRequest(String instruction)
        {
            if (instruction.Equals(PC.GET_STATUS))
            {
                this._listener.onGetRequest(GetRequest.Status);
            }
        }
        private void DispatchSetRequest(String instruction)
        {
            //set request is always in form of "key=value"		
            string[] param = instruction.Split('=');

            switch(param[0])
            {
                case PC.SET_MODE:
                    this._listener.onSetRequest(SetRequest.Mode, param[1]);
                    break;
                case PC.SET_INTERVAL:
                    this._listener.onSetRequest(SetRequest.Interval, param[1]);
                    break;
            }
        }
        private void DispatchCommand(String instruction)
        {
            switch(instruction)
            {
                case PC.COMMAND_ACTIVATE:
                    this._listener.onCommandReceived(Command.Activate);
                    break;
                case PC.COMMAND_DEACTIVATE:
                    this._listener.onCommandReceived(Command.Deactivate);
                    break;
                case PC.COMMAND_SIGNAL:
                    this._listener.onCommandReceived(Command.Signal);
                    break;
            }
        }
    }
}
