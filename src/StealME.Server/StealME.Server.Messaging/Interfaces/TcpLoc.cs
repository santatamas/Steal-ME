namespace StealME.Server.Messaging.Protocol
{
    public struct TcpLoc
    {
        public string IMEI { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string MNC { get; set; }
        public string MCC { get; set; }
        public string LAC { get; set; }
        public string CellID { get; set; }
        public string LatestRssi { get; set; }
    }
}
