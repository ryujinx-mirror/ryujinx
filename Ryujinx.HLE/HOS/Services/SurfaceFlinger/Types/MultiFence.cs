using Ryujinx.HLE.HOS.Services.Nv.Types;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.SurfaceFlinger
{
    [StructLayout(LayoutKind.Explicit, Size = 0x24)]
    struct MultiFence
    {
        [FieldOffset(0x0)]
        public int FenceCount;

        [FieldOffset(0x4)]
        public NvFence Fence0;

        [FieldOffset(0xC)]
        public NvFence Fence1;

        [FieldOffset(0x14)]
        public NvFence Fence2;

        [FieldOffset(0x1C)]
        public NvFence Fence3;
    }
}