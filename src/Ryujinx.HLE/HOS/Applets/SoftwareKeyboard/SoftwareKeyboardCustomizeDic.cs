using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Applets.SoftwareKeyboard
{
    /// <summary>
    /// A structure used by SetCustomizeDic request to software keyboard.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 0x70)]
    struct SoftwareKeyboardCustomizeDic
    {
        // Unknown
    }
}
