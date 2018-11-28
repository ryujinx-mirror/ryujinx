namespace Ryujinx.HLE.HOS.Kernel
{
    class KPort : KAutoObject
    {
        public KServerPort ServerPort { get; private set; }
        public KClientPort ClientPort { get; private set; }

        private long NameAddress;
        private bool IsLight;

        public KPort(Horizon System) : base(System)
        {
            ServerPort = new KServerPort(System);
            ClientPort = new KClientPort(System);
        }

        public void Initialize(int MaxSessions, bool IsLight, long NameAddress)
        {
            ServerPort.Initialize(this);
            ClientPort.Initialize(this, MaxSessions);

            this.IsLight     = IsLight;
            this.NameAddress = NameAddress;
        }
    }
}