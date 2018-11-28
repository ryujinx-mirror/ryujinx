namespace Ryujinx.HLE.HOS.Kernel
{
    class KAutoObject
    {
        protected Horizon System;

        public KAutoObject(Horizon System)
        {
            this.System = System;
        }

        public virtual KernelResult SetName(string Name)
        {
            if (!System.AutoObjectNames.TryAdd(Name, this))
            {
                return KernelResult.InvalidState;
            }

            return KernelResult.Success;
        }

        public static KernelResult RemoveName(Horizon System, string Name)
        {
            if (!System.AutoObjectNames.TryRemove(Name, out _))
            {
                return KernelResult.NotFound;
            }

            return KernelResult.Success;
        }

        public static KAutoObject FindNamedObject(Horizon System, string Name)
        {
            if (System.AutoObjectNames.TryGetValue(Name, out KAutoObject Obj))
            {
                return Obj;
            }

            return null;
        }
    }
}