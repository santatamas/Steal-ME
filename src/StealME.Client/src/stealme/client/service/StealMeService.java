package stealme.client.service;

import java.util.ArrayList;
import java.util.Calendar;

import stealme.client.communication.CommunicationState;
import stealme.client.communication.MessageHandler;
import stealme.client.communication.PC;
import stealme.client.communication.IMessageListener;
import stealme.client.communication.Enums.Command;
import stealme.client.communication.Enums.GetRequest;
import stealme.client.communication.Enums.Message;
import stealme.client.communication.Enums.SetRequest;
import stealme.client.location.CellLocationInfo;
import stealme.client.location.LocationHandler;
import stealme.client.location.ILocationListener;
import stealme.client.main.LogItem;
import stealme.client.main.MainActivity;
import stealme.client.main.R;
import stealme.client.networking.LocationMessage;
import stealme.client.networking.ProtocolVersionResponse;
import stealme.client.sensor.IAccelerometerListener;
import stealme.client.sensor.SensorHandler;

import android.app.Notification;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.app.Service;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.SharedPreferences;
import android.content.SharedPreferences.OnSharedPreferenceChangeListener;
import android.location.Location;
import android.os.Binder;
import android.os.IBinder;
import android.os.PowerManager;
import android.os.PowerManager.WakeLock;
import android.preference.PreferenceManager;
import android.telephony.TelephonyManager;
import android.telephony.gsm.GsmCellLocation;
import android.telephony.PhoneStateListener;
import android.telephony.SignalStrength;
import android.text.format.Time;
import android.util.Log;
import android.view.WindowManager;

