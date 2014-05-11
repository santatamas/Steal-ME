package stealme.client.networking;

import java.util.ArrayList;
import java.util.List;

public class PacketReceiverLogic {
	
	   public int TransferredBytesCount;
       public List<byte[]> Output = new ArrayList<byte[]>();

       public byte[] GetIncomingDataBuffer()
       {
          return incomingDataBuffer;
       }

       private int _prefixSize;
       private int _lengthOfCurrentIncomingMessage;
       public byte[] incomingDataBuffer;
       private byte[] _byteArrayForPrefix;
       private byte[] _byteArrayForMessage;
       private int _receivedPrefixBytesDoneCount = 0;
       private int _receivedMessageBytesDoneCount = 0;

       public PacketReceiverLogic(int bufferSize, int prefixSize)
       {
           incomingDataBuffer = new byte[bufferSize];
           _byteArrayForPrefix = new byte[prefixSize];
           _prefixSize = prefixSize;
       }

       public boolean Process()
       {
           int originalTransferredBytes = TransferredBytesCount;
           int remainingBytesToProcess = originalTransferredBytes;
           int bufferIndex = 0;
           boolean result = false;
           int cycleCnt = 0;
           Output.clear();

           while (remainingBytesToProcess != 0)
           {
               cycleCnt++;

               /// --- Prefix Processing ---
               if (_receivedPrefixBytesDoneCount == 0)
               {
                   //let's just clear out the prefix byte array
                   for (int index = 0; index < _byteArrayForPrefix.length; index++)
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
            	   System.arraycopy(
                       incomingDataBuffer, bufferIndex, //where from        
                       _byteArrayForPrefix, _receivedPrefixBytesDoneCount, //where to
                       _prefixSize - _receivedPrefixBytesDoneCount); //length    

                   bufferIndex += _prefixSize - _receivedPrefixBytesDoneCount;
                   remainingBytesToProcess = remainingBytesToProcess - _prefixSize + _receivedPrefixBytesDoneCount;

                   _receivedPrefixBytesDoneCount = _prefixSize;
                   _lengthOfCurrentIncomingMessage = IntByteConverter.byteArrayToInt(_byteArrayForPrefix);


                   if (remainingBytesToProcess == 0)
                   {
                       continue; //step out from the processing cycle
                   }
               }
               else
               {
            	   System.arraycopy(
                       incomingDataBuffer, bufferIndex, //where from
                       _byteArrayForPrefix, _receivedPrefixBytesDoneCount, //where to
                       remainingBytesToProcess); //length    

                   _receivedPrefixBytesDoneCount += remainingBytesToProcess;
                   remainingBytesToProcess = 0;
                   continue; //step out from the processing cycle
               }

               
               /// --- Message Processing ---
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
            	   System.arraycopy(
                       incomingDataBuffer, bufferIndex, //where from    
                       _byteArrayForMessage, _receivedMessageBytesDoneCount, //where to
                       remainingBytesToProcess); //length

                   remainingBytesToProcess = 0;
                   Output.add(_byteArrayForMessage);
                   result = true;

                   ResetCurrentMessageState();
                   continue; // continue with the next processing cycle
               }

               // if we need another round...just process here what we can, then we'll do the rest later
               if (remainingBytesToProcess + _receivedMessageBytesDoneCount < _lengthOfCurrentIncomingMessage)
               {

            	   System.arraycopy(
                       incomingDataBuffer, bufferIndex,
                       _byteArrayForMessage, _receivedMessageBytesDoneCount, remainingBytesToProcess);

                   _receivedMessageBytesDoneCount += remainingBytesToProcess;
                   remainingBytesToProcess = 0;
                   continue; //step out from the processing cycle
               }

               // calculate how many bytes do we need for a complete packet
               int remainingBytesForCurrentMessage = _lengthOfCurrentIncomingMessage -
                                                     _receivedMessageBytesDoneCount;

               // calculate how many bytes do we have in our remaining buffer space
               int remainingBytesInBufferForCurrentMessage = incomingDataBuffer.length - bufferIndex;

               // if we have the remaining part of the message in our data buffer
               if (remainingBytesInBufferForCurrentMessage >= remainingBytesForCurrentMessage)
               {
            	   System.arraycopy(
                       incomingDataBuffer, bufferIndex, //where from    
                       _byteArrayForMessage, _receivedMessageBytesDoneCount, //where to
                       remainingBytesForCurrentMessage); //length

                   bufferIndex += remainingBytesForCurrentMessage;
                   _receivedMessageBytesDoneCount += remainingBytesForCurrentMessage;
                   remainingBytesToProcess -= remainingBytesForCurrentMessage;

                   Output.add(_byteArrayForMessage);
                   result = true;

                   ResetCurrentMessageState();
                   continue; // step out from this cycle, and continoue with the next message
               }

               // if we doesn't have the rest of the message in our buffer, grab what we can and go on...
               if (remainingBytesInBufferForCurrentMessage < remainingBytesForCurrentMessage)
               {
            	   System.arraycopy(
                       incomingDataBuffer, bufferIndex, //where from    
                       _byteArrayForMessage, _receivedMessageBytesDoneCount, //where to
                       remainingBytesInBufferForCurrentMessage); //length

                   _receivedMessageBytesDoneCount += remainingBytesInBufferForCurrentMessage;
                   remainingBytesToProcess = 0;
               }
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
