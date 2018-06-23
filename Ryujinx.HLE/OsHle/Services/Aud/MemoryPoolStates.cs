namespace Ryujinx.HLE.OsHle.Services.Aud
{
    enum MemoryPoolStates : int
    {
        Invalid       = 0x0,
        Unknown       = 0x1,
        RequestDetach = 0x2,
        Detached      = 0x3,
        RequestAttach = 0x4,
        Attached      = 0x5,
        Released      = 0x6,
    }
}
