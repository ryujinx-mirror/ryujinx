namespace Ryujinx.HLE.HOS.Kernel
{
    class KClientPort : KSynchronizationObject
    {
        private int SessionsCount;
        private int CurrentCapacity;
        private int MaxSessions;

        private KPort Parent;

        public KClientPort(Horizon System) : base(System) { }

        public void Initialize(KPort Parent, int MaxSessions)
        {
            this.MaxSessions = MaxSessions;
            this.Parent      = Parent;
        }

        public new static KernelResult RemoveName(Horizon System, string Name)
        {
            KAutoObject FoundObj = KAutoObject.FindNamedObject(System, Name);

            if (!(FoundObj is KClientPort))
            {
                return KernelResult.NotFound;
            }

            return KAutoObject.RemoveName(System, Name);
        }
    }
}