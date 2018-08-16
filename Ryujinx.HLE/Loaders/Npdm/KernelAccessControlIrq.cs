namespace Ryujinx.HLE.Loaders.Npdm
{
    struct KernelAccessControlIrq
    {
        public uint Irq0 { get; private set; }
        public uint Irq1 { get; private set; }

        public KernelAccessControlIrq(uint Irq0, uint Irq1)
        {
            this.Irq0 = Irq0;
            this.Irq1 = Irq1;
        }
    }
}