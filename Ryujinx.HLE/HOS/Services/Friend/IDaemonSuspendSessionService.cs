using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Friend
{
    class IDaemonSuspendSessionService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        private FriendServicePermissionLevel PermissionLevel;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public IDaemonSuspendSessionService(FriendServicePermissionLevel permissionLevel)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                //{ 0, Unknown0 }, // 4.0.0+
                //{ 1, Unknown1 }, // 4.0.0+
                //{ 2, Unknown2 }, // 4.0.0+
                //{ 3, Unknown3 }, // 4.0.0+
                //{ 4, Unknown4 }, // 4.0.0+
            };

            PermissionLevel = permissionLevel;
        }
    }
}