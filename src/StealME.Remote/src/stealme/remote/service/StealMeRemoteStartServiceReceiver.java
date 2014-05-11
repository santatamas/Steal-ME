package stealme.remote.service;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;

public class StealMeRemoteStartServiceReceiver extends BroadcastReceiver {

	@Override
	public void onReceive(Context context, Intent intent) {
		Intent service = new Intent(context, StealMeRemoteService.class);
		context.startService(service);
	}
}
