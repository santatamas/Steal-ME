package stealme.client.networking;

import org.msgpack.annotation.Message;

@Message
public class AuthResponse {
	public boolean Result;
    public int ErrorCode;
}
