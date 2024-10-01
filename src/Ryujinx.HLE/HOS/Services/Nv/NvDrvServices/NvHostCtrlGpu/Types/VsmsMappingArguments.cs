using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostCtrlGpu.Types
{
    [StructLayout(LayoutKind.Sequential)]
    struct VsmsMappingArguments
    {
        public byte Sm0GpcIndex;
        public byte Sm0TpcIndex;
        public byte Sm1GpcIndex;
        public byte Sm1TpcIndex;
        public uint Reserved;
    }
}
