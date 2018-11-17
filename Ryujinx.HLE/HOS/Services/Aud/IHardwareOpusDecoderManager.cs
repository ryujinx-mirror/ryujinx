using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Aud
{
    class IHardwareOpusDecoderManager : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IHardwareOpusDecoderManager()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, Initialize        },
                { 1, GetWorkBufferSize }
            };
        }

        public long Initialize(ServiceCtx Context)
        {
            int SampleRate    = Context.RequestData.ReadInt32();
            int ChannelsCount = Context.RequestData.ReadInt32();

            MakeObject(Context, new IHardwareOpusDecoder(SampleRate, ChannelsCount));

            return 0;
        }

        public long GetWorkBufferSize(ServiceCtx Context)
        {
            //Note: The sample rate is ignored because it is fixed to 48KHz.
            int SampleRate    = Context.RequestData.ReadInt32();
            int ChannelsCount = Context.RequestData.ReadInt32();

            Context.ResponseData.Write(GetOpusDecoderSize(ChannelsCount));

            return 0;
        }

        private static int GetOpusDecoderSize(int ChannelsCount)
        {
            const int SilkDecoderSize = 0x2198;

            if (ChannelsCount < 1 || ChannelsCount > 2)
            {
                return 0;
            }

            int CeltDecoderSize = GetCeltDecoderSize(ChannelsCount);

            int OpusDecoderSize = (ChannelsCount * 0x800 + 0x4807) & -0x800 | 0x50;

            return OpusDecoderSize + SilkDecoderSize + CeltDecoderSize;
        }

        private static int GetCeltDecoderSize(int ChannelsCount)
        {
            const int DecodeBufferSize = 0x2030;
            const int CeltDecoderSize  = 0x58;
            const int CeltSigSize      = 0x4;
            const int Overlap          = 120;
            const int EBandsCount      = 21;

            return (DecodeBufferSize + Overlap * 4) * ChannelsCount +
                    EBandsCount * 16 +
                    CeltDecoderSize +
                    CeltSigSize;
        }
    }
}
