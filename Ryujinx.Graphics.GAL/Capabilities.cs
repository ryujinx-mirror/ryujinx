namespace Ryujinx.Graphics.GAL
{
    public struct Capabilities
    {
        public bool SupportsAstcCompression { get; }

        public Capabilities(bool supportsAstcCompression)
        {
            SupportsAstcCompression = supportsAstcCompression;
        }
    }
}