namespace StealME.Server.Networking.EventArgs
{
    public class RawMessageEventArgs : System.EventArgs
    {
        public byte[] Message { get; set; }
        //public int MessageType { get; set; }
    }
}
