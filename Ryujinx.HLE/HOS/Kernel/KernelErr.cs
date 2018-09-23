namespace Ryujinx.HLE.HOS.Kernel
{
    static class KernelErr
    {
        public const int ThreadTerminating = 59;
        public const int InvalidSize       = 101;
        public const int InvalidAddress    = 102;
        public const int OutOfMemory       = 104;
        public const int HandleTableFull   = 105;
        public const int NoAccessPerm      = 106;
        public const int InvalidPermission = 108;
        public const int InvalidMemRange   = 110;
        public const int InvalidPriority   = 112;
        public const int InvalidCoreId     = 113;
        public const int InvalidHandle     = 114;
        public const int InvalidMaskValue  = 116;
        public const int Timeout           = 117;
        public const int Cancelled         = 118;
        public const int CountOutOfRange   = 119;
        public const int InvalidEnumValue  = 120;
        public const int InvalidThread     = 122;
        public const int InvalidState      = 125;
    }
}