namespace Ryujinx.Core.OsHle.Services.Nv.NvGpuAS
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