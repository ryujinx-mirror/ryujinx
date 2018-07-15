using System.Runtime.InteropServices;

namespace Ryujinx.HLE.OsHle.Services.Aud.AudioOut
{
    [StructLayout(LayoutKind.Sequential)]
    struct AudioOutData
    {
        public long NextBufferPtr;
        public long SampleBufferPtr;
        public long SampleBufferCapacity;
        public long SampleBufferSize;
        public long SampleBufferInnerOffset;
    }
}