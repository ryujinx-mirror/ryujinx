using Ryujinx.Cpu.LightningJit.State;

namespace Ryujinx.Cpu.LightningJit
{
    class NativeContextOffsets
    {
        public static int GprBaseOffset => NativeContext.GetXOffset();
        public static int FpSimdBaseOffset => NativeContext.GetVOffset();
        public static int FlagsBaseOffset => NativeContext.GetFlagsOffset();
        public static int FpFlagsBaseOffset => NativeContext.GetFpFlagsOffset();
        public static int TpidrEl0Offset => NativeContext.GetTpidrEl0Offset();
        public static int TpidrroEl0Offset => NativeContext.GetTpidrroEl0Offset();
        public static int RunningOffset => NativeContext.GetRunningOffset();
        public static int CounterOffset => NativeContext.GetCounterOffset();
        public static int DispatchAddressOffset => NativeContext.GetDispatchAddressOffset();
    }
}
