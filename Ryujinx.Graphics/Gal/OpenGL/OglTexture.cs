using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.Texture;
using System;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    class OglTexture : IGalTexture
    {
        private const long MaxTextureCacheSize = 768 * 1024 * 1024;

        private OglCachedResource<ImageHandler> _textureCache;

        public EventHandler<int> TextureDeleted { get; set; }

        public OglTexture()
        {
            _textureCache = new OglCachedResource<ImageHandler>(DeleteTexture, MaxTextureCacheSize);
        }

        public void LockCache()
        {
            _textureCache.Lock();
        }

        public void UnlockCache()
        {
            _textureCache.Unlock();
        }

        private void DeleteTexture(ImageHandler cachedImage)
        {
            TextureDeleted?.Invoke(this, cachedImage.Handle);

            GL.DeleteTexture(cachedImage.Handle);
        }

        public void Create(long key, int size, GalImage image)
        {
            int handle = GL.GenTexture();

            TextureTarget target = ImageUtils.GetTextureTarget(image.TextureTarget);

            GL.BindTexture(target, handle);

            const int level  = 0; //TODO: Support mipmap textures.
            const int border = 0;

            _textureCache.AddOrUpdate(key, new ImageHandler(handle, image), (uint)size);

            if (ImageUtils.IsCompressed(image.Format))
            {
                throw new InvalidOperationException("Surfaces with compressed formats are not supported!");
            }

            (PixelInternalFormat internalFmt,
             PixelFormat         format,
             PixelType           type) = OglEnumConverter.GetImageFormat(image.Format);

            switch (target)
            {
                case TextureTarget.Texture1D:
                    GL.TexImage1D(
                        target,
                        level,
                        internalFmt,
                        image.Width,
                        border,
                        format,
                        type,
                        IntPtr.Zero);
                    break;

                case TextureTarget.Texture2D:
                    GL.TexImage2D(
                        target,
                        level,
                        internalFmt,
                        image.Width,
                        image.Height,
                        border,
                        format,
                        type,
                        IntPtr.Zero);
                    break;
                case TextureTarget.Texture3D:
                    GL.TexImage3D(
                        target,
                        level,
                        internalFmt,
                        image.Width,
                        image.Height,
                        image.Depth,
                        border,
                        format,
                        type,
                        IntPtr.Zero);
                    break;
                case TextureTarget.Texture2DArray:
                    GL.TexImage3D(
                        target,
                        level,
                        internalFmt,
                        image.Width,
                        image.Height,
                        image.LayerCount,
                        border,
                        format,
                        type,
                        IntPtr.Zero);
                    break;
                default:
                    throw new NotImplementedException($"Unsupported texture target type: {target}");
            }
        }

        public void Create(long key, byte[] data, GalImage image)
        {
            int handle = GL.GenTexture();

            TextureTarget target = ImageUtils.GetTextureTarget(image.TextureTarget);

            GL.BindTexture(target, handle);

            const int level  = 0; //TODO: Support mipmap textures.
            const int border = 0;

            _textureCache.AddOrUpdate(key, new ImageHandler(handle, image), (uint)data.Length);

            if (ImageUtils.IsCompressed(image.Format) && !IsAstc(image.Format))
            {
                InternalFormat internalFmt = OglEnumConverter.GetCompressedImageFormat(image.Format);

                switch (target)
                {
                    case TextureTarget.Texture1D:
                        GL.CompressedTexImage1D(
                            target,
                            level,
                            internalFmt,
                            image.Width,
                            border,
                            data.Length,
                            data);
                        break;
                    case TextureTarget.Texture2D:
                        GL.CompressedTexImage2D(
                            target,
                            level,
                            internalFmt,
                            image.Width,
                            image.Height,
                            border,
                            data.Length,
                            data);
                        break;
                    case TextureTarget.Texture3D:
                        GL.CompressedTexImage3D(
                            target,
                            level,
                            internalFmt,
                            image.Width,
                            image.Height,
                            image.Depth,
                            border,
                            data.Length,
                            data);
                        break;
                    case TextureTarget.Texture2DArray:
                        GL.CompressedTexImage3D(
                            target,
                            level,
                            internalFmt,
                            image.Width,
                            image.Height,
                            image.LayerCount,
                            border,
                            data.Length,
                            data);
                        break;
                    case TextureTarget.TextureCubeMap:
                        Span<byte> array = new Span<byte>(data);

                        int faceSize = ImageUtils.GetSize(image) / 6;

                        for (int Face = 0; Face < 6; Face++)
                        {
                            GL.CompressedTexImage2D(
                                TextureTarget.TextureCubeMapPositiveX + Face,
                                level,
                                internalFmt,
                                image.Width,
                                image.Height,
                                border,
                                faceSize,
                                array.Slice(Face * faceSize, faceSize).ToArray());
                        }
                        break;
                    default:
                        throw new NotImplementedException($"Unsupported texture target type: {target}");
                }
            }
            else
            {
                //TODO: Use KHR_texture_compression_astc_hdr when available
                if (IsAstc(image.Format))
                {
                    int textureBlockWidth  = ImageUtils.GetBlockWidth(image.Format);
                    int textureBlockHeight = ImageUtils.GetBlockHeight(image.Format);
                    int textureBlockDepth  = ImageUtils.GetBlockDepth(image.Format);

                    data = AstcDecoder.DecodeToRgba8888(
                        data,
                        textureBlockWidth,
                        textureBlockHeight,
                        textureBlockDepth,
                        image.Width,
                        image.Height,
                        image.Depth);

                    image.Format = GalImageFormat.Rgba8 | (image.Format & GalImageFormat.TypeMask);
                }

                (PixelInternalFormat internalFmt,
                 PixelFormat         format,
                 PixelType           type) = OglEnumConverter.GetImageFormat(image.Format);


                switch (target)
                {
                    case TextureTarget.Texture1D:
                        GL.TexImage1D(
                            target,
                            level,
                            internalFmt,
                            image.Width,
                            border,
                            format,
                            type,
                            data);
                        break;
                    case TextureTarget.Texture2D:
                        GL.TexImage2D(
                            target,
                            level,
                            internalFmt,
                            image.Width,
                            image.Height,
                            border,
                            format,
                            type,
                            data);
                        break;
                    case TextureTarget.Texture3D:
                        GL.TexImage3D(
                            target,
                            level,
                            internalFmt,
                            image.Width,
                            image.Height,
                            image.Depth,
                            border,
                            format,
                            type,
                            data);
                        break;
                    case TextureTarget.Texture2DArray:
                        GL.TexImage3D(
                            target,
                            level,
                            internalFmt,
                            image.Width,
                            image.Height,
                            image.LayerCount,
                            border,
                            format,
                            type,
                            data);
                        break;
                    case TextureTarget.TextureCubeMap:
                        Span<byte> array = new Span<byte>(data);

                        int faceSize = ImageUtils.GetSize(image) / 6;

                        for (int face = 0; face < 6; face++)
                        {
                            GL.TexImage2D(
                                TextureTarget.TextureCubeMapPositiveX + face,
                                level,
                                internalFmt,
                                image.Width,
                                image.Height,
                                border,
                                format,
                                type,
                                array.Slice(face * faceSize, faceSize).ToArray());
                        }
                        break;
                    default:
                        throw new NotImplementedException($"Unsupported texture target type: {target}");
                }
            }
        }

        private static bool IsAstc(GalImageFormat format)
        {
            format &= GalImageFormat.FormatMask;

            return format > GalImageFormat.Astc2DStart && format < GalImageFormat.Astc2DEnd;
        }

        public bool TryGetImage(long key, out GalImage image)
        {
            if (_textureCache.TryGetValue(key, out ImageHandler cachedImage))
            {
                image = cachedImage.Image;

                return true;
            }

            image = default(GalImage);

            return false;
        }

        public bool TryGetImageHandler(long key, out ImageHandler cachedImage)
        {
            if (_textureCache.TryGetValue(key, out cachedImage))
            {
                return true;
            }

            cachedImage = null;

            return false;
        }

        public void Bind(long key, int index, GalImage image)
        {
            if (_textureCache.TryGetValue(key, out ImageHandler cachedImage))
            {
                GL.ActiveTexture(TextureUnit.Texture0 + index);

                TextureTarget target = ImageUtils.GetTextureTarget(image.TextureTarget);

                GL.BindTexture(target, cachedImage.Handle);

                int[] swizzleRgba = new int[]
                {
                    (int)OglEnumConverter.GetTextureSwizzle(image.XSource),
                    (int)OglEnumConverter.GetTextureSwizzle(image.YSource),
                    (int)OglEnumConverter.GetTextureSwizzle(image.ZSource),
                    (int)OglEnumConverter.GetTextureSwizzle(image.WSource)
                };

                GL.TexParameter(target, TextureParameterName.TextureSwizzleRgba, swizzleRgba);
            }
        }

        public void SetSampler(GalImage image, GalTextureSampler sampler)
        {
            int wrapS = (int)OglEnumConverter.GetTextureWrapMode(sampler.AddressU);
            int wrapT = (int)OglEnumConverter.GetTextureWrapMode(sampler.AddressV);
            int wrapR = (int)OglEnumConverter.GetTextureWrapMode(sampler.AddressP);

            int minFilter = (int)OglEnumConverter.GetTextureMinFilter(sampler.MinFilter, sampler.MipFilter);
            int magFilter = (int)OglEnumConverter.GetTextureMagFilter(sampler.MagFilter);

            TextureTarget target = ImageUtils.GetTextureTarget(image.TextureTarget);

            GL.TexParameter(target, TextureParameterName.TextureWrapS, wrapS);
            GL.TexParameter(target, TextureParameterName.TextureWrapT, wrapT);
            GL.TexParameter(target, TextureParameterName.TextureWrapR, wrapR);

            GL.TexParameter(target, TextureParameterName.TextureMinFilter, minFilter);
            GL.TexParameter(target, TextureParameterName.TextureMagFilter, magFilter);

            float[] color = new float[]
            {
                sampler.BorderColor.Red,
                sampler.BorderColor.Green,
                sampler.BorderColor.Blue,
                sampler.BorderColor.Alpha
            };

            GL.TexParameter(target, TextureParameterName.TextureBorderColor, color);

            if (sampler.DepthCompare)
            {
                GL.TexParameter(target, TextureParameterName.TextureCompareMode, (int)All.CompareRToTexture);
                GL.TexParameter(target, TextureParameterName.TextureCompareFunc, (int)OglEnumConverter.GetDepthCompareFunc(sampler.DepthCompareFunc));
            }
            else
            {
                GL.TexParameter(target, TextureParameterName.TextureCompareMode, (int)All.None);
                GL.TexParameter(target, TextureParameterName.TextureCompareFunc, (int)All.Never);
            }
        }
    }
}
