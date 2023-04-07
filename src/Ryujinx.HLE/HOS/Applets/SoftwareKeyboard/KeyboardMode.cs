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
        Default = 0,

        /// <summary>
        /// Number pad.
        /// </summary>
        NumbersOnly = 1,

        /// <summary>
        /// ASCII characters keyboard.
        /// </summary>
        ASCII = 2,

        FullLatin          = 3,
        Alphabet           = 4,
        SimplifiedChinese  = 5,
        TraditionalChinese = 6,
        Korean             = 7,
        LanguageSet2       = 8,
        LanguageSet2Latin  = 9,
    }
}
