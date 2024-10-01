using Ryujinx.Horizon.Common;

namespace Ryujinx.Horizon.Sdk.Arp
{
    static class ArpResult
    {
        private const int ModuleId = 157;

        public static Result InvalidArgument => new(ModuleId, 30);
        public static Result InvalidPid => new(ModuleId, 31);
        public static Result InvalidPointer => new(ModuleId, 32);
        public static Result DataAlreadyBound => new(ModuleId, 42);
        public static Result AllocationFailed => new(ModuleId, 63);
        public static Result NoFreeInstance => new(ModuleId, 101);
        public static Result InvalidInstanceId => new(ModuleId, 102);
    }
}
