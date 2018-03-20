using Ryujinx.Core.OsHle.Handles;
using Ryujinx.Core.OsHle.Ipc;
using System;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Services.Nifm
{
    class IRequest : IpcService, IDisposable
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private KEvent Event;

        public IRequest()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, GetRequestState               },
                { 1, GetResult                     },
                { 2, GetSystemEventReadableHandles }
            };

            Event = new KEvent();
        }

        public long GetRequestState(ServiceCtx Context)
        {
            Context.ResponseData.Write(0);

            //Todo: Stub

            return 0;
        }

        public long GetResult(ServiceCtx Context)
        {
            //Todo: Stub

            return 0;
        }

        //GetSystemEventReadableHandles() -> (KObject, KObject)
        public long GetSystemEventReadableHandles(ServiceCtx Context)
        {
            //FIXME: Is this supposed to return 2 events?
            int Handle = Context.Process.HandleTable.OpenHandle(Event);

            Context.Response.HandleDesc = IpcHandleDesc.MakeMove(Handle);

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
                Event.Dispose();
            }
        }
    }
}