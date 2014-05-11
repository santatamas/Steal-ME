using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StealME.Networking.Protocol
{
    public class PacketReceiverLogic
    {
        public int TransferredBytesCount { get; set; }
        public List<byte[]> Output { get; private set; }

        public byte[] IncomingDataBuffer
        {
            get { return _incomingDataBuffer; }
        }

        private int _prefixSize;
        private int _lengthOfCurrentIncomingMessage;
        private byte[] _incomingDataBuffer;
        private byte[] _byteArrayForPrefix;
        private byte[] _byteArrayForMessage;
        private int _receivedPrefixBytesDoneCount = 0;
        private int _receivedMessageBytesDoneCount = 0;

        public PacketReceiverLogic(int bufferSize, int prefixSize)
        {
            _incomingDataBuffer = new byte[bufferSize];
            _byteArrayForPrefix = new byte[prefixSize];
            _prefixSize = prefixSize;
            Output = new List<byte[]>();
        }

        public bool Process()
        {
            var originalTransferredBytes = TransferredBytesCount;
            var remainingBytesToProcess = originalTransferredBytes;
            var bufferIndex = 0;
            var result = false;
            int cycleCnt = 0;
            Output.Clear();

            while (remainingBytesToProcess != 0)
            {
                cycleCnt++;

                #region Prefix processing

                if (_receivedPrefixBytesDoneCount == 0)
                {
                    //let's just clear out the prefix byte array
                    for (int index = 0; index < _byteArrayForPrefix.Length; index++)
                    {
                        _byteArrayForPrefix[index] = 0;
                    }
                }

                // If this next if-statement is true, then we have received >=
                // enough bytes to have the prefix. So we can determine the 
                // length of the message that we are working on.
                if (remainingBytesToProcess >= _prefixSize - _receivedPrefixBytesDoneCount)
                {
                    //Now copy that many bytes to byteArrayForPrefix.
                    //We can use the variable receiveMessageOffset as our main
                    //index to show which index to get data from in the TCP
                    //buffer.
                    Buffer.BlockCopy(
                        _incomingDataBuffer, bufferIndex, //where from        
                        _byteArrayForPrefix, _receivedPrefixBytesDoneCount, //where to
                        _prefixSize - _receivedPrefixBytesDoneCount); //length    

                    bufferIndex += _prefixSize - _receivedPrefixBytesDoneCount;
                    remainingBytesToProcess = remainingBytesToProcess - _prefixSize + _receivedPrefixBytesDoneCount;

                    _receivedPrefixBytesDoneCount = _prefixSize;
                    _lengthOfCurrentIncomingMessage = BitConverter.ToInt32(_byteArrayForPrefix, 0);

                    if(_lengthOfCurrentIncomingMessage < 0)
                        Debugger.Break();

                    if (remainingBytesToProcess == 0)
                    {
                        continue; //step out from the processing cycle
                    }
                }
                else
                {
                    Buffer.BlockCopy(
                        _incomingDataBuffer, bufferIndex, //where from
                        _byteArrayForPrefix, _receivedPrefixBytesDoneCount, //where to
                        remainingBytesToProcess); //length    

                    _receivedPrefixBytesDoneCount += remainingBytesToProcess;
                    remainingBytesToProcess = 0;
                    continue; //step out from the processing cycle
                }

                #endregion

                #region Message payload processing

                if (_receivedMessageBytesDoneCount == 0)
                {
                    _byteArrayForMessage = new byte[_lengthOfCurrentIncomingMessage];
                }

                // Remember there is a receiveSendToken.receivedPrefixBytesDoneCount
                // variable, which allowed us to handle the prefix even when it
                // requires multiple receive ops. In the same way, we have a 
                // receiveSendToken.receivedMessageBytesDoneCount variable, which
                // helps us handle message data, whether it requires one receive
                // operation or many.
                if (remainingBytesToProcess + _receivedMessageBytesDoneCount == _lengthOfCurrentIncomingMessage)
                {
                    // If we are inside this if-statement, then we got 
                    // the end of the message. In other words,
                    // the total number of bytes we received for this message matched the 
                    // message length value that we got from the prefix.
                    // Write/append the bytes received to the byte array in the 
                    // DataHolder object that we are using to store our data.
                    Buffer.BlockCopy(
                        _incomingDataBuffer, bufferIndex, //where from    
                        _byteArrayForMessage, _receivedMessageBytesDoneCount, //where to
                        remainingBytesToProcess); //length

                    remainingBytesToProcess = 0;
                    Output.Add(_byteArrayForMessage);
                    result = true;

                    ResetCurrentMessageState();
                    continue; // continue with the next processing cycle
                }

                // if we need another round...just process here what we can, then we'll do the rest later
                if (remainingBytesToProcess + _receivedMessageBytesDoneCount < _lengthOfCurrentIncomingMessage)
                {
                    if(bufferIndex + remainingBytesToProcess > _incomingDataBuffer.Length ||
                        _receivedMessageBytesDoneCount + remainingBytesToProcess > _byteArrayForMessage.Length)
                    {
                        Debugger.Break();
                    }

                    Buffer.BlockCopy(
                        _incomingDataBuffer, bufferIndex,
                        _byteArrayForMessage, _receivedMessageBytesDoneCount, remainingBytesToProcess);

                    _receivedMessageBytesDoneCount += remainingBytesToProcess;
                    remainingBytesToProcess = 0;
                    continue; //step out from the processing cycle
                }

                // calculate how many bytes do we need for a complete packet
                var remainingBytesForCurrentMessage = _lengthOfCurrentIncomingMessage -
                                                      _receivedMessageBytesDoneCount;

                // calculate how many bytes do we have in our remaining buffer space
                var remainingBytesInBufferForCurrentMessage = _incomingDataBuffer.Length - bufferIndex;

                // if we have the remaining part of the message in our data buffer
                if (remainingBytesInBufferForCurrentMessage >= remainingBytesForCurrentMessage)
                {
                    Buffer.BlockCopy(
                        _incomingDataBuffer, bufferIndex, //where from    
                        _byteArrayForMessage, _receivedMessageBytesDoneCount, //where to
                        remainingBytesForCurrentMessage); //length

                    bufferIndex += remainingBytesForCurrentMessage;
                    _receivedMessageBytesDoneCount += remainingBytesForCurrentMessage;
                    remainingBytesToProcess -= remainingBytesForCurrentMessage;

                    Output.Add(_byteArrayForMessage);
                    result = true;

                    ResetCurrentMessageState();
                    continue; // step out from this cycle, and continoue with the next message
                }

                // if we doesn't have the rest of the message in our buffer, grab what we can and go on...
                if (remainingBytesInBufferForCurrentMessage < remainingBytesForCurrentMessage)
                {
                    Buffer.BlockCopy(
                        _incomingDataBuffer, bufferIndex, //where from    
                        _byteArrayForMessage, _receivedMessageBytesDoneCount, //where to
                        remainingBytesInBufferForCurrentMessage); //length

                    _receivedMessageBytesDoneCount += remainingBytesInBufferForCurrentMessage;
                    remainingBytesToProcess = 0;
                }

                #endregion
            }
            return result;
        }

        private void ResetCurrentMessageState()
        {
            _receivedPrefixBytesDoneCount = 0;
            _receivedMessageBytesDoneCount = 0;
            _byteArrayForMessage = null;
            _lengthOfCurrentIncomingMessage = 0;
        }
    }
}