using Ryujinx.HLE.HOS.Kernel.Common;

namespace Ryujinx.HLE.HOS.Kernel.Ipc
{
    class KLightClientSession : KAutoObject
    {
        private KLightSession _parent;

        public KLightClientSession(Horizon system, KLightSession parent) : base(system)
        {
            _parent = parent;
        }
    }
}