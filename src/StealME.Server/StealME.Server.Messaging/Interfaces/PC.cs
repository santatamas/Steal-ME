namespace StealME.Server.Messaging.Protocol
{
    using System;

    public class PC
    {
        public const char LOGICAL_MESSAGE_SEPARATOR = '|';

        //message prefixes
        public const String COMMAND_PREFIX = "CMD";
        public const String MESSAGE_PREFIX = "MSG";
        public const String GET_PREFIX = "GET";
        public const String SET_PREFIX = "SET";

        //command types
        public const String COMMAND_ACTIVATE = "ACTIVATE";
        public const String COMMAND_DEACTIVATE = "DEACTIVATE";
        public const String COMMAND_SIGNAL = "SIGNAL";

        //set types
        public const String SET_MODE = "MODE";
        public const String SET_INTERVAL = "INTERVAL";

        //get types
        public const String GET_STATUS = "STATUS";
        public const String GET_IMEI = "IMEI";

        //message types
        public const String MESSAGE_IMEI = "IMEI";
        public const String MESSAGE_TCP_LOCATION = "TCP";
        public const String MESSAGE_LOCATION = "LOC";
        public const String MESSAGE_STATUS = "STAT";
        public const String MESSAGE_STATE = "STATE";

        //mode types
        public const String MODE_ACTIVATED = "ACTIVATED";
        public const String MODE_DEACTIVATED = "DEACTIVATED";
        public const String MODE_ALARM = "ALARM";
    }
}
