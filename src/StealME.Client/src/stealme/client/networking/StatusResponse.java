package stealme.client.networking;

import org.msgpack.annotation.Message;

@Message
public class StatusResponse {
	 public int RequestId;
     public int BattLvl;
     public int Rx;
}
