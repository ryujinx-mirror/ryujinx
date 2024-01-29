using System.Runtime.InteropServices;

namespace Ryujinx.Horizon.Sdk.Settings.System
{
    [StructLayout(LayoutKind.Sequential, Size = 0x28, Pack = 0x4)]
    struct BacklightSettings
    {
        // TODO: Determine field names.
        public uint Unknown0x00;
        public float Unknown0x04;
        // 1st group
        public float Unknown0x08;
        public float Unknown0x0C;
        public float Unknown0x10;
        // 2nd group
        public float Unknown0x14;
        public float Unknown0x18;
        public float Unknown0x1C;
        public float Unknown0x20;
        public float Unknown0x24;
    }
}
