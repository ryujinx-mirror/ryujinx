using Ryujinx.Horizon.Common;

namespace Ryujinx.Horizon.Sm
{
    static class SmResult
    {
        private const int ModuleId = 21;

        public static Result OutOfProcess => new Result(ModuleId, 1);
        public static Result InvalidClient => new Result(ModuleId, 2);
        public static Result OutOfSessions => new Result(ModuleId, 3);
        public static Result AlreadyRegistered => new Result(ModuleId, 4);
        public static Result OutOfServices => new Result(ModuleId, 5);
        public static Result InvalidServiceName => new Result(ModuleId, 6);
        public static Result NotRegistered => new Result(ModuleId, 7);
        public static Result NotAllowed => new Result(ModuleId, 8);
        public static Result TooLargeAccessControl => new Result(ModuleId, 9);
    }
}
