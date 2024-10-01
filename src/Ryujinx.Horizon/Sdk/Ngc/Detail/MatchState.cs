using System;

namespace Ryujinx.Horizon.Sdk.Ngc.Detail
{
    readonly ref struct MatchState
    {
        public readonly Span<byte> OriginalText;
        public readonly Span<byte> ConvertedText;
        public readonly ReadOnlySpan<sbyte> DeltaTable;
        public readonly ref int MaskedCount;
        public readonly MaskMode MaskMode;
        public readonly Sbv NoSeparatorMap;
        public readonly AhoCorasick DelimitedWordsTrie;

        public MatchState(
            Span<byte> originalText,
            Span<byte> convertedText,
            ReadOnlySpan<sbyte> deltaTable,
            ref int maskedCount,
            MaskMode maskMode,
            Sbv noSeparatorMap = null,
            AhoCorasick delimitedWordsTrie = null)
        {
            OriginalText = originalText;
            ConvertedText = convertedText;
            DeltaTable = deltaTable;
            MaskedCount = ref maskedCount;
            MaskMode = maskMode;
            NoSeparatorMap = noSeparatorMap;
            DelimitedWordsTrie = delimitedWordsTrie;
        }

        public readonly (int, int) GetOriginalRange(int convertedStartOffest, int convertedEndOffset)
        {
            int originalStartOffset = 0;
            int originalEndOffset = 0;

            for (int index = 0; index < convertedEndOffset; index++)
            {
                int byteLength = Math.Abs(DeltaTable[index]);

                originalStartOffset += index < convertedStartOffest ? byteLength : 0;
                originalEndOffset += byteLength;
            }

            return (originalStartOffset, originalEndOffset);
        }
    }
}
