package stealme.client.location;

import android.app.Notification;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.location.Location;
import android.location.LocationListener;
import android.location.LocationManager;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;

public class LocationHandler {
	private static float threshold 	= 0.002f;
	private static int interval 	= 1000;
	
	//private static final long MINIMUM_DISTANCECHANGE_FOR_UPDATE = 1; // in Meters
    //private static final long MINIMUM_TIME_BETWEEN_UPDATE = 1000; // in Milliseconds
    
    private static final long POINT_RADIUS = 15; // in Meters
    private static final long PROX_ALERT_EXPIRATION = -1; 

    private static final String POINT_LATITUDE_KEY = "POINT_LATITUDE_KEY";
    private static final String POINT_LONGITUDE_KEY = "POINT_LONGITUDE_KEY";
    
    private static final String PROX_ALERT_INTENT = "com.javacodegeeks.android.lbs.ProximityAlert";
	private static PendingIntent _lastProximityIntent;
    
	private static ILocationListener listener;

	private static boolean _waitingForFirstAccurateLocation = true;
	
	private static LocationManager locManager;
	
	private static Boolean supported;
	private static boolean running = false;

	public static boolean isListening() {
		return running;
	}

	public static void stopListening() {
		running = false;
		try {
			if (locManager != null && listener != null) {
				locManager.removeUpdates(locListener);
			}
		} catch (Exception e) {
		}
	}

	public static boolean isSupported() {
		if (supported == null) {

		}
		return supported;
	}

	public static void configure(int threshold, int interval) {
		LocationHandler.threshold = threshold;
		LocationHandler.interval = interval;
	}

	public static void startListening(ILocationListener smLocationListener) {
		Handler mHandler = new Handler(Looper.getMainLooper());

		final ILocationListener finListener = smLocationListener;
		mHandler.post(new Runnable() {
	          public void run() {
				try{
					
				locManager = (LocationManager) finListener.getContext().getSystemService(Context.LOCATION_SERVICE);		
				locManager.requestLocationUpdates(LocationManager.GPS_PROVIDER, finListener.getInterval() * 1000, 0,locListener);
						
				_waitingForFirstAccurateLocation = true;
				listener = finListener;
				}
				catch(Exception ex)
				{
					Log.e("SM", ex.getMessage());		
				}
		    }
		});
	}
	
	public static void startListening(
			ILocationListener smLocationListener, int threshold,
			int interval) {
		configure(threshold, interval);
		startListening(smLocationListener);
	}
	
	private static void addProximityAlert(double latitude, double longitude) {
	        
		if(_lastProximityIntent != null)
	        locManager.removeProximityAlert(_lastProximityIntent);
		
	        Intent intent = new Intent(PROX_ALERT_INTENT);
	        _lastProximityIntent = PendingIntent.getBroadcast(listener.getContext(), 0, intent, 0);

	        locManager.addProximityAlert(
	            latitude, // the latitude of the central point of the alert region
	            longitude, // the longitude of the central point of the alert region
	            POINT_RADIUS, // the radius of the central point of the alert region, in meters
	            PROX_ALERT_EXPIRATION, // time for this proximity alert, in milliseconds, or -1 to indicate no expiration 
	            _lastProximityIntent // will be used to generate an Intent to fire when entry to or exit from the alert region is detected
	       );
	        
	       IntentFilter filter = new IntentFilter(PROX_ALERT_INTENT);  
	       listener.getContext().registerReceiver(new ProximityIntentReceiver(), filter);
	       
	    }
	
	public static LocationListener locListener = new LocationListener() {
		private Location currentLoc;
		public void onLocationChanged(Location location) {
			
			if(location.getAccuracy() < POINT_RADIUS)
			{
				_waitingForFirstAccurateLocation = false;
				addProximityAlert(location.getLatitude(), location.getLongitude());
			}
			
			if (isBetterLocation(location, this.currentLoc)) {
			//Location is changed - it's time to notify our listener!
			LocationHandler.listener.onLocationChanged(location);
			}
		}

		public void onProviderDisabled(String provider) {
			//GPS Off
		}

		public void onProviderEnabled(String provider) {
			//GPS On
		}

		public void onStatusChanged(String provider, int status, Bundle extras) {
			//We don't need this kind of sillyness :)
		}
		
		private static final int HALF_MINUTE = 1000 * 30;
		
		protected boolean isBetterLocation(Location location,
				Location currentBestLocation) {
			if(location == null)
			{
				return false;
			}
			
			if (currentBestLocation == null) {
				// A new location is always better than no location
				return true;
			}

			// Check whether the new location fix is newer or older
			long timeDelta = location.getTime() - currentBestLocation.getTime();
			boolean isSignificantlyNewer = timeDelta > HALF_MINUTE;
			boolean isSignificantlyOlder = timeDelta < -HALF_MINUTE;
			boolean isNewer = timeDelta > 0;

			// If it's been more than two minutes since the current location, use
			// the new location
			// because the user has likely moved
			if (isSignificantlyNewer) {
				return true;
				// If the new location is more than two minutes older, it must be
				// worse
			} else if (isSignificantlyOlder) {
				return false;
			}

			// Check whether the new location fix is more or less accurate
			int accuracyDelta = (int) (location.getAccuracy() - currentBestLocation
					.getAccuracy());
			boolean isLessAccurate = accuracyDelta > 0;
			boolean isMoreAccurate = accuracyDelta < 0;
			boolean isSignificantlyLessAccurate = accuracyDelta > 200;

			// Check if the old and new location are from the same provider
			boolean isFromSameProvider = isSameProvider(location.getProvider(),
					currentBestLocation.getProvider());

			// Determine location quality using a combination of timeliness and
			// accuracy
			if (isMoreAccurate) {
				return true;
			} else if (isNewer && !isLessAccurate) {
				return true;
			} else if (isNewer && !isSignificantlyLessAccurate
					&& isFromSameProvider) {
				return true;
			}
			return false;
		}

		/** Checks whether two providers are the same */
		private boolean isSameProvider(String provider1, String provider2) {
			if (provider1 == null) {
				return provider2 == null;
			}
			return provider1.equals(provider2);
		}

	};

	public static class ProximityIntentReceiver extends BroadcastReceiver {
	    
	    @Override
	    public void onReceive(Context context, Intent intent) {
	        
	        String key = LocationManager.KEY_PROXIMITY_ENTERING;

	        Boolean entering = intent.getBooleanExtra(key, false);
	        
	        if (entering) {
	            Log.d(getClass().getSimpleName(), "entering");
	        }
	        else {
	        	listener.Alert();
	            Log.d(getClass().getSimpleName(), "exiting");
	        }
	    }	    
	}
}
