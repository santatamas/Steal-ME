namespace StealME.Server.Networking.Helpers
{
    using System.Threading;

    public class SharedState
    {
        public bool ContinueProcess;
        public int NumberOfClients;
        public AutoResetEvent Ev;
    }
}
