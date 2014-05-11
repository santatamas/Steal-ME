package stealme.client.communication;

import java.io.BufferedReader;
import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.net.Socket;
import java.net.SocketTimeoutException;
import java.net.UnknownHostException;
import java.util.LinkedList;
import java.util.Queue;

import stealme.client.networking.AuthRequest;
import stealme.client.networking.IntByteConverter;
import stealme.client.networking.LocationMessage;
import stealme.client.networking.MsgPackSerializer;
import stealme.client.networking.PacketReceiverLogic;
import stealme.client.networking.PingRequest;
import stealme.client.networking.ProtocolVersionResponse;
import stealme.client.networking.StatusResponse;
import stealme.client.networking.TypeResolver;

import android.content.Context;
import android.content.IntentFilter;
import android.os.Looper;
import android.telephony.TelephonyManager;
import android.text.format.Time;

public class SocketReceiver {

	IMessageListener _listener;
	String _IMEI = null;
	DataOutputStream dataOutputStream = null;
	DataInputStream dataInputStream = null;
	BufferedReader dataInputReader = null;
	Socket socket = null;
	int _socketTimeout = 5000;
	Thread listenerThread;
	Thread heartBeatThread;
	Thread queueProcessorThread;
	Queue<Object> _messageQueue = new LinkedList<Object>();
	Time _lastHeartBeat = new Time();
	boolean _socketConnected = false;
	boolean _terminateConnectionFlag = false;
	MessageDispatcher _parser;
	
	public SocketReceiver(IMessageListener listener) {
		this._listener = listener;
		_parser = new MessageDispatcher(listener);
		
		initializeWorkerThreads();
	}

	private void initializeWorkerThreads() {
		_terminateConnectionFlag = false;
		socket = null;
		
		listenerThread = new Thread(new ListenerWorker());
		listenerThread.setName("listenerThread");
		listenerThread.start();
		
		heartBeatThread = new Thread(new HeartBeatWorker());
		heartBeatThread.setName("heartBeatThread");
		heartBeatThread.start();
		
		queueProcessorThread = new Thread(new QueueProcessorWorker());
		queueProcessorThread.setName("queueProcessorThread");
		queueProcessorThread.start();
	}

	public String getIMEI() {
		if (_IMEI == null) {
			TelephonyManager telMan = (TelephonyManager) _listener.getContext()
					.getSystemService(Context.TELEPHONY_SERVICE);
			_IMEI = telMan.getDeviceId();
		}
		return _IMEI;
	}
	
	public boolean isConnectionAlive() {
		return socket != null && _socketConnected;
	}

	private void manageIncomingMessage(Object message)
	{
		_parser.processMessageObject(message);
	}
	
	public void queueMessage(Object message) {
		_messageQueue.add(message);
	}
	
	MsgPackSerializer serializer = new MsgPackSerializer(new TypeResolver());
	private byte[] getBytes(Object obj) throws IOException 
	{
		byte[] msg;
		if(obj instanceof ProtocolVersionResponse)
		{
			msg = serializer.Serialize((ProtocolVersionResponse)obj);
		}
		else if(obj instanceof PingRequest)
		{
			msg = serializer.Serialize((PingRequest)obj);
		}
		else if(obj instanceof LocationMessage)
		{
			msg = serializer.Serialize((LocationMessage)obj);
		}
		else if(obj instanceof AuthRequest)
		{
			msg = serializer.Serialize((AuthRequest)obj);
		}
		else if(obj instanceof StatusResponse)
		{
			msg = serializer.Serialize((StatusResponse)obj);
		}
		else
		{
			msg = serializer.Serialize(obj);
		}
		
		byte[] prefix = IntByteConverter.intToByteArray(msg.length);
		byte[] merged = new byte[prefix.length + msg.length];
		
		System.arraycopy(prefix, 0, merged, 0, 4);
		System.arraycopy(msg, 0, merged, prefix.length, msg.length);
		return merged;
	}
	
	private void sendMessage(Object obj)
	{
		try {
			if (socket != null && socket.isConnected() && _socketConnected && dataOutputStream != null) {
				
				byte[] message = getBytes(obj);
				
				dataOutputStream.write(message);
				dataOutputStream.flush();
			}	
		} catch (IOException e) {
			e.printStackTrace();
		}
	}

	class HeartBeatWorker implements Runnable {

		private void sendHeartBeat() {
			queueMessage(new PingRequest());
		}

