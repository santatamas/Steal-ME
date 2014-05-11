using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using StealME.Server.Networking.EventArgs;
using StealME.Server.Networking.Helpers;

namespace StealME.Server.Networking.Async
{
    public class SocketListener
    {
        #region Construction
        // Constructor.
        public SocketListener(NetworkSettings theNetworkSettings)
        {
            this._networkSettings = theNetworkSettings;
            //Allocate memory for buffers. We are using a separate buffer space for
            //receive and send, instead of sharing the buffer space, like the Microsoft
            //example does.            
            this._theBufferManager = new BufferManager(this._networkSettings.ReceiveBufferSize * this._networkSettings.NumberOfSaeaForRecSend * this._networkSettings.OpsToPreAllocate,
            this._networkSettings.ReceiveBufferSize);

            this._poolOfReceiveEventArgs = new SocketAsyncEventArgsPool(this._networkSettings.NumberOfSaeaForRecSend);
            this._poolOfSendEventArgs = new SocketAsyncEventArgsPool(this._networkSettings.NumberOfSaeaForRecSend);
            this._poolOfAcceptEventArgs = new SocketAsyncEventArgsPool(this._networkSettings.MaxSimultaneousAcceptOps);

            // Create connections count enforcer
            this._theMaxConnectionsEnforcer = new Semaphore(this._networkSettings.MaxConnections, this._networkSettings.MaxConnections);

            ActiveChannels = new ObservableCollection<AsyncTransportChannel>();

            //Microsoft's example called these from Main method, which you 
            //can easily do if you wish.
            Init();
            StartListen();
        }

        // initializes the server by preallocating reusable buffers and 
        // context objects (SocketAsyncEventArgs objects).  
        // It is NOT mandatory that you preallocate them or reuse them. But, but it is 
        // done this way to illustrate how the API can 
        // easily be used to create reusable objects to increase server performance.
        internal void Init()
        {
            // Allocate one large byte buffer block, which all I/O operations will 
            //use a piece of. This gaurds against memory fragmentation.
            this._theBufferManager.InitBuffer();

            // preallocate pool of SocketAsyncEventArgs objects for accept operations           
            for (Int32 i = 0; i < this._networkSettings.MaxSimultaneousAcceptOps; i++)
            {
                // add SocketAsyncEventArg to the pool
                this._poolOfAcceptEventArgs.Push(CreateNewSaeaForAccept(_poolOfAcceptEventArgs));
            }

            //The pool that we built ABOVE is for SocketAsyncEventArgs objects that do
            // accept operations. 
            //Now we will build a separate pool for SAEAs objects 
            //that do receive/send operations. One reason to separate them is that accept
            //operations do NOT need a buffer, but receive/send operations do. 
            //ReceiveAsync and SendAsync require
            //a parameter for buffer size in SocketAsyncEventArgs.Buffer.
            // So, create pool of SAEA objects for receive/send operations.
            SocketAsyncEventArgs eventArgObjectForPool;

            Int32 tokenId;

            for (Int32 i = 0; i < this._networkSettings.NumberOfSaeaForRecSend * 2; i++)
            {
                //Allocate the SocketAsyncEventArgs object for this loop, 
                //to go in its place in the stack which will be the pool
                //for receive/send operation context objects.
                eventArgObjectForPool = new SocketAsyncEventArgs();

                // assign a byte buffer from the buffer block to 
                //this particular SocketAsyncEventArg object
                
                if(!this._theBufferManager.SetBuffer(eventArgObjectForPool))
                {
                    Debugger.Break();
                }

                //tokenId = _poolOfReceiveEventArgs.AssignTokenId() + 1000000;

                //Attach the SocketAsyncEventArgs object
                //to its event handler. Since this SocketAsyncEventArgs object is 
                //used for both receive and send operations, whenever either of those 
                //completes, the IO_Completed method will be called.
                //eventArgObjectForPool.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);

                //We can store data in the UserToken property of SAEA object.
                var theTempReceiveSendUserToken = new TransactionStateToken(
                    eventArgObjectForPool, 
                    eventArgObjectForPool.Offset, 
                    eventArgObjectForPool.Offset + this._networkSettings.ReceiveBufferSize, 
                    this._networkSettings.ReceivePrefixLength, 
                    this._networkSettings.SendPrefixLength, 0);

                //We'll have an object that we call DataHolder, that we can remove from
                //the UserToken when we are finished with it. So, we can hang on to the
                //DataHolder, pass it to an app, serialize it, or whatever.
                //theTempReceiveSendUserToken.CreateNewDataHolder();

                eventArgObjectForPool.UserToken = theTempReceiveSendUserToken;

                if(i % 2 == 0)
                {
                    // add this SocketAsyncEventArg object to the pool.
                    this._poolOfReceiveEventArgs.Push(eventArgObjectForPool);
                }
                else
                {
                    // add this SocketAsyncEventArg object to the pool.
                    this._poolOfSendEventArgs.Push(eventArgObjectForPool);
                }
                
            }
        }
        #endregion

