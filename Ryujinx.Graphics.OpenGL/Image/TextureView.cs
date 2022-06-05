using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL;
using System;

namespace Ryujinx.Graphics.OpenGL.Image
{
    class TextureView : TextureBase, ITexture, ITextureInfo
    {
        private readonly Renderer _renderer;

        private readonly TextureStorage _parent;

        public ITextureInfo Storage => _parent;

        public int FirstLayer { get; private set; }
        public int FirstLevel { get; private set; }

        public TextureView(
            Renderer          renderer,
            TextureStorage    parent,
            TextureCreateInfo info,
            int               firstLayer,
            int               firstLevel) : base(info, parent.ScaleFactor)
        {
            _renderer = renderer;
            _parent   = parent;

            FirstLayer = firstLayer;
            FirstLevel = firstLevel;

            CreateView();
        }

        private void CreateView()
        {
            TextureTarget target = Target.Convert();

            FormatInfo format = FormatTable.GetFormatInfo(Info.Format);

            PixelInternalFormat pixelInternalFormat;

            if (format.IsCompressed)
            {
                pixelInternalFormat = (PixelInternalFormat)format.PixelFormat;
            }
            else
            {
                pixelInternalFormat = format.PixelInternalFormat;
            }

            int levels = Info.GetLevelsClamped();

            GL.TextureView(
                Handle,
                target,
                _parent.Handle,
                pixelInternalFormat,
                FirstLevel,
                levels,
                FirstLayer,
                Info.GetLayers());

            GL.ActiveTexture(TextureUnit.Texture0);

            GL.BindTexture(target, Handle);

            int[] swizzleRgba = new int[]
            {
                (int)Info.SwizzleR.Convert(),
                (int)Info.SwizzleG.Convert(),
                (int)Info.SwizzleB.Convert(),
                (int)Info.SwizzleA.Convert()
            };

            if (Info.Format == Format.A1B5G5R5Unorm)
            {
                int temp = swizzleRgba[0];
                int temp2 = swizzleRgba[1];
                swizzleRgba[0] = swizzleRgba[3];
                swizzleRgba[1] = swizzleRgba[2];
                swizzleRgba[2] = temp2;
                swizzleRgba[3] = temp;
            }
            else if (Info.Format.IsBgr())
            {
                // Swap B <-> R for BGRA formats, as OpenGL has no support for them
                // and we need to manually swap the components on read/write on the GPU.
                int temp = swizzleRgba[0];
                swizzleRgba[0] = swizzleRgba[2];
                swizzleRgba[2] = temp;
            }

            GL.TexParameter(target, TextureParameterName.TextureSwizzleRgba, swizzleRgba);

            int maxLevel = levels - 1;

            if (maxLevel < 0)
            {
                maxLevel = 0;
            }

            GL.TexParameter(target, TextureParameterName.TextureMaxLevel, maxLevel);
            GL.TexParameter(target, TextureParameterName.DepthStencilTextureMode, (int)Info.DepthStencilMode.Convert());
        }

        public ITexture CreateView(TextureCreateInfo info, int firstLayer, int firstLevel)
        {
            firstLayer += FirstLayer;
            firstLevel += FirstLevel;

            return _parent.CreateView(info, firstLayer, firstLevel);
        }

