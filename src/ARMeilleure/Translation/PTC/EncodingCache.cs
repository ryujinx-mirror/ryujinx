using System.Text;

namespace ARMeilleure.Translation.PTC
{
    static class EncodingCache
    {
        public static readonly Encoding UTF8NoBOM = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
    }
}
