using System.Diagnostics.CodeAnalysis;

namespace Ryujinx.HLE.HOS.Services.SurfaceFlinger
{
    [SuppressMessage("Design", "CA1069: Enums values should not be duplicated")]
    enum Status
    {
        Success = 0,
        WouldBlock = -11,
        NoMemory = -12,
        Busy = -16,
        NoInit = -19,
        BadValue = -22,
        InvalidOperation = -37,

        // Producer flags
        BufferNeedsReallocation = 1,
        ReleaseAllBuffers = 2,

        // Consumer errors
        StaleBufferSlot = 1,
        NoBufferAvailaible = 2,
        PresentLater = 3,
    }
}
