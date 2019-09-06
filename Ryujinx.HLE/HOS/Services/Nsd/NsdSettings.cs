namespace Ryujinx.HLE.HOS.Services.Nsd
{
    class NsdSettings
    {
        public bool   Initialized;
        public bool   TestMode;
        public string Environment = "lp1";  // or "dd1" if devkit.
    }
}