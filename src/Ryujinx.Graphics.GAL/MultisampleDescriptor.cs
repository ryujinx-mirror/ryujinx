namespace Ryujinx.Graphics.GAL
{
    public readonly struct MultisampleDescriptor
    {
        public bool AlphaToCoverageEnable { get; }
        public bool AlphaToCoverageDitherEnable { get; }
        public bool AlphaToOneEnable { get; }

        public MultisampleDescriptor(
            bool alphaToCoverageEnable,
            bool alphaToCoverageDitherEnable,
            bool alphaToOneEnable)
        {
            AlphaToCoverageEnable = alphaToCoverageEnable;
            AlphaToCoverageDitherEnable = alphaToCoverageDitherEnable;
            AlphaToOneEnable = alphaToOneEnable;
        }
    }
}
