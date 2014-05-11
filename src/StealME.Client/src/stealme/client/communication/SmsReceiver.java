package stealme.client.communication;

import stealme.client.communication.Enums.Command;
import stealme.client.communication.Enums.GetRequest;
import stealme.client.communication.Enums.Message;
import stealme.client.communication.Enums.SetRequest;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.telephony.SmsMessage;

public class SmsReceiver extends BroadcastReceiver
{
	private IMessageListener listener;
	MessageDispatcher parser;
	public SmsReceiver(IMessageListener lst)
	{
		listener = lst;
		parser = new MessageDispatcher(lst);
	}
	@Override
	public void onReceive(Context arg0, Intent intent) {
		Bundle bundle = intent.getExtras();

		Object messages[] = (Object[]) bundle.get("pdus");
		SmsMessage smsMessages[] = new SmsMessage[messages.length];
		for (int n = 0; n < messages.length; n++) {
			smsMessages[n] = SmsMessage.createFromPdu((byte[]) messages[n]);
		}

		String message = smsMessages[0].getMessageBody();
		String address = smsMessages[0].getOriginatingAddress();
		// the form of the sms must be like:
		// CMD.ACTIVATE, or GET.STATUS, or SET.MODE=ACTIVE or MSG.STAT=, or MSG.LOC=12,2121 54,54858

		//if(address.equals(listener.RemoteNumber))
		{
			parser.processMessage(message);
		}
	}
	

}
