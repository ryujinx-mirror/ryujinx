using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Nfp
{
    class IUserManager : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public IUserManager()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, GetUserInterface }
            };
        }

        public long GetUserInterface(ServiceCtx context)
        {
            MakeObject(context, new IUser(context.Device.System));

            return 0;
        }
    }
}