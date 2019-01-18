using Ryujinx.HLE.HOS.Kernel.Common;

namespace Ryujinx.HLE.HOS.Kernel.Ipc
{
    class KLightServerSession : KAutoObject
    {
        private KLightSession _parent;

        public KLightServerSession(Horizon system, KLightSession parent) : base(system)
        {
            _parent = parent;
        }
    }
}