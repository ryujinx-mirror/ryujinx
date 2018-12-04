namespace Ryujinx.HLE.HOS.Kernel
{
    class KMemoryArrange
    {
        public KMemoryArrangeRegion Service     { get; }
        public KMemoryArrangeRegion NvServices  { get; }
        public KMemoryArrangeRegion Applet      { get; }
        public KMemoryArrangeRegion Application { get; }

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