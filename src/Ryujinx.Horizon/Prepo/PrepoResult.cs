using Ryujinx.Horizon.Common;

namespace Ryujinx.Horizon.Prepo
{
    static class PrepoResult
    {
        private const int ModuleId = 129;

#pragma warning disable IDE0055 // Disable formatting
        public static Result InvalidArgument   => new(ModuleId, 1);
        public static Result InvalidState      => new(ModuleId, 5);
        public static Result InvalidBufferSize => new(ModuleId, 9);
        public static Result PermissionDenied  => new(ModuleId, 90);
        public static Result InvalidPid        => new(ModuleId, 101);
#pragma warning restore IDE0055
    }
}
