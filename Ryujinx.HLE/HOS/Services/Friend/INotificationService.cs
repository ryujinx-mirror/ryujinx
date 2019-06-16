using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.Utilities;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Friend
{
    class INotificationService : IpcService
    {
        private UInt128 _userId;

        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public INotificationService(UInt128 userId)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
              //{ 0, GetEvent },
              //{ 1, Pop      },
              //{ 2, Clear    },
            };

            _userId = userId;
        }
    }
}