using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Prepo
{
    [Service("prepo:a")]
    [Service("prepo:u")]
    class IPrepoService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public IPrepoService(ServiceCtx context)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 10101, SaveReportWithUser }
            };
        }

        public static long SaveReportWithUser(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServicePrepo);

            return 0;
        }
    }
}