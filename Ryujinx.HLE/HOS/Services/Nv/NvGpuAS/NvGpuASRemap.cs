namespace Ryujinx.HLE.HOS.Services.Nv.NvGpuAS
{
    struct NvGpuASRemap
    {
        public short Flags;
        public short Kind;
        public int   NvMapHandle;
        public int   Padding;
        public int   Offset;
        public int   Pages;
    }
}