		Time lastCheckTime = new Time();
		public void run() {

			lastCheckTime.setToNow();
			_lastHeartBeat.setToNow();
			while (!_terminateConnectionFlag) {
				
				long diff = lastCheckTime.toMillis(true) - _lastHeartBeat.toMillis(true);
				
				if(diff > 30000)
				{
					_socketConnected = false;
					_terminateConnectionFlag = true;
					
					try {
						try {
							if(socket != null)
								socket.close();
							if(dataOutputStream != null)
							dataOutputStream.close();
							
							if(dataInputStream != null)
							dataInputStream.close();
						} catch (IOException e) {
							// TODO Auto-generated catch block
							e.printStackTrace();
						}
						
						listenerThread.join();
						queueProcessorThread.join();
					} catch (InterruptedException e) {
						// TODO Auto-generated catch block
						e.printStackTrace();
					}
					
					initializeWorkerThreads();
					break;
				}else if(diff > 5000)
				{
					sendHeartBeat();
				}
				try {
					Thread.sleep(1000);
				} catch (InterruptedException e) {
					e.printStackTrace();
				}
				lastCheckTime.setToNow();
			}
		}
	}

	class ListenerWorker implements Runnable {

		byte[] buffer = new byte[50];
		PacketReceiverLogic receiverLogic = new PacketReceiverLogic(50, 4);
		MsgPackSerializer serializer = new MsgPackSerializer(new TypeResolver());
		
		private void connect() {
			try {
				socket = new Socket(_listener.getTrackingIP(),Integer.parseInt(_listener.getTrackingPort()));
				socket.setSoTimeout(_socketTimeout);
				
				_socketConnected = true;
				
				dataOutputStream = new DataOutputStream(socket.getOutputStream());
				dataInputStream = new DataInputStream(socket.getInputStream());
				dataInputReader = new BufferedReader(new InputStreamReader(dataInputStream));

			} catch (UnknownHostException e) {
				e.printStackTrace();
				_socketConnected = false;
			} catch (IOException e) {
				e.printStackTrace();
				_socketConnected = false;
			}
			catch (Exception e) {
				e.printStackTrace();
				_socketConnected = false;
			}
		}

		private void disconnect() {
			try {
				if (dataInputStream != null)
					dataInputStream.close();
			} catch (IOException e) {
				e.printStackTrace();
			}
			try {
				if (dataInputStream != null)
					dataOutputStream.close();
			} catch (IOException e) {
				e.printStackTrace();
			}

			try {
				if (socket != null)
					socket.close();
			} catch (IOException e) {
				e.printStackTrace();
			}
			_socketConnected = false;
		}

		String message;
		public void run() {

			while (socket == null && !_terminateConnectionFlag)
			{
				connect();
				
				if(!_socketConnected)
				{
					try {
						Thread.sleep(5000);
					} catch (InterruptedException e) {
						// TODO Auto-generated catch block
						e.printStackTrace();
					}
				}
				else
				{
					//queueMessage(new AuthRequest(){{IMEI = getIMEI();}});
				}
			}
			
			while (!_terminateConnectionFlag) {

				while (socket.isConnected() && !_terminateConnectionFlag) {  
					
					try {
						int readBytes = dataInputStream.read(buffer);

	                         System.arraycopy(buffer, 0, receiverLogic.incomingDataBuffer, 0, readBytes);
	                         receiverLogic.TransferredBytesCount = readBytes;
	                         receiverLogic.Process();
	                         
	                         for (byte[] rawMessage : receiverLogic.Output) {
	                        	 if (rawMessage.length > 0)
	                             {
	                        		 manageIncomingMessage(serializer.Deserialize(rawMessage));
	                             }
							}
						
					} catch (IOException e) {
						// TODO Auto-generated catch block
						e.printStackTrace();
					}	
				}
			}
		}
	}

	class QueueProcessorWorker implements Runnable {

		public void run() {
			while (!_terminateConnectionFlag) {
			
				if(!_messageQueue.isEmpty())
				{
					sendMessage(_messageQueue.remove());
				}
				try {
					Thread.sleep(200);
				} catch (InterruptedException e) {
					e.printStackTrace();
				}
			}	
		}
	}
	
	public void Close() {
		if (!socket.isClosed())
			try {
				socket.close();
			} catch (IOException e) {
				e.printStackTrace();
			}
	}
}
