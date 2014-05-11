package stealme.client.service;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;

public class StealMeStartServiceReceiver extends BroadcastReceiver {

	@Override
	public void onReceive(Context context, Intent intent) {
		Intent service = new Intent(context, StealMeService.class);
		context.startService(service);
	}
}
