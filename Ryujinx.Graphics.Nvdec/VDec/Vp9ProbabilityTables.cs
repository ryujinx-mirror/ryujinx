namespace Ryujinx.Graphics.VDec
{
    struct Vp9ProbabilityTables
    {
        public byte[] SegmentationTreeProbs;
        public byte[] SegmentationPredProbs;
        public byte[] Tx8x8Probs;
        public byte[] Tx16x16Probs;
        public byte[] Tx32x32Probs;
        public byte[] CoefProbs;
        public byte[] SkipProbs;
        public byte[] InterModeProbs;
        public byte[] InterpFilterProbs;
        public byte[] IsInterProbs;
        public byte[] CompModeProbs;
        public byte[] SingleRefProbs;
        public byte[] CompRefProbs;
        public byte[] YModeProbs0;
        public byte[] YModeProbs1;
        public byte[] PartitionProbs;
        public byte[] MvJointProbs;
        public byte[] MvSignProbs;
        public byte[] MvClassProbs;
        public byte[] MvClass0BitProbs;
        public byte[] MvBitsProbs;
        public byte[] MvClass0FrProbs;
        public byte[] MvFrProbs;
        public byte[] MvClass0HpProbs;
        public byte[] MvHpProbs;
    }
}