using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Applets.SoftwareKeyboard
{
    /// <summary>
    /// A structure used by SetUserWordInfo request to the software keyboard.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 0x64)]
    struct SoftwareKeyboardUserWord
    {
        // Unknown
    }
}
