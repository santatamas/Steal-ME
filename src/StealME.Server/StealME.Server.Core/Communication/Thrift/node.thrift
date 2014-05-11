namespace csharp StealME.Server.Networking.Thrift

	/**
	* Predefined tracker commands (not really flexible, but it'll do for start)
	*/
	enum tracker_command {
	ACTIVATE = 0,
	DEACTIVATE = 1,
	UPDATE_STATUS = 2,
	SIGNAL = 3,
	}



/**
 * Standard base service
 */
service NodeService {

	/**
	* Returns th ids of connected (and authenticated!) trackers
	*/
	list<i32> getConnectedTrackerIds(),

	/**
	* Checks is the given tracker is connected to the Node
	*/
	bool isTrackerConnected(1: i32 trackerId),

	/**
	* Returns the id of this Node
	*/
	string getNodeId(),

	/**
	* Sends a predefined command to the tracker
	*/
	oneway void sendCommand(1: i32 trackerId, 2: tracker_command command),
}