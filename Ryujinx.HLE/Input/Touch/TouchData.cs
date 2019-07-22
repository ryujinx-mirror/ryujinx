using System.Runtime.InteropServices;

namespace Ryujinx.HLE.Input
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TouchData
    {
        public long SampleTimestamp;
        public int  Padding;
        public int  Index;
        public int  X;
        public int  Y;
        public int  DiameterX;
        public int  DiameterY;
        public int  Angle;
        public int  Padding2;
    }
}
