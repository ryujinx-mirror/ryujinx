namespace Ryujinx.HLE.HOS.Kernel.Memory
{
    class KMemoryArrange
    {
        public KMemoryArrangeRegion Service     { get; private set; }
        public KMemoryArrangeRegion NvServices  { get; private set; }
        public KMemoryArrangeRegion Applet      { get; private set; }
        public KMemoryArrangeRegion Application { get; private set; }

        public KMemoryArrange(
            KMemoryArrangeRegion service,
            KMemoryArrangeRegion nvServices,
            KMemoryArrangeRegion applet,
            KMemoryArrangeRegion application)
        {
            Service     = service;
            NvServices  = nvServices;
            Applet      = applet;
            Application = application;
        }
    }
}