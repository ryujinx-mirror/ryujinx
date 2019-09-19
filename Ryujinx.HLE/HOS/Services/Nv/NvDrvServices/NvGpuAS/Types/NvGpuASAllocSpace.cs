namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvGpuAS
{
    struct NvGpuASAllocSpace
    {
        public int  Pages;
        public int  PageSize;
        public int  Flags;
        public int  Padding;
        public long Offset;
    }
}