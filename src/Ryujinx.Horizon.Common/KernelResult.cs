namespace Ryujinx.Horizon.Common
{
    public static class KernelResult
    {
        private const int ModuleId = 1;

        public static Result SessionCountExceeded => new Result(ModuleId, 7);
        public static Result InvalidCapability => new Result(ModuleId, 14);
        public static Result ThreadNotStarted => new Result(ModuleId, 57);
        public static Result ThreadTerminating => new Result(ModuleId, 59);
        public static Result InvalidSize => new Result(ModuleId, 101);
        public static Result InvalidAddress => new Result(ModuleId, 102);
        public static Result OutOfResource => new Result(ModuleId, 103);
        public static Result OutOfMemory => new Result(ModuleId, 104);
        public static Result HandleTableFull => new Result(ModuleId, 105);
        public static Result InvalidMemState => new Result(ModuleId, 106);
        public static Result InvalidPermission => new Result(ModuleId, 108);
        public static Result InvalidMemRange => new Result(ModuleId, 110);
        public static Result InvalidPriority => new Result(ModuleId, 112);
        public static Result InvalidCpuCore => new Result(ModuleId, 113);
        public static Result InvalidHandle => new Result(ModuleId, 114);
        public static Result UserCopyFailed => new Result(ModuleId, 115);
        public static Result InvalidCombination => new Result(ModuleId, 116);
        public static Result TimedOut => new Result(ModuleId, 117);
        public static Result Cancelled => new Result(ModuleId, 118);
        public static Result MaximumExceeded => new Result(ModuleId, 119);
        public static Result InvalidEnumValue => new Result(ModuleId, 120);
        public static Result NotFound => new Result(ModuleId, 121);
        public static Result InvalidThread => new Result(ModuleId, 122);
        public static Result PortRemoteClosed => new Result(ModuleId, 123);
        public static Result InvalidState => new Result(ModuleId, 125);
        public static Result ReservedValue => new Result(ModuleId, 126);
        public static Result PortClosed => new Result(ModuleId, 131);
        public static Result ResLimitExceeded => new Result(ModuleId, 132);
        public static Result ReceiveListBroken => new Result(ModuleId, 258);
        public static Result OutOfVaSpace => new Result(ModuleId, 259);
        public static Result CmdBufferTooSmall => new Result(ModuleId, 260);
    }
}
