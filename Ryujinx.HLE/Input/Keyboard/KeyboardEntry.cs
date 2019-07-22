using System.Runtime.InteropServices;

namespace Ryujinx.HLE.Input
{
    [StructLayout(LayoutKind.Sequential)]
    public struct KeyboardEntry
    {
        public long SamplesTimestamp;
        public long SamplesTimestamp2;
        public long Modifier;

        [MarshalAs(UnmanagedType.ByValArray , SizeConst = 0x8)]
        public int[] Keys;
    }
}
