namespace Ryujinx.HLE.HOS.Applets.SoftwareKeyboard
{
    /// <summary>
    /// Identifies the display mode of text in a password field.
    /// </summary>
    enum PasswordMode : uint
    {
        /// <summary>
        /// Display input characters.
        /// </summary>
        Disabled,

        /// <summary>
        /// Hide input characters.
        /// </summary>
        Enabled,
    }
}
