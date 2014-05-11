package stealme.client.networking;

import org.msgpack.annotation.Message;

@Message
public class GetRequest {
	 public int Id;
     public int GetTypeId;
}
