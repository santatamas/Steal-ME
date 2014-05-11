package stealme.client.networking;

import java.util.Dictionary;
import java.util.HashMap;
import java.util.Map;
import java.util.Map.Entry;

public class TypeResolver {

	 Map<Integer, Class> _typeDictionary = new HashMap<Integer, Class>(){{
		    put(0, TestRequest.class);
		    put(1, TestResponse.class);
		    
		    put(11, AuthRequest.class);
		    put(12, CommandRequest.class);
		    put(13, GetRequest.class);
		    put(14, SetRequest.class);
		    put(15, PingRequest.class);
		    
		    put(21, ACKResponse.class);
		    put(22, AuthResponse.class);
		    put(23, StatusResponse.class);
		    put(24, PingResponse.class);
		    put(25, ProtocolVersionResponse.class);
		    
		    put(31, LocationMessage.class);
		    put(32, TrackerStateMessage.class);    
		}};
	 
   
	public Class[] GetTypes()
	{
		return (Class[]) _typeDictionary.values().toArray();
	}

	public Class GetTypeById(int id)
	{
		return _typeDictionary.get(id);
	}
	
	public int GetIdByType(Class type)
	{
		return getKeyByValue(_typeDictionary, type);
	}
	
	public static <T, E> T getKeyByValue(Map<T, E> map, E value) {
	    for (Entry<T, E> entry : map.entrySet()) {
	        if (value.equals(entry.getValue())) {
	            return entry.getKey();
	        }
	    }
	    return null;
	}
}
