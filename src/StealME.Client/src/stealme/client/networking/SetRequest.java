package stealme.client.networking;

import org.msgpack.annotation.Message;

@Message
public class SetRequest {
	 public int SetTypeID;
     public int SetValue;
}
