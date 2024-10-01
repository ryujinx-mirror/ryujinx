namespace Ryujinx.HLE.HOS.Applets.SoftwareKeyboard
{
    /// <summary>
    /// Possible responses from the software keyboard when running in inline mode.
    /// </summary>
    enum InlineKeyboardResponse : uint
    {
        /// <summary>
        /// The software keyboard received a Calc and it is fully initialized. Reply data is ignored by the user-process.
        /// </summary>
        FinishedInitialize = 0x0,

        /// <summary>
        /// Default response. Official sw has no handling for this besides just closing the storage.
        /// </summary>
        Default = 0x1,

        /// <summary>
        /// The text data in the software keyboard changed (UTF-16 encoding).
        /// </summary>
        ChangedString = 0x2,

        /// <summary>
        /// The cursor position in the software keyboard changed (UTF-16 encoding).
        /// </summary>
        MovedCursor = 0x3,

        /// <summary>
        /// A tab in the software keyboard changed.
        /// </summary>
        MovedTab = 0x4,

        /// <summary>
        /// The OK key was pressed in the software keyboard, confirming the input text (UTF-16 encoding).
        /// </summary>
        DecidedEnter = 0x5,

        /// <summary>
        /// The Cancel key was pressed in the software keyboard, cancelling the input.
        /// </summary>
        DecidedCancel = 0x6,

        /// <summary>
        /// Same as ChangedString, but with UTF-8 encoding.
        /// </summary>
        ChangedStringUtf8 = 0x7,

        /// <summary>
        /// Same as MovedCursor, but with UTF-8 encoding.
        /// </summary>
        MovedCursorUtf8 = 0x8,

        /// <summary>
        /// Same as DecidedEnter, but with UTF-8 encoding.
        /// </summary>
        DecidedEnterUtf8 = 0x9,

        /// <summary>
        /// They software keyboard is releasing the data previously set by a SetCustomizeDic request.
        /// </summary>
        UnsetCustomizeDic = 0xA,

        /// <summary>
        /// They software keyboard is releasing the data previously set by a SetUserWordInfo request.
        /// </summary>
        ReleasedUserWordInfo = 0xB,

        /// <summary>
        /// They software keyboard is releasing the data previously set by a SetCustomizedDictionaries request.
        /// </summary>
        UnsetCustomizedDictionaries = 0xC,

        /// <summary>
        /// Same as ChangedString, but with additional fields.
        /// </summary>
        ChangedStringV2 = 0xD,

        /// <summary>
        /// Same as MovedCursor, but with additional fields.
        /// </summary>
        MovedCursorV2 = 0xE,

        /// <summary>
        /// Same as ChangedStringUtf8, but with additional fields.
        /// </summary>
        ChangedStringUtf8V2 = 0xF,

        /// <summary>
        /// Same as MovedCursorUtf8, but with additional fields.
        /// </summary>
        MovedCursorUtf8V2 = 0x10,
    }
}
