using Ryujinx.Horizon.Common;

namespace Ryujinx.Horizon.Sm
{
    static class SmResult
    {
        private const int ModuleId = 21;

#pragma warning disable IDE0055 // Disable formatting
        public static Result OutOfProcess          => new(ModuleId, 1);
        public static Result InvalidClient         => new(ModuleId, 2);
        public static Result OutOfSessions         => new(ModuleId, 3);
        public static Result AlreadyRegistered     => new(ModuleId, 4);
        public static Result OutOfServices         => new(ModuleId, 5);
        public static Result InvalidServiceName    => new(ModuleId, 6);
        public static Result NotRegistered         => new(ModuleId, 7);
        public static Result NotAllowed            => new(ModuleId, 8);
        public static Result TooLargeAccessControl => new(ModuleId, 9);
#pragma warning restore IDE0055
    }
}
