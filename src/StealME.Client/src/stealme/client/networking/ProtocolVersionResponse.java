package stealme.client.networking;

import org.msgpack.annotation.Message;

@Message
public class ProtocolVersionResponse {
	 public int Version;
     public int Id;
}
