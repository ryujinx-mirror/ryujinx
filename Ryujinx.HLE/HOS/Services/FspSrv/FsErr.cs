namespace Ryujinx.HLE.HOS.Services.FspSrv
{
    static class FsErr
    {
        public const int PathDoesNotExist  = 1;
        public const int PathAlreadyExists = 2;
        public const int PathAlreadyInUse  = 7;
        public const int PartitionNotFound = 1001;
        public const int InvalidInput      = 6001;
    }
}