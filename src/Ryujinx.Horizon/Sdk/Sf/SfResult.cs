using Ryujinx.Horizon.Common;

namespace Ryujinx.Horizon.Sdk.Sf
{
    static class SfResult
    {
        public const int ModuleId = 10;

#pragma warning disable IDE0055 // Disable formatting
        public static Result NotSupported           => new(ModuleId, 1);
        public static Result InvalidHeaderSize      => new(ModuleId, 202);
        public static Result InvalidInHeader        => new(ModuleId, 211);
        public static Result InvalidOutHeader       => new(ModuleId, 212);
        public static Result UnknownCommandId       => new(ModuleId, 221);
        public static Result InvalidOutRawSize      => new(ModuleId, 232);
        public static Result InvalidInObjectsCount  => new(ModuleId, 235);
        public static Result InvalidOutObjectsCount => new(ModuleId, 236);
        public static Result InvalidInObject        => new(ModuleId, 239);
        public static Result TargetNotFound         => new(ModuleId, 261);
        public static Result OutOfDomainEntries     => new(ModuleId, 301);
        public static Result InvalidatedByUser      => new(ModuleId, 802);
        public static Result RequestDeferredByUser  => new(ModuleId, 812);

        public static bool RequestContextChanged(Result result) => result.InRange(800, 899);
        public static bool Invalidated(Result result)           => result.InRange(801, 809);
        public static bool RequestDeferred(Result result)       => result.InRange(811, 819);
#pragma warning restore IDE0055
    }
}
