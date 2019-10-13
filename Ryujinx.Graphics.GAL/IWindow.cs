using System;

namespace Ryujinx.Graphics.GAL
{
    public interface IWindow
    {
        void Present();

        void QueueTexture(ITexture texture, ImageCrop crop, object context);

        void RegisterTextureReleaseCallback(TextureReleaseCallback callback);
    }
}
