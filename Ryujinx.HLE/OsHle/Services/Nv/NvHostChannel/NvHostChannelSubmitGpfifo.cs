namespace Ryujinx.HLE.OsHle.Services.Nv.NvHostChannel
{
    struct NvHostChannelSubmitGpfifo
    {
        public long Address;
        public int  NumEntries;
        public int  Flags;
        public int  SyncptId;
        public int  SyncptValue;
    }
}