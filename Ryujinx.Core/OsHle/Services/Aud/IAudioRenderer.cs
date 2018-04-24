using Ryujinx.Core.Logging;
using Ryujinx.Core.OsHle.Handles;
using Ryujinx.Core.OsHle.Ipc;
using System;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Services.Aud
{
    class IAudioRenderer : IpcService, IDisposable
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private KEvent UpdateEvent;

        public IAudioRenderer()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 4, RequestUpdateAudioRenderer },
                { 5, StartAudioRenderer         },
                { 6, StopAudioRenderer          },
                { 7, QuerySystemEvent           }
            };

            UpdateEvent = new KEvent();
        }

        public long RequestUpdateAudioRenderer(ServiceCtx Context)
        {
            //(buffer<unknown, 5, 0>) -> (buffer<unknown, 6, 0>, buffer<unknown, 6, 0>)

            long Position = Context.Request.ReceiveBuff[0].Position;

            //0x40 bytes header
            Context.Memory.WriteInt32(Position + 0x4, 0xb0); //Behavior Out State Size? (note: this is the last section)
            Context.Memory.WriteInt32(Position + 0x8, 0x18e0); //Memory Pool Out State Size?
            Context.Memory.WriteInt32(Position + 0xc, 0x600); //Voice Out State Size?
            Context.Memory.WriteInt32(Position + 0x14, 0xe0); //Effect Out State Size?
            Context.Memory.WriteInt32(Position + 0x1c, 0x20); //Sink Out State Size?
            Context.Memory.WriteInt32(Position + 0x20, 0x10); //Performance Out State Size?
            Context.Memory.WriteInt32(Position + 0x3c, 0x20e0); //Total Size (including 0x40 bytes header)

            for (int Offset = 0x40; Offset < 0x40 + 0x18e0; Offset += 0x10)
            {
                Context.Memory.WriteInt32(Position + Offset, 5);
            }

            //TODO: We shouldn't be signaling this here.
            UpdateEvent.WaitEvent.Set();

            return 0;
        }

        public long StartAudioRenderer(ServiceCtx Context)
        {
            Context.Ns.Log.PrintStub(LogClass.ServiceAudio, "Stubbed.");

            return 0;
        }

        public long StopAudioRenderer(ServiceCtx Context)
        {
            Context.Ns.Log.PrintStub(LogClass.ServiceAudio, "Stubbed.");

            return 0;
        }

        public long QuerySystemEvent(ServiceCtx Context)
        {
            int Handle = Context.Process.HandleTable.OpenHandle(UpdateEvent);

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            return 0;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (Disposing)
            {
                UpdateEvent.Dispose();
            }
        }
    }
}