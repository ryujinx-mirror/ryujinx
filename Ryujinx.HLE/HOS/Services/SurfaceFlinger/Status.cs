namespace Ryujinx.HLE.HOS.Services.SurfaceFlinger
{
    enum Status : int
    {
        Success          = 0,
        WouldBlock       = -11,
        NoMemory         = -12,
        Busy             = -16,
        NoInit           = -19,
        BadValue         = -22,
        InvalidOperation = -37,

        // Producer flags
        BufferNeedsReallocation = 1,
        ReleaseAllBuffers       = 2,

        // Consumer errors
        StaleBufferSlot    = 1,
        NoBufferAvailaible = 2,
        PresentLater       = 3,
    }
}
