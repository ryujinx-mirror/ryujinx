namespace Ryujinx.HLE.OsHle.Services.Nv
{
    static class NvResult
    {
        public const int NotAvailableInProduction = 196614;
        public const int Success                  = 0;
        public const int TryAgain                 = -11;
        public const int OutOfMemory              = -12;
        public const int InvalidInput             = -22;
        public const int NotSupported             = -25;
        public const int Restart                  = -85;
        public const int TimedOut                 = -110;
    }
}
