using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Applets.SoftwareKeyboard
{
    /// <summary>
    /// A structure that indicates the initialization the inline software keyboard.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct SoftwareKeyboardInitialize
    {
        public uint Unknown;
        public byte LibMode;
        public byte FivePlus;
        public byte Padding1;
        public byte Padding2;
    }
}
