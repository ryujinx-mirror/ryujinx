using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Time.Clock
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct SystemClockContext
    {
        public long Offset;
        public SteadyClockTimePoint SteadyTimePoint;
    }
}
