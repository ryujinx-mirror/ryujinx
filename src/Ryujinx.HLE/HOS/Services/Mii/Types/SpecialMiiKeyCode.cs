using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Mii.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 4)]
    struct SpecialMiiKeyCode
    {
        private const uint SpecialMiiMagic = 0xA523B78F;

        public uint RawValue;

        public readonly bool IsEnabledSpecialMii()
        {
            return RawValue == SpecialMiiMagic;
        }
    }
}
