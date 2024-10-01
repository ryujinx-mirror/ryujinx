namespace Ryujinx.HLE.HOS.Kernel.Process
{
    enum ProcessState : byte
    {
        Created = 0,
        CreatedAttached = 1,
        Started = 2,
        Crashed = 3,
        Attached = 4,
        Exiting = 5,
        Exited = 6,
        DebugSuspended = 7,
    }
}
