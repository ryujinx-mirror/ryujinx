using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.SurfaceFlinger
{
    [StructLayout(LayoutKind.Explicit, Size = 0x58)]
    struct NvGraphicBufferSurface
    {
        [FieldOffset(0)]
        public uint Width;

        [FieldOffset(0x4)]
        public uint Height;

        [FieldOffset(0x8)]
        public ColorFormat ColorFormat;

        [FieldOffset(0x10)]
        public int Layout;

        [FieldOffset(0x14)]
        public int Pitch;

        [FieldOffset(0x18)]
        public int NvMapHandle;

        [FieldOffset(0x1C)]
        public int Offset;

        [FieldOffset(0x20)]
        public int Kind;

        [FieldOffset(0x24)]
        public int BlockHeightLog2;

        [FieldOffset(0x28)]
        public int ScanFormat;

        [FieldOffset(0x30)]
        public long Flags;

        [FieldOffset(0x38)]
        public long Size;
    }
}
