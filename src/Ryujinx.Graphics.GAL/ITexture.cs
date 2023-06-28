using Ryujinx.Common.Memory;

namespace Ryujinx.Graphics.GAL
{
    public interface ITexture
    {
        int Width { get; }
        int Height { get; }
        float ScaleFactor { get; }

        void CopyTo(ITexture destination, int firstLayer, int firstLevel);
        void CopyTo(ITexture destination, int srcLayer, int dstLayer, int srcLevel, int dstLevel);
        void CopyTo(ITexture destination, Extents2D srcRegion, Extents2D dstRegion, bool linearFilter);
        void CopyTo(BufferRange range, int layer, int level, int stride);

        ITexture CreateView(TextureCreateInfo info, int firstLayer, int firstLevel);

        PinnedSpan<byte> GetData();
        PinnedSpan<byte> GetData(int layer, int level);

        void SetData(SpanOrArray<byte> data);
        void SetData(SpanOrArray<byte> data, int layer, int level);
        void SetData(SpanOrArray<byte> data, int layer, int level, Rectangle<int> region);
        void SetStorage(BufferRange buffer);
        void Release();
    }
}
