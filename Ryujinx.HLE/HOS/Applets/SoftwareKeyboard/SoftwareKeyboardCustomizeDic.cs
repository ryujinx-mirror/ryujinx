using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Applets.SoftwareKeyboard
{
    /// <summary>
    /// A structure used by SetCustomizeDic request to software keyboard.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    struct SoftwareKeyboardCustomizeDic
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 112)]
        public byte[] Unknown;
    }
}
