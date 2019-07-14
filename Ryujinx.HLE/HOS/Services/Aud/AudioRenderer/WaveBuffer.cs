using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Aud.AudioRenderer
{
    [StructLayout(LayoutKind.Sequential, Size = 0x38, Pack = 1)]
    struct WaveBuffer
    {
        public long  Position;
        public long  Size;
        public int   FirstSampleOffset;
        public int   LastSampleOffset;
        public byte  Looping;
        public byte  LastBuffer;
        public short Unknown1A;
        public int   Unknown1C;
        public long  AdpcmLoopContextPosition;
        public long  AdpcmLoopContextSize;
        public long  Unknown30;
    }
}