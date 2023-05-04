using Ryujinx.Horizon.Common;

namespace Ryujinx.Horizon.Bcat
{
    class BcatResult
    {
        private const int ModuleId = 122;

        public static Result Success => new(ModuleId, 0);
        public static Result InvalidArgument => new(ModuleId, 1);
        public static Result NotFound => new(ModuleId, 2);
        public static Result TargetLocked => new(ModuleId, 3);
        public static Result TargetAlreadyMounted => new(ModuleId, 4);
        public static Result TargetNotMounted => new(ModuleId, 5);
        public static Result AlreadyOpen => new(ModuleId, 6);
        public static Result NotOpen => new(ModuleId, 7);
        public static Result InternetRequestDenied => new(ModuleId, 8);
        public static Result ServiceOpenLimitReached => new(ModuleId, 9);
        public static Result SaveDataNotFound => new(ModuleId, 10);
        public static Result NetworkServiceAccountNotAvailable => new(ModuleId, 31);
        public static Result PassphrasePathNotFound => new(ModuleId, 80);
        public static Result DataVerificationFailed => new(ModuleId, 81);
        public static Result PermissionDenied => new(ModuleId, 90);
        public static Result AllocationFailed => new(ModuleId, 91);
        public static Result InvalidOperation => new(ModuleId, 98);
        public static Result InvalidDeliveryCacheStorageFile => new(ModuleId, 204);
        public static Result StorageOpenLimitReached => new(ModuleId, 205);
    }
}
