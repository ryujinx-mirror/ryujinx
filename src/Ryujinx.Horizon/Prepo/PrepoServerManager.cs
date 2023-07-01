using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Prepo.Ipc;
using Ryujinx.Horizon.Prepo.Types;
using Ryujinx.Horizon.Sdk.Sf.Hipc;
using Ryujinx.Horizon.Sdk.Sm;
using System;

namespace Ryujinx.Horizon.Prepo
{
    class PrepoServerManager : ServerManager
    {
        public PrepoServerManager(HeapAllocator allocator, SmApi sm, int maxPorts, ManagerOptions options, int maxSessions) : base(allocator, sm, maxPorts, options, maxSessions)
        {
        }

        protected override Result OnNeedsToAccept(int portIndex, Server server)
        {
            return (PrepoPortIndex)portIndex switch
            {
#pragma warning disable IDE0055 // Disable formatting
                PrepoPortIndex.Admin   => AcceptImpl(server, new PrepoService(PrepoServicePermissionLevel.Admin)),
                PrepoPortIndex.Admin2  => AcceptImpl(server, new PrepoService(PrepoServicePermissionLevel.Admin)),
                PrepoPortIndex.Manager => AcceptImpl(server, new PrepoService(PrepoServicePermissionLevel.Manager)),
                PrepoPortIndex.User    => AcceptImpl(server, new PrepoService(PrepoServicePermissionLevel.User)),
                PrepoPortIndex.System  => AcceptImpl(server, new PrepoService(PrepoServicePermissionLevel.System)),
                PrepoPortIndex.Debug   => AcceptImpl(server, new PrepoService(PrepoServicePermissionLevel.Debug)),
                _                      => throw new ArgumentOutOfRangeException(nameof(portIndex)),
#pragma warning restore IDE0055
            };
        }
    }
}
