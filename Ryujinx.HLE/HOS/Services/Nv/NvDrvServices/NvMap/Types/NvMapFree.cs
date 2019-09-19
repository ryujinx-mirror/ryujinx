namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvMap
{
    struct NvMapFree
    {
        public int  Handle;
        public int  Padding;
        public long Address;
        public int  Size;
        public int  Flags;
    }
}