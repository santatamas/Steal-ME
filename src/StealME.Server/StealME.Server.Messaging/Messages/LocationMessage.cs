namespace StealME.Server.Messaging.Messages
{
    public class LocationMessage
    {
        public double Latitude;
        public double Longitude;
        public int MNC;
        public int MCC;
        public int LAC;
        public int CellID;
        public int LatestRssi;
    }
}
