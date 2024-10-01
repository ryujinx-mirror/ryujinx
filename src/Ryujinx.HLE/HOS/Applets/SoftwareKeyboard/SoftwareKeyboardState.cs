namespace Ryujinx.HLE.HOS.Applets.SoftwareKeyboard
{
    /// <summary>
    /// Identifies the software keyboard state.
    /// </summary>
    enum SoftwareKeyboardState
    {
        /// <summary>
        /// swkbd is uninitialized.
        /// </summary>
        Uninitialized,

        /// <summary>
        /// swkbd is ready to process data.
        /// </summary>
        Ready,

        /// <summary>
        /// swkbd is awaiting an interactive reply with a validation status.
        /// </summary>
        ValidationPending,

        /// <summary>
        /// swkbd has completed.
        /// </summary>
        Complete,
    }
}
