namespace Ryujinx.Horizon.Sdk.Sf.Hipc
{
    struct HipcMetadata
    {
        public int Type;
        public int SendStaticsCount;
        public int SendBuffersCount;
        public int ReceiveBuffersCount;
        public int ExchangeBuffersCount;
        public int DataWordsCount;
        public int ReceiveStaticsCount;
        public bool SendPid;
        public int CopyHandlesCount;
        public int MoveHandlesCount;
    }
}
