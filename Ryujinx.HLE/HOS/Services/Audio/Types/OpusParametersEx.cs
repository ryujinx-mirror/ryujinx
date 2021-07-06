using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Audio.Types
{
    [StructLayout(LayoutKind.Sequential, Size = 0x10)]
    struct OpusParametersEx
    {
        public int SampleRate;
        public int ChannelCount;
        public OpusDecoderFlags UseLargeFrameSize;

        Array4<byte> Padding1;
    }
}
