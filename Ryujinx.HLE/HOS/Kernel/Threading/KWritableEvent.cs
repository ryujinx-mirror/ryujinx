using Ryujinx.HLE.HOS.Kernel.Common;

namespace Ryujinx.HLE.HOS.Kernel.Threading
{
    class KWritableEvent : KAutoObject
    {
        private KEvent _parent;

        public KWritableEvent(Horizon system, KEvent parent) : base(system)
        {
            _parent = parent;
        }

        public void Signal()
        {
            _parent.ReadableEvent.Signal();
        }

        public KernelResult Clear()
        {
            return _parent.ReadableEvent.Clear();
        }
    }
}