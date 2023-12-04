using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Time.Clock.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct ContinuousAdjustmentTimePoint
    {
        public ulong ClockOffset;
        public long Multiplier;
        public long DivisorLog2;
        public SystemClockContext Context;
    }
}
