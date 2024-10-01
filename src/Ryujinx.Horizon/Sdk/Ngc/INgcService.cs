using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Sf;
using System;

namespace Ryujinx.Horizon.Sdk.Ngc
{
    interface INgcService : IServiceObject
    {
        Result GetContentVersion(out uint version);
        Result Check(out uint checkMask, ReadOnlySpan<byte> text, uint regionMask, ProfanityFilterOption option);
        Result Mask(out int maskedWordsCount, Span<byte> filteredText, ReadOnlySpan<byte> text, uint regionMask, ProfanityFilterOption option);
        Result Reload();
    }
}
