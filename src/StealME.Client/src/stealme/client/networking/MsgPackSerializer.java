package stealme.client.networking;

import java.io.IOException;

import org.msgpack.MessagePack;

public class MsgPackSerializer {
    private TypeResolver _typeResolver;
    public MsgPackSerializer(TypeResolver typeResolver)
    {
        _typeResolver = typeResolver;
    }

    public byte[] Serialize(Object obj) throws IOException
    {
    	MessagePack msgpack = new MessagePack();
        // Serialize
        byte[] messageContent = msgpack.write(obj);
        TypedMessagePackObject objToSend = new TypedMessagePackObject();
        objToSend.InnerObject = messageContent;
        objToSend.InnerObjectTypeId= _typeResolver.GetIdByType(obj.getClass());
        
        byte[] result = msgpack.write(objToSend);
        
        return result;
    }


    public Object Deserialize(byte[] raw) throws IOException
    {
    	MessagePack msgpack = new MessagePack();
    	TypedMessagePackObject dst = msgpack.read(raw, TypedMessagePackObject.class);
    	return msgpack.read(dst.InnerObject, _typeResolver.GetTypeById(dst.InnerObjectTypeId));
    }
}
