using OpenTK.Graphics.OpenGL;
using System;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    class ImageHandler
    {
        //TODO: Use a variable value here
        public const int MaxBpp = 16;

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

        public void EnsureSetup(GalImage Image)
        {
            if (Width  != Image.Width  ||
                Height != Image.Height ||
                Format != Image.Format ||
                !Initialized)
            {
                (PixelInternalFormat InternalFormat, PixelFormat PixelFormat, PixelType PixelType) =
                    OGLEnumConverter.GetImageFormat(Image.Format);

                GL.BindTexture(TextureTarget.Texture2D, Handle);

                if (Initialized)
                {
                    if (CopyBuffer == 0)
                    {
                        CopyBuffer = GL.GenBuffer();
                    }

                    int MaxWidth  = Math.Max(Image.Width, Width);
                    int MaxHeight = Math.Max(Image.Height, Height);

                    int CurrentSize = MaxWidth * MaxHeight * MaxBpp;

                    GL.BindBuffer(BufferTarget.PixelPackBuffer, CopyBuffer);
                    GL.BindBuffer(BufferTarget.PixelUnpackBuffer, CopyBuffer);

                    if (CopyBufferSize < CurrentSize)
                    {
                        CopyBufferSize = CurrentSize;

                        GL.BufferData(BufferTarget.PixelPackBuffer, CurrentSize, IntPtr.Zero, BufferUsageHint.StreamCopy);
                    }

                    GL.GetTexImage(TextureTarget.Texture2D, 0, this.PixelFormat, this.PixelType, IntPtr.Zero);

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

                GL.TexImage2D(
                    TextureTarget.Texture2D,
                    Level,
                    InternalFormat,
                    Image.Width,
                    Image.Height,
                    Border,
                    PixelFormat,
                    PixelType,
                    IntPtr.Zero);

                if (Initialized)
                {
                    GL.BindBuffer(BufferTarget.PixelPackBuffer, 0);
                    GL.BindBuffer(BufferTarget.PixelUnpackBuffer, 0);
                }

                this.Image = Image;

                this.InternalFormat = InternalFormat;
                this.PixelFormat = PixelFormat;
                this.PixelType = PixelType;

                Initialized = true;
            }
        }

        public bool HasColor   { get => ImageFormatConverter.HasColor(Format); }
        public bool HasDepth   { get => ImageFormatConverter.HasDepth(Format); }
        public bool HasStencil { get => ImageFormatConverter.HasStencil(Format); }
    }
}
