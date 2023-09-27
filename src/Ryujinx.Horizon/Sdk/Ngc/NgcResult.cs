using Ryujinx.Horizon.Common;

namespace Ryujinx.Horizon.Sdk.Ngc
{
    static class NgcResult
    {
        private const int ModuleId = 146;

        public static Result InvalidPointer => new(ModuleId, 3);
        public static Result InvalidSize => new(ModuleId, 4);
        public static Result InvalidUtf8Encoding => new(ModuleId, 5);
        public static Result AllocationFailed => new(ModuleId, 101);
        public static Result DataAccessError => new(ModuleId, 102);
        public static Result GenericUtf8Error => new(ModuleId, 103);
    }
}
