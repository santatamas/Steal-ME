package stealme.client.networking;

import org.msgpack.annotation.Message;

@Message
public class LocationMessage {
	 public double Latitude;
     public double Longitude;
     public int MNC;
     public int MCC;
     public int LAC;
     public int CellID;
     public int LatestRssi;
}
