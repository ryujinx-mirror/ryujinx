using Ryujinx.HLE.HOS.Kernel.Common;

namespace Ryujinx.HLE.HOS.Kernel.Ipc
{
    class KPort : KAutoObject
    {
        public KServerPort ServerPort { get; private set; }
        public KClientPort ClientPort { get; private set; }

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