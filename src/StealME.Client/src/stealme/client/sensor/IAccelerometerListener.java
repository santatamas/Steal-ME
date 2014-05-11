package stealme.client.sensor;

import android.content.Context;

public interface IAccelerometerListener {

	public void onAccelerationChanged(float x, float y, float z);
	
	public void onShake(float force);	
	
	public Context getContext();
	public boolean getAcceleroEnabled();
} 
