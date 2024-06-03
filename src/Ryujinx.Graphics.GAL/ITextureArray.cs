using System;

namespace Ryujinx.Graphics.GAL
{
    public interface ITextureArray : IDisposable
    {
        void SetSamplers(int index, ISampler[] samplers);
        void SetTextures(int index, ITexture[] textures);
    }
}
