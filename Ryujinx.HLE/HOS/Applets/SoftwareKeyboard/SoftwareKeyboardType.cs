namespace Ryujinx.HLE.HOS.Applets.SoftwareKeyboard
{
    internal enum SoftwareKeyboardType : uint
    {
        /// <summary>
        /// Normal keyboard.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Number pad. The buttons at the bottom left/right are only available when they're set in the config by leftButtonText / rightButtonText.
        /// </summary>
        NumbersOnly = 1,

        /// <summary>
        /// QWERTY (and variants) keyboard only.
        /// </summary>
        LettersOnly = 2
    }
}