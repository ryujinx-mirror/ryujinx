namespace Ryujinx.HLE.HOS.Applets.SoftwareKeyboard
{
    /// <summary>
    /// Identifies the variant of keyboard displayed on screen.
    /// </summary>
    public enum KeyboardMode : uint
    {
        /// <summary>
        /// All UTF-16 characters allowed.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Only 0-9 or '.' allowed.
        /// </summary>
        Numeric = 1,

        /// <summary>
        /// Only ASCII characters allowed.
        /// </summary>
        ASCII = 2,

        /// <summary>
        /// Synonymous with default.
        /// </summary>
        FullLatin = 3,

        /// <summary>
        /// All UTF-16 characters except CJK characters allowed.
        /// </summary>
        Alphabet = 4,

        SimplifiedChinese = 5,
        TraditionalChinese = 6,
        Korean = 7,
        LanguageSet2 = 8,
        LanguageSet2Latin = 9,
    }
}
