package stealme.client.sensor;

import java.util.List;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import android.os.PowerManager;
import android.os.PowerManager.WakeLock;
import android.util.Log;

public class SensorHandler {

	/** Accuracy configuration */
	private static float threshold = 0.0000002f;
	private static int interval = 1000;

	private static Sensor sensor;
	private static SensorManager sensorManager;
	private static IAccelerometerListener listener;

	/** indicates whether or not Accelerometer Sensor is supported */
	private static Boolean supported;
	/** indicates whether or not Accelerometer Sensor is running */
	private static boolean running = false;

	public static boolean isListening() {
		return running;
	}

	public static void stopListening() {
		running = false;
		/*if(_wakeLock != null)
			_wakeLock.release();*/
		try {
			if (sensorManager != null && sensorEventListener != null) {
				sensorManager.unregisterListener(sensorEventListener);
			}
		} catch (Exception e) {
		}
	}

	public static boolean isSupported() {
		if (supported == null) {
			if (listener != null && listener.getContext() != null) {
				sensorManager = (SensorManager) listener.getContext()
						.getSystemService(Context.SENSOR_SERVICE);
				List<Sensor> sensors = sensorManager
						.getSensorList(Sensor.TYPE_ACCELEROMETER);
				supported = new Boolean(sensors.size() > 0);
			} else {
				supported = Boolean.FALSE;
			}
		}
		return supported;
	}

	public static void configure(int threshold, int interval) {
		SensorHandler.threshold = threshold;
		SensorHandler.interval = interval;
	}

	
	//static PowerManager _powerManager;
	//static WakeLock _wakeLock;
	public static void startListening(
			IAccelerometerListener accelerometerListener) {
		
		/*_powerManager = (PowerManager) accelerometerListener.getContext().getSystemService(Context.POWER_SERVICE);
		_wakeLock = _powerManager.newWakeLock(PowerManager.PARTIAL_WAKE_LOCK, "StealME.Service");
		_wakeLock.setReferenceCounted(true);
		_wakeLock.acquire();*/
		
		sensorManager = (SensorManager) accelerometerListener.getContext()
				.getSystemService(Context.SENSOR_SERVICE);
		List<Sensor> sensors = sensorManager
				.getSensorList(Sensor.TYPE_ACCELEROMETER);
		if (sensors.size() > 0) {
			sensor = sensors.get(0);
			running = sensorManager.registerListener(sensorEventListener,
					sensor, SensorManager.SENSOR_DELAY_NORMAL);
			listener = accelerometerListener;
		}
	}

	public static void startListening(
			IAccelerometerListener accelerometerListener, int threshold,
			int interval) {
		configure(threshold, interval);
		startListening(accelerometerListener);
	}

	public static BroadcastReceiver mReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            // Check action just to be on the safe side.
            if (intent.getAction().equals(Intent.ACTION_SCREEN_OFF) && listener.getAcceleroEnabled()) {
                Log.v("shake mediator screen off","trying re-registration");
                
                try
                {
	                // Unregisters the listener and registers it again.
	                sensorManager.unregisterListener(sensorEventListener, sensor);
	                sensorManager.registerListener(sensorEventListener, sensor,
	                    SensorManager.SENSOR_DELAY_NORMAL);
                }
                catch(Exception e)
                {
                	Log.e("sm.client",e.getMessage());
                }
            }
 }};
	
	private static SensorEventListener sensorEventListener = new SensorEventListener() {

		private float mAccel = 0.00f;
		private float mAccelCurrent = SensorManager.GRAVITY_EARTH;
		private float mAccelLast = SensorManager.GRAVITY_EARTH;

		public void onAccuracyChanged(Sensor sensor, int accuracy) {
		}

		public void onSensorChanged(SensorEvent se) {

			float x = se.values[0];
			float y = se.values[1];
			float z = se.values[2];
			
			Log.d("sm.client", String.valueOf(x) + " " + String.valueOf(y) + " " + String.valueOf(z));
			
			mAccelLast = mAccelCurrent;
			mAccelCurrent = (float) Math.sqrt((double) (x * x + y * y + z * z));
			float delta = Math.abs(mAccelCurrent - mAccelLast);
			mAccel = mAccel * 0.9f + delta; // perform low-cut filter
			mAccel = Math.round(mAccel * 10f) / 100f;

			if (mAccel > 0.07)
				// trigger change event
				listener.onShake(mAccel);
		}
	};
}
