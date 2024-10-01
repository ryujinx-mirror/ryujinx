namespace Ryujinx.Horizon.Sdk.Ngc.Detail
{
    struct MatchSimilarFormState
    {
        public MatchRangeList MatchRanges;
        public SimilarFormTable SimilarFormTable;
        public Utf8Text CanonicalText;
        public int ReplaceEndOffset;

        public MatchSimilarFormState(MatchRangeList matchRanges, SimilarFormTable similarFormTable)
        {
            MatchRanges = matchRanges;
            SimilarFormTable = similarFormTable;
            CanonicalText = new();
            ReplaceEndOffset = 0;
        }
    }
}
