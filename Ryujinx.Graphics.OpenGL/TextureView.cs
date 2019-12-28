using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.GAL.Texture;
using System;

namespace Ryujinx.Graphics.OpenGL
{
    class TextureView : ITexture
    {
        public int Handle { get; private set; }

        private Renderer _renderer;

        private TextureStorage _parent;

        private TextureView _emulatedViewParent;

        private TextureCreateInfo _info;

        private int _firstLayer;
        private int _firstLevel;

        private bool _acquired;
        private bool _pendingDelete;

        public int Width         => _info.Width;
        public int Height        => _info.Height;
        public int DepthOrLayers => _info.GetDepthOrLayers();
        public int Levels        => _info.Levels;

        public Target Target => _info.Target;
        public Format Format => _info.Format;

        public int BlockWidth  => _info.BlockWidth;
        public int BlockHeight => _info.BlockHeight;

        public bool IsCompressed => _info.IsCompressed;

        public TextureView(
            Renderer          renderer,
            TextureStorage    parent,
            TextureCreateInfo info,
            int               firstLayer,
            int               firstLevel)
        {
            _renderer = renderer;
            _parent   = parent;
            _info     = info;

            _firstLayer = firstLayer;
            _firstLevel = firstLevel;

            Handle = GL.GenTexture();

            CreateView();
        }

        private void CreateView()
        {
            TextureTarget target = Target.Convert();

            FormatInfo format = FormatTable.GetFormatInfo(_info.Format);

            PixelInternalFormat pixelInternalFormat;

            if (format.IsCompressed)
            {
                pixelInternalFormat = (PixelInternalFormat)format.PixelFormat;
            }
            else
            {
                pixelInternalFormat = format.PixelInternalFormat;
            }

            GL.TextureView(
                Handle,
                target,
                _parent.Handle,
                pixelInternalFormat,
                _firstLevel,
                _info.Levels,
                _firstLayer,
                _info.GetLayers());

            GL.ActiveTexture(TextureUnit.Texture0);

            GL.BindTexture(target, Handle);

            int[] swizzleRgba = new int[]
            {
                (int)_info.SwizzleR.Convert(),
                (int)_info.SwizzleG.Convert(),
                (int)_info.SwizzleB.Convert(),
                (int)_info.SwizzleA.Convert()
            };

            GL.TexParameter(target, TextureParameterName.TextureSwizzleRgba, swizzleRgba);

            int maxLevel = _info.Levels - 1;

            if (maxLevel < 0)
            {
                maxLevel = 0;
            }

            GL.TexParameter(target, TextureParameterName.TextureMaxLevel, maxLevel);

            // GL.TexParameter(target, TextureParameterName.DepthStencilTextureMode, (int)_info.DepthStencilMode.Convert());
        }

        public ITexture CreateView(TextureCreateInfo info, int firstLayer, int firstLevel)
        {
            if (_info.IsCompressed == info.IsCompressed)
            {
                firstLayer += _firstLayer;
                firstLevel += _firstLevel;

                return _parent.CreateView(info, firstLayer, firstLevel);
            }
            else
            {
                // TODO: Improve
                TextureView emulatedView = (TextureView)_renderer.CreateTexture(info);

                emulatedView._emulatedViewParent = this;

                emulatedView._firstLayer = firstLayer;
                emulatedView._firstLevel = firstLevel;

                return emulatedView;
            }
        }

        public int GetStorageDebugId()
        {
            return _parent.GetHashCode();
        }

        public void CopyTo(ITexture destination, int firstLayer, int firstLevel)
        {
            TextureView destinationView = (TextureView)destination;

            TextureCopyUnscaled.Copy(this, destinationView, firstLayer, firstLevel);

            int width  = Math.Min(Width,  destinationView.Width);
            int height = Math.Min(Height, destinationView.Height);

            int depth = Math.Min(_info.GetDepthOrLayers(), destinationView._info.GetDepthOrLayers());

            int levels = Math.Min(_info.Levels, destinationView._info.Levels);

            if (destinationView._emulatedViewParent != null)
            {
                TextureCopyUnscaled.Copy(
                    this,
                    destinationView._emulatedViewParent,
                    destinationView._firstLayer,
                    destinationView._firstLevel);
            }
        }

        public void CopyTo(ITexture destination, Extents2D srcRegion, Extents2D dstRegion, bool linearFilter)
        {
            _renderer.TextureCopy.Copy(this, (TextureView)destination, srcRegion, dstRegion, linearFilter);
        }

        public byte[] GetData()
        {
            int size = 0;

            for (int level = 0; level < _info.Levels; level++)
            {
                size += _info.GetMipSize(level);
            }

            byte[] data = new byte[size];

            unsafe
            {
                fixed (byte* ptr = data)
                {
                    WriteTo((IntPtr)ptr);
                }
            }

            return data;
        }

        private void WriteTo(IntPtr ptr)
        {
            TextureTarget target = Target.Convert();

            Bind(target, 0);

            FormatInfo format = FormatTable.GetFormatInfo(_info.Format);

            int faces = 1;

            if (target == TextureTarget.TextureCubeMap)
            {
                target = TextureTarget.TextureCubeMapPositiveX;

                faces = 6;
            }

            for (int level = 0; level < _info.Levels; level++)
            {
                for (int face = 0; face < faces; face++)
                {
                    int faceOffset = face * _info.GetMipSize2D(level);

                    if (format.IsCompressed)
                    {
                        GL.GetCompressedTexImage(target + face, level, ptr + faceOffset);
                    }
                    else
                    {
                        GL.GetTexImage(
                            target + face,
                            level,
                            format.PixelFormat,
                            format.PixelType,
                            ptr + faceOffset);
                    }
                }

                ptr += _info.GetMipSize(level);
            }
        }

        public void SetData(Span<byte> data)
        {
            unsafe
            {
                fixed (byte* ptr = data)
                {
                    SetData((IntPtr)ptr, data.Length);
                }
            }
        }

        private void SetData(IntPtr data, int size)
        {
            TextureTarget target = Target.Convert();

            Bind(target, 0);

            FormatInfo format = FormatTable.GetFormatInfo(_info.Format);

            int width  = _info.Width;
            int height = _info.Height;
            int depth  = _info.Depth;

            int offset = 0;

            for (int level = 0; level < _info.Levels; level++)
            {
                int mipSize = _info.GetMipSize(level);

                int endOffset = offset + mipSize;

                if ((uint)endOffset > (uint)size)
                {
                    return;
                }

                switch (_info.Target)
                {
                    case Target.Texture1D:
                        if (format.IsCompressed)
                        {
                            GL.CompressedTexSubImage1D(
                                target,
                                level,
                                0,
                                width,
                                format.PixelFormat,
                                mipSize,
                                data);
                        }
                        else
                        {
                            GL.TexSubImage1D(
                                target,
                                level,
                                0,
                                width,
                                format.PixelFormat,
                                format.PixelType,
                                data);
                        }
                        break;

                    case Target.Texture1DArray:
                    case Target.Texture2D:
                        if (format.IsCompressed)
                        {
                            GL.CompressedTexSubImage2D(
                                target,
                                level,
                                0,
                                0,
                                width,
                                height,
                                format.PixelFormat,
                                mipSize,
                                data);
                        }
                        else
                        {
                            GL.TexSubImage2D(
                                target,
                                level,
                                0,
                                0,
                                width,
                                height,
                                format.PixelFormat,
                                format.PixelType,
                                data);
                        }
                        break;

                    case Target.Texture2DArray:
                    case Target.Texture3D:
                    case Target.CubemapArray:
                        if (format.IsCompressed)
                        {
                            GL.CompressedTexSubImage3D(
                                target,
                                level,
                                0,
                                0,
                                0,
                                width,
                                height,
                                depth,
                                format.PixelFormat,
                                mipSize,
                                data);
                        }
                        else
                        {
                            GL.TexSubImage3D(
                                target,
                                level,
                                0,
                                0,
                                0,
                                width,
                                height,
                                depth,
                                format.PixelFormat,
                                format.PixelType,
                                data);
                        }
                        break;

                    case Target.Cubemap:
                        int faceOffset = 0;

                        for (int face = 0; face < 6; face++, faceOffset += mipSize / 6)
                        {
                            if (format.IsCompressed)
                            {
                                GL.CompressedTexSubImage2D(
                                    TextureTarget.TextureCubeMapPositiveX + face,
                                    level,
                                    0,
                                    0,
                                    width,
                                    height,
                                    format.PixelFormat,
                                    mipSize / 6,
                                    data + faceOffset);
                            }
                            else
                            {
                                GL.TexSubImage2D(
                                    TextureTarget.TextureCubeMapPositiveX + face,
                                    level,
                                    0,
                                    0,
                                    width,
                                    height,
                                    format.PixelFormat,
                                    format.PixelType,
                                    data + faceOffset);
                            }
                        }
                        break;
                }

                data   += mipSize;
                offset += mipSize;

                width  = Math.Max(1, width  >> 1);
                height = Math.Max(1, height >> 1);

                if (Target == Target.Texture3D)
                {
                    depth = Math.Max(1, depth >> 1);
                }
            }
        }

        public void Bind(int unit)
        {
            Bind(Target.Convert(), unit);
        }

        private void Bind(TextureTarget target, int unit)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + unit);

            GL.BindTexture(target, Handle);
        }

        public void Acquire()
        {
            _acquired = true;
        }

        public void Release()
        {
            _acquired = false;

            if (_pendingDelete)
            {
                _pendingDelete = false;

                Dispose();
            }
        }

        public void Dispose()
        {
            if (_acquired)
            {
                _pendingDelete = true;

                return;
            }

            if (Handle != 0)
            {
                GL.DeleteTexture(Handle);

                _parent.DecrementViewsCount();

                Handle = 0;
            }
        }
    }
}