        #region Private State

        public ObservableCollection<AsyncTransportChannel> ActiveChannels { get; set; }
        BufferManager _theBufferManager;

        // the socket used to listen for incoming connection requests
        Socket _listenSocket;

        // A Semaphore has two parameters, the initial number of available slots
        // and the maximum number of slots. We'll make them the same. 
        // This Semaphore is used to keep from going over max connection #.
        Semaphore _theMaxConnectionsEnforcer;

        NetworkSettings _networkSettings;

        // pool of reusable SocketAsyncEventArgs objects for accept operations
        SocketAsyncEventArgsPool _poolOfAcceptEventArgs;
        // pool of reusable SocketAsyncEventArgs objects for receive and send socket operations
        SocketAsyncEventArgsPool _poolOfReceiveEventArgs;
        SocketAsyncEventArgsPool _poolOfSendEventArgs;

        #endregion

        #region Accept
        // This method is called when we need to create a new SAEA object to do
        // accept operations. The reason to put it in a separate method is so that
        // we can easily add more objects to the pool if we need to.
        // You can do that if you do NOT use a buffer in the SAEA object that does
        // the accept operations.
        internal SocketAsyncEventArgs CreateNewSaeaForAccept(SocketAsyncEventArgsPool pool)
        {
            //Allocate the SocketAsyncEventArgs object. 
            SocketAsyncEventArgs acceptEventArg = new SocketAsyncEventArgs();

            //SocketAsyncEventArgs.Completed is an event, (the only event,) 
            //declared in the SocketAsyncEventArgs class.
            //See http://msdn.microsoft.com/en-us/library/system.net.sockets.socketasynceventargs.completed.aspx.
            //An event handler should be attached to the event within 
            //a SocketAsyncEventArgs instance when an asynchronous socket 
            //operation is initiated, otherwise the application will not be able 
            //to determine when the operation completes.
            //Attach the event handler, which causes the calling of the 
            //AcceptEventArg_Completed object when the accept op completes.
            acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);

            return acceptEventArg;

            // accept operations do NOT need a buffer.                
            // You can see that is true by looking at the
            // methods in the .NET Socket class on the Microsoft website. AcceptAsync does
            // not take require a parameter for buffer size.
        }

