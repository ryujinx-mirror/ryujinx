namespace Ryujinx.HLE.HOS.Services.Sockets.Nsd
{
    class NsdSettings
    {
        public bool   Initialized;
        public bool   TestMode;
        public string Environment = "lp1";  // or "dd1" if devkit.
    }
}