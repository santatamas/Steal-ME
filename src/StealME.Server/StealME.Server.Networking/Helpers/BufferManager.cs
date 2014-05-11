using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace StealME.Server.Networking.Helpers
{
    /// <summary>
    /// This class creates a single large buffer which can be divided up 
    /// and assigned to SocketAsyncEventArgs objects for use with each 
    /// socket I/O operation.  
    /// This enables buffers to be easily reused and guards against 
    /// fragmenting heap memory.
    /// This buffer is a byte array which the Windows TCP buffer can copy its data to.
    /// </summary>
    internal class BufferManager
    {
        /// <summary>
        /// The total number of bytes controlled by the buffer pool
        /// </summary>
        private Int32 _totalBytesInBufferBlock;

        // Byte array maintained by the Buffer Manager.
        private byte[] _bufferBlock;
        private Stack<int> _freeIndexPool;
        private Int32 _currentIndex;
        private Int32 _bufferBytesAllocatedForEachSaea;

        public BufferManager(Int32 totalBytes, Int32 totalBufferBytesInEachSaeaObject)
        {
            _totalBytesInBufferBlock = totalBytes;
            this._currentIndex = 0;
            this._bufferBytesAllocatedForEachSaea = totalBufferBytesInEachSaeaObject;
            this._freeIndexPool = new Stack<int>();
        }

        // Allocates buffer space used by the buffer pool
        internal void InitBuffer()
        {
            // Create one large buffer block.
            this._bufferBlock = new byte[_totalBytesInBufferBlock];
        }

        // Divide that one large buffer block out to each SocketAsyncEventArg object.
        // Assign a buffer space from the buffer block to the 
        // specified SocketAsyncEventArgs object.
        // returns true if the buffer was successfully set, else false
        internal bool SetBuffer(SocketAsyncEventArgs args)
        {
            if (this._freeIndexPool.Count > 0)
            {
                //This if-statement is only true if you have called the FreeBuffer
                //method previously, which would put an offset for a buffer space 
                //back into this stack.
                args.SetBuffer(this._bufferBlock, this._freeIndexPool.Pop(), this._bufferBytesAllocatedForEachSaea);
            }
            else
            {
                //Inside this else-statement is the code that is used to set the 
                //buffer for each SAEA object when the pool of SAEA objects is built
                //in the Init method.
                if ((_totalBytesInBufferBlock - this._bufferBytesAllocatedForEachSaea) < this._currentIndex)
                {
                    return false;
                }
                args.SetBuffer(this._bufferBlock, this._currentIndex, this._bufferBytesAllocatedForEachSaea);
                this._currentIndex += this._bufferBytesAllocatedForEachSaea;
            }

            return true;
        }

        // Removes the buffer from a SocketAsyncEventArg object.   This frees the
        // buffer back to the buffer pool. Try NOT to use the FreeBuffer method,
        // unless you need to destroy the SAEA object, or maybe in the case
        // of some exception handling. Instead, on the server
        // keep the same buffer space assigned to one SAEA object for the duration of
        // this app's running.
        internal void FreeBuffer(SocketAsyncEventArgs args)
        {
            this._freeIndexPool.Push(args.Offset);
            args.SetBuffer(null, 0, 0);

        }
    }
}