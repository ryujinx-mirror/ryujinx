namespace Ryujinx.HLE.HOS.Applets.SoftwareKeyboard
{
    /// <summary>
    /// Possible requests to the software keyboard when running in inline mode.
    /// </summary>
    enum InlineKeyboardRequest : uint
    {
        /// <summary>
        /// Finalize the keyboard applet.
        /// </summary>
        Finalize = 0x4,

        /// <summary>
        /// Set user words for text prediction.
        /// </summary>
        SetUserWordInfo = 0x6,

        /// <summary>
        /// Sets the CustomizeDic data. Can't be used if CustomizedDictionaries is already set.
        /// </summary>
        SetCustomizeDic = 0x7,

        /// <summary>
        /// Configure the keyboard applet and put it in a state where it is processing input.
        /// </summary>
        Calc = 0xA,

        /// <summary>
        /// Set custom dictionaries for text prediction. Can't be used if SetCustomizeDic is already set.
        /// </summary>
        SetCustomizedDictionaries = 0xB,

        /// <summary>
        /// Release custom dictionaries data.
        /// </summary>
        UnsetCustomizedDictionaries = 0xC,

        /// <summary>
        /// [8.0.0+] Request the keyboard applet to use the ChangedStringV2 response when notifying changes in text data.
        /// </summary>
        UseChangedStringV2 = 0xD,

        /// <summary>
        /// [8.0.0+] Request the keyboard applet to use the MovedCursorV2 response when notifying changes in cursor position.
        /// </summary>
        UseMovedCursorV2 = 0xE
    }
}
