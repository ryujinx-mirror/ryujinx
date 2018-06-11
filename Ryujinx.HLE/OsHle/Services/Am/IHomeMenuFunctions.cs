using Ryujinx.HLE.Logging;
using Ryujinx.HLE.OsHle.Handles;
using Ryujinx.HLE.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.OsHle.Services.Am
{
    class IHomeMenuFunctions : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private KEvent ChannelEvent;

        public IHomeMenuFunctions()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 10, RequestToGetForeground        },
                { 21, GetPopFromGeneralChannelEvent }
            };

            //ToDo: Signal this Event somewhere in future.
            ChannelEvent = new KEvent();
        }

        public long RequestToGetForeground(ServiceCtx Context)
        {
            Context.Ns.Log.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long GetPopFromGeneralChannelEvent(ServiceCtx Context)
        {
            int Handle = Context.Process.HandleTable.OpenHandle(ChannelEvent);

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            Context.Ns.Log.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }
    }
}
