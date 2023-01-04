using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.Horizon.Common;

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

        public Result Clear()
        {
            _signaled = false;

            return Result.Success;
        }

        public Result ClearIfSignaled()
        {
            Result result;

            KernelContext.CriticalSection.Enter();

            if (_signaled)
            {
                _signaled = false;

                result = Result.Success;
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