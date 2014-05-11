using System;
using System.Net.Sockets;

namespace StealME.Server.Networking.Async
{
    class TransactionStateToken
    {
        internal readonly Int32 bufferOffsetReceive;
        internal readonly Int32 bufferOffsetSend;
        internal readonly Int32 receivePrefixLength;

        internal Int32 sendBytesRemainingCount;
        internal readonly Int32 sendPrefixLength;
        internal Byte[] dataToSend;
        internal Int32 bytesSentAlreadyCount;

        private Int32 sessionId;

        public TransactionStateToken(SocketAsyncEventArgs e, Int32 rOffset, Int32 sOffset, Int32 receivePrefixLength, Int32 sendPrefixLength, Int32 identifier)
        {
            this.bufferOffsetReceive = rOffset;
            this.bufferOffsetSend = sOffset;
            this.receivePrefixLength = receivePrefixLength;
            this.sendPrefixLength = sendPrefixLength;
        }

        public Int32 SessionId
        {
            get
            {
                return this.sessionId;
            }
        }
    }
}
