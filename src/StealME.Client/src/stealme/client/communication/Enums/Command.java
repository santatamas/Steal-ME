package stealme.client.communication.Enums;

public enum Command {
	Activate (0), 
	Deactivate (1), 
	Signal (2);
	
	private final int index;   

	Command(int index) {
        this.index = index;
    }

    public int index() { 
        return index; 
    }
}