        public void CopyTo(ITexture destination, int firstLayer, int firstLevel)
        {
            TextureView destinationView = (TextureView)destination;

            if (destinationView.Target.IsMultisample() || Target.IsMultisample())
            {
                Extents2D srcRegion = new Extents2D(0, 0, Width, Height);
                Extents2D dstRegion = new Extents2D(0, 0, destinationView.Width, destinationView.Height);

                TextureView intermmediate = _renderer.TextureCopy.IntermmediatePool.GetOrCreateWithAtLeast(
                    GetIntermmediateTarget(Target),
                    Info.BlockWidth,
                    Info.BlockHeight,
                    Info.BytesPerPixel,
                    Format,
                    Width,
                    Height,
                    Info.Depth,
                    Info.Levels);

                GL.Disable(EnableCap.FramebufferSrgb);

                _renderer.TextureCopy.Copy(this, intermmediate, srcRegion, srcRegion, true);
                _renderer.TextureCopy.Copy(intermmediate, destinationView, srcRegion, dstRegion, true, 0, firstLayer, 0, firstLevel);

                GL.Enable(EnableCap.FramebufferSrgb);
            }
            else
            {
                _renderer.TextureCopy.CopyUnscaled(this, destinationView, 0, firstLayer, 0, firstLevel);
            }
        }

        public void CopyTo(ITexture destination, int srcLayer, int dstLayer, int srcLevel, int dstLevel)
        {
            TextureView destinationView = (TextureView)destination;

            if (destinationView.Target.IsMultisample() || Target.IsMultisample())
            {
                Extents2D srcRegion = new Extents2D(0, 0, Width, Height);
                Extents2D dstRegion = new Extents2D(0, 0, destinationView.Width, destinationView.Height);

                TextureView intermmediate = _renderer.TextureCopy.IntermmediatePool.GetOrCreateWithAtLeast(
                    GetIntermmediateTarget(Target),
                    Info.BlockWidth,
                    Info.BlockHeight,
                    Info.BytesPerPixel,
                    Format,
                    Math.Max(1, Width >> srcLevel),
                    Math.Max(1, Height >> srcLevel),
                    1,
                    1);

                GL.Disable(EnableCap.FramebufferSrgb);

                _renderer.TextureCopy.Copy(this, intermmediate, srcRegion, srcRegion, true, srcLayer, 0, srcLevel, 0, 1, 1);
                _renderer.TextureCopy.Copy(intermmediate, destinationView, srcRegion, dstRegion, true, 0, dstLayer, 0, dstLevel, 1, 1);

                GL.Enable(EnableCap.FramebufferSrgb);
            }
            else
            {
                _renderer.TextureCopy.CopyUnscaled(this, destinationView, srcLayer, dstLayer, srcLevel, dstLevel, 1, 1);
            }
        }

        private static Target GetIntermmediateTarget(Target srcTarget)
        {
            return srcTarget switch
            {
                Target.Texture2D => Target.Texture2DMultisample,
                Target.Texture2DArray => Target.Texture2DMultisampleArray,
                Target.Texture2DMultisampleArray => Target.Texture2DArray,
                _ => Target.Texture2D
            };
        }

        public void CopyTo(ITexture destination, Extents2D srcRegion, Extents2D dstRegion, bool linearFilter)
        {
            _renderer.TextureCopy.Copy(this, (TextureView)destination, srcRegion, dstRegion, linearFilter);
        }

        public unsafe ReadOnlySpan<byte> GetData()
        {
            int size = 0;
            int levels = Info.GetLevelsClamped();

            for (int level = 0; level < levels; level++)
            {
                size += Info.GetMipSize(level);
            }

            ReadOnlySpan<byte> data;

            if (HwCapabilities.UsePersistentBufferForFlush)
            {
                data = _renderer.PersistentBuffers.Default.GetTextureData(this, size);
            }
            else
            {
                IntPtr target = _renderer.PersistentBuffers.Default.GetHostArray(size);

                WriteTo(target);

                data = new ReadOnlySpan<byte>(target.ToPointer(), size);
            }

            if (Format == Format.S8UintD24Unorm)
            {
                data = FormatConverter.ConvertD24S8ToS8D24(data);
            }

            return data;
        }

        public unsafe ReadOnlySpan<byte> GetData(int layer, int level)
        {
            int size = Info.GetMipSize(level);

            if (HwCapabilities.UsePersistentBufferForFlush)
            {
                return _renderer.PersistentBuffers.Default.GetTextureData(this, size, layer, level);
            }
            else
            {
                IntPtr target = _renderer.PersistentBuffers.Default.GetHostArray(size);

                int offset = WriteTo2D(target, layer, level);

                return new ReadOnlySpan<byte>(target.ToPointer(), size).Slice(offset);
            }
        }

        public void WriteToPbo(int offset, bool forceBgra)
        {
            WriteTo(IntPtr.Zero + offset, forceBgra);
        }

        public int WriteToPbo2D(int offset, int layer, int level)
        {
            return WriteTo2D(IntPtr.Zero + offset, layer, level);
        }

        private int WriteTo2D(IntPtr data, int layer, int level)
        {
            TextureTarget target = Target.Convert();

            Bind(target, 0);

            FormatInfo format = FormatTable.GetFormatInfo(Info.Format);

            PixelFormat pixelFormat = format.PixelFormat;
            PixelType pixelType = format.PixelType;

            if (target == TextureTarget.TextureCubeMap || target == TextureTarget.TextureCubeMapArray)
            {
                target = TextureTarget.TextureCubeMapPositiveX + (layer % 6);
            }

            int mipSize = Info.GetMipSize2D(level);

            if (format.IsCompressed)
            {
                GL.GetCompressedTextureSubImage(Handle, level, 0, 0, layer, Math.Max(1, Info.Width >> level), Math.Max(1, Info.Height >> level), 1, mipSize, data);
            }
            else if (format.PixelFormat != PixelFormat.DepthStencil)
            {
                GL.GetTextureSubImage(Handle, level, 0, 0, layer, Math.Max(1, Info.Width >> level), Math.Max(1, Info.Height >> level), 1, pixelFormat, pixelType, mipSize, data);
            }
            else
            {
                GL.GetTexImage(target, level, pixelFormat, pixelType, data);

                // The GL function returns all layers. Must return the offset of the layer we're interested in.
                return target switch
                {
                    TextureTarget.TextureCubeMapArray => (layer / 6) * mipSize,
                    TextureTarget.Texture1DArray => layer * mipSize,
                    TextureTarget.Texture2DArray => layer * mipSize,
                    _ => 0
                };
            }

            return 0;
        }

        private void WriteTo(IntPtr data, bool forceBgra = false)
        {
            TextureTarget target = Target.Convert();

            Bind(target, 0);

            FormatInfo format = FormatTable.GetFormatInfo(Info.Format);

            PixelFormat pixelFormat = format.PixelFormat;
            PixelType   pixelType   = format.PixelType;

            if (forceBgra)
            {
                if (pixelType == PixelType.UnsignedShort565)
                {
                    pixelType = PixelType.UnsignedShort565Reversed;
                }
                else if (pixelType == PixelType.UnsignedShort565Reversed)
                {
                    pixelType = PixelType.UnsignedShort565;
                }
                else
                {
                    pixelFormat = PixelFormat.Bgra;
                }
            }

            int faces = 1;

            if (target == TextureTarget.TextureCubeMap)
            {
                target = TextureTarget.TextureCubeMapPositiveX;

                faces = 6;
            }

            int levels = Info.GetLevelsClamped();

            for (int level = 0; level < levels; level++)
            {
                for (int face = 0; face < faces; face++)
                {
                    int faceOffset = face * Info.GetMipSize2D(level);

                    if (format.IsCompressed)
                    {
                        GL.GetCompressedTexImage(target + face, level, data + faceOffset);
                    }
                    else
                    {
                        GL.GetTexImage(target + face, level, pixelFormat, pixelType, data + faceOffset);
                    }
                }

                data += Info.GetMipSize(level);
            }
        }

        public void SetData(ReadOnlySpan<byte> data)
        {
            if (Format == Format.S8UintD24Unorm)
            {
                data = FormatConverter.ConvertS8D24ToD24S8(data);
            }

            unsafe
            {
                fixed (byte* ptr = data)
                {
                    ReadFrom((IntPtr)ptr, data.Length);
                }
            }
        }

        public void SetData(ReadOnlySpan<byte> data, int layer, int level)
        {
            if (Format == Format.S8UintD24Unorm)
            {
                data = FormatConverter.ConvertS8D24ToD24S8(data);
            }

            unsafe
            {
                fixed (byte* ptr = data)
                {
                    int width = Math.Max(Info.Width >> level, 1);
                    int height = Math.Max(Info.Height >> level, 1);

                    ReadFrom2D((IntPtr)ptr, layer, level, width, height);
                }
            }
        }

        public void ReadFromPbo(int offset, int size)
        {
            ReadFrom(IntPtr.Zero + offset, size);
        }

        public void ReadFromPbo2D(int offset, int layer, int level, int width, int height)
        {
            ReadFrom2D(IntPtr.Zero + offset, layer, level, width, height);
        }

        private void ReadFrom2D(IntPtr data, int layer, int level, int width, int height)
        {
            TextureTarget target = Target.Convert();

            int mipSize = Info.GetMipSize2D(level);

            Bind(target, 0);

            FormatInfo format = FormatTable.GetFormatInfo(Info.Format);

            switch (Target)
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
                    if (format.IsCompressed)
                    {
                        GL.CompressedTexSubImage2D(
                            target,
                            level,
                            0,
                            layer,
                            width,
                            1,
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
                            layer,
                            width,
                            1,
                            format.PixelFormat,
                            format.PixelType,
                            data);
                    }
                    break;

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
                            layer,
                            width,
                            height,
                            1,
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
                            layer,
                            width,
                            height,
                            1,
                            format.PixelFormat,
                            format.PixelType,
                            data);
                    }
                    break;

                case Target.Cubemap:
                    if (format.IsCompressed)
                    {
                        GL.CompressedTexSubImage2D(
                            TextureTarget.TextureCubeMapPositiveX + layer,
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
                            TextureTarget.TextureCubeMapPositiveX + layer,
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
            }
        }

        private void ReadFrom(IntPtr data, int size)
        {
            TextureTarget target = Target.Convert();
            int baseLevel = 0;

            // glTexSubImage on cubemap views is broken on Intel, we have to use the storage instead.
            if (Target == Target.Cubemap && HwCapabilities.Vendor == HwCapabilities.GpuVendor.IntelWindows)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(target, Storage.Handle);
                baseLevel = FirstLevel;
            }
            else
            {
                Bind(target, 0);
            }

            FormatInfo format = FormatTable.GetFormatInfo(Info.Format);

            int width  = Info.Width;
            int height = Info.Height;
            int depth  = Info.Depth;
            int levels = Info.GetLevelsClamped();

            int offset = 0;

            for (int level = 0; level < levels; level++)
            {
                int mipSize = Info.GetMipSize(level);

                int endOffset = offset + mipSize;

                if ((uint)endOffset > (uint)size)
                {
                    return;
                }

                switch (Target)
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
                                    baseLevel + level,
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
                                    baseLevel + level,
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

        public void SetStorage(BufferRange buffer)
        {
            throw new NotSupportedException();
        }

        private void DisposeHandles()
        {
            if (Handle != 0)
            {
                GL.DeleteTexture(Handle);

                Handle = 0;
            }
        }

        /// <summary>
        /// Release the view without necessarily disposing the parent if we are the default view.
        /// This allows it to be added to the resource pool and reused later.
        /// </summary>
        public void Release()
        {
            bool hadHandle = Handle != 0;

            if (_parent.DefaultView != this)
            {
                DisposeHandles();
            }

            if (hadHandle)
            {
                _parent.DecrementViewsCount();
            }
        }

        public void Dispose()
        {
            if (_parent.DefaultView == this)
            {
                // Remove the default view (us), so that the texture cannot be released to the cache.
                _parent.DeleteDefault();
            }

            Release();
        }
    }
}
