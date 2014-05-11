package stealme.client.communication;


import stealme.client.communication.Enums.Command;
import stealme.client.communication.Enums.GetRequest;
import stealme.client.communication.Enums.Message;
import stealme.client.communication.Enums.SetRequest;
import android.content.Context;

public interface IMessageListener {
	public Context getContext();
	public String getRemoteNumber();
	public String getTrackingIP();
	public String getTrackingPort();
	public boolean getSMSEnabled();
	
	public void onCommandReceived(Command command);
	public void onGetRequest(GetRequest request);
	public void onSetRequest(SetRequest request, String value);
	public void onMessage(Message message, String value);
	public void LogThis(String title, String message);
}
