namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvGpuAS
{
    struct NvGpuASMapBufferEx
    {
        public int  Flags;
        public int  Kind;
        public int  NvMapHandle;
        public int  PageSize;
        public long BufferOffset;
        public long MappingSize;
        public long Offset;
    }
}