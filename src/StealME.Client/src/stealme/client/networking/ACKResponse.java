package stealme.client.networking;

import org.msgpack.annotation.Message;

@Message
public class ACKResponse {
	public int RequestId;
}
