namespace Ryujinx.HLE.HOS.Services.Audio.AudioRendererManager
{
    enum MemoryPoolState
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