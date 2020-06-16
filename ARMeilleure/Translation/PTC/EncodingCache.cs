using System.Text;

namespace ARMeilleure.Translation.PTC
{
    internal static class EncodingCache
    {
        internal static readonly Encoding UTF8NoBOM = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
    }
}