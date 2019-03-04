using Ryujinx.Graphics.Texture;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    class ImageHandler
    {
        public GalImage Image { get; private set; }

        public int Width  => Image.Width;
        public int Height => Image.Height;
        public int Depth  => Image.Depth;

        public GalImageFormat Format => Image.Format;

        public int Handle { get; private set; }

        public bool HasColor   => ImageUtils.HasColor(Image.Format);
        public bool HasDepth   => ImageUtils.HasDepth(Image.Format);
        public bool HasStencil => ImageUtils.HasStencil(Image.Format);

        public ImageHandler(int handle, GalImage image)
        {
            Handle = handle;
            Image  = image;
        }
    }
}
