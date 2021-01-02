using Ryujinx.Common;
using Ryujinx.HLE.HOS.Services.Account.Acc;
using Ryujinx.HLE.HOS.Services.Friend.ServiceCreator;

namespace Ryujinx.HLE.HOS.Services.Friend
{
    [Service("friend:a", FriendServicePermissionLevel.Admin)]
    [Service("friend:m", FriendServicePermissionLevel.Manager)]
    [Service("friend:s", FriendServicePermissionLevel.System)]
    [Service("friend:u", FriendServicePermissionLevel.User)]
    [Service("friend:v", FriendServicePermissionLevel.Overlay)]
    class IServiceCreator : IpcService
    {
        private FriendServicePermissionLevel _permissionLevel;

        public IServiceCreator(ServiceCtx context, FriendServicePermissionLevel permissionLevel)
        {
            _permissionLevel = permissionLevel;
        }

        [Command(0)]
        // CreateFriendService() -> object<nn::friends::detail::ipc::IFriendService>
        public ResultCode CreateFriendService(ServiceCtx context)
        {
            MakeObject(context, new IFriendService(_permissionLevel));

            return ResultCode.Success;
        }

        [Command(1)] // 2.0.0+
        // CreateNotificationService(nn::account::Uid userId) -> object<nn::friends::detail::ipc::INotificationService>
        public ResultCode CreateNotificationService(ServiceCtx context)
        {
            UserId userId = context.RequestData.ReadStruct<UserId>();

            if (userId.IsNull)
            {
                return ResultCode.InvalidArgument;
            }

            MakeObject(context, new INotificationService(context, userId, _permissionLevel));

            return ResultCode.Success;
        }

        [Command(2)] // 4.0.0+
        // CreateDaemonSuspendSessionService() -> object<nn::friends::detail::ipc::IDaemonSuspendSessionService>
        public ResultCode CreateDaemonSuspendSessionService(ServiceCtx context)
        {
            MakeObject(context, new IDaemonSuspendSessionService(_permissionLevel));

            return ResultCode.Success;
        }
    }
}