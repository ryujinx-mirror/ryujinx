using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.Texture;
using System;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    class ImageHandler
    {
        private static int CopyBuffer = 0;
        private static int CopyBufferSize = 0;

        public GalImage Image { get; private set; }

        public int Width  => Image.Width;
        public int Height => Image.Height;

        public GalImageFormat Format => Image.Format;

        public PixelInternalFormat InternalFormat { get; private set; }
        public PixelFormat         PixelFormat    { get; private set; }
        public PixelType           PixelType      { get; private set; }

        public int Handle { get; private set; }

        private bool Initialized;

        public ImageHandler()
        {
            Handle = GL.GenTexture();
        }

        public ImageHandler(int Handle, GalImage Image)
        {
            this.Handle = Handle;

            this.Image = Image;
        }

        public void EnsureSetup(GalImage NewImage)
        {
            if (Width  == NewImage.Width  &&
                Height == NewImage.Height &&
                Format == NewImage.Format &&
                Initialized)
            {
                return;
            }

            PixelInternalFormat InternalFmt;
            PixelFormat         PixelFormat;
            PixelType           PixelType;

            if (ImageUtils.IsCompressed(NewImage.Format))
            {
                InternalFmt = (PixelInternalFormat)OGLEnumConverter.GetCompressedImageFormat(NewImage.Format);

                PixelFormat = default(PixelFormat);
                PixelType   = default(PixelType);
            }
            else
            {
                (InternalFmt, PixelFormat, PixelType) = OGLEnumConverter.GetImageFormat(NewImage.Format);
            }

            GL.BindTexture(TextureTarget.Texture2D, Handle);

            if (Initialized)
            {
                if (CopyBuffer == 0)
                {
                    CopyBuffer = GL.GenBuffer();
                }

                int CurrentSize = Math.Max(ImageUtils.GetSize(NewImage),
                                           ImageUtils.GetSize(Image));

                GL.BindBuffer(BufferTarget.PixelPackBuffer, CopyBuffer);
                GL.BindBuffer(BufferTarget.PixelUnpackBuffer, CopyBuffer);

                if (CopyBufferSize < CurrentSize)
                {
                    CopyBufferSize = CurrentSize;

                    GL.BufferData(BufferTarget.PixelPackBuffer, CurrentSize, IntPtr.Zero, BufferUsageHint.StreamCopy);
                }

                if (ImageUtils.IsCompressed(Image.Format))
                {
                    GL.GetCompressedTexImage(TextureTarget.Texture2D, 0, IntPtr.Zero);
                }
                else
                {
                    GL.GetTexImage(TextureTarget.Texture2D, 0, this.PixelFormat, this.PixelType, IntPtr.Zero);
                }

                GL.DeleteTexture(Handle);

                Handle = GL.GenTexture();

                GL.BindTexture(TextureTarget.Texture2D, Handle);
            }

            const int MinFilter = (int)TextureMinFilter.Linear;
            const int MagFilter = (int)TextureMagFilter.Linear;

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, MinFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, MagFilter);

            const int Level = 0;
            const int Border = 0;

            if (ImageUtils.IsCompressed(NewImage.Format))
            {
                Console.WriteLine("Hit");

                GL.CompressedTexImage2D(
                    TextureTarget.Texture2D,
                    Level,
                    (InternalFormat)InternalFmt,
                    NewImage.Width,
                    NewImage.Height,
                    Border,
                    ImageUtils.GetSize(NewImage),
                    IntPtr.Zero);
            }
            else
            {
                GL.TexImage2D(
                    TextureTarget.Texture2D,
                    Level,
                    InternalFmt,
                    NewImage.Width,
                    NewImage.Height,
                    Border,
                    PixelFormat,
                    PixelType,
                    IntPtr.Zero);
            }

            if (Initialized)
            {
                GL.BindBuffer(BufferTarget.PixelPackBuffer,   0);
                GL.BindBuffer(BufferTarget.PixelUnpackBuffer, 0);
            }

            Image = NewImage;

            this.InternalFormat = InternalFmt;
            this.PixelFormat = PixelFormat;
            this.PixelType = PixelType;

            Initialized = true;
        }

        public bool HasColor   => ImageUtils.HasColor(Image.Format);
        public bool HasDepth   => ImageUtils.HasDepth(Image.Format);
        public bool HasStencil => ImageUtils.HasStencil(Image.Format);
    }
}
