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
            MakeObject(Context, new IAudioRenderer());

            return 0;
        }

        public long GetAudioRendererWorkBufferSize(ServiceCtx Context)
        {
            long SampleRate = Context.RequestData.ReadUInt32();
            long Unknown4   = Context.RequestData.ReadUInt32();
            long Unknown8   = Context.RequestData.ReadUInt32();
            long UnknownC   = Context.RequestData.ReadUInt32();
            long Unknown10  = Context.RequestData.ReadUInt32();
            long Unknown14  = Context.RequestData.ReadUInt32();
            long Unknown18  = Context.RequestData.ReadUInt32();
            long Unknown1c  = Context.RequestData.ReadUInt32();
            uint Reserved20 = Context.RequestData.ReadUInt32(); //Not used in FW1.0
            uint Reserved24 = Context.RequestData.ReadUInt32(); //Not used in FW1.0
            uint Reserved28 = Context.RequestData.ReadUInt32(); //Not used in FW1.0
            uint Reserved2c = Context.RequestData.ReadUInt32(); //Not used in FW1.0
            uint Rev1Magic  = Context.RequestData.ReadUInt32();

            if (Rev1Magic == 0x31564552) //REV1
            {
                long Size;

                Size  = UnknownC * 0x400 + 0x50;
                Size += RoundUp(Unknown8 * 4, 64);
                Size += (UnknownC + 1) * 0x940;
                Size += Unknown14 * 0x170;
                Size += Unknown10 * 0x100;
                Size += (Unknown14 + UnknownC) * 0x2C0;
                Size += Unknown10 * 0x2F0;
                Size += Unknown10 * 0x100 + 0x40;
                Size += Unknown18 * 0x4B0;
                Size += RoundUp((UnknownC + 1) * 8, 16);
                Size += RoundUp(Unknown10 * 8, 16);
                Size += (Unknown18 + Unknown10 * 4) * 0x20;
                Size += RoundUp((Unknown4 * 4 + (UnknownC + Unknown14) * 0x3C0) * (Unknown8 + 6), 64);

                if (Unknown1c == 0)
                {
                    Size += (((((UnknownC + 1) + Unknown10 + Unknown14 + Unknown18) * 16 + 0x658) * (Unknown1c + 1) + 0xFF) & ~0x3FL);
                }

                long WorkBufferSize = (Size + 0x1907D) & ~0xFFFL;

                Context.ResponseData.Write(WorkBufferSize);

                Context.Ns.Log.PrintDebug(LogClass.ServiceAudio, $"WorkBufferSize is 0x{WorkBufferSize:x16}.");

                return 0;
            }
            else
            {
                Context.ResponseData.Write(0L);

                Context.Ns.Log.PrintError(LogClass.ServiceAudio, "REV1 magic not found!");

                return 0x499;
            }
        }

        private static long RoundUp(long Value, int Size)
        {
            return (Value + (Size - 1)) & ~((long)Size - 1);
        }

        public long GetAudioDevice(ServiceCtx Context)
        {
            long UserId = Context.RequestData.ReadInt64();

            MakeObject(Context, new IAudioDevice());

            return 0;
        }
    }
}
