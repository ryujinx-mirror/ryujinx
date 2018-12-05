namespace Ryujinx.HLE.HOS.Kernel
{
    class KMemoryArrange
    {
        public KMemoryArrangeRegion Service     { get; private set; }
        public KMemoryArrangeRegion NvServices  { get; private set; }
        public KMemoryArrangeRegion Applet      { get; private set; }
        public KMemoryArrangeRegion Application { get; private set; }

        public KMemoryArrange(
            KMemoryArrangeRegion Service,
            KMemoryArrangeRegion NvServices,
            KMemoryArrangeRegion Applet,
            KMemoryArrangeRegion Application)
        {
            this.Service     = Service;
            this.NvServices  = NvServices;
            this.Applet      = Applet;
            this.Application = Application;
        }
    }
}