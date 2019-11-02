using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostAsGpu.Types
{
    [StructLayout(LayoutKind.Sequential)]
    struct MapBufferExArguments
    {
        public AddressSpaceFlags Flags;
        public int               Kind;
        public int               NvMapHandle;
        public int               PageSize;
        public long              BufferOffset;
        public long              MappingSize;
        public long              Offset;
    }
}
