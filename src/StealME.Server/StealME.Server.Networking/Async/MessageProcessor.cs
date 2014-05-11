using System;
using System.Threading;
using StealME.Networking.Protocol;
using System.Collections.Concurrent;
using StealME.Server.Networking.EventArgs;

namespace StealME.Server.Networking.Async
{
    public class MessageProcessor
    {
        private IMessageSerializer _serializer;
        private ITransportChannel _transportChannel;
        private ITypeResolver _typeResolver;
        private ConcurrentQueue<object> _messagesToSend;

        public event EventHandler TransportChannelClosed;
        public event EventHandler<MessageEventArgs> MessageReceived;

        public MessageProcessor(ITransportChannel channel, IMessageSerializer serializer)
        {
            _serializer = serializer;
            _transportChannel = channel;
            _messagesToSend = new ConcurrentQueue<object>();

            _transportChannel.MessageReceived += new System.EventHandler<RawMessageEventArgs>(_transportChannel_MessageReceived);
            _transportChannel.Closed += new System.EventHandler<TransportChannelClosedEventArgs>(_transportChannel_Closed);
        }

        public void Send(object message)
        {
            _messagesToSend.Enqueue(message);
            ThreadPool.QueueUserWorkItem(SerializeAndSend);
        }

        private void SerializeAndSend(object arg)
        {
            object messageToSend;
            if (_messagesToSend.TryDequeue(out messageToSend))
                _transportChannel.Send(_serializer.Serialize(messageToSend));
        }
        private void DeserializeAndSignal(object arg)
        {
            object message = _serializer.Deserialize((byte[])arg);
            OnMessageReceived(message);
        }

        private void OnTransportChannelClosed()
        {
            EventHandler handler = TransportChannelClosed;
            if (handler != null) handler(this, new System.EventArgs());
        }
        private void OnMessageReceived(object message)
        {
            EventHandler<MessageEventArgs> handler = MessageReceived;
            if (handler != null) handler(this, new MessageEventArgs() { Message = message });
        }

        private void _transportChannel_Closed(object sender, TransportChannelClosedEventArgs e)
        {
            _transportChannel.MessageReceived -= _transportChannel_MessageReceived;
            _transportChannel.Closed -= _transportChannel_Closed;

            OnTransportChannelClosed();
        }
        private void _transportChannel_MessageReceived(object sender, RawMessageEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(DeserializeAndSignal, e.Message);
        }
    }
}
