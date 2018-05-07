namespace Ryujinx.Core.OsHle.Kernel
{
    static class KernelErr
    {
        public const int InvalidAlignment = 102;
        public const int InvalidAddress   = 106;
        public const int InvalidMemRange  = 110;
        public const int InvalidHandle    = 114;
        public const int Timeout          = 117;
        public const int Canceled         = 118;
        public const int CountOutOfRange  = 119;
        public const int InvalidInfo      = 120;
    }
}