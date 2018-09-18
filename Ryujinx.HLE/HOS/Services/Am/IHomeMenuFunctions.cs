using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.Logging;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Am
{
    class IHomeMenuFunctions : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private KEvent ChannelEvent;

        public IHomeMenuFunctions(Horizon System)
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 10, RequestToGetForeground        },
                { 21, GetPopFromGeneralChannelEvent }
            };

            //ToDo: Signal this Event somewhere in future.
            ChannelEvent = new KEvent(System);
        }

        public long RequestToGetForeground(ServiceCtx Context)
        {
            Context.Device.Log.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long GetPopFromGeneralChannelEvent(ServiceCtx Context)
        {
            int Handle = Context.Process.HandleTable.OpenHandle(ChannelEvent);

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            Context.Device.Log.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }
    }
}
