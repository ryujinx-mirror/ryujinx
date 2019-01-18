using Ryujinx.HLE.HOS.Kernel.Common;

namespace Ryujinx.HLE.HOS.Kernel.Ipc
{
    class KPort : KAutoObject
    {
        public KServerPort ServerPort { get; }
        public KClientPort ClientPort { get; }

        private long _nameAddress;

        private ChannelState _state;

        public bool IsLight { get; private set; }

        public KPort(Horizon system, int maxSessions, bool isLight, long nameAddress) : base(system)
        {
            ServerPort = new KServerPort(system, this);
            ClientPort = new KClientPort(system, this, maxSessions);

            IsLight      = isLight;
            _nameAddress = nameAddress;

            _state = ChannelState.Open;
        }

        public KernelResult EnqueueIncomingSession(KServerSession session)
        {
            KernelResult result;

            System.CriticalSection.Enter();

            if (_state == ChannelState.Open)
            {
                ServerPort.EnqueueIncomingSession(session);

                result = KernelResult.Success;
            }
            else
            {
                result = KernelResult.PortClosed;
            }

            System.CriticalSection.Leave();

            return result;
        }

        public KernelResult EnqueueIncomingLightSession(KLightServerSession session)
        {
            KernelResult result;

            System.CriticalSection.Enter();

            if (_state == ChannelState.Open)
            {
                ServerPort.EnqueueIncomingLightSession(session);

                result = KernelResult.Success;
            }
            else
            {
                result = KernelResult.PortClosed;
            }

            System.CriticalSection.Leave();

            return result;
        }
    }
}