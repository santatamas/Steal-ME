package stealme.client.networking;

public class IntByteConverter {
	public static int byteArrayToInt(byte[] b) 
	{
	    return   b[0] & 0xFF |
	            (b[1] & 0xFF) << 8 |
	            (b[2] & 0xFF) << 16 |
	            (b[3] & 0xFF) << 24;
	}

	public static byte[] intToByteArray(int a)
	{
	    return new byte[] {
	    		
		        (byte) (a & 0xFF),
		        (byte) ((a >> 8) & 0xFF), 
		        (byte) ((a >> 16) & 0xFF),
	    		(byte) ((a >> 24) & 0xFF),
	    };
	}
}
