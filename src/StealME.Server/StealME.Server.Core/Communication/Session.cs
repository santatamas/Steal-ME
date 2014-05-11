using System;
using System.Diagnostics;
using StealME.Networking.Protocol;
using StealME.Server.Messaging;
using StealME.Server.Messaging.Exceptions;
using StealME.Server.Messaging.Interfaces;
using StealME.Server.Messaging.Requests;
using StealME.Server.Messaging.Responses;
using StealME.Server.Networking.Async;
using StealME.Server.Networking.EventArgs;
using StealME.Server.Data.DAL;
using GetRequest = StealME.Server.Messaging.Requests.GetRequest;

namespace StealME.Server.Core.Communication
{
    public class Session
    {
        private ITransportChannel _channel;
        private MessageProcessor _msgProc;
        private IProtocol _protocol;
        private Tracker _tracker;
        private bool _isAuthenticated;

        public Tracker Tracker
        {
            get { return _tracker; }
            set { _tracker = value; }
        }
        public bool IsAuthenticated
        {
            get { return _isAuthenticated; }
            set { _isAuthenticated = value; }
        }

        public ITransportChannel Channel
        {
            get { return _channel; }
        }

        public Session(ITransportChannel channel)
        {
            _channel = channel;
            Channel.Closed += _channel_Closed;
            _msgProc = new MessageProcessor(Channel, new MsgPackSerializer(new TypeResolver()));
            _msgProc.MessageReceived += msgProc_MessageReceived;

            // Get Protocol version from Tracker
            _msgProc.Send(new GetRequest { GetTypeId = 2 });
        }

        private void msgProc_MessageReceived(object sender, MessageEventArgs e)
        {
            try
            {
                // not really optimal, better use a hashtable here - with integer keys
                switch (e.Message.GetType().Name)
                {
                    case "TestRequest":
                        var testRequest = (TestRequest)e.Message;
                        _msgProc.Send(new TestResponse { Message = "Well hello to you too!", Number = testRequest.Number });
                        break;
                    case "ProtocolVersionResponse":
                        var protocolVersion = (ProtocolVersionResponse)e.Message;
                        // todo: use factory pattern here to get the proper protocol version
                        _protocol = new ProtocolBase(this);
                        _msgProc.Send(new ACKResponse(protocolVersion.Id));
                        break;
                    default:
                        // if it's not handled by the session, pass it to the Protocol
                        _protocol.ProcessMessage(e.Message);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            
        }
        private void _channel_Closed(object sender, TransportChannelClosedEventArgs e)
        {
            Channel.Closed -= new EventHandler<TransportChannelClosedEventArgs>(_channel_Closed);
            Terminate();
        }

        public void Terminate()
        {
            // todo: mark Tracker OffLine in DB

            if(_msgProc != null)
                _msgProc.MessageReceived -= msgProc_MessageReceived;

            Channel.TerminateChannel();
            _msgProc = null;
            _channel = null;
            _protocol = null;
            _tracker = null;
        }

        public void SendMessage(object message)
        {
            if (_protocol.IsKnownMessageType(message))
            {
                _msgProc.Send(message);
            }
            else
            {
                throw new UnknownMessageException();
            }
        }
    }
}
