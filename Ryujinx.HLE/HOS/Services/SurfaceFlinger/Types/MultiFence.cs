using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.SurfaceFlinger
{
    [StructLayout(LayoutKind.Explicit, Size = 0x24)]
    struct MultiFence
    {
        [FieldOffset(0x0)]
        public int FenceCount;

        [FieldOffset(0x4)]
        public Fence Fence0;

        [FieldOffset(0xC)]
        public Fence Fence1;

        [FieldOffset(0x14)]
        public Fence Fence2;

        [FieldOffset(0x1C)]
        public Fence Fence3;
    }
}