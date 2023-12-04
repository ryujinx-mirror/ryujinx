using System;
using System.Text;

namespace Ryujinx.Horizon.Sdk.Ngc.Detail
{
    static class Utf8Util
    {
        public static Utf8ParseResult NormalizeFormKC(Span<byte> output, ReadOnlySpan<byte> input)
        {
            int length = input.IndexOf((byte)0);
            if (length >= 0)
            {
                input = input[..length];
            }

            UTF8Encoding encoding = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

            string text;

            try
            {
                text = encoding.GetString(input);
            }
            catch (ArgumentException)
            {
                return Utf8ParseResult.InvalidCharacter;
            }

            string normalizedText = text.Normalize(NormalizationForm.FormKC);

            int outputIndex = Encoding.UTF8.GetBytes(normalizedText, output);

            if (outputIndex < output.Length)
            {
                output[outputIndex] = 0;
            }

            return Utf8ParseResult.Success;
        }
    }
}
