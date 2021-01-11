namespace Ryujinx.HLE.HOS.Applets.SoftwareKeyboard
{
    /// <summary>
    /// Possible responses from the keyboard when running in inline mode.
    /// </summary>
    enum InlineKeyboardResponse : uint
    {
        FinishedInitialize          = 0x0,
        Default                     = 0x1,
        ChangedString               = 0x2,
        MovedCursor                 = 0x3,
        MovedTab                    = 0x4,
        DecidedEnter                = 0x5,
        DecidedCancel               = 0x6,
        ChangedStringUtf8           = 0x7,
        MovedCursorUtf8             = 0x8,
        DecidedEnterUtf8            = 0x9,
        UnsetCustomizeDic           = 0xA,
        ReleasedUserWordInfo        = 0xB,
        UnsetCustomizedDictionaries = 0xC,
        ChangedStringV2             = 0xD,
        MovedCursorV2               = 0xE,
        ChangedStringUtf8V2         = 0xF,
        MovedCursorUtf8V2           = 0x10
    }
}
