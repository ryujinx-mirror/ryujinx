using Ryujinx.Core.Logging;
using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Services.Aud
{
    class IAudioRendererManager : IpcService
    {
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
            //Same buffer as GetAudioRendererWorkBufferSize is receive here.

            MakeObject(Context, new IAudioRenderer());

            return 0;
        }

        public long GetAudioRendererWorkBufferSize(ServiceCtx Context)
        {
            long SampleRate = Context.RequestData.ReadUInt32();
            long Unknown4   = Context.RequestData.ReadUInt32();
            long Unknown8   = Context.RequestData.ReadUInt32();
            long UnknownC   = Context.RequestData.ReadUInt32();
            long Unknown10  = Context.RequestData.ReadUInt32(); //VoiceCount
            long Unknown14  = Context.RequestData.ReadUInt32(); //SinkCount
            long Unknown18  = Context.RequestData.ReadUInt32(); //EffectCount
            long Unknown1c  = Context.RequestData.ReadUInt32(); //Boolean
            long Unknown20  = Context.RequestData.ReadUInt32(); //Not used here in FW3.0.1 - Boolean
            long Unknown24  = Context.RequestData.ReadUInt32();
            long Unknown28  = Context.RequestData.ReadUInt32(); //SplitterCount
            long Unknown2c  = Context.RequestData.ReadUInt32(); //Not used here in FW3.0.1
            uint RevMagic   = Context.RequestData.ReadUInt32();

            if (RevMagic - 0x52455630 <= 3) //REV3 Max is supported
            {
                long Size  = RoundUp(Unknown8 * 4, 64);
                     Size += (UnknownC << 10);
                     Size += (UnknownC + 1) * 0x940;
                     Size += Unknown10 * 0x3F0;
                     Size += RoundUp((UnknownC + 1) * 8, 16);
                     Size += RoundUp(Unknown10 * 8, 16);
                     Size += RoundUp((0x3C0 * (Unknown14 + UnknownC) + 4 * Unknown4) * (Unknown8 + 6), 64);
                     Size += 0x2C0 * (Unknown14 + UnknownC) + 0x30 * (Unknown18 + (4 * Unknown10)) + 0x50;

                if (RevMagic - 0x52455630 >= 3) //IsSplitterSupported
                {
                    Size += RoundUp((NodeStatesGetWorkBufferSize((int)UnknownC + 1) + EdgeMatrixGetWorkBufferSize((int)UnknownC + 1)), 16);
                    Size += 0xE0 * Unknown28 + 0x20 * Unknown24 + RoundUp(Unknown28 * 4, 16);
                }

                Size = 0x4C0 * Unknown18 + RoundUp(Size, 64) + 0x170 * Unknown14 + ((Unknown10 << 8) | 0x40);

                if (Unknown1c >= 1)
                {
                    Size += ((((Unknown18 + Unknown14 + Unknown10 + UnknownC + 1) * 16) + 0x658) * (Unknown1c + 1) + 0x13F) & ~0x3FL;
                }

                long WorkBufferSize = (Size + 0x1907D) & ~0xFFFL;

                Context.ResponseData.Write(WorkBufferSize);

                Context.Ns.Log.PrintDebug(LogClass.ServiceAudio, $"WorkBufferSize is 0x{WorkBufferSize:x16}.");

                return 0;
            }
            else
            {
                Context.ResponseData.Write(0L);

                Context.Ns.Log.PrintError(LogClass.ServiceAudio, "Library Revision is not supported!");

                return 0x499;
            }
        }

        private static long RoundUp(long Value, int Size)
        {
            return (Value + (Size - 1)) & ~((long)Size - 1);
        }

        private static int NodeStatesGetWorkBufferSize(int Value)
        {
            int Result = (int)RoundUp(Value, 64);

            if (Result < 0)
            {
                Result |= 7;
            }

            return 4 * (Value * Value) + 0x12 * Value + 2 * (Result / 8);
        }

        private static int EdgeMatrixGetWorkBufferSize(int Value)
        {
            int Result = (int)RoundUp(Value * Value, 64);

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
