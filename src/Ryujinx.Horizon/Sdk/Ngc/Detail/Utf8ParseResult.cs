using Ryujinx.Horizon.Common;

namespace Ryujinx.Horizon.Sdk.Ngc.Detail
{
    enum Utf8ParseResult
    {
        Success = 0,
        InvalidCharacter = 2,
        InvalidPointer = 0x16,
        InvalidSize = 0x22,
        InvalidString = 0x54,
    }

    static class Utf8ParseResultExtensions
    {
        public static Result ToHorizonResult(this Utf8ParseResult result)
        {
            return result switch
            {
                Utf8ParseResult.Success => Result.Success,
                Utf8ParseResult.InvalidSize => NgcResult.InvalidSize,
                Utf8ParseResult.InvalidString => NgcResult.InvalidUtf8Encoding,
                _ => NgcResult.InvalidPointer,
            };
        }
    }
}
