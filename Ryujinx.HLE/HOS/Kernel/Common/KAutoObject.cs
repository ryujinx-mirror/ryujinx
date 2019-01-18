using System.Threading;

namespace Ryujinx.HLE.HOS.Kernel.Common
{
    class KAutoObject
    {
        protected Horizon System;

        private int _referenceCount;

        public KAutoObject(Horizon system)
        {
            System = system;

            _referenceCount = 1;
        }

        public virtual KernelResult SetName(string name)
        {
            if (!System.AutoObjectNames.TryAdd(name, this))
            {
                return KernelResult.InvalidState;
            }

            return KernelResult.Success;
        }

        public static KernelResult RemoveName(Horizon system, string name)
        {
            if (!system.AutoObjectNames.TryRemove(name, out _))
            {
                return KernelResult.NotFound;
            }

            return KernelResult.Success;
        }

        public static KAutoObject FindNamedObject(Horizon system, string name)
        {
            if (system.AutoObjectNames.TryGetValue(name, out KAutoObject obj))
            {
                return obj;
            }

            return null;
        }

        public void IncrementReferenceCount()
        {
            Interlocked.Increment(ref _referenceCount);
        }

        public void DecrementReferenceCount()
        {
            if (Interlocked.Decrement(ref _referenceCount) == 0)
            {
                Destroy();
            }
        }

        protected virtual void Destroy() { }
    }
}