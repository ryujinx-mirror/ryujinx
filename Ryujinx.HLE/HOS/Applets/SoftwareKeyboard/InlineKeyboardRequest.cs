namespace Ryujinx.HLE.HOS.Applets.SoftwareKeyboard
{
    /// <summary>
    /// Possible requests to the keyboard when running in inline mode.
    /// </summary>
    enum InlineKeyboardRequest : uint
    {
        Unknown0                    = 0x0,
        Finalize                    = 0x4,
        SetUserWordInfo             = 0x6,
        SetCustomizeDic             = 0x7,
        Calc                        = 0xA,
        SetCustomizedDictionaries   = 0xB,
        UnsetCustomizedDictionaries = 0xC,
        UseChangedStringV2          = 0xD,
        UseMovedCursorV2            = 0xE
    }
}
