using Ryujinx.Common;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.Utilities;
using System.Collections.Generic;

using static Ryujinx.HLE.HOS.ErrorCode;

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

        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public IServiceCreator(ServiceCtx context, FriendServicePermissionLevel permissionLevel)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, CreateFriendService               },
                { 1, CreateNotificationService         }, // 2.0.0+
                { 2, CreateDaemonSuspendSessionService }, // 4.0.0+
            };

            _permissionLevel = permissionLevel;
        }

        // CreateFriendService() -> object<nn::friends::detail::ipc::IFriendService>
        public long CreateFriendService(ServiceCtx context)
        {
            MakeObject(context, new IFriendService(_permissionLevel));

            return 0;
        }

        // CreateNotificationService(nn::account::Uid) -> object<nn::friends::detail::ipc::INotificationService>
        public long CreateNotificationService(ServiceCtx context)
        {
            UInt128 userId = context.RequestData.ReadStruct<UInt128>();

            if (userId.IsNull)
            {
                return MakeError(ErrorModule.Friends, FriendError.InvalidArgument);
            }

            MakeObject(context, new INotificationService(context, userId, _permissionLevel));

            return 0;
        }

        // CreateDaemonSuspendSessionService() -> object<nn::friends::detail::ipc::IDaemonSuspendSessionService>
        public long CreateDaemonSuspendSessionService(ServiceCtx context)
        {
            MakeObject(context, new IDaemonSuspendSessionService(_permissionLevel));

            return 0;
        }
    }
}
