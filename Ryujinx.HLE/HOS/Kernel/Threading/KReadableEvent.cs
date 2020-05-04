using Ryujinx.HLE.HOS.Kernel.Common;

namespace Ryujinx.HLE.HOS.Kernel.Threading
{
    class KReadableEvent : KSynchronizationObject
    {
        private readonly KEvent _parent;

        private bool _signaled;

        public KReadableEvent(KernelContext context, KEvent parent) : base(context)
        {
            _parent = parent;
        }

        public override void Signal()
        {
            KernelContext.CriticalSection.Enter();

            if (!_signaled)
            {
                _signaled = true;

                base.Signal();
            }

            KernelContext.CriticalSection.Leave();
        }

        public KernelResult Clear()
        {
            _signaled = false;

            return KernelResult.Success;
        }

        public KernelResult ClearIfSignaled()
        {
            KernelResult result;

            KernelContext.CriticalSection.Enter();

            if (_signaled)
            {
                _signaled = false;

                result = KernelResult.Success;
            }
            else
            {
                result = KernelResult.InvalidState;
            }

            KernelContext.CriticalSection.Leave();

            return result;
        }

        public override bool IsSignaled()
        {
            return _signaled;
        }
    }
}