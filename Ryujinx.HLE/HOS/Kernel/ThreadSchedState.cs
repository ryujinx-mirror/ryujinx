namespace Ryujinx.HLE.HOS.Kernel
{
    enum ThreadSchedState : ushort
    {
        LowMask        = 0xf,
        HighMask       = 0xfff0,
        ForcePauseMask = 0x70,

        ProcessPauseFlag      = 1 << 4,
        ThreadPauseFlag       = 1 << 5,
        ProcessDebugPauseFlag = 1 << 6,
        KernelInitPauseFlag   = 1 << 8,

        None               = 0,
        Paused             = 1,
        Running            = 2,
        TerminationPending = 3
    }
}