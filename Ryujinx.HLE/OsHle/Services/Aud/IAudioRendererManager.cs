using Ryujinx.Audio;
using Ryujinx.HLE.Logging;
using Ryujinx.HLE.OsHle.Ipc;
using Ryujinx.HLE.OsHle.Services.Aud.AudioRenderer;
using Ryujinx.HLE.OsHle.Utilities;
using System.Collections.Generic;

using static Ryujinx.HLE.OsHle.ErrorCode;

namespace Ryujinx.HLE.OsHle.Services.Aud
{
    class IAudioRendererManager : IpcService
    {
        private const int Rev0Magic = ('R' << 0)  |
                                      ('E' << 8)  |
                                      ('V' << 16) |
                                      ('0' << 24);

        private const int Rev = 4;

        public const int RevMagic = Rev0Magic + Rev;

        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IAudioRendererManager()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, OpenAudioRenderer              },
                { 1, GetAudioRendererWorkBufferSize },
                { 2, GetAudioDevice                 }
            };
        }

        public long OpenAudioRenderer(ServiceCtx Context)
        {
            IAalOutput AudioOut = Context.Ns.AudioOut;

            AudioRendererParameter Params = GetAudioRendererParameter(Context);

            MakeObject(Context, new IAudioRenderer(Context.Memory, AudioOut, Params));

            return 0;
        }

        public long GetAudioRendererWorkBufferSize(ServiceCtx Context)
        {
            AudioRendererParameter Params = GetAudioRendererParameter(Context);

            int Revision = (Params.Revision - Rev0Magic) >> 24;

            if (Revision <= Rev) //REV3 Max is supported
            {
                bool IsSplitterSupported = Revision >= 3;

                long Size;

                Size  = IntUtils.AlignUp(Params.Unknown8 * 4, 64);
                Size += Params.MixCount * 0x400;
                Size += (Params.MixCount + 1) * 0x940;
                Size += Params.VoiceCount * 0x3F0;
                Size += IntUtils.AlignUp((Params.MixCount + 1) * 8, 16);
                Size += IntUtils.AlignUp(Params.VoiceCount * 8, 16);
                Size += IntUtils.AlignUp(
                    ((Params.SinkCount + Params.MixCount) * 0x3C0 + Params.SampleCount * 4) *
                    (Params.Unknown8 + 6), 64);
                Size += (Params.SinkCount + Params.MixCount) * 0x2C0;
                Size += (Params.EffectCount + 4 * Params.VoiceCount) * 0x30 + 0x50;

                if (IsSplitterSupported)
                {
                    Size += IntUtils.AlignUp((
                        NodeStatesGetWorkBufferSize(Params.MixCount + 1) +
                        EdgeMatrixGetWorkBufferSize(Params.MixCount + 1)), 16);

                    Size += Params.SplitterDestinationDataCount * 0xE0;
                    Size += Params.SplitterCount * 0x20;
                    Size += IntUtils.AlignUp(Params.SplitterDestinationDataCount * 4, 16);
                }

                Size = Params.EffectCount * 0x4C0 +
                       Params.SinkCount * 0x170 +
                       Params.VoiceCount * 0x100 +
                       IntUtils.AlignUp(Size, 64) + 0x40;

                if (Params.PerformanceManagerCount >= 1)
                {
                    Size += (((Params.EffectCount +
                               Params.SinkCount +
                               Params.VoiceCount +
                               Params.MixCount + 1) * 16 + 0x658) *
                               (Params.PerformanceManagerCount + 1) + 0x13F) & ~0x3FL;
                }

                Size = (Size + 0x1907D) & ~0xFFFL;

                Context.ResponseData.Write(Size);

                Context.Ns.Log.PrintDebug(LogClass.ServiceAudio, $"WorkBufferSize is 0x{Size:x16}.");

                return 0;
            }
            else
            {
                Context.ResponseData.Write(0L);

                Context.Ns.Log.PrintWarning(LogClass.ServiceAudio, $"Library Revision 0x{Params.Revision:x8} is not supported!");

                return MakeError(ErrorModule.Audio, AudErr.UnsupportedRevision);
            }
        }

        private AudioRendererParameter GetAudioRendererParameter(ServiceCtx Context)
        {
            AudioRendererParameter Params = new AudioRendererParameter();

            Params.SampleRate                   = Context.RequestData.ReadInt32();
            Params.SampleCount                  = Context.RequestData.ReadInt32();
            Params.Unknown8                     = Context.RequestData.ReadInt32();
            Params.MixCount                     = Context.RequestData.ReadInt32();
            Params.VoiceCount                   = Context.RequestData.ReadInt32();
            Params.SinkCount                    = Context.RequestData.ReadInt32();
            Params.EffectCount                  = Context.RequestData.ReadInt32();
            Params.PerformanceManagerCount      = Context.RequestData.ReadInt32();
            Params.VoiceDropEnable              = Context.RequestData.ReadInt32();
            Params.SplitterCount                = Context.RequestData.ReadInt32();
            Params.SplitterDestinationDataCount = Context.RequestData.ReadInt32();
            Params.Unknown2C                    = Context.RequestData.ReadInt32();
            Params.Revision                     = Context.RequestData.ReadInt32();

            return Params;
        }

        private static int NodeStatesGetWorkBufferSize(int Value)
        {
            int Result = IntUtils.AlignUp(Value, 64);

            if (Result < 0)
            {
                Result |= 7;
            }

            return 4 * (Value * Value) + 0x12 * Value + 2 * (Result / 8);
        }

        private static int EdgeMatrixGetWorkBufferSize(int Value)
        {
            int Result = IntUtils.AlignUp(Value * Value, 64);

            if (Result < 0)
            {
                Result |= 7;
            }

            return Result / 8;
        }

        public long GetAudioDevice(ServiceCtx Context)
        {
            long UserId = Context.RequestData.ReadInt64();

            MakeObject(Context, new IAudioDevice());

            return 0;
        }
    }
}
