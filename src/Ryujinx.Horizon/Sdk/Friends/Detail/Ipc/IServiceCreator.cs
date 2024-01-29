using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Account;
using Ryujinx.Horizon.Sdk.Sf;

namespace Ryujinx.Horizon.Sdk.Friends.Detail.Ipc
{
    interface IServiceCreator : IServiceObject
    {
        Result CreateFriendService(out IFriendService friendService);
        Result CreateNotificationService(out INotificationService notificationService, Uid userId);
        Result CreateDaemonSuspendSessionService(out IDaemonSuspendSessionService daemonSuspendSessionService);
    }
}
