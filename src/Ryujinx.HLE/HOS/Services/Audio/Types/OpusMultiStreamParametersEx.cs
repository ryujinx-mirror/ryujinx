using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Audio.Types
{
    [StructLayout(LayoutKind.Sequential, Size = 0x118)]
    struct OpusMultiStreamParametersEx
    {
        public int SampleRate;
        public int ChannelsCount;
        public int NumberOfStreams;
        public int NumberOfStereoStreams;
        public OpusDecoderFlags Flags;

        Array4<byte> Padding1;

        public Array64<uint> ChannelMappings;
    }
}
