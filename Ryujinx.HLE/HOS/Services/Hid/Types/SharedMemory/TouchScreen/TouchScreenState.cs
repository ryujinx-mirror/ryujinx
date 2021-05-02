using Ryujinx.Common.Memory;
using Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Common;

namespace Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.TouchScreen
{
    struct TouchScreenState : ISampledData
    {
        public ulong SamplingNumber;
        public int TouchesCount;
        private int _reserved;
        public Array16<TouchState> Touches;

        ulong ISampledData.SamplingNumber => SamplingNumber;
    }
}
