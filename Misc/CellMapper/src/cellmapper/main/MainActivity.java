package cellmapper.main;

import java.io.BufferedWriter;
import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Calendar;

import android.app.Activity;
import android.content.Context;
import android.content.pm.ActivityInfo;
import android.content.res.Configuration;
import android.location.Location;
import android.location.LocationListener;
import android.location.LocationManager;
import android.os.Bundle;
import android.os.Environment;
import android.telephony.PhoneStateListener;
import android.telephony.SignalStrength;
import android.telephony.TelephonyManager;
import android.telephony.gsm.GsmCellLocation;
import android.text.format.Time;
import android.util.Log;
import android.widget.TextView;

public class MainActivity extends Activity {

	// ===================== UI Elements
	TextView txtLat;
	TextView txtLon;
	TextView txtAcc;
	TextView txtId;
	TextView txtLac;
	TextView txtMnc;
	TextView txtMcc;
	TextView txtRX;
	public static MainActivity Instance;

	// ===================== Telephony
	TelephonyManager telephonyManager;
	RssiPhoneStateListener signalStrengthListener;
	GsmCellLocation cellLocation;
	// ===================== Location
	LocationManager locationManager;
	// ===================== Cell Infos
	/*double latitude;
	double longtitude;
	double accurancy;
	String mcc;
	String mnc;
	String networkOperator;
	int cellId;
	int lac;*/
	public int LatestRssi;

	ArrayList<CellLocationInfo> cellInfos;
	/** Called when the activity is first created. */
	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.main);

		// get UI control references
		txtLat = (TextView) findViewById(R.id.txtLat);
		txtLon = (TextView) findViewById(R.id.txtLon);
		txtAcc = (TextView) findViewById(R.id.txtAcc);
		txtId = (TextView) findViewById(R.id.txtId);
		txtLac = (TextView) findViewById(R.id.txtLac);
		txtMnc = (TextView) findViewById(R.id.txtMnc);
		txtMcc = (TextView) findViewById(R.id.txtMcc);
		txtRX = (TextView) findViewById(R.id.txtRX);

		cellInfos = new ArrayList<CellLocationInfo>();
		
		Instance = this;
		signalStrengthListener = new RssiPhoneStateListener();
		// retrieve a reference to an instance of TelephonyManager
		telephonyManager = (TelephonyManager) getSystemService(Context.TELEPHONY_SERVICE);
		telephonyManager.listen(signalStrengthListener, PhoneStateListener.LISTEN_SIGNAL_STRENGTHS);
		locationManager = (LocationManager) getSystemService(Context.LOCATION_SERVICE);
		locationManager.requestLocationUpdates(LocationManager.GPS_PROVIDER, 0, 0, locListener);
	}

	// ================================================================= Activity Lifecycle

	@Override
	protected void onStart() {
		super.onStart();

	}

	@Override
	protected void onResume() {
		super.onResume();

	}

	@Override
	protected void onPause() {
		super.onPause();

	}

	@Override
	protected void onStop() {
		super.onStop();

	}

	@Override
	protected void onDestroy() {
		super.onDestroy();
	}

	@Override
	public void onConfigurationChanged(Configuration newConfig) {
		super.onConfigurationChanged(newConfig);
		setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_PORTRAIT);
	}

	// ================================================================= Location Listener

	CellLocationInfo latestCellLocInfo;
	public void onLocationChanged(Location location) {

		latestCellLocInfo = new CellLocationInfo();
		latestCellLocInfo.latitude = String.valueOf(location.getLatitude());
		latestCellLocInfo.longtitude = String.valueOf(location.getLongitude());
		latestCellLocInfo.accurancy = String.valueOf(location.getAccuracy());
		cellLocation = (GsmCellLocation) telephonyManager.getCellLocation();
		latestCellLocInfo.networkOperator = telephonyManager.getNetworkOperator();
		if(!latestCellLocInfo.networkOperator.equals(null))
		{
			latestCellLocInfo.mcc = latestCellLocInfo.networkOperator.substring(0, 3);
			latestCellLocInfo.mnc = latestCellLocInfo.networkOperator.substring(3);
		}
		latestCellLocInfo.cellId = String.valueOf(cellLocation.getCid());
		latestCellLocInfo.lac = String.valueOf(cellLocation.getLac());
		latestCellLocInfo.LatestRssi = String.valueOf(this.LatestRssi);
		latestCellLocInfo.createdOn = Calendar.getInstance().getTime();
		
		txtLat.setText(latestCellLocInfo.latitude);
		txtLon.setText(latestCellLocInfo.longtitude);
		txtAcc.setText(latestCellLocInfo.accurancy);
		txtMcc.setText(latestCellLocInfo.mcc);
		txtMnc.setText(latestCellLocInfo.mnc);
		txtId.setText(latestCellLocInfo.cellId);
		txtLac.setText(latestCellLocInfo.lac);
		txtRX.setText(latestCellLocInfo.LatestRssi);
		
		cellInfos.add(latestCellLocInfo);
		
		if(cellInfos.size() > 10)
		{
			LogCellInfos(cellInfos);
			cellInfos.clear();
		}	
	}

	private void LogCellInfos(ArrayList<CellLocationInfo> cellInfos)
	{
		File logFilePath = new File(Environment.getExternalStorageDirectory(), "/CellLogger/");	
		if(!logFilePath.exists())
		{
			logFilePath.mkdirs();
		}
		
		File logFile = new File(logFilePath, "location.dat");		
		if(!logFile.exists())
		{
			try {
				logFile.createNewFile();
			} catch (IOException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
		}
		
	    if (logFile.canWrite()){               
	        try {            
			        FileWriter logWriter = new FileWriter(logFile, true);      
			        BufferedWriter out = new BufferedWriter(logWriter); 
			        
			        for(CellLocationInfo info : cellInfos)
			        {
				        out.write(	info.createdOn.toString() + ";" + 
				        			info.latitude + ";" + 
				        			info.longtitude+ ";" + 
				        			info.accurancy+ ";" + 
				        			info.cellId+ ";" + 
				        			info.lac+ ";" + 
				        			info.mcc+ ";" + 
				        			info.mnc+ ";" + 
				        			info.LatestRssi+ ";" + 
				        			info.networkOperator); 
				        out.newLine();
			        }
			        out.close();
		        }
		        catch (IOException e) {
		            Log.e("test", "Could not read/write file " + e.getMessage());
		        }
	    }	
	}
	
	// ================================================================= Location Listener
	public static LocationListener locListener = new LocationListener() {
		private Location currentLoc;

		public void onLocationChanged(Location location) {
			MainActivity.Instance.onLocationChanged(location);
		}

		public void onProviderDisabled(String provider) {
			// GPS Off
		}

		public void onProviderEnabled(String provider) {
			// GPS On
		}

		public void onStatusChanged(String provider, int status, Bundle extras) {
			// TODO Auto-generated method stub
		}
	};

	// ================================================================= Phone State Listener
	public class RssiPhoneStateListener extends PhoneStateListener {
		/*
		 * Get the Signal strength from the provider, each time there is an update
		 */
		@Override
		public void onSignalStrengthsChanged(SignalStrength signalStrength) {
			super.onSignalStrengthsChanged(signalStrength);
			MainActivity.Instance.LatestRssi = signalStrength.getGsmSignalStrength();
		}
	};
}