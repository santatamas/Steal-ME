using System;
using System.Net.Sockets;
using StealME.Networking.Protocol;
using StealME.Server.Networking.EventArgs;
using StealME.Server.Networking.Helpers;

namespace StealME.Server.Networking.Async
{
    public class AsyncTransportChannel : IDisposable, ITransportChannel
    {
        #region Properties
        public int Id { get; private set; }
        #endregion

        #region Events
        public event EventHandler<TransportChannelClosedEventArgs> Closed;
        public event EventHandler Opened;
        public event EventHandler<RawMessageEventArgs> MessageReceived;
        public event EventHandler MessageSent;
        #endregion

        #region Construction
        public AsyncTransportChannel(SocketAsyncEventArgs receiveEventArgs, SocketAsyncEventArgs sendEventArgs, NetworkSettings settings)
        {
            _receiveSAEA = receiveEventArgs;
            _sendSAEA = sendEventArgs;
            _networkSettings = settings;
            _packetReceiverLogic = new PacketReceiverLogic(settings.BufferSize, settings.ReceivePrefixLength);
            _packetSenderLogic = new PacketSenderLogic(settings.BufferSize, settings.SendPrefixLength);

            Initialize();
        }
        #endregion

        #region Private

        private void Initialize()
        {
            _receiveSAEA.Completed += IO_Completed;
            _sendSAEA.Completed += IO_Completed;

            Random rnd = new Random();
            Id = rnd.Next(10000);

            OnSessionStarted();
            StartReceive(_receiveSAEA);
        }

        private void OnMessageSent()
        {
            EventHandler handler = MessageSent;
            if (handler != null) handler(this, new System.EventArgs());
        }
        private void OnMessageReceived(byte[] message)
        {
            EventHandler<RawMessageEventArgs> handler = MessageReceived;
            if (handler != null) handler(this, new RawMessageEventArgs { Message = message });
        }
        private void OnSessionStarted()
        {
            EventHandler handler = Opened;
            if (handler != null) handler(this, new System.EventArgs());
        }
        private void OnSessionEnded()
        {
            EventHandler<TransportChannelClosedEventArgs> handler = Closed;
            if (handler != null) handler(this, new TransportChannelClosedEventArgs { ReceiveEventArgs = _receiveSAEA, SendEventArgs = _sendSAEA });
        }

        #region Receive
        // Set the receive buffer and post a receive op.
        private void StartReceive(SocketAsyncEventArgs receiveSendEventArgs)
        {
            //TransactionStateToken receiveSendToken = (TransactionStateToken)receiveSendEventArgs.UserToken;

            //Set the buffer for the receive operation.
            //receiveSendEventArgs.SetBuffer(receiveSendToken.bufferOffsetReceive, this._networkSettings.BufferSize);

            try
            {
                // Post async receive operation on the socket.
                bool willRaiseEvent = receiveSendEventArgs.AcceptSocket.ReceiveAsync(receiveSendEventArgs);

                //Socket.ReceiveAsync returns true if the I/O operation is pending. The 
                //SocketAsyncEventArgs.Completed event on the e parameter will be raised 
                //upon completion of the operation. So, true will cause the IO_Completed
                //method to be called when the receive operation completes. 
                //That's because of the event handler we created when building
                //the pool of SocketAsyncEventArgs objects that perform receive/send.
                //It was the line that said
                //eventArgObjectForPool.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);

                //Socket.ReceiveAsync returns false if I/O operation completed synchronously. 
                //In that case, the SocketAsyncEventArgs.Completed event on the e parameter 
                //will not be raised and the e object passed as a parameter may be 
                //examined immediately after the method call 
                //returns to retrieve the result of the operation.
                // It may be false in the case of a socket error.

                if (!willRaiseEvent)
                {
                    //If the op completed synchronously, we need to call ProcessReceive 
                    //method directly. This will probably be used rarely, as you will 
                    //see in testing.
                    ProcessReceive(receiveSendEventArgs);
                }
            }
            catch (ObjectDisposedException ex)
            {
                TerminateChannel();
            }
        }

        // This method is invoked by the IO_Completed method
        // when an asynchronous receive operation completes. 
        // If the remote host closed the connection, then the socket is closed.
        // Otherwise, we process the received data. And if a complete message was
        // received, then we do some additional processing, to 
        // respond to the client.
        private void ProcessReceive(SocketAsyncEventArgs receiveSendEventArgs)
        {
            TransactionStateToken receiveSendToken = (TransactionStateToken)receiveSendEventArgs.UserToken;
            // If there was a socket error, close the connection. This is NOT a normal
            // situation, if you get an error here.
            // In the Microsoft example code they had this error situation handled
            // at the end of ProcessReceive. Putting it here improves readability
            // by reducing nesting some.
            if (receiveSendEventArgs.SocketError != SocketError.Success)
            {
                TerminateChannel();
                //Jump out of the ProcessReceive method.
                return;
            }

            // If no data was received, close the connection. This is a NORMAL
            // situation that shows when the client has finished sending data.
            if (receiveSendEventArgs.BytesTransferred == 0)
            {
                TerminateChannel();
                //StartReceive(receiveSendEventArgs);
                return;
            }

            //The BytesTransferred property tells us how many bytes 
            //we need to process.
            Buffer.BlockCopy(receiveSendEventArgs.Buffer, receiveSendToken.bufferOffsetReceive, _packetReceiverLogic.IncomingDataBuffer, 0, receiveSendEventArgs.BytesTransferred);
            _packetReceiverLogic.TransferredBytesCount = receiveSendEventArgs.BytesTransferred;
            if (_packetReceiverLogic.Process())
            {
                foreach (var packet in _packetReceiverLogic.Output)
                {
                    OnMessageReceived(packet);
                }
            }

            //Console.WriteLine("received");
            StartReceive(receiveSendEventArgs);
        }
        #endregion

        #region Send

        //Post a send.    
        private void StartSend(SocketAsyncEventArgs sendEventArgs)
        {
            _isSendOperationPending = true;

            TransactionStateToken receiveSendToken = (TransactionStateToken)sendEventArgs.UserToken;

            //Set the buffer. You can see on Microsoft's page at 
            //http://msdn.microsoft.com/en-us/library/system.net.sockets.socketasynceventargs.setbuffer.aspx
            //that there are two overloads. One of the overloads has 3 parameters.
            //When setting the buffer, you need 3 parameters the first time you set it,
            //which we did in the Init method. The first of the three parameters
            //tells what byte array to use as the buffer. After we tell what byte array
            //to use we do not need to use the overload with 3 parameters any more.
            //(That is the whole reason for using the buffer block. You keep the same
            //byte array as buffer always, and keep it all in one block.)
            //Now we use the overload with two parameters. We tell 
            // (1) the offset and
            // (2) the number of bytes to use, starting at the offset.

            if (receiveSendToken.sendBytesRemainingCount == 0)
            {
                var messageChunk = _packetSenderLogic.GetNextMessageChunk();
                if (messageChunk.Length < _networkSettings.BufferSize)
                {
                    sendEventArgs.SetBuffer(0, messageChunk.Length);
                    //Copy the bytes to the buffer associated with this SAEA object.
                    Buffer.BlockCopy(
                        messageChunk, 0,
                        sendEventArgs.Buffer, 0,
                        messageChunk.Length);
                    receiveSendToken.sendBytesRemainingCount = messageChunk.Length;
                }
                else
                {
                    sendEventArgs.SetBuffer(0, _networkSettings.BufferSize);
                    //Copy the bytes to the buffer associated with this SAEA object.
                    Buffer.BlockCopy(
                        messageChunk, 0,
                        sendEventArgs.Buffer, 0,
                        _networkSettings.BufferSize);
                    receiveSendToken.sendBytesRemainingCount = _networkSettings.BufferSize;
                }
            }

            //post asynchronous send operation
            try
            {
                bool willRaiseEvent = sendEventArgs.AcceptSocket.SendAsync(sendEventArgs);
                if (!willRaiseEvent)
                {
                    ProcessSend(sendEventArgs);
                }
            }
            catch (ObjectDisposedException ex)
            {
                TerminateChannel();
            }

           
        }

        // This method is called by I/O Completed() when an asynchronous send completes.  
        // If all of the data has been sent, then this method calls StartReceive
        //to start another receive op on the socket to read any additional 
        // data sent from the client. If all of the data has NOT been sent, then it 
        //calls StartSend to send more data.        
        private void ProcessSend(SocketAsyncEventArgs receiveSendEventArgs)
        {
            TransactionStateToken receiveSendToken = (TransactionStateToken)receiveSendEventArgs.UserToken;

            if (receiveSendEventArgs.SocketError == SocketError.Success)
            {
                receiveSendToken.sendBytesRemainingCount -= receiveSendEventArgs.BytesTransferred;

                if (receiveSendToken.sendBytesRemainingCount == 0)
                {
                    OnMessageSent();
                    if (_packetSenderLogic.HasMessageToSend())
                    {
                        StartSend(receiveSendEventArgs);
                    }
                    else
                    {
                        _isSendOperationPending = false;
                    }
                }
            }
            else
            {
                // If we are in this else-statement, there was a socket error.
                // We'll just close the socket if there was a
                // socket error when receiving data from the client.
                TerminateChannel();
            }
        }
        #endregion

        #region Handle IO Completed
        void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            //Any code that you put in this method will NOT be called if
            //the operation completes synchronously, which will probably happen when
            //there is some kind of socket error.

            // determine which type of operation just completed and call the associated handler
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;

                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;

                default:
                    //This exception will occur if you code the Completed event of some
                    //operation to come to this method, by mistake.
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }
        #endregion

        #region Terminate

        /// <summary>
        /// Does the normal destroying of sockets after 
        /// we finish receiving and sending on a connection.
        /// </summary>
        /// <param name="e">The <see cref="SocketAsyncEventArgs" /> instance containing the event data.</param>
        private void CloseClientSocket(SocketAsyncEventArgs e)
        {
            // do a shutdown before you close the socket
            try
            {
                e.AcceptSocket.Shutdown(SocketShutdown.Both);
            }
            catch (SocketException ex)
            {
                //todo: proper exception handling
            }
            catch (ObjectDisposedException ex)
            {
                //todo: proper exception handling
            }

            //This method closes the socket and releases all resources, both
            //managed and unmanaged. It internally calls Dispose.
            try
            {
                e.AcceptSocket.Close();
            }
            catch (Exception)
            {

            }

        }
        #endregion

        #region Private State
        private bool _isSendOperationPending;
        private SocketAsyncEventArgs _receiveSAEA;
        private SocketAsyncEventArgs _sendSAEA;
        private NetworkSettings _networkSettings;
        private PacketReceiverLogic _packetReceiverLogic;
        private PacketSenderLogic _packetSenderLogic;
        #endregion
        #endregion

        object lockObject = new object();

        #region Public
        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>True, if the async message sending began successfully, false if an async sending process is still pending</returns>
        public void Send(byte[] message)
        {
            lock (lockObject)
            {
                if (_isSendOperationPending)
                {
                    _packetSenderLogic.QueueMessage(message);
                }
                else
                {
                    _isSendOperationPending = true;
                    _packetSenderLogic.QueueMessage(message);
                    StartSend(_sendSAEA);
                }
            }          
        }
        public void Dispose()
        {
            TerminateChannel();
        }

        public void TerminateChannel()
        {
            _receiveSAEA.Completed -= IO_Completed;
            _sendSAEA.Completed -= IO_Completed;
            CloseClientSocket(_receiveSAEA);
            OnSessionEnded();
        }
        #endregion
    }
}