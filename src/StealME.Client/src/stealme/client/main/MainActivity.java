package stealme.client.main;

import java.util.ArrayList;

import stealme.client.communication.CommunicationState;
import stealme.client.configuration.ConfigurationManager;
import stealme.client.service.AlertState;
import stealme.client.service.StealMeService;
import android.os.Bundle;
import android.os.IBinder;
import android.app.Activity;
import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.content.ServiceConnection;
import android.content.pm.ActivityInfo;
import android.content.res.Configuration;
import android.view.Menu;
import android.view.MenuInflater;
import android.view.MenuItem;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.ListView;
import android.widget.TextView;
import android.widget.Toast;

public class MainActivity extends Activity {
	
	// ================================================================= Application variables
	StealMeService _stealMeService;
	TextView _connectionState;
	TextView _alertState;
	ListView _logList;
	Button _powerSW;
	private boolean _powerSWState = false;
	ArrayList<LogItem> _logMessages;
	boolean _isServiceConnected = false;

	// ================================================================= Activity LifeCycle
	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_main);

		_connectionState = (TextView) findViewById(R.id.txtConnectionState);
		_alertState = (TextView) findViewById(R.id.txtAlertState);
		_powerSW = (Button) findViewById(R.id.btnPowerSwitch);
		_powerSW.setOnClickListener(new OnClickListener() {
			
			public void onClick(View v) {
				if(_powerSWState)
				{
					_stealMeService.setAppState(AlertState.Deactivated);
				}
				else
				{
					_stealMeService.setAppState(AlertState.Activated);
				}
			}
		});
	}

	@Override
	protected void onStart() {
		
		super.onStart();
		
		// check if the service has been already started - if not, then start it now!
		Intent service = new Intent(this.getApplicationContext(),StealMeService.class);
		if (this.getApplicationContext().startService(service) != null) { 
			Toast.makeText(getBaseContext(), "Service is already running",
					Toast.LENGTH_SHORT).show();
		} else {
			Toast.makeText(getBaseContext(),
					"There is no service running, starting service..",
					Toast.LENGTH_SHORT).show();
		}

		// bind our service, and start polling status data in a background thread
		doBindService();
	}
	
	@Override
	protected void onStop() {	
		super.onStop();
		
		doUnBindService();
		
		_stealMeService = null;
		_isServiceConnected = false;
	}
	
	@Override
	public void onConfigurationChanged(Configuration newConfig) {
	    super.onConfigurationChanged(newConfig);
	    setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_PORTRAIT);
	}
	
	// ================================================================= Application Menu
	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		MenuInflater inflater = getMenuInflater();
		inflater.inflate(R.menu.menu, menu);
		return true;
	}

	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		switch (item.getItemId()) {
		case R.id.activate:
			_stealMeService.setAppState(AlertState.Activated);
			break;
		case R.id.deactivate:
			_stealMeService.setAppState(AlertState.Deactivated);
			break;
		case R.id.settings:
			startActivity(new Intent(this,ConfigurationManager.class));
			break;
		}
		return true;
	}

	// ================================================================= Service Connection / Polling
	private ServiceConnection mConnection = new ServiceConnection() {

		public void onServiceConnected(ComponentName className, IBinder binder) {
			_stealMeService = ((StealMeService.SMBinder) binder).getService();
			_isServiceConnected = true;
			
			AlertState currentServiceState = _stealMeService.getAlertState();
			
			// If the service is activated, or tracking, set the button state to active (Green)
			setPowerSWState(_stealMeService.getAlertState() != AlertState.Deactivated);

			//logMessages = new ArrayList<LogItem>();
			//_logList.setAdapter(new LogItemAdapter(getApplicationContext(),R.layout.logitem, _logMessages));
			
			Thread listenerThread = new Thread(new PollingWorker());
			listenerThread.start();

			Toast.makeText(MainActivity.this, "Connected", Toast.LENGTH_SHORT)
					.show();
		}

		public void onServiceDisconnected(ComponentName className) {
			_stealMeService = null;
			_isServiceConnected = false;
		}
	};

	void doBindService() {
		if(bindService(new Intent(this, StealMeService.class), mConnection,
				Context.BIND_AUTO_CREATE))
		{
			Toast.makeText(MainActivity.this, "Successfully binded to service", Toast.LENGTH_SHORT).show();		
		}
		else
		{
			Toast.makeText(MainActivity.this, "Cannot bind to service", Toast.LENGTH_SHORT).show();		
		}
	}
	
	void doUnBindService()
	{
		unbindService(mConnection);
	}
	
	class PollingWorker implements Runnable {

		public void run() {
			while (_isServiceConnected) {

				runOnUiThread(new Runnable(){
				    public void run(){
				    	try {
							_alertState.setText(_stealMeService.getAlertStateText());
							_connectionState.setText(_stealMeService.getCommunicationStateText());
							
							//_logMessages.clear();
							//_logMessages.addAll(_stealMeService.getLogMessages());
							
							setPowerSWState(_stealMeService.getAlertState() != AlertState.Deactivated);
							
							//((LogItemAdapter) _logList.getAdapter()).notifyDataSetChanged();
						} catch (Exception e) {
							//this is not nice
							e.printStackTrace();
						}
				    }
				});

				try {
					Thread.sleep(200);
				} catch (InterruptedException e) {
					e.printStackTrace();
				}
			}
		}
	}

	// ================================================================= App. Logic
	void setPowerSWState(boolean _powerSWState) {
		this._powerSWState = _powerSWState;
		if(_powerSWState)
		{
			_powerSW.setBackgroundDrawable(this.getResources().getDrawable(R.drawable.button_on));
		}
		else
		{
			_powerSW.setBackgroundDrawable(this.getResources().getDrawable(R.drawable.button_off));
		}
	}
}
