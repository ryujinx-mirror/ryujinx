namespace Ryujinx.HLE.HOS.Applets.SoftwareKeyboard
{
    /// <summary>
    /// Identifies the text entry mode.
    /// </summary>
    enum InputFormMode : uint
    {
        /// <summary>
        /// Displays the text entry area as a single-line field.
        /// </summary>
        SingleLine,

        /// <summary>
        /// Displays the text entry area as a multi-line field.
        /// </summary>
        MultiLine,
    }
}
