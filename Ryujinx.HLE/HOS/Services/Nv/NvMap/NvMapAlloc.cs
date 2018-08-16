namespace Ryujinx.HLE.HOS.Services.Nv.NvMap
{
    struct NvMapAlloc
    {
        public int  Handle;
        public int  HeapMask;
        public int  Flags;
        public int  Align;
        public long Kind;
        public long Address;
    }
}