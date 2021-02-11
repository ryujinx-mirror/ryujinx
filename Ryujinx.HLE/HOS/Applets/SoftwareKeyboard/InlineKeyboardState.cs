namespace Ryujinx.HLE.HOS.Applets.SoftwareKeyboard
{
    /// <summary>
    /// Possible states for the software keyboard when running in inline mode.
    /// </summary>
    enum InlineKeyboardState : uint
    {
        /// <summary>
        /// The software keyboard has just been created or finalized and is uninitialized.
        /// </summary>
        Uninitialized = 0x0,

        /// <summary>
        /// A Calc was previously received and fulfilled, so the software keyboard is initialized, but is not processing input.
        /// </summary>
        Initialized = 0x1,

        /// <summary>
        /// A Calc was received and the software keyboard is processing input.
        /// </summary>
        Ready = 0x2,

        /// <summary>
        /// New text data or cursor position of the software keyboard are available.
        /// </summary>
        DataAvailable = 0x3,

        /// <summary>
        /// The Calc request was fulfilled with either a text input or a cancel.
        /// </summary>
        Complete = 0x4
    }
}
