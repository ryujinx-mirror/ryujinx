using Concentus.Structs;
using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

using static Ryujinx.HLE.HOS.ErrorCode;

namespace Ryujinx.HLE.HOS.Services.Aud
{
    class IHardwareOpusDecoder : IpcService
    {
        private const int FixedSampleRate = 48000;

        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private int SampleRate;
        private int ChannelsCount;

        private OpusDecoder Decoder;

        public IHardwareOpusDecoder(int SampleRate, int ChannelsCount)
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, DecodeInterleaved         },
                { 4, DecodeInterleavedWithPerf }
            };

            this.SampleRate    = SampleRate;
            this.ChannelsCount = ChannelsCount;

            Decoder = new OpusDecoder(FixedSampleRate, ChannelsCount);
        }

        public long DecodeInterleavedWithPerf(ServiceCtx Context)
        {
            long Result = DecodeInterleaved(Context);

            //TODO: Figure out what this value is.
            //According to switchbrew, it is now used.
            Context.ResponseData.Write(0L);

            return Result;
        }

        public long DecodeInterleaved(ServiceCtx Context)
        {
            long InPosition = Context.Request.SendBuff[0].Position;
            long InSize     = Context.Request.SendBuff[0].Size;

            if (InSize < 8)
            {
                return MakeError(ErrorModule.Audio, AudErr.OpusInvalidInput);
            }

            long OutPosition = Context.Request.ReceiveBuff[0].Position;
            long OutSize     = Context.Request.ReceiveBuff[0].Size;

            byte[] OpusData = Context.Memory.ReadBytes(InPosition, InSize);

            int Processed = ((OpusData[0] << 24) |
                             (OpusData[1] << 16) |
                             (OpusData[2] << 8)  |
                             (OpusData[3] << 0)) + 8;

            if ((uint)Processed > (ulong)InSize)
            {
                return MakeError(ErrorModule.Audio, AudErr.OpusInvalidInput);
            }

            short[] Pcm = new short[OutSize / 2];

            int FrameSize = Pcm.Length / (ChannelsCount * 2);

            int Samples = Decoder.Decode(OpusData, 0, OpusData.Length, Pcm, 0, FrameSize);

            foreach (short Sample in Pcm)
            {
                Context.Memory.WriteInt16(OutPosition, Sample);

                OutPosition += 2;
            }

            Context.ResponseData.Write(Processed);
            Context.ResponseData.Write(Samples);

            return 0;
        }
    }
}
