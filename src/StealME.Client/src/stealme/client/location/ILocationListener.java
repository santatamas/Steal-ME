package stealme.client.location;

import android.content.Context;
import android.location.Location;

public interface ILocationListener {
	public Context getContext();
	public void onLocationChanged(Location location);
	public Integer getInterval();
	public void Alert();
}
