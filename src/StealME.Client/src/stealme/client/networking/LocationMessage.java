package stealme.client.networking;

import org.msgpack.annotation.Message;

@Message
public class LocationMessage {
	 public String Latitude;
     public String Longitude;
     public String MNC;
     public String MCC;
     public String LAC;
     public String CellID;
     public String LatestRssi;
}
