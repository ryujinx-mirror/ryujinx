using System.Text.RegularExpressions;

namespace Ryujinx.HLE.HOS.Applets.SoftwareKeyboard
{
    public static partial class CJKCharacterValidation
    {
        public static bool IsCJK(char value)
        {
            Regex regex = CJKRegex();

            return regex.IsMatch(value.ToString());
        }

        [GeneratedRegex("\\p{IsHangulJamo}|\\p{IsCJKRadicalsSupplement}|\\p{IsCJKSymbolsandPunctuation}|\\p{IsEnclosedCJKLettersandMonths}|\\p{IsCJKCompatibility}|\\p{IsCJKUnifiedIdeographsExtensionA}|\\p{IsCJKUnifiedIdeographs}|\\p{IsHangulSyllables}|\\p{IsCJKCompatibilityForms}")]
        private static partial Regex CJKRegex();
    }
}
