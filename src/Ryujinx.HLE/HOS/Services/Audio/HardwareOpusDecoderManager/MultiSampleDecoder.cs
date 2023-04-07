using Concentus.Structs;

namespace Ryujinx.HLE.HOS.Services.Audio.HardwareOpusDecoderManager
{
    class MultiSampleDecoder : IDecoder
    {
        private readonly OpusMSDecoder _decoder;

        public int SampleRate => _decoder.SampleRate;
        public int ChannelsCount { get; }

        public MultiSampleDecoder(int sampleRate, int channelsCount, int streams, int coupledStreams, byte[] mapping)
        {
            ChannelsCount = channelsCount;
            _decoder = new OpusMSDecoder(sampleRate, channelsCount, streams, coupledStreams, mapping);
        }

        public int Decode(byte[] inData, int inDataOffset, int len, short[] outPcm, int outPcmOffset, int frameSize)
        {
            return _decoder.DecodeMultistream(inData, inDataOffset, len, outPcm, outPcmOffset, frameSize, 0);
        }

        public void ResetState()
        {
            _decoder.ResetState();
        }
    }
}