using Ryujinx.Core.Logging;
using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Services.Am
{
    class IWindowController : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IWindowController()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 1,  GetAppletResourceUserId },
                { 10, AcquireForegroundRights }
            };
        }

        public long GetAppletResourceUserId(ServiceCtx Context)
        {
            Context.Ns.Log.PrintStub(LogClass.ServiceAm, "Stubbed.");

            Context.ResponseData.Write(0L);

            return 0;
        }

        public long AcquireForegroundRights(ServiceCtx Context)
        {
            Context.Ns.Log.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }
    }
}