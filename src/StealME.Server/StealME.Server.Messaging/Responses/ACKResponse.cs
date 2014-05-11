namespace StealME.Server.Messaging.Responses
{
    public class ACKResponse
    {
        public ACKResponse()
        {
        }

        public ACKResponse(int requestId)
        {
            RequestId = requestId;
        }

        public int RequestId;
    }
}
