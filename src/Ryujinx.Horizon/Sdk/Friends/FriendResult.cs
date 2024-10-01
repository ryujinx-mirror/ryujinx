using Ryujinx.Horizon.Common;

namespace Ryujinx.Horizon.Sdk.Friends
{
    static class FriendResult
    {
        private const int ModuleId = 121;

        public static Result InvalidArgument => new(ModuleId, 2);
        public static Result InternetRequestDenied => new(ModuleId, 6);
        public static Result NotificationQueueEmpty => new(ModuleId, 15);
    }
}
