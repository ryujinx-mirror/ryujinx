namespace Ryujinx.Core.OsHle.Services.Nv
{
    class NvMap
    {
        public int  Handle;
        public int  Id;
        public int  Size;
        public int  Align;
        public int  Kind;
        public long CpuAddress;
        public long GpuAddress;
    }
}