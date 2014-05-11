package stealme.client.networking;

import org.msgpack.annotation.Message;

@Message
public class AuthRequest {
	public String IMEI;
}
