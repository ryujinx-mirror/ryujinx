using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Audio.AudioRendererManager
{
    [StructLayout(LayoutKind.Sequential, Size = 0xc, Pack = 1)]
    struct BiquadFilter
    {
        public byte  Enable;
        public byte  Padding;
        public short B0;
        public short B1;
        public short B2;
        public short A1;
        public short A2;
    }
}