        // This method starts the socket server such that it is listening for 
        // incoming connection requests.            
        internal void StartListen()
        {
            // create the socket which listens for incoming connections
            _listenSocket = new Socket(this._networkSettings.LocalEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            //bind it to the port
            _listenSocket.Bind(this._networkSettings.LocalEndPoint);

            // Start the listener with a backlog of however many connections.
            //"backlog" means pending connections. 
            //The backlog number is the number of clients that can wait for a
            //SocketAsyncEventArg object that will do an accept operation.
            //The listening socket keeps the backlog as a queue. The backlog allows 
            //for a certain # of excess clients waiting to be connected.
            //If the backlog is maxed out, then the client will receive an error when
            //trying to connect.
            //max # for backlog can be limited by the operating system.
            _listenSocket.Listen(this._networkSettings.Backlog);

            Console.WriteLine("\r\n\r\n*************************\r\n** Server is listening **\r\n*************************\r\n\r\n");

            // Calls the method which will post accepts on the listening socket.            
            // This call just occurs one time from this StartListen method. 
            // After that the StartAccept method will be called in a loop.
            StartAccept();
        }

        // Begins an operation to accept a conne6ction request from the client         
        internal void StartAccept()
        {
            SocketAsyncEventArgs acceptEventArg;

            //Get a SocketAsyncEventArgs object to accept the connection.                        
            //Get it from the pool if there is more than one in the pool.
            //We could use zero as bottom, but one is a little safer.            
            acceptEventArg = this._poolOfAcceptEventArgs.Count > 1 ? this._poolOfAcceptEventArgs.Pop() : CreateNewSaeaForAccept(_poolOfAcceptEventArgs);

            this._theMaxConnectionsEnforcer.WaitOne();

            // Socket.AcceptAsync begins asynchronous operation to accept the connection.
            // Note the listening socket will pass info to the SocketAsyncEventArgs
            // object that has the Socket that does the accept operation.
            // If you do not create a Socket object and put it in the SAEA object
            // before calling AcceptAsync and use the AcceptSocket property to get it,
            // then a new Socket object will be created for you by .NET.            
            bool willRaiseEvent = _listenSocket.AcceptAsync(acceptEventArg);

            // Socket.AcceptAsync returns true if the I/O operation is pending, i.e. is 
            // working asynchronously. The 
            // SocketAsyncEventArgs.Completed event on the acceptEventArg parameter 
            // will be raised upon completion of accept op.
            // AcceptAsync will call the AcceptEventArg_Completed
            // method when it completes, because when we created this SocketAsyncEventArgs
            // object before putting it in the pool, we set the event handler to do it.
            // AcceptAsync returns false if the I/O operation completed synchronously.            
            // The SocketAsyncEventArgs.Completed event on the acceptEventArg 
            // parameter will NOT be raised when AcceptAsync returns false.
            if (!willRaiseEvent)
            {
                // The code in this if (!willRaiseEvent) statement only runs 
                // when the operation was completed synchronously. It is needed because 
                // when Socket.AcceptAsync returns false, 
                // it does NOT raise the SocketAsyncEventArgs.Completed event.
                // And we need to call ProcessAccept and pass it the SAEA object.
                // This is only when a new connection is being accepted.
                // Probably only relevant in the case of a socket error.
                ProcessAccept(acceptEventArg);
            }
        }

        // This method is the callback method associated with Socket.AcceptAsync 
        // operations and is invoked when an async accept operation completes.
        // This is only when a new connection is being accepted.
        // Notice that Socket.AcceptAsync is returning a value of true, and
        // raising the Completed event when the AcceptAsync method completes.
        private void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            //Any code that you put in this method will NOT be called if
            //the operation completes synchronously, which will probably happen when
            //there is some kind of socket error. It might be better to put the code
            //in the ProcessAccept method.

            ProcessAccept(e);
        }

        //The e parameter passed from the AcceptEventArg_Completed method
        //represents the SocketAsyncEventArgs object that did
        //the accept operation. in this method we'll do the handoff from it to the 
        //SocketAsyncEventArgs object that will do receive/send.
        private void ProcessAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            // This is when there was an error with the accept op. That should NOT
            // be happening often. It could indicate that there is a problem with
            // that socket. If there is a problem, then we would have an infinite
            // loop here, if we tried to reuse that same socket.
            if (acceptEventArgs.SocketError != SocketError.Success)
            {
                // Loop back to post another accept op. Notice that we are NOT
                // passing the SAEA object here.
                StartAccept();

                //Let's destroy this socket, since it could be bad.
                HandleBadAccept(acceptEventArgs);

                //Jump out of the method.
                return;
            }

            // todo:increment connected socket variable

