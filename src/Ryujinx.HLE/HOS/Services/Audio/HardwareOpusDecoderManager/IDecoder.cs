namespace Ryujinx.HLE.HOS.Services.Audio.HardwareOpusDecoderManager
{
    interface IDecoder
    {
        int SampleRate { get; }
        int ChannelsCount { get; }

        int Decode(byte[] inData, int inDataOffset, int len, short[] outPcm, int outPcmOffset, int frameSize);
        void ResetState();
    }
}