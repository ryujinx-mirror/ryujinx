namespace Ryujinx.HLE.HOS.Kernel
{
    class KServerPort : KSynchronizationObject
    {
        private KPort Parent;

        public KServerPort(Horizon System) : base(System) { }

        public void Initialize(KPort Parent)
        {
            this.Parent = Parent;
        }
    }
}