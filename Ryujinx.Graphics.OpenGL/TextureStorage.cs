using OpenTK.Graphics.OpenGL;
using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;

namespace Ryujinx.Graphics.OpenGL
{
    class TextureStorage
    {
        public int Handle { get; private set; }

        public TextureCreateInfo Info { get; }

        private readonly Renderer _renderer;

        private int _viewsCount;

        public TextureStorage(Renderer renderer, TextureCreateInfo info)
        {
            _renderer = renderer;
            Info      = info;

            Handle = GL.GenTexture();

            CreateImmutableStorage();
        }

        private void CreateImmutableStorage()
        {
            TextureTarget target = Info.Target.Convert();

            GL.ActiveTexture(TextureUnit.Texture0);

            GL.BindTexture(target, Handle);

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
                        Info.Width);
                    break;

                case Target.Texture1DArray:
                    GL.TexStorage2D(
                        TextureTarget2d.Texture1DArray,
                        Info.Levels,
                        internalFormat,
                        Info.Width,
                        Info.Height);
                    break;

                case Target.Texture2D:
                    GL.TexStorage2D(
                        TextureTarget2d.Texture2D,
                        Info.Levels,
                        internalFormat,
                        Info.Width,
                        Info.Height);
                    break;

                case Target.Texture2DArray:
                    GL.TexStorage3D(
                        TextureTarget3d.Texture2DArray,
                        Info.Levels,
                        internalFormat,
                        Info.Width,
                        Info.Height,
                        Info.Depth);
                    break;

                case Target.Texture2DMultisample:
                    GL.TexStorage2DMultisample(
                        TextureTargetMultisample2d.Texture2DMultisample,
                        Info.Samples,
                        internalFormat,
                        Info.Width,
                        Info.Height,
                        true);
                    break;

                case Target.Texture2DMultisampleArray:
                    GL.TexStorage3DMultisample(
                        TextureTargetMultisample3d.Texture2DMultisampleArray,
                        Info.Samples,
                        internalFormat,
                        Info.Width,
                        Info.Height,
                        Info.Depth,
                        true);
                    break;

                case Target.Texture3D:
                    GL.TexStorage3D(
                        TextureTarget3d.Texture3D,
                        Info.Levels,
                        internalFormat,
                        Info.Width,
                        Info.Height,
                        Info.Depth);
                    break;

                case Target.Cubemap:
                    GL.TexStorage2D(
                        TextureTarget2d.TextureCubeMap,
                        Info.Levels,
                        internalFormat,
                        Info.Width,
                        Info.Height);
                    break;

                case Target.CubemapArray:
                    GL.TexStorage3D(
                        (TextureTarget3d)All.TextureCubeMapArray,
                        Info.Levels,
                        internalFormat,
                        Info.Width,
                        Info.Height,
                        Info.Depth);
                    break;

                default:
                    Logger.PrintDebug(LogClass.Gpu, $"Invalid or unsupported texture target: {target}.");
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
