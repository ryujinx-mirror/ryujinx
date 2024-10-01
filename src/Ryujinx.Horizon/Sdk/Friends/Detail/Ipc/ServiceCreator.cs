using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Account;
using Ryujinx.Horizon.Sdk.Sf;

namespace Ryujinx.Horizon.Sdk.Friends.Detail.Ipc
{
    partial class ServiceCreator : IServiceCreator
    {
        private readonly IEmulatorAccountManager _accountManager;
        private readonly NotificationEventHandler _notificationEventHandler;
        private readonly FriendsServicePermissionLevel _permissionLevel;

        public ServiceCreator(IEmulatorAccountManager accountManager, NotificationEventHandler notificationEventHandler, FriendsServicePermissionLevel permissionLevel)
        {
            _accountManager = accountManager;
            _notificationEventHandler = notificationEventHandler;
            _permissionLevel = permissionLevel;
        }

        [CmifCommand(0)]
        public Result CreateFriendService(out IFriendService friendService)
        {
            friendService = new FriendService(_accountManager, _permissionLevel);

            return Result.Success;
        }

        [CmifCommand(1)] // 2.0.0+
        public Result CreateNotificationService(out INotificationService notificationService, Uid userId)
        {
            if (userId.IsNull)
            {
                notificationService = null;

                return FriendResult.InvalidArgument;
            }

            notificationService = new NotificationService(_notificationEventHandler, userId, _permissionLevel);

            return Result.Success;
        }

        [CmifCommand(2)] // 4.0.0+
        public Result CreateDaemonSuspendSessionService(out IDaemonSuspendSessionService daemonSuspendSessionService)
        {
            daemonSuspendSessionService = new DaemonSuspendSessionService();

            return Result.Success;
        }
    }
}
