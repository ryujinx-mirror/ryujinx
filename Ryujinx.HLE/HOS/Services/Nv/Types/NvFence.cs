using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Nv.Types
{
    [StructLayout(LayoutKind.Sequential, Size = 0x8)]
    internal struct NvFence
    {
        public uint Id;
        public uint Value;
    }
}
