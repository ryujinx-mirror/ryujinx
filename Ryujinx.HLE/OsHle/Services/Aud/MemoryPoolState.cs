namespace Ryujinx.HLE.OsHle.Services.Aud
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
