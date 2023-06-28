namespace Ryujinx.Graphics.GAL
{
    public readonly struct AdvancedBlendDescriptor
    {
        public AdvancedBlendOp Op { get; }
        public AdvancedBlendOverlap Overlap { get; }
        public bool SrcPreMultiplied { get; }

        public AdvancedBlendDescriptor(AdvancedBlendOp op, AdvancedBlendOverlap overlap, bool srcPreMultiplied)
        {
            Op = op;
            Overlap = overlap;
            SrcPreMultiplied = srcPreMultiplied;
        }
    }
}
