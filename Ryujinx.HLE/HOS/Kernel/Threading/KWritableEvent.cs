using Ryujinx.HLE.HOS.Kernel.Common;

namespace Ryujinx.HLE.HOS.Kernel.Threading
{
    class KWritableEvent
    {
        private KEvent _parent;

        public KWritableEvent(KEvent parent)
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