namespace Ryujinx.HLE.HOS.Kernel
{
    class KPort : KAutoObject
    {
        public KServerPort ServerPort { get; }
        public KClientPort ClientPort { get; }

        private long _nameAddress;
        private bool _isLight;

        public KPort(Horizon system) : base(system)
        {
            ServerPort = new KServerPort(system);
            ClientPort = new KClientPort(system);
        }

        public void Initialize(int maxSessions, bool isLight, long nameAddress)
        {
            ServerPort.Initialize(this);
            ClientPort.Initialize(this, maxSessions);

            _isLight     = isLight;
            _nameAddress = nameAddress;
        }
    }
}