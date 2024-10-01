using Ryujinx.HLE.HOS.Services.Time.Clock;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.SurfaceFlinger.Types
{
    [StructLayout(LayoutKind.Sequential, Size = 0x1C, Pack = 1)]
    struct BufferInfo
    {
        public ulong FrameNumber;
        public TimeSpanType QueueTime;
        public TimeSpanType PresentationTime;
        public BufferState State;
    }
}
