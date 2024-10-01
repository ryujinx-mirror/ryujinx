using Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Common;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Npad
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct NpadGcTriggerState : ISampledDataStruct
    {
#pragma warning disable CS0649 // Field is never assigned to
        public ulong SamplingNumber;
        public uint TriggerL;
        public uint TriggerR;
#pragma warning restore CS0649
    }
}
