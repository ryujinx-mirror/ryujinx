using Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Common;

namespace Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.DebugPad
{
    struct DebugPadState : ISampledData
    {
        public ulong SamplingNumber;
        public DebugPadAttribute Attributes;
        public DebugPadButton Buttons;
        public AnalogStickState AnalogStickR;
        public AnalogStickState AnalogStickL;

        ulong ISampledData.SamplingNumber => SamplingNumber;
    }
}
