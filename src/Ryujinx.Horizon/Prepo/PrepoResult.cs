using Ryujinx.Horizon.Common;

namespace Ryujinx.Horizon.Prepo
{
    static class PrepoResult
    {
        private const int ModuleId = 129;

        public static Result InvalidArgument   => new(ModuleId, 1);
        public static Result InvalidState      => new(ModuleId, 5);
        public static Result InvalidBufferSize => new(ModuleId, 9);
        public static Result PermissionDenied  => new(ModuleId, 90);
        public static Result InvalidPid        => new(ModuleId, 101);
    }
}