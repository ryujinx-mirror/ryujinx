using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Account;
using Ryujinx.Horizon.Sdk.Friends.Detail.Ipc;
using Ryujinx.Horizon.Sdk.Sf.Hipc;
using Ryujinx.Horizon.Sdk.Sm;
using System;

namespace Ryujinx.Horizon.Friends
{
    class FriendsServerManager : ServerManager
    {
        private readonly IEmulatorAccountManager _accountManager;
        private readonly NotificationEventHandler _notificationEventHandler;

        public FriendsServerManager(HeapAllocator allocator, SmApi sm, int maxPorts, ManagerOptions options, int maxSessions) : base(allocator, sm, maxPorts, options, maxSessions)
        {
            _accountManager = HorizonStatic.Options.AccountManager;
            _notificationEventHandler = new();
        }

        protected override Result OnNeedsToAccept(int portIndex, Server server)
        {
            return (FriendsPortIndex)portIndex switch
            {
#pragma warning disable IDE0055 // Disable formatting
                FriendsPortIndex.Admin   => AcceptImpl(server, new ServiceCreator(_accountManager, _notificationEventHandler, FriendsServicePermissionLevel.Admin)),
                FriendsPortIndex.User    => AcceptImpl(server, new ServiceCreator(_accountManager, _notificationEventHandler, FriendsServicePermissionLevel.User)),
                FriendsPortIndex.Viewer  => AcceptImpl(server, new ServiceCreator(_accountManager, _notificationEventHandler, FriendsServicePermissionLevel.Viewer)),
                FriendsPortIndex.Manager => AcceptImpl(server, new ServiceCreator(_accountManager, _notificationEventHandler, FriendsServicePermissionLevel.Manager)),
                FriendsPortIndex.System  => AcceptImpl(server, new ServiceCreator(_accountManager, _notificationEventHandler, FriendsServicePermissionLevel.System)),
                _                        => throw new ArgumentOutOfRangeException(nameof(portIndex)),
#pragma warning restore IDE0055
            };
        }
    }
}
