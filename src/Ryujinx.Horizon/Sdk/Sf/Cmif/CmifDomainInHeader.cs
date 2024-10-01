namespace Ryujinx.Horizon.Sdk.Sf.Cmif
{
    struct CmifDomainInHeader
    {
        public CmifDomainRequestType Type;
        public byte ObjectsCount;
        public ushort DataSize;
        public int ObjectId;
        public uint Padding;
        public uint Token;
    }
}