public class StealMeService extends Service implements IAccelerometerListener,
		ILocationListener, IMessageListener, OnSharedPreferenceChangeListener {

	// ================================================================= Application variables	
	private AlertState _appState = AlertState.Deactivated;
	private CommunicationState _commState = CommunicationState.SMS;
	private ArrayList<LogItem> _logMessages = new ArrayList<LogItem>();
	private Location _lastKnownLocation = null;
	private int _batteryLevel = 0;	
	private final IBinder mBinder = new SMBinder();
	private Time _activationTime;
	private Time _lastSendTime = new Time();
	private LogItem _lastLogItem;
	private Time _lastLogTime;
	public static final String LogPrefix = "SM.Client";
	private SharedPreferences settings;
	private int _notificationCounter = 0;
	// ================================================================= Application Settings
	private String _remoteNumber;
	private String _trackingIP;
	private String _trackingPort;
	private Integer _interval = 10;
	private boolean _useGPS;
	private boolean _useNetwork;
	private boolean _SMSEnabled;
	private boolean _acceleroEnabled;
	private Integer _acceleroSensitivity;
	// ===================== Telephony
	TelephonyManager telephonyManager;
	PhoneStateListener signalStrengthListener;
	GsmCellLocation cellLocation;
	CellLocationInfo latestCellLocInfo;
	PowerManager _powerManager;
	WakeLock _wakeLock;
	public int LatestRssi;
	
	// ================================================================= Construction
	@Override
	public void onCreate() {
		setAppState(AlertState.Deactivated);
		LoadSettings();
		MessageHandler.startListening(this);
		
		settings = PreferenceManager.getDefaultSharedPreferences(this);
		settings.registerOnSharedPreferenceChangeListener(this);
		
		signalStrengthListener = new PhoneStateListener();
		// retrieve a reference to an instance of TelephonyManager
		telephonyManager = (TelephonyManager) getSystemService(Context.TELEPHONY_SERVICE);
		telephonyManager.listen(signalStrengthListener, PhoneStateListener.LISTEN_SIGNAL_STRENGTHS);
		
		//IntentFilter filter = new IntentFilter(Intent.ACTION_SCREEN_OFF);
		// registerReceiver(SensorHandler.mReceiver, filter);
		
		super.onCreate();
	}
	
	@Override
	public int onStartCommand(Intent intent, int flags, int startId) {
		return super.onStartCommand(intent, flags, startId);
	}
	
	@Override
	public void onDestroy() {
		DetachListeners();
		MessageHandler.stopListening();
		//_wakeLock.release();
		super.onDestroy();
	}

	public void onSharedPreferenceChanged(SharedPreferences sharedPreferences,
			String key) {
		LoadSettings();
	}
	
	public void LoadSettings() {

		Log.d(LogPrefix, "LoadSettings()");
		
		SharedPreferences settings = PreferenceManager.getDefaultSharedPreferences(this);
		
		this.setInterval(Integer.parseInt(settings.getString("minimum_location_polling_interval", "10000")));
		this.setRemoteNumber(settings.getString("controller_phone", ""));
		this.setTrackingIP(settings.getString("tracker_server_ip", "192.168.0.191"));
		this.setTrackingPort(settings.getString("tracker_server_port", "4444"));
		this.setUseGPS(settings.getBoolean("use_gps", true));
		this.setUseNetwork(settings.getBoolean("use_network_provider", true));
		this.setSMSEnabled(settings.getBoolean("enable_sms", false));
		this.setAcceleroEnabled(settings.getBoolean("enable_accelero", true));
		this.setAcceleroSensitivity(Integer.parseInt(settings.getString("accelero_sensitivity", "60")));
	}

	// ================================================================= Application Logic

	public void LogThis(String title, String message)
	{
		Log.d(LogPrefix, title + ": " + message);
		
		_lastLogTime = new Time();
		_lastLogTime.setToNow();
		_lastLogItem = new LogItem(_lastLogTime, title, message);
		this._logMessages.add(_lastLogItem);
		
		if(this._logMessages.size() > 50)
		{
			this._logMessages.remove(0);
		}
		ShowNotification(title, message);
	}
	
	private void AttachListeners() {
		LocationHandler.startListening(this);
		
		if(getAcceleroEnabled())
			SensorHandler.startListening(this);
		Log.d(LogPrefix, "AttachListeners()");
	}

	private void DetachListeners() {
		LocationHandler.stopListening();
		
		if(getAcceleroEnabled())
			SensorHandler.stopListening();
		Log.d(LogPrefix, "DetachListeners()");
	}

	public void setAppState(AlertState appState) {
		this._appState = appState;

		switch (appState) {
		case Activated:
			_activationTime = new Time();
			_activationTime.setToNow();
			AttachListeners();
			break;
		case Deactivated:
			DetachListeners();
			break;
		case Tracking:
			MessageHandler.sendMessage(Message.State, PC.MODE_ALARM);
			break;
		}
		this.LogThis(getString(R.string.log_alert_state_changed), getAlertStateText());
	}

	@Override
	public IBinder onBind(Intent intent) {
		return mBinder;
	}

	public class SMBinder extends Binder {
		public StealMeService getService() {
			return StealMeService.this;
		}
	}

	// ================================================================= Message Listener
	public String getRemoteNumber() {
		return _remoteNumber;
	}

	public void onCommandReceived(Command command) {
		switch (command) {
		case Activate:
			setAppState(AlertState.Activated);
			break;
		case Deactivate:
			setAppState(AlertState.Deactivated);
			break;
		case Signal:
			//TODO: implement alarm signal
			//PlayAlarmSignal();
			break;
		}
		this.LogThis(getString(R.string.log_alert_state_changed), command.name());
	}

	public void onGetRequest(GetRequest request) {
		switch (request) {
		case Status:
			MessageHandler.sendMessage(Message.Status, GetDeviceStatus());
			break;
		case IMEI:
			TelephonyManager telMan = (TelephonyManager) this.getSystemService(Context.TELEPHONY_SERVICE);
			MessageHandler.sendMessage(Message.IMEI, telMan.getDeviceId());
			break;
		case ProtocolVersion:
			ProtocolVersionResponse p = new ProtocolVersionResponse();
			p.Version = 1;
			MessageHandler.sendMessage(Message.ProtocolVersion, p);
			break;
		}
		this.LogThis(getString(R.string.log_command_received), "GET-" + request.name());
	}

	public void onSetRequest(SetRequest request, String value) {
		switch (request) {
		case Mode:
			if(value.equals(PC.MODE_ALARM))
			{
				this.setAppState(AlertState.Tracking);
			}
			break;
		case Interval:
			setInterval(Integer.parseInt(value));
			break;
		}
		this.LogThis(getString(R.string.log_command_received), "SET-" + request.name() + ": " + value);
	}

	public void onMessage(Message message, String value) {
		// nothing to do here, since the device will broadcast messages only -
		// to the controller
	}

	// ================================================================= Location Listener
	
	public void onLocationChanged(Location location) {
		Log.d(LogPrefix, "onLocationChanged:" + location.getLatitude() + " " + location.getLongitude());
		
		if(this._appState == AlertState.Tracking)
		{		
			_lastKnownLocation = location;
			/*Time now = new Time();
			now.setToNow();
			
			long after = now.toMillis(true);
			long before = _lastSendTime.toMillis(true);
			
			long diff = after - before;
			
			if(diff > (_interval * 1000))
			{*/
				updateLatestCellInfo(location);
				
				if(getCommunicationState() == CommunicationState.OnLine)
				{
					MessageHandler.sendMessage(Message.TcpLoc, getTcpMessage(location, latestCellLocInfo));
					Log.d(LogPrefix, "locationPackage sent");
				}
				else
				{
					String message = String.valueOf(_lastKnownLocation.getLatitude()) + " " + 
									 String.valueOf(_lastKnownLocation.getLongitude());
					MessageHandler.sendMessage(Message.Location, message);
				}
				/*_lastSendTime.setToNow();
			}*/
		}
	}

	private LocationMessage getTcpMessage(Location location, CellLocationInfo latestCellLoc)
	{
		TelephonyManager telMan = (TelephonyManager) getContext().getSystemService(Context.TELEPHONY_SERVICE);
		LocationMessage loc = new LocationMessage();
		loc.Latitude = Double.toString(_lastKnownLocation.getLatitude());
		loc.Longitude = Double.toString(_lastKnownLocation.getLongitude());
		loc.MNC = Integer.toString(latestCellLocInfo.mnc);
		loc.MCC = Integer.toString(latestCellLocInfo.mcc);
		loc.LAC = Integer.toString(latestCellLocInfo.lac);
		loc.CellID = Integer.toString(latestCellLocInfo.cellId);
		loc.LatestRssi = Integer.toString(latestCellLocInfo.LatestRssi);
			
		return  loc;
	}
	
	private void updateLatestCellInfo(Location location) {
		latestCellLocInfo = new CellLocationInfo();
		latestCellLocInfo.latitude = String.valueOf(location.getLatitude());
		latestCellLocInfo.longtitude = String.valueOf(location.getLongitude());
		latestCellLocInfo.accurancy = String.valueOf(location.getAccuracy());
		cellLocation = (GsmCellLocation) telephonyManager.getCellLocation();
		latestCellLocInfo.networkOperator = telephonyManager.getNetworkOperator();

		if(!latestCellLocInfo.networkOperator.equals(null))
		{
			latestCellLocInfo.mcc = Integer.parseInt(latestCellLocInfo.networkOperator.substring(0, 3));
			latestCellLocInfo.mnc = Integer.parseInt(latestCellLocInfo.networkOperator.substring(3));
		}
		
		latestCellLocInfo.cellId = cellLocation.getCid();
		latestCellLocInfo.lac = cellLocation.getLac();
		latestCellLocInfo.LatestRssi = this.LatestRssi;
		latestCellLocInfo.createdOn = Calendar.getInstance().getTime();
	}

	// ================================================================= Sensor Listener
	public void onAccelerationChanged(float x, float y, float z) {
		//currently not in use
	}

	public void onShake(float force) {	
		Log.d(LogPrefix, "onShake:" + force);
		
		Time now = new Time();
		now.setToNow();
		
		//if activated more than 5 secs ago
		if(this._appState == AlertState.Activated && (now.toMillis(true) - _activationTime.toMillis(true)) > 5000)
			setAppState(AlertState.Tracking);
	}

	// ================================================================= Application Logic
	String GetDeviceStatus()
	{		
		if(_lastKnownLocation != null)
		{
			//TODO: include GSM signal strength
			return "BattLvl: " + _batteryLevel + "%" + " | " +
				   "LastLOC: " + _lastKnownLocation.getLatitude() + " " + _lastKnownLocation.getLongitude();	
		}
		return "BattLvl: " + _batteryLevel + "%" + " | " + "LastLOC: N/A";
	}
	
	public Context getContext() {
		return getApplicationContext();
	}

	public String getAlertStateText() {

		switch (this._appState) {
		case Deactivated:
			return this.getString(R.string.lbl_alstate_disabled);
		case Activated:
			return this.getString(R.string.lbl_alstate_enabled);
		case Tracking:
			return this.getString(R.string.lbl_alstate_tracking);
		}
		return null;
	}

	public String getCommunicationStateText() {

		switch (MessageHandler.getCommunicationState()) {
		case NoSignal:
			return this.getString(R.string.lbl_connstate_nogsm);
		case OnLine:
			return this.getString(R.string.lbl_connstate_online);
		case SMS:
			return this.getString(R.string.lbl_connstate_smsonly);
		}
		return null;
	}
	
	public void ShowNotification(String title, String message)
	{
		// get notification manager
		String ns = Context.NOTIFICATION_SERVICE;
		NotificationManager mNotificationManager = (NotificationManager) getSystemService(ns);

		// initiate the notification
		int icon = R.drawable.stealme_icon;
		CharSequence tickerText = "New StealME event!";
		long when = System.currentTimeMillis();

		Notification notification = new Notification(icon,
				tickerText, when);

		// Define the notification's message and PendingIntent

		Context context = getApplicationContext();
		CharSequence contentTitle = title;
		CharSequence contentText = message;
		Intent notificationIntent = new Intent(getApplicationContext(),MainActivity.class);
		PendingIntent contentIntent = PendingIntent.getActivity(getApplicationContext(), 0, notificationIntent, 0);

		notification.setLatestEventInfo(context, contentTitle,
				contentText, contentIntent);

		
		mNotificationManager.notify(0, notification);
		
		/*if(_notificationCounter < 5)
		{
			_notificationCounter++;
		}else
		{
			_notificationCounter = 0;		
		}*/
	}
	
	// ================================================================= Getters / Setters
	
	public ArrayList<LogItem> getLogMessages()
	{
		return _logMessages;
	}

	public AlertState getAlertState()
	{
		return _appState;
	}
	
	public void Alert(){
		this.setAppState(AlertState.Tracking);
	}
	
 	public CommunicationState getCommunicationState()
	{
		return MessageHandler.getCommunicationState();
	}
	
	void setInterval(Integer interval) {
		this._interval = interval;
	}
	
	public Integer getInterval()
	{
		return this._interval;
	}
	
	void setRemoteNumber(String remoteNumber)
	{
		this._remoteNumber = remoteNumber;
	}
	
	void setTrackingIP(String trackingIP)
	{
		this._trackingIP = trackingIP;
	}
	
	public String getTrackingIP()
	{
		return this._trackingIP;
	}
	
	public boolean getUseGPS() {
		return _useGPS;
	}

	public void setUseGPS(boolean _useGPS) {
		this._useGPS = _useGPS;
	}

	public boolean getUseNetwork() {
		return _useNetwork;
	}

	public void setUseNetwork(boolean _useNetwork) {
		this._useNetwork = _useNetwork;
	}

	public boolean getSMSEnabled() {
		return _SMSEnabled;
	}

	public void setSMSEnabled(boolean _SMSEnabled) {
		this._SMSEnabled = _SMSEnabled;
	}

	public Integer getAcceleroSensitivity() {
		return _acceleroSensitivity;
	}

	public void setAcceleroSensitivity(Integer _acceleroSensitivity) {
		this._acceleroSensitivity = _acceleroSensitivity;
	}

	public String getTrackingPort() {
		return _trackingPort;
	}

	private void setTrackingPort(String _trackingPort) {
		this._trackingPort = _trackingPort;
	}

	public boolean getAcceleroEnabled() {
		return _acceleroEnabled;
	}

	private void setAcceleroEnabled(boolean _acceleroEnabled) {
		this._acceleroEnabled = _acceleroEnabled;
	}
}
