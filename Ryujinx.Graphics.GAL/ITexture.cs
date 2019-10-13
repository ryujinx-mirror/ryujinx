using Ryujinx.Graphics.GAL.Texture;
using System;

namespace Ryujinx.Graphics.GAL
{
    public interface ITexture : IDisposable
    {
        int Handle { get; }

        void CopyTo(ITexture destination);
        void CopyTo(ITexture destination, Extents2D srcRegion, Extents2D dstRegion, bool linearFilter);

        ITexture CreateView(TextureCreateInfo info, int firstLayer, int firstLevel);

        int GetStorageDebugId();

        byte[] GetData(int face);

        void SetData(Span<byte> data);
    }
}