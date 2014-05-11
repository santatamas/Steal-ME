package stealme.remote.service;

import stealme.client.communication.IMessageListener;
import stealme.client.communication.Enums.Command;
import stealme.client.communication.Enums.GetRequest;
import stealme.client.communication.Enums.Message;
import stealme.client.communication.Enums.SetRequest;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.SharedPreferences.OnSharedPreferenceChangeListener;
import android.os.IBinder;
import android.app.Service;

public class StealMeRemoteService extends Service implements IMessageListener,OnSharedPreferenceChangeListener {

	public void onSharedPreferenceChanged(SharedPreferences arg0, String arg1) {
		// TODO Auto-generated method stub
		
	}

	public Context getContext() {
		// TODO Auto-generated method stub
		return null;
	}

	public String getRemoteNumber() {
		// TODO Auto-generated method stub
		return null;
	}

	public String getTrackingIP() {
		// TODO Auto-generated method stub
		return null;
	}

	public String getTrackingPort() {
		// TODO Auto-generated method stub
		return null;
	}

	public boolean getSMSEnabled() {
		// TODO Auto-generated method stub
		return false;
	}

	public void onCommandReceived(Command command) {
		// TODO Auto-generated method stub
		
	}

	public void onGetRequest(GetRequest request) {
		// TODO Auto-generated method stub
		
	}

	public void onSetRequest(SetRequest request, String value) {
		// TODO Auto-generated method stub
		
	}

	public void onMessage(Message message, String value) {
		// TODO Auto-generated method stub
		
	}

	public void LogThis(String title, String message) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public IBinder onBind(Intent arg0) {
		// TODO Auto-generated method stub
		return null;
	}

}
