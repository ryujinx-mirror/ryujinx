using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

using static Ryujinx.Core.OsHle.IpcServices.ObjHelper;

namespace Ryujinx.Core.OsHle.IpcServices.Aud
{
    class ServiceAudRen : IIpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ServiceAudRen()
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
            int SampleRate = Context.RequestData.ReadInt32();
            int Unknown4   = Context.RequestData.ReadInt32();
            int Unknown8   = Context.RequestData.ReadInt32();
            int UnknownC   = Context.RequestData.ReadInt32();
            int Unknown10  = Context.RequestData.ReadInt32();
            int Unknown14  = Context.RequestData.ReadInt32();
            int Unknown18  = Context.RequestData.ReadInt32();
            int Unknown1c  = Context.RequestData.ReadInt32();
            int Unknown20  = Context.RequestData.ReadInt32();
            int Unknown24  = Context.RequestData.ReadInt32();
            int Unknown28  = Context.RequestData.ReadInt32();
            int Unknown2c  = Context.RequestData.ReadInt32();
            int Rev1Magic  = Context.RequestData.ReadInt32();

            Context.ResponseData.Write(0x400L);

            return 0;
        }

        public long GetAudioDevice(ServiceCtx Context)
        {
            long UserId = Context.RequestData.ReadInt64();

            MakeObject(Context, new IAudioDevice());

            return 0;
        }
    }
}