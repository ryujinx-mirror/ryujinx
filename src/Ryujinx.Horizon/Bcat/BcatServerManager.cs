using Ryujinx.Horizon.Bcat.Ipc;
using Ryujinx.Horizon.Bcat.Types;
using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Sf.Hipc;
using Ryujinx.Horizon.Sdk.Sm;
using System;

namespace Ryujinx.Horizon.Bcat
{
    class BcatServerManager : ServerManager
    {
        public BcatServerManager(HeapAllocator allocator, SmApi sm, int maxPorts, ManagerOptions options, int maxSessions) : base(allocator, sm, maxPorts, options, maxSessions)
        {
        }

        protected override Result OnNeedsToAccept(int portIndex, Server server)
        {
            return (BcatPortIndex)portIndex switch
            {
                BcatPortIndex.Admin => AcceptImpl(server, new ServiceCreator("bcat:a", BcatServicePermissionLevel.Admin)),
                BcatPortIndex.Manager => AcceptImpl(server, new ServiceCreator("bcat:m", BcatServicePermissionLevel.Manager)),
                BcatPortIndex.User => AcceptImpl(server, new ServiceCreator("bcat:u", BcatServicePermissionLevel.User)),
                BcatPortIndex.System => AcceptImpl(server, new ServiceCreator("bcat:s", BcatServicePermissionLevel.System)),
                _ => throw new ArgumentOutOfRangeException(nameof(portIndex)),
            };
        }
    }
}