            //Now that the accept operation completed, we can start another
            //accept operation, which will do the same. Notice that we are NOT
            //passing the SAEA object here.
            StartAccept();

            // Get a SocketAsyncEventArgs object from the pool of receive/send op 
            // SocketAsyncEventArgs objects
            SocketAsyncEventArgs receiveEventArgs = this._poolOfReceiveEventArgs.Pop();
            SocketAsyncEventArgs sendEventArgs = this._poolOfSendEventArgs.Pop();

            //Create sessionId in UserToken.
            //todo:investigate
            //((TransactionStateToken)receiveSendEventArgs.UserToken).CreateSessionId();

            //A new socket was created by the AcceptAsync method. The 
            //SocketAsyncEventArgs object which did the accept operation has that 
            //socket info in its AcceptSocket property. Now we will give
            //a reference for that socket to the SocketAsyncEventArgs 
            //object which will do receive/send.
            receiveEventArgs.AcceptSocket = acceptEventArgs.AcceptSocket;
            sendEventArgs.AcceptSocket = acceptEventArgs.AcceptSocket;

            //We have handed off the connection info from the
            //accepting socket to the receiving socket. So, now we can
            //put the SocketAsyncEventArgs object that did the accept operation 
            //back in the pool for them. But first we will clear 
            //the socket info from that object, so it will be 
            //ready for a new socket when it comes out of the pool.
            acceptEventArgs.AcceptSocket = null;
            this._poolOfAcceptEventArgs.Push(acceptEventArgs);

            // Create a new session with the preconfigured SocketAsyncEventArgs object
            AsyncTransportChannel connectionAsyncTransportChannel = new AsyncTransportChannel(receiveEventArgs, sendEventArgs, _networkSettings);

            // Add session to active session collection
            ActiveChannels.Add(connectionAsyncTransportChannel);

            connectionAsyncTransportChannel.Closed += new EventHandler<TransportChannelClosedEventArgs>(TransportChannelClosed);

            // Ready to accept a new connection
            StartAccept();
        }
        #endregion

        #region Cleanup/Close

        void TransportChannelClosed(object sender, TransportChannelClosedEventArgs e)
        {
            ((AsyncTransportChannel)sender).Closed -= TransportChannelClosed;

            // Put the SocketAsyncEventArg back into the pool,
            // to be used by another client. This 
            this._poolOfReceiveEventArgs.Push(e.ReceiveEventArgs);
            this._poolOfSendEventArgs.Push(e.SendEventArgs);

            // decrement the counter keeping track of the total number of clients 
            //connected to the server, for testing
            //todo: decrease connected socket number variable

            //Release Semaphore so that its connection counter will be decremented.
            //This must be done AFTER putting the SocketAsyncEventArg back into the pool,
            //or you can run into problems.
            this._theMaxConnectionsEnforcer.Release();
            try
            {
                // Remove session from active sessions
                ActiveChannels.Remove((AsyncTransportChannel)sender);
            }
            catch (Exception)
            {

            }
        }

        private void HandleBadAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            //This method closes the socket and releases all resources, both
            //managed and unmanaged. It internally calls Dispose.           
            acceptEventArgs.AcceptSocket.Close();

            //Put the SAEA back in the pool.
            _poolOfAcceptEventArgs.Push(acceptEventArgs);
        }

        internal void CleanUpOnExit()
        {
            DisposeAllSaeaObjects();
        }

        private void DisposeAllSaeaObjects()
        {
            SocketAsyncEventArgs eventArgs;
            while (this._poolOfAcceptEventArgs.Count > 0)
            {
                eventArgs = _poolOfAcceptEventArgs.Pop();
                eventArgs.Dispose();
            }
            while (this._poolOfReceiveEventArgs.Count > 0)
            {
                eventArgs = _poolOfReceiveEventArgs.Pop();
                eventArgs.Dispose();
            }
            while (this._poolOfSendEventArgs.Count > 0)
            {
                eventArgs = _poolOfSendEventArgs.Pop();
                eventArgs.Dispose();
            }
        }
        #endregion
    }
}