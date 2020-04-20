using Ryujinx.HLE.HOS.Kernel.Common;

namespace Ryujinx.HLE.HOS.Kernel.Ipc
{
    class KLightSession : KAutoObject
    {
        public KLightServerSession ServerSession { get; }
        public KLightClientSession ClientSession { get; }

        public KLightSession(Horizon system) : base(system)
        {
            ServerSession = new KLightServerSession(system, this);
            ClientSession = new KLightClientSession(system, this);
        }
    }
}