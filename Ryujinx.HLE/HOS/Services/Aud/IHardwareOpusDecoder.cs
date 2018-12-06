using Concentus.Structs;
using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

using static Ryujinx.HLE.HOS.ErrorCode;

namespace Ryujinx.HLE.HOS.Services.Aud
{
    class IHardwareOpusDecoder : IpcService
    {
        private const int FixedSampleRate = 48000;

        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        private int _sampleRate;
        private int _channelsCount;

        private OpusDecoder _decoder;

        public IHardwareOpusDecoder(int sampleRate, int channelsCount)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, DecodeInterleaved         },
                { 4, DecodeInterleavedWithPerf }
            };

            _sampleRate    = sampleRate;
            _channelsCount = channelsCount;

            _decoder = new OpusDecoder(FixedSampleRate, channelsCount);
        }

        public long DecodeInterleavedWithPerf(ServiceCtx context)
        {
            long result = DecodeInterleaved(context);

            //TODO: Figure out what this value is.
            //According to switchbrew, it is now used.
            context.ResponseData.Write(0L);

            return result;
        }

        public long DecodeInterleaved(ServiceCtx context)
        {
            long inPosition = context.Request.SendBuff[0].Position;
            long inSize     = context.Request.SendBuff[0].Size;

            if (inSize < 8)
            {
                return MakeError(ErrorModule.Audio, AudErr.OpusInvalidInput);
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
                return MakeError(ErrorModule.Audio, AudErr.OpusInvalidInput);
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

            return 0;
        }
    }
}
