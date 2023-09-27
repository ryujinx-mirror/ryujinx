using Ryujinx.Horizon.Common;

namespace Ryujinx.Horizon.Sdk.Fs
{
    static class FsResult
    {
        private const int ModuleId = 2;

        public static Result PathNotFound => new(ModuleId, 1);
        public static Result PathAlreadyExists => new(ModuleId, 2);
        public static Result TargetNotFound => new(ModuleId, 1002);
    }
}
