namespace Ryujinx.Horizon.Sdk.Sf.Cmif
{
    struct CmifInHeader
    {
        public uint Magic;
        public uint Version;
        public uint CommandId;
        public uint Token;
    }
}
