using System;

namespace Ryujinx.Graphics.GAL
{
    public interface IWindow
    {
        void Present(ITexture texture, ImageCrop crop, Action<object> swapBuffersCallback);

        void SetSize(int width, int height);
    }
}
