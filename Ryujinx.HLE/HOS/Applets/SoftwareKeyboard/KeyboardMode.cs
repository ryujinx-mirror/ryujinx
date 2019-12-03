namespace Ryujinx.HLE.HOS.Applets.SoftwareKeyboard
{
    /// <summary>
    /// Identifies the variant of keyboard displayed on screen.
    /// </summary>
    enum KeyboardMode : uint
    {
        /// <summary>
        /// A full alpha-numeric keyboard.
        /// </summary>
        Default,

        /// <summary>
        /// Number pad.
        /// </summary>
        NumbersOnly,

        /// <summary>
        /// QWERTY (and variants) keyboard only.
        /// </summary>
        LettersOnly,

        /// <summary>
        /// Unknown keyboard variant.
        /// </summary>
        Unknown
    }
}
