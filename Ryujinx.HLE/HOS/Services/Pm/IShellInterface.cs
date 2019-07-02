using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Pm
{
    class IShellInterface : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public IShellInterface()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 6, GetApplicationPid }
            };
        }

        // GetApplicationPid() -> u64
        public long GetApplicationPid(ServiceCtx context)
        {
            // FIXME: This is wrong but needed to make hb loader works
            // TODO: Change this when we will have a way to process via a PM like interface.
            long pid = context.Process.Pid;

            context.ResponseData.Write(pid);

            return 0;
        }
    }
}
