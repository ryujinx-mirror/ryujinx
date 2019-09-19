using Concentus.Structs;

namespace Ryujinx.HLE.HOS.Services.Audio.HardwareOpusDecoderManager
{
    class IHardwareOpusDecoder : IpcService
    {
        private const int FixedSampleRate = 48000;

        private int _sampleRate;
        private int _channelsCount;

        private OpusDecoder _decoder;

        public IHardwareOpusDecoder(int sampleRate, int channelsCount)
        {
            _sampleRate    = sampleRate;
            _channelsCount = channelsCount;

            _decoder = new OpusDecoder(FixedSampleRate, channelsCount);
        }

        [Command(0)]
        // DecodeInterleaved(buffer<unknown, 5>) -> (u32, u32, buffer<unknown, 6>)
        public ResultCode DecodeInterleaved(ServiceCtx context)
        {
            long inPosition = context.Request.SendBuff[0].Position;
            long inSize     = context.Request.SendBuff[0].Size;

            if (inSize < 8)
            {
                return ResultCode.OpusInvalidInput;
            }

            long outPosition = context.Request.ReceiveBuff[0].Position;
            long outSize     = context.Request.ReceiveBuff[0].Size;

            byte[] opusData = context.Memory.ReadBytes(inPosition, inSize);

            int processed = ((opusData[0] << 24) |
                             (opusData[1] << 16) |
                             (opusData[2] << 8)  |
                             (opusData[3] << 0)) + 8;

            if ((uint)processed > (ulong)inSize)
            {
                return ResultCode.OpusInvalidInput;
            }

            short[] pcm = new short[outSize / 2];

            int frameSize = pcm.Length / (_channelsCount * 2);

            int samples = _decoder.Decode(opusData, 0, opusData.Length, pcm, 0, frameSize);

            foreach (short sample in pcm)
            {
                context.Memory.WriteInt16(outPosition, sample);

                outPosition += 2;
            }

            context.ResponseData.Write(processed);
            context.ResponseData.Write(samples);

            return ResultCode.Success;
        }

        [Command(4)]
        // DecodeInterleavedWithPerf(buffer<unknown, 5>) -> (u32, u32, u64, buffer<unknown, 0x46>)
        public ResultCode DecodeInterleavedWithPerf(ServiceCtx context)
        {
            ResultCode result = DecodeInterleaved(context);

            // TODO: Figure out what this value is.
            // According to switchbrew, it is now used.
            context.ResponseData.Write(0L);

            return result;
        }
    }
}