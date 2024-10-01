using System.Runtime.InteropServices;

namespace Ryujinx.Horizon.Sdk.Settings.Factory
{
    [StructLayout(LayoutKind.Sequential, Size = 0x6, Pack = 0x2)]
    struct ConsoleSixAxisSensorHorizontalOffset
    {
        public ushort X;
        public ushort Y;
        public ushort Z;
    }
}
