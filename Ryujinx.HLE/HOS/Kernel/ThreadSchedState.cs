namespace Ryujinx.HLE.HOS.Kernel
{
    enum ThreadSchedState : byte
    {
        LowNibbleMask   = 0xf,
        HighNibbleMask  = 0xf0,
        ExceptionalMask = 0x70,
        ForcePauseFlag  = 0x20,

        None               = 0,
        Paused             = 1,
        Running            = 2,
        TerminationPending = 3
    }
}