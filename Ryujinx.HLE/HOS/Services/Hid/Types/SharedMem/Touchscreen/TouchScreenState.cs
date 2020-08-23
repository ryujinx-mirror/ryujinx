using Ryujinx.Common.Memory;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    struct TouchScreenState
    {
        public ulong SampleTimestamp;
        public ulong SampleTimestamp2;
        public ulong NumTouches;
        public Array16<TouchScreenStateData> Touches;
    }
}