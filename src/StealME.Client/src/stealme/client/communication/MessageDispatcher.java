package stealme.client.communication;

import stealme.client.communication.Enums.Command;
import stealme.client.communication.Enums.GetRequest;
import stealme.client.communication.Enums.Message;
import stealme.client.communication.Enums.SetRequest;
import stealme.client.networking.CommandRequest;
import stealme.client.networking.TypeResolver;

public class MessageDispatcher {

	IMessageListener _listener;
	
	public MessageDispatcher(IMessageListener listener){
		_listener = listener;
	}
	
	public void processMessage(String message)
	{
		String prefix = message.substring(0, 3).toUpperCase();
		String instruction = message.substring(4, message.length())
				.toUpperCase();	
		
		if (prefix.equals(PC.COMMAND_PREFIX)) {
			DispatchCommand(instruction);
		} else if (prefix.equals(PC.SET_PREFIX)) {
			DispatchSetRequest(instruction);
		} else if (prefix.equals(PC.GET_PREFIX)) {
			DispatchGetRequest(instruction);
		} else if (prefix.equals(PC.MESSAGE_PREFIX)) {
			DispatchMessage(instruction);
		}
	}
	
	TypeResolver _typeResolver = new TypeResolver();
	public void processMessageObject(Object message)
	{
		Class messageType = message.getClass();
		int typeId = _typeResolver.GetIdByType(messageType);
				
		// todo: it's temporary use a dictionary instead
		switch(typeId)
		{
			// commandrequest
		 	case 12:
		 		CommandRequest cmd = (CommandRequest)message;
		 		Command c = null;
		 		for (Command command : Command.values()) {
		 	       if(command.index() == cmd.CommandId)
		 	       {
		 	    	   c = command;
		 	       }
		 	    }
		 		_listener.onCommandReceived(c);
			 break;
			// getrequest
		 	case 13:
		 		stealme.client.networking.GetRequest getReq = (stealme.client.networking.GetRequest)message;
		 		GetRequest g = null;
		 		for (GetRequest command : GetRequest.values()) {
			 	       if(command.index() == getReq.GetTypeId)
			 	       {
			 	    	   g = command;
			 	       }
			 	    }
			 		_listener.onGetRequest(g);
		 		break;
		}
	}
	
	private void DispatchMessage(String instruction) {
		
		String[] params = instruction.split("=");
		
		if (params[0].equals(PC.MESSAGE_STATUS)) {
			_listener.onMessage(Message.Status, params[1]);
		} else if (params[0].equals(PC.MESSAGE_LOCATION)) {
			_listener.onMessage(Message.Location, params[1]);
			} else if (params[0].equals(PC.MESSAGE_STATE)) {
				_listener.onMessage(Message.State, params[1]);
			}
		
	}
	private void DispatchGetRequest(String instruction) {
		if (instruction.equals(PC.GET_STATUS)) {
			_listener.onGetRequest(GetRequest.Status);
		}else if (instruction.equals(PC.GET_IMEI)) {
			_listener.onGetRequest(GetRequest.IMEI);
		}
	}
	private void DispatchSetRequest(String instruction) {
		//set request is always in form of "key=value"		
		String[] params = instruction.split("=");
		
		if (params[0].equals(PC.SET_MODE)) {
			_listener.onSetRequest(SetRequest.Mode,params[1]);
		} else if (params[0].equals(PC.SET_INTERVAL)) {
			_listener.onSetRequest(SetRequest.Interval,params[1]);
		}
	}
	private void DispatchCommand(String instruction) {
		if (instruction.equals(PC.COMMAND_ACTIVATE)) {
			_listener.onCommandReceived(Command.Activate);
		} else if (instruction.equals(PC.COMMAND_DEACTIVATE)) {
			_listener.onCommandReceived(Command.Deactivate);
		} else if (instruction.equals(PC.COMMAND_SIGNAL)) {
			_listener.onCommandReceived(Command.Signal);
		}
	}
}
