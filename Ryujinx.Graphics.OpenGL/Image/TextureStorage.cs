using OpenTK.Graphics.OpenGL;
using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;
using System;

namespace Ryujinx.Graphics.OpenGL.Image
{
    class TextureStorage
    {
        public int Handle { get; private set; }
        public float ScaleFactor { get; private set; }

        public TextureCreateInfo Info { get; }

        private readonly Renderer _renderer;

        private int _viewsCount;

        public TextureStorage(Renderer renderer, TextureCreateInfo info, float scaleFactor)
        {
            _renderer = renderer;
            Info      = info;

            Handle = GL.GenTexture();
            ScaleFactor = scaleFactor;

            CreateImmutableStorage();
        }

        private void CreateImmutableStorage()
        {
            TextureTarget target = Info.Target.Convert();

            GL.ActiveTexture(TextureUnit.Texture0);

            GL.BindTexture(target, Handle);

            int width = (int)Math.Ceiling(Info.Width * ScaleFactor);
            int height = (int)Math.Ceiling(Info.Height * ScaleFactor);

            FormatInfo format = FormatTable.GetFormatInfo(Info.Format);

            SizedInternalFormat internalFormat;

            if (format.IsCompressed)
            {
                internalFormat = (SizedInternalFormat)format.PixelFormat;
            }
            else
            {
                internalFormat = (SizedInternalFormat)format.PixelInternalFormat;
            }

            switch (Info.Target)
            {
                case Target.Texture1D:
                    GL.TexStorage1D(
                        TextureTarget1d.Texture1D,
                        Info.Levels,
                        internalFormat,
                        width);
                    break;

                case Target.Texture1DArray:
                    GL.TexStorage2D(
                        TextureTarget2d.Texture1DArray,
                        Info.Levels,
                        internalFormat,
                        width,
                        height);
                    break;

                case Target.Texture2D:
                    GL.TexStorage2D(
                        TextureTarget2d.Texture2D,
                        Info.Levels,
                        internalFormat,
                        width,
                        height);
                    break;

                case Target.Texture2DArray:
                    GL.TexStorage3D(
                        TextureTarget3d.Texture2DArray,
                        Info.Levels,
                        internalFormat,
                        width,
                        height,
                        Info.Depth);
                    break;

                case Target.Texture2DMultisample:
                    GL.TexStorage2DMultisample(
                        TextureTargetMultisample2d.Texture2DMultisample,
                        Info.Samples,
                        internalFormat,
                        width,
                        height,
                        true);
                    break;

                case Target.Texture2DMultisampleArray:
                    GL.TexStorage3DMultisample(
                        TextureTargetMultisample3d.Texture2DMultisampleArray,
                        Info.Samples,
                        internalFormat,
                        width,
                        height,
                        Info.Depth,
                        true);
                    break;

                case Target.Texture3D:
                    GL.TexStorage3D(
                        TextureTarget3d.Texture3D,
                        Info.Levels,
                        internalFormat,
                        width,
                        height,
                        Info.Depth);
                    break;

                case Target.Cubemap:
                    GL.TexStorage2D(
                        TextureTarget2d.TextureCubeMap,
                        Info.Levels,
                        internalFormat,
                        width,
                        height);
                    break;

                case Target.CubemapArray:
                    GL.TexStorage3D(
                        (TextureTarget3d)All.TextureCubeMapArray,
                        Info.Levels,
                        internalFormat,
                        width,
                        height,
                        Info.Depth);
                    break;

                default:
                    Logger.Debug?.Print(LogClass.Gpu, $"Invalid or unsupported texture target: {target}.");
                    break;
            }
        }

        public ITexture CreateDefaultView()
        {
            return CreateView(Info, 0, 0);
        }

        public ITexture CreateView(TextureCreateInfo info, int firstLayer, int firstLevel)
        {
            IncrementViewsCount();

            return new TextureView(_renderer, this, info, firstLayer, firstLevel);
        }

        private void IncrementViewsCount()
        {
            _viewsCount++;
        }

        public void DecrementViewsCount()
        {
            // If we don't have any views, then the storage is now useless, delete it.
            if (--_viewsCount == 0)
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            if (Handle != 0)
            {
                GL.DeleteTexture(Handle);

                Handle = 0;
            }
        }
    }
}
