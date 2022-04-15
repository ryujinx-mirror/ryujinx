using Concentus.Structs;

namespace Ryujinx.HLE.HOS.Services.Audio.HardwareOpusDecoderManager
{
    class Decoder : IDecoder
    {
        private readonly OpusDecoder _decoder;

        public int SampleRate => _decoder.SampleRate;
        public int ChannelsCount => _decoder.NumChannels;

        public Decoder(int sampleRate, int channelsCount)
        {
            _decoder = new OpusDecoder(sampleRate, channelsCount);
        }

        public int Decode(byte[] inData, int inDataOffset, int len, short[] outPcm, int outPcmOffset, int frameSize)
        {
            return _decoder.Decode(inData, inDataOffset, len, outPcm, outPcmOffset, frameSize);
        }

        public void ResetState()
        {
            _decoder.ResetState();
        }
    }
}