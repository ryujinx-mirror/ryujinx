namespace Ryujinx.HLE.HOS.Services.Aud.AudioRenderer
{
    enum MemoryPoolState : int
    {
        Invalid       = 0,
        Unknown       = 1,
        RequestDetach = 2,
        Detached      = 3,
        RequestAttach = 4,
        Attached      = 5,
        Released      = 6
    }
}
