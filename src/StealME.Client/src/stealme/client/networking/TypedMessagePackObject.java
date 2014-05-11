package stealme.client.networking;
import org.msgpack.annotation.Message;

@Message
public class TypedMessagePackObject {
	public int InnerObjectTypeId;
    public byte[] InnerObject;
}
