namespace Ryujinx.HLE.OsHle.Kernel
{
    static class KernelErr
    {
        public const int InvalidAlignment = 102;
        public const int InvalidAddress   = 106;
        public const int InvalidMemRange  = 110;
        public const int InvalidPriority  = 112;
        public const int InvalidCoreId    = 113;
        public const int InvalidHandle    = 114;
        public const int InvalidCoreMask  = 116;
        public const int Timeout          = 117;
        public const int Canceled         = 118;
        public const int CountOutOfRange  = 119;
        public const int InvalidInfo      = 120;
        public const int InvalidThread    = 122;
        public const int InvalidState     = 125;
    }
}