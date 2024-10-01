using System;

namespace Ryujinx.HLE.HOS.Applets.SoftwareKeyboard
{
    /// <summary>
    /// Bitmask of commands encoded in the Flags field of the Calc structs.
    /// </summary>
    [Flags]
    enum KeyboardCalcFlags : ulong
    {
        Initialize = 0x1,
        SetVolume = 0x2,
        Appear = 0x4,
        SetInputText = 0x8,
        SetCursorPos = 0x10,
        SetUtf8Mode = 0x20,
        SetKeyboardBackground = 0x100,
        SetKeyboardOptions1 = 0x200,
        SetKeyboardOptions2 = 0x800,
        EnableSeGroup = 0x2000,
        DisableSeGroup = 0x4000,
        SetBackspaceEnabled = 0x8000,
        AppearTrigger = 0x10000,
        MustShow = Appear | SetInputText | AppearTrigger,
    }
}
