using System;

namespace Ryujinx.Graphics.GAL
{
    public interface IImageArray : IDisposable
    {
        void SetFormats(int index, Format[] imageFormats);
        void SetImages(int index, ITexture[] images);
    }
}
