using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace StealME.Networking.Protocol
{
    public class PacketSenderLogic
    {
        private int _prefixSize;
        private int _bufferSize;
        private ConcurrentQueue<byte[]> _messagesToSend;
        private byte[] _currentMessage;
        private byte[] _currentPrefix;
        private int _currentMessageArrayIndex;

        public PacketSenderLogic(int bufferSize, int prefixSize)
        {
            _prefixSize = prefixSize;
            _bufferSize = bufferSize;

            if (_prefixSize > _bufferSize)
                throw new Exception("Prefix size cannot be bigger than buffer size!");

            _messagesToSend = new ConcurrentQueue<byte[]>();
        }

        public void QueueMessage(byte[] msg)
        {
            _messagesToSend.Enqueue(msg);
        }

        public bool HasMessageToSend()
        {
            return _messagesToSend.Count != 0 || _currentMessage != null;
        }

        public byte[] GetNextMessageChunk()
        {
            byte[] result = null;

            #region New message
            // if we begin to process a new message
            if (_currentMessage == null)
            {
                //if no message left to process
                if (_messagesToSend.Count == 0) return result;

                // get the next message
                if (_messagesToSend.TryDequeue(out _currentMessage))
                {
                    _currentPrefix = BitConverter.GetBytes(_currentMessage.Length);

                    // if we can send the message in one round
                    if (_prefixSize + _currentMessage.Length <= _bufferSize)
                    {
                        result = new byte[_prefixSize + _currentMessage.Length];
                        _currentPrefix.CopyTo(result, 0);
                        _currentMessage.CopyTo(result, _prefixSize);

                        _currentPrefix = null;
                        _currentMessage = null;
                        _currentMessageArrayIndex = 0;

                        return result;
                    }

                    // else just grab what we can to fill the buffer, and remember the current message index...we'll need it next round
                    result = new byte[_bufferSize];
                    _currentPrefix.CopyTo(result, 0);
                    Buffer.BlockCopy(_currentMessage, 0, result, _prefixSize, _bufferSize - _prefixSize);
                    _currentMessageArrayIndex += _bufferSize - _prefixSize;
                }
                return result;
            }
            #endregion

            #region Partial message
            // if we're in the middle of an unfinished message, first determine how much do we send
            var messageBytesLeft = _currentMessage.Length - _currentMessageArrayIndex;

            // if it fits to the current buffer, just send the rest of the message
            if (messageBytesLeft <= _bufferSize)
            {
                result = new byte[messageBytesLeft];
                Buffer.BlockCopy(_currentMessage, _currentMessageArrayIndex, result, 0, messageBytesLeft);

                _currentPrefix = null;
                _currentMessage = null;
                _currentMessageArrayIndex = 0;
            }
            else // just grab what we can to fill the buffer, and remember the current message index...we'll need it next round
            {
                result = new byte[_bufferSize];
                _currentPrefix.CopyTo(result, 0);
                Buffer.BlockCopy(_currentMessage, 0, result, 0, _bufferSize);
                _currentMessageArrayIndex += _bufferSize;

            }
            #endregion

            return result;
        }
    }
}