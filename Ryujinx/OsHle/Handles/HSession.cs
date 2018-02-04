namespace Ryujinx.OsHle.Handles
{
    class HSession
    {
        public string ServiceName { get; private set; }

        public bool IsInitialized { get; private set; }

        public int State { get; set; }

        public HSession(string ServiceName)
        {
            this.ServiceName = ServiceName;
        }

        public HSession(HSession Session)
        {
            ServiceName   = Session.ServiceName;
            IsInitialized = Session.IsInitialized;
        }

        public void Initialize()
        {
            IsInitialized = true;
        }
    }
}