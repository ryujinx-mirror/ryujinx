namespace Ryujinx.HLE.HOS.Kernel.Common
{
    class KAutoObject
    {
        protected Horizon System;

        public KAutoObject(Horizon system)
        {
            System = system;
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
    }
}