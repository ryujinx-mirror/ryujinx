using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.Horizon.Common;

namespace Ryujinx.HLE.HOS.Kernel.Ipc
{
    class KPort : KAutoObject
    {
        public KServerPort ServerPort { get; }
        public KClientPort ClientPort { get; }

#pragma warning disable IDE0052 // Remove unread private member
        private readonly string _name;
#pragma warning restore IDE0052

        private readonly ChannelState _state;

        public bool IsLight { get; private set; }

        public KPort(KernelContext context, int maxSessions, bool isLight, string name) : base(context)
        {
            ServerPort = new KServerPort(context, this);
            ClientPort = new KClientPort(context, this, maxSessions);

            IsLight = isLight;
            _name = name;

            _state = ChannelState.Open;
        }

        public Result EnqueueIncomingSession(KServerSession session)
        {
            Result result;

            KernelContext.CriticalSection.Enter();

            if (_state == ChannelState.Open)
            {
                ServerPort.EnqueueIncomingSession(session);

                result = Result.Success;
            }
            else
            {
                result = KernelResult.PortClosed;
            }

            KernelContext.CriticalSection.Leave();

            return result;
        }

        public Result EnqueueIncomingLightSession(KLightServerSession session)
        {
            Result result;

            KernelContext.CriticalSection.Enter();

            if (_state == ChannelState.Open)
            {
                ServerPort.EnqueueIncomingLightSession(session);

                result = Result.Success;
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
