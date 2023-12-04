namespace Ryujinx.Horizon.Sdk.Sf.Cmif
{
    struct CmifRequestFormat
    {
#pragma warning disable CS0649 // Field is never assigned to
        public int ObjectId;
        public uint RequestId;
        public uint Context;
        public int DataSize;
        public int ServerPointerSize;
        public int InAutoBuffersCount;
        public int OutAutoBuffersCount;
        public int InBuffersCount;
        public int OutBuffersCount;
        public int InOutBuffersCount;
        public int InPointersCount;
        public int OutPointersCount;
        public int OutFixedPointersCount;
        public int ObjectsCount;
        public int HandlesCount;
        public bool SendPid;
#pragma warning restore CS0649
    }
}
