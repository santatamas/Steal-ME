package stealme.client.communication.Enums;

public enum GetRequest {
	Status (0), 
	IMEI (1),
	ProtocolVersion(2);
	
	private final int index;   

	GetRequest(int index) {
        this.index = index;
    }

    public int index() { 
        return index; 
    }
}
