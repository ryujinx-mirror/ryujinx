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

        public KPort(KernelContext context, int maxSessions, bool isLight, long nameAddress) : base(context)
        {
            ServerPort = new KServerPort(context, this);
            ClientPort = new KClientPort(context, this, maxSessions);

            IsLight      = isLight;
            _nameAddress = nameAddress;

            _state = ChannelState.Open;
        }

        public KernelResult EnqueueIncomingSession(KServerSession session)
        {
            KernelResult result;

            KernelContext.CriticalSection.Enter();

            if (_state == ChannelState.Open)
            {
                ServerPort.EnqueueIncomingSession(session);

                result = KernelResult.Success;
            }
            else
            {
                result = KernelResult.PortClosed;
            }

            KernelContext.CriticalSection.Leave();

            return result;
        }

        public KernelResult EnqueueIncomingLightSession(KLightServerSession session)
        {
            KernelResult result;

            KernelContext.CriticalSection.Enter();

            if (_state == ChannelState.Open)
            {
                ServerPort.EnqueueIncomingLightSession(session);

                result = KernelResult.Success;
            }
            else
            {
                result = KernelResult.PortClosed;
            }

            KernelContext.CriticalSection.Leave();

            return result;
        }
    }
}