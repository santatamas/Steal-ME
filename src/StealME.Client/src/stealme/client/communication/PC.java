package stealme.client.communication;

public class PC {

	//public final static char LOGICAL_MESSAGE_SEPARATOR = '|';
	// message prefixes
	public final static String COMMAND_PREFIX = "CMD";
	public final static String MESSAGE_PREFIX = "MSG";
	public final static String GET_PREFIX = "GET";
	public final static String SET_PREFIX = "SET";

	// command types
	public final static String COMMAND_ACTIVATE = "ACTIVATE";
	public final static String COMMAND_DEACTIVATE = "DEACTIVATE";
	public final static String COMMAND_SIGNAL = "SIGNAL";

	// set types
	public final static String SET_MODE = "MODE";
	public final static String SET_INTERVAL = "INTERVAL";

	// get types
	public final static String GET_STATUS = "STATUS";
	public final static String GET_IMEI = "IMEI";

	// message types
	public final static String MESSAGE_IMEI = "IMEI";
	public final static String MESSAGE_TCP_LOCATION = "TCP";
	public final static String MESSAGE_LOCATION = "LOC";
	public final static String MESSAGE_STATUS = "STAT";
	public final static String MESSAGE_STATE = "STATE";

	// mode types
	public final static String MODE_ACTIVATED = "ACTIVATED";
	public final static String MODE_DEACTIVATED = "DEACTIVATED";
	public final static String MODE_ALARM = "ALARM";
}
