using System.Runtime.InteropServices;

namespace Ryujinx.HLE.Input
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct TouchEntry
    {
        public long SamplesTimestamp;
        public long TouchCount;
    }
}
