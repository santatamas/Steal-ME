using StealME.Server.Messaging.Enums;

namespace StealME.Server.Messaging.Requests
{
    public class GetRequest
    {
        public int Id;
        public int GetTypeId;

        public static GetRequest GetStatus()
        {
            return new GetRequest { GetTypeId = (int)GetRequestType.Status};
        }

        public static GetRequest GetIMEI()
        {
            return new GetRequest { GetTypeId = (int)GetRequestType.IMEI };
        }

        public static GetRequest GetProtocolVersion()
        {
            return new GetRequest { GetTypeId = (int)GetRequestType.ProtocolVersion };
        }
    }
}
