using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Android
{
    [StructLayout(LayoutKind.Explicit)]
    struct QueueBufferObject
    {
        [FieldOffset(0x0)]
        public long Timestamp;

        [FieldOffset(0x8)]
        public int IsAutoTimestamp;

        [FieldOffset(0xC)]
        public Rect Crop;

        [FieldOffset(0x1C)]
        public int ScalingMode;

        [FieldOffset(0x20)]
        public HalTransform Transform;

        [FieldOffset(0x24)]
        public int StickyTransform;

        [FieldOffset(0x28)]
        public int Unknown;

        [FieldOffset(0x2C)]
        public int SwapInterval;

        [FieldOffset(0x30)]
        public MultiFence Fence;
    }
}