using System.Runtime.InteropServices;

namespace Ryujinx.Horizon.Sdk.Codec.Detail
{
    [StructLayout(LayoutKind.Sequential, Size = 0x10, Pack = 0x4)]
    struct HardwareOpusDecoderParameterInternalEx
    {
        public int SampleRate;
        public int ChannelsCount;
        public OpusDecoderFlags Flags;
        public uint Reserved;
    }
}
