namespace Ryujinx.HLE.OsHle.Services.Nv.NvHostChannel
{
    struct NvHostChannelSubmitGpfifo
    {
        public long Gpfifo;
        public int  NumEntries;
        public int  Flags;
        public int  SyncptId;
        public int  SyncptValue;
    }
}