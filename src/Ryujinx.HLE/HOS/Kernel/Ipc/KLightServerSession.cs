using Ryujinx.HLE.HOS.Kernel.Common;

namespace Ryujinx.HLE.HOS.Kernel.Ipc
{
    class KLightServerSession : KAutoObject
    {
        private readonly KLightSession _parent;

        public KLightServerSession(KernelContext context, KLightSession parent) : base(context)
        {
            _parent = parent;
        }
    }
}