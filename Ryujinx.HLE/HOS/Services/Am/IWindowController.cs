using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Am
{
    class IWindowController : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public IWindowController()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 1,  GetAppletResourceUserId },
                { 10, AcquireForegroundRights }
            };
        }

        public long GetAppletResourceUserId(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceAm);

            context.ResponseData.Write(0L);

            return 0;
        }

        public long AcquireForegroundRights(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }
    }
}