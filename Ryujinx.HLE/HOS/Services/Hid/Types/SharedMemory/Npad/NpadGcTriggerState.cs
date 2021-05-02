using Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Common;

namespace Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Npad
{
    struct NpadGcTriggerState : ISampledData
    {
#pragma warning disable CS0649
        public ulong SamplingNumber;
        public uint TriggerL;
        public uint TriggerR;
#pragma warning restore CS0649

        ulong ISampledData.SamplingNumber => SamplingNumber;
    }
}