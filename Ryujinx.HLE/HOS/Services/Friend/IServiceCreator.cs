using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.Utilities;
using System.Collections.Generic;

using static Ryujinx.HLE.HOS.ErrorCode;

namespace Ryujinx.HLE.HOS.Services.Friend
{
    class IServiceCreator : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public IServiceCreator()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, CreateFriendService               },
                { 1, CreateNotificationService         }, // 2.0.0+
                { 2, CreateDaemonSuspendSessionService }, // 4.0.0+
            };
        }

        // CreateFriendService() -> object<nn::friends::detail::ipc::IFriendService>
        public static long CreateFriendService(ServiceCtx context)
        {
            MakeObject(context, new IFriendService());

            return 0;
        }

        // CreateNotificationService(nn::account::Uid) -> object<nn::friends::detail::ipc::INotificationService>
        public static long CreateNotificationService(ServiceCtx context)
        {
            UInt128 userId = new UInt128(context.RequestData.ReadBytes(0x10));

            if (userId.IsNull)
            {
                return MakeError(ErrorModule.Friends, FriendErr.InvalidArgument);
            }

            MakeObject(context, new INotificationService(userId));

            return 0;
        }

        // CreateDaemonSuspendSessionService() -> object<nn::friends::detail::ipc::IDaemonSuspendSessionService>
        public static long CreateDaemonSuspendSessionService(ServiceCtx context)
        {
            MakeObject(context, new IDaemonSuspendSessionService());

            return 0;
        }
    }
}
