using Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Common;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Keyboard
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct KeyboardState : ISampledDataStruct
    {
        public ulong SamplingNumber;
        public KeyboardModifier Modifiers;
        public KeyboardKey Keys;
    }
}
