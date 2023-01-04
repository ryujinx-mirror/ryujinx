using Ryujinx.Horizon.Common;

namespace Ryujinx.Horizon.Sdk.Sf.Hipc
{
    static class HipcResult
    {
        public const int ModuleId = 11;

        public static Result OutOfSessionMemory => new Result(ModuleId, 102);
        public static Result OutOfSessions => new Result(ModuleId, 131);
        public static Result PointerBufferTooSmall => new Result(ModuleId, 141);
        public static Result OutOfDomains => new Result(ModuleId, 200);

        public static Result InvalidRequestSize => new Result(ModuleId, 402);
        public static Result UnknownCommandType => new Result(ModuleId, 403);

        public static Result InvalidCmifRequest => new Result(ModuleId, 420);

        public static Result TargetNotDomain => new Result(ModuleId, 491);
        public static Result DomainObjectNotFound => new Result(ModuleId, 492);
    }
}
