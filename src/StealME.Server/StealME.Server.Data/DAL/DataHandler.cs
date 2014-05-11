namespace StealME.Server.Data.DAL
{
    public class DataHandler
    {
        public static DataHandler Instance
        {
            get
            {
                if (_instance == null) _instance = new DataHandler();
                return _instance;
            }
        }

        private StealMEEntities _context;
        private static DataHandler _instance;

        public DataHandler()
        {
            this._context = new StealMEEntities();
        }

        public static StealMEEntities GetContext()
        {
            return new StealMEEntities();
        }
    }
}
