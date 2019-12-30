using OpenTK.Graphics.OpenGL;
using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;

namespace Ryujinx.Graphics.OpenGL
{
    class TextureStorage
    {
        public int Handle { get; private set; }

        private readonly Renderer _renderer;

        private readonly TextureCreateInfo _info;

        public Target Target => _info.Target;

        private int _viewsCount;

        public TextureStorage(Renderer renderer, TextureCreateInfo info)
        {
            _renderer = renderer;
            _info     = info;

            Handle = GL.GenTexture();

            CreateImmutableStorage();
        }

        private void CreateImmutableStorage()
        {
            TextureTarget target = _info.Target.Convert();

            GL.ActiveTexture(TextureUnit.Texture0);

            GL.BindTexture(target, Handle);

            FormatInfo format = FormatTable.GetFormatInfo(_info.Format);

            SizedInternalFormat internalFormat;

            if (format.IsCompressed)
            {
                internalFormat = (SizedInternalFormat)format.PixelFormat;
            }
            else
            {
                internalFormat = (SizedInternalFormat)format.PixelInternalFormat;
            }

            switch (_info.Target)
            {
                case Target.Texture1D:
                    GL.TexStorage1D(
                        TextureTarget1d.Texture1D,
                        _info.Levels,
                        internalFormat,
                        _info.Width);
                    break;

                case Target.Texture1DArray:
                    GL.TexStorage2D(
                        TextureTarget2d.Texture1DArray,
                        _info.Levels,
                        internalFormat,
                        _info.Width,
                        _info.Height);
                    break;

                case Target.Texture2D:
                    GL.TexStorage2D(
                        TextureTarget2d.Texture2D,
                        _info.Levels,
                        internalFormat,
                        _info.Width,
                        _info.Height);
                    break;

                case Target.Texture2DArray:
                    GL.TexStorage3D(
                        TextureTarget3d.Texture2DArray,
                        _info.Levels,
                        internalFormat,
                        _info.Width,
                        _info.Height,
                        _info.Depth);
                    break;

                case Target.Texture2DMultisample:
                    GL.TexStorage2DMultisample(
                        TextureTargetMultisample2d.Texture2DMultisample,
                        _info.Samples,
                        internalFormat,
                        _info.Width,
                        _info.Height,
                        true);
                    break;

                case Target.Texture2DMultisampleArray:
                    GL.TexStorage3DMultisample(
                        TextureTargetMultisample3d.Texture2DMultisampleArray,
                        _info.Samples,
                        internalFormat,
                        _info.Width,
                        _info.Height,
                        _info.Depth,
                        true);
                    break;

                case Target.Texture3D:
                    GL.TexStorage3D(
                        TextureTarget3d.Texture3D,
                        _info.Levels,
                        internalFormat,
                        _info.Width,
                        _info.Height,
                        _info.Depth);
                    break;

                case Target.Cubemap:
                    GL.TexStorage2D(
                        TextureTarget2d.TextureCubeMap,
                        _info.Levels,
                        internalFormat,
                        _info.Width,
                        _info.Height);
                    break;

                case Target.CubemapArray:
                    GL.TexStorage3D(
                        (TextureTarget3d)All.TextureCubeMapArray,
                        _info.Levels,
                        internalFormat,
                        _info.Width,
                        _info.Height,
                        _info.Depth);
                    break;

                default:
                    Logger.PrintDebug(LogClass.Gpu, $"Invalid or unsupported texture target: {target}.");
                    break;
            }
        }

        public ITexture CreateDefaultView()
        {
            return CreateView(_info, 0, 0);
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
