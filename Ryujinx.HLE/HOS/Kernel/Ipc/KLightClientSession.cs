using Ryujinx.HLE.HOS.Kernel.Common;

namespace Ryujinx.HLE.HOS.Kernel.Ipc
{
    class KLightClientSession : KAutoObject
    {
        private readonly KLightSession _parent;

        public KLightClientSession(KernelContext context, KLightSession parent) : base(context)
        {
            _parent = parent;
        }
    }
}