using Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Common;

namespace Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Npad
{
    struct NpadCommonState : ISampledData
    {
        public ulong SamplingNumber;
        public NpadButton Buttons;
        public AnalogStickState AnalogStickL;
        public AnalogStickState AnalogStickR;
        public NpadAttribute Attributes;
        private uint _reserved;

        ulong ISampledData.SamplingNumber => SamplingNumber;
    }
}
