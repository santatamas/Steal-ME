package stealme.client.communication;

import stealme.client.communication.Enums.Command;
import stealme.client.communication.Enums.GetRequest;
import stealme.client.communication.Enums.Message;
import stealme.client.communication.Enums.SetRequest;
import android.app.Activity;
import android.app.PendingIntent;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.telephony.SmsManager;
import android.util.Log;

public class MessageHandler {

	final static String LogPrefix = "SM.Client";
	static IMessageListener listener;
	static Context context;
	final static String SENT = "SMS_SENT";
	final static String DELIVERED = "SMS_DELIVERED";
	final static String RECEIVED = "android.provider.Telephony.SMS_RECEIVED";
	static PendingIntent sentPI;
	static PendingIntent deliveredPI;
	private static SmsReceiver smsReceiver;
	private static SocketReceiver socketReceiver;

	private static void RegisterReceivers() {
		context.registerReceiver(new BroadcastReceiver() {
			@Override
			public void onReceive(Context arg0, Intent arg1) {
				switch (getResultCode()) {
				case Activity.RESULT_OK:

					break;
				case SmsManager.RESULT_ERROR_GENERIC_FAILURE:

					break;
				case SmsManager.RESULT_ERROR_NO_SERVICE:

					break;
				case SmsManager.RESULT_ERROR_NULL_PDU:

					break;
				case SmsManager.RESULT_ERROR_RADIO_OFF:

					break;
				}
			}
		}, new IntentFilter(SENT));
		context.registerReceiver(new BroadcastReceiver() {
			@Override
			public void onReceive(Context arg0, Intent arg1) {
				switch (getResultCode()) {
				case Activity.RESULT_OK:

					break;
				case Activity.RESULT_CANCELED:

					break;
				}
			}
		}, new IntentFilter(DELIVERED));
	}

	public static void startListening(IMessageListener lsn) {
		if (listener == null) {
			listener = lsn;
			context = listener.getContext();
			smsReceiver = new SmsReceiver(listener);
			context.registerReceiver(smsReceiver, new IntentFilter(RECEIVED));

			socketReceiver = new SocketReceiver(lsn);
		}
	}

	public static void stopListening() {
		if (listener != null) {
			context.unregisterReceiver(smsReceiver);
			listener = null;
			context = null;

			socketReceiver.Close();
		}
	}

	public static void sendMessage(Message messageType, String message) {
		String msgPrefix = PC.MESSAGE_PREFIX + ".";

		switch (messageType) {
		case Status:
			sendMessage(msgPrefix + PC.MESSAGE_STATUS + "=" + message);
			break;
		case Location:
			sendMessage(msgPrefix + PC.MESSAGE_LOCATION + "=" + message);
			break;
		case State:
			sendMessage(msgPrefix + PC.MESSAGE_STATE + "=" + message);
			break;
		case IMEI:
			sendMessage(msgPrefix + PC.MESSAGE_IMEI + "=" + message, true);
			break;
		}
	}
	
	public static void sendMessage(Message messageType, Object message) {
		socketReceiver.queueMessage(message);
	}

	public static void sendCommand(Command command) {
		String cmdPrefix = PC.COMMAND_PREFIX + ".";
		switch (command) {
		case Activate:
			sendMessage(cmdPrefix + PC.COMMAND_ACTIVATE);
			break;
		case Deactivate:
			sendMessage(cmdPrefix + PC.COMMAND_DEACTIVATE);
			break;
		case Signal:
			sendMessage(cmdPrefix + PC.COMMAND_SIGNAL);
			break;
		}
	}

	public static void sendGetRequest(GetRequest request) {
		String getPrefix = PC.GET_PREFIX + ".";
		switch (request) {
		case Status:
			sendMessage(getPrefix + PC.GET_STATUS);
			break;
		}
	}

	public static void sendSetRequest(SetRequest request, String param) {
		String setPrefix = PC.MESSAGE_PREFIX + ".";

		switch (request) {
		case Mode:
			sendMessage(setPrefix + PC.SET_MODE + "=" + param);
			break;
		case Interval:
			sendMessage(setPrefix + PC.SET_INTERVAL + "=" + param);
			break;
		}
	}

	public static CommunicationState getCommunicationState() {
		if(socketReceiver != null && socketReceiver.isConnectionAlive())
		{
			return CommunicationState.OnLine;
		}
		return CommunicationState.SMS;
	}

	private static void sendMessage(String message) {
		Log.d(LogPrefix, "sendSMS: " + message);
		if (listener.getSMSEnabled()) {
			SmsManager sms = SmsManager.getDefault();
			Log.i("SM", "SMS sent:" + message);
			sms.sendTextMessage(listener.getRemoteNumber(), null, message,sentPI, deliveredPI);
		}
	}
	
	private static void sendMessage(Object message) {
		socketReceiver.queueMessage(message);
	}
	
	private static void sendMessage(String message, boolean isTcpOnly) {
		Log.d(LogPrefix, "sendTcp: " + message);
		
		if(!isTcpOnly)
		{
			sendMessage(message);
		}
		else
		{
			if(socketReceiver != null)
				socketReceiver.queueMessage(message);
		}
	}
}