using Ryujinx.Horizon.Common;

namespace Ryujinx.Horizon.Sdk.Sf
{
    static class SfResult
    {
        public const int ModuleId = 10;

        public static Result NotSupported => new Result(ModuleId, 1);
        public static Result InvalidHeaderSize => new Result(ModuleId, 202);
        public static Result InvalidInHeader => new Result(ModuleId, 211);
        public static Result InvalidOutHeader => new Result(ModuleId, 212);
        public static Result UnknownCommandId => new Result(ModuleId, 221);
        public static Result InvalidOutRawSize => new Result(ModuleId, 232);
        public static Result InvalidInObjectsCount => new Result(ModuleId, 235);
        public static Result InvalidOutObjectsCount => new Result(ModuleId, 236);
        public static Result InvalidInObject => new Result(ModuleId, 239);

        public static Result TargetNotFound => new Result(ModuleId, 261);

        public static Result OutOfDomainEntries => new Result(ModuleId, 301);

        public static Result InvalidatedByUser => new Result(ModuleId, 802);
        public static Result RequestDeferredByUser => new Result(ModuleId, 812);

        public static bool RequestContextChanged(Result result) => result.InRange(800, 899);
        public static bool Invalidated(Result result) => result.InRange(801, 809);

        public static bool RequestDeferred(Result result) => result.InRange(811, 819);
    }
}
