using Ryujinx.Common.Memory;
using Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Common;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.TouchScreen
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct TouchScreenState : ISampledDataStruct
    {
        public ulong SamplingNumber;
        public int TouchesCount;
        private readonly int _reserved;
        public Array16<TouchState> Touches;
    }
}
