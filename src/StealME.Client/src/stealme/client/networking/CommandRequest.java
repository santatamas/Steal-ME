package stealme.client.networking;

import org.msgpack.annotation.Message;

@Message
public class CommandRequest {
	public int Id;
    public int CommandId;
}
