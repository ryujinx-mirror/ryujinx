using Ryujinx.Horizon.Common;

namespace Ryujinx.Horizon.Sdk.Sf.Hipc
{
    static class HipcResult
    {
        public const int ModuleId = 11;

#pragma warning disable IDE0055 // Disable formatting
        public static Result OutOfSessionMemory    => new(ModuleId, 102);
        public static Result OutOfSessions         => new(ModuleId, 131);
        public static Result PointerBufferTooSmall => new(ModuleId, 141);
        public static Result OutOfDomains          => new(ModuleId, 200);
        public static Result InvalidRequestSize    => new(ModuleId, 402);
        public static Result UnknownCommandType    => new(ModuleId, 403);
        public static Result InvalidCmifRequest    => new(ModuleId, 420);
        public static Result TargetNotDomain       => new(ModuleId, 491);
        public static Result DomainObjectNotFound  => new(ModuleId, 492);
        #pragma warning restore IDE0055
    }
}
