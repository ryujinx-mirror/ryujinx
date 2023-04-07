namespace Ryujinx.HLE.HOS.Services.SurfaceFlinger
{
    internal enum BufferState
    {
        Free     = 0,
        Dequeued = 1,
        Queued   = 2,
        Acquired = 3
    }
}