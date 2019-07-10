using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Ns
{
    [Service("aoc:u")]
    class IAddOnContentManager : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public IAddOnContentManager(ServiceCtx context)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 2, CountAddOnContent },
                { 3, ListAddOnContent  }
            };
        }

        public static long CountAddOnContent(ServiceCtx context)
        {
            context.ResponseData.Write(0);

            Logger.PrintStub(LogClass.ServiceNs);

            return 0;
        }

        public static long ListAddOnContent(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceNs);

            // TODO: This is supposed to write a u32 array aswell.
            // It's unknown what it contains.
            context.ResponseData.Write(0);

            return 0;
        }
    }
}