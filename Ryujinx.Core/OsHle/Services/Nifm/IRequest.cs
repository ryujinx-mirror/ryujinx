using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.IpcServices.Nifm
{
    class IRequest : IIpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IRequest()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, GetRequestState               },
                { 1, GetResult                     },
                { 2, GetSystemEventReadableHandles }
            };
        }

        // -> i32
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
            Context.Response.HandleDesc = IpcHandleDesc.MakeMove(0xbadcafe);

            //Todo: Stub

            return 0;
        }
    }
}