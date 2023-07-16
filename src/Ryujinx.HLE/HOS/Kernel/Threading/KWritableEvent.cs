using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.Horizon.Common;

namespace Ryujinx.HLE.HOS.Kernel.Threading
{
    class KWritableEvent : KAutoObject
    {
        private readonly KEvent _parent;

        public KWritableEvent(KernelContext context, KEvent parent) : base(context)
        {
            _parent = parent;
        }

        public void Signal()
        {
            _parent.ReadableEvent.Signal();
        }

        public Result Clear()
        {
            return _parent.ReadableEvent.Clear();
        }
    }
}
