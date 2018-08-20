using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.Gal.Texture;
using System;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    class OGLTexture : IGalTexture
    {
        private OGLCachedResource<ImageHandler> TextureCache;

        public OGLTexture()
        {
            TextureCache = new OGLCachedResource<ImageHandler>(DeleteTexture);
        }

        public void LockCache()
        {
            TextureCache.Lock();
        }

        public void UnlockCache()
        {
            TextureCache.Unlock();
        }

        private static void DeleteTexture(ImageHandler CachedImage)
        {
            GL.DeleteTexture(CachedImage.Handle);
        }

        public void Create(long Key, byte[] Data, GalImage Image)
        {
            int Handle = GL.GenTexture();

            TextureCache.AddOrUpdate(Key, new ImageHandler(Handle, Image), (uint)Data.Length);

            GL.BindTexture(TextureTarget.Texture2D, Handle);

            const int Level  = 0; //TODO: Support mipmap textures.
            const int Border = 0;

            if (IsCompressedTextureFormat(Image.Format))
            {
                InternalFormat InternalFmt = OGLEnumConverter.GetCompressedImageFormat(Image.Format);

                GL.CompressedTexImage2D(
                    TextureTarget.Texture2D,
                    Level,
                    InternalFmt,
                    Image.Width,
                    Image.Height,
                    Border,
                    Data.Length,
                    Data);
            }
            else
            {
                if (Image.Format >= GalImageFormat.ASTC_BEGIN && Image.Format <= GalImageFormat.ASTC_END)
                {
                    int TextureBlockWidth  = GetAstcBlockWidth(Image.Format);
                    int TextureBlockHeight = GetAstcBlockHeight(Image.Format);

                    Data = ASTCDecoder.DecodeToRGBA8888(
                        Data,
                        TextureBlockWidth,
                        TextureBlockHeight, 1,
                        Image.Width,
                        Image.Height, 1);

                    Image.Format = GalImageFormat.A8B8G8R8_UNORM_PACK32;
                }

                (PixelInternalFormat InternalFormat, PixelFormat Format, PixelType Type) = OGLEnumConverter.GetImageFormat(Image.Format);

                GL.TexImage2D(
                    TextureTarget.Texture2D,
                    Level,
                    InternalFormat,
                    Image.Width,
                    Image.Height,
                    Border,
                    Format,
                    Type,
                    Data);
            }

            int SwizzleR = (int)OGLEnumConverter.GetTextureSwizzle(Image.XSource);
            int SwizzleG = (int)OGLEnumConverter.GetTextureSwizzle(Image.YSource);
            int SwizzleB = (int)OGLEnumConverter.GetTextureSwizzle(Image.ZSource);
            int SwizzleA = (int)OGLEnumConverter.GetTextureSwizzle(Image.WSource);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureSwizzleR, SwizzleR);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureSwizzleG, SwizzleG);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureSwizzleB, SwizzleB);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureSwizzleA, SwizzleA);
        }

        public void CreateFb(long Key, long Size, GalImage Image)
        {
            if (!TryGetImage(Key, out ImageHandler CachedImage))
            {
                CachedImage = new ImageHandler();

                TextureCache.AddOrUpdate(Key, CachedImage, Size);
            }

            CachedImage.EnsureSetup(Image);
        }

        public bool TryGetImage(long Key, out ImageHandler CachedImage)
        {
            if (TextureCache.TryGetValue(Key, out CachedImage))
            {
                return true;
            }

            CachedImage = null;

            return false;
        }

        private static int GetAstcBlockWidth(GalImageFormat Format)
        {
            switch (Format)
            {
                case GalImageFormat.ASTC_4x4_UNORM_BLOCK:   return 4;
                case GalImageFormat.ASTC_5x5_UNORM_BLOCK:   return 5;
                case GalImageFormat.ASTC_6x6_UNORM_BLOCK:   return 6;
                case GalImageFormat.ASTC_8x8_UNORM_BLOCK:   return 8;
                case GalImageFormat.ASTC_10x10_UNORM_BLOCK: return 10;
                case GalImageFormat.ASTC_12x12_UNORM_BLOCK: return 12;
                case GalImageFormat.ASTC_5x4_UNORM_BLOCK:   return 5;
                case GalImageFormat.ASTC_6x5_UNORM_BLOCK:   return 6;
                case GalImageFormat.ASTC_8x6_UNORM_BLOCK:   return 8;
                case GalImageFormat.ASTC_10x8_UNORM_BLOCK:  return 10;
                case GalImageFormat.ASTC_12x10_UNORM_BLOCK: return 12;
                case GalImageFormat.ASTC_8x5_UNORM_BLOCK:   return 8;
                case GalImageFormat.ASTC_10x5_UNORM_BLOCK:  return 10;
                case GalImageFormat.ASTC_10x6_UNORM_BLOCK:  return 10;
            }

            throw new ArgumentException(nameof(Format));
        }

        private static int GetAstcBlockHeight(GalImageFormat Format)
        {
            switch (Format)
            {
                case GalImageFormat.ASTC_4x4_UNORM_BLOCK:   return 4;
                case GalImageFormat.ASTC_5x5_UNORM_BLOCK:   return 5;
                case GalImageFormat.ASTC_6x6_UNORM_BLOCK:   return 6;
                case GalImageFormat.ASTC_8x8_UNORM_BLOCK:   return 8;
                case GalImageFormat.ASTC_10x10_UNORM_BLOCK: return 10;
                case GalImageFormat.ASTC_12x12_UNORM_BLOCK: return 12;
                case GalImageFormat.ASTC_5x4_UNORM_BLOCK:   return 4;
                case GalImageFormat.ASTC_6x5_UNORM_BLOCK:   return 5;
                case GalImageFormat.ASTC_8x6_UNORM_BLOCK:   return 6;
                case GalImageFormat.ASTC_10x8_UNORM_BLOCK:  return 8;
                case GalImageFormat.ASTC_12x10_UNORM_BLOCK: return 10;
                case GalImageFormat.ASTC_8x5_UNORM_BLOCK:   return 5;
                case GalImageFormat.ASTC_10x5_UNORM_BLOCK:  return 5;
                case GalImageFormat.ASTC_10x6_UNORM_BLOCK:  return 6;
            }

            throw new ArgumentException(nameof(Format));
        }

        public bool TryGetCachedTexture(long Key, long DataSize, out GalImage Image)
        {
            if (TextureCache.TryGetSize(Key, out long Size) && Size == DataSize)
            {
                if (TextureCache.TryGetValue(Key, out ImageHandler CachedImage))
                {
                    Image = CachedImage.Image;

                    return true;
                }
            }

            Image = default(GalImage);

            return false;
        }

        public void Bind(long Key, int Index)
        {
            if (TextureCache.TryGetValue(Key, out ImageHandler CachedImage))
            {
                GL.ActiveTexture(TextureUnit.Texture0 + Index);

                GL.BindTexture(TextureTarget.Texture2D, CachedImage.Handle);
            }
        }

        public void SetSampler(GalTextureSampler Sampler)
        {
            int WrapS = (int)OGLEnumConverter.GetTextureWrapMode(Sampler.AddressU);
            int WrapT = (int)OGLEnumConverter.GetTextureWrapMode(Sampler.AddressV);

            int MinFilter = (int)OGLEnumConverter.GetTextureMinFilter(Sampler.MinFilter, Sampler.MipFilter);
            int MagFilter = (int)OGLEnumConverter.GetTextureMagFilter(Sampler.MagFilter);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, WrapS);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, WrapT);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, MinFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, MagFilter);

            float[] Color = new float[]
            {
                Sampler.BorderColor.Red,
                Sampler.BorderColor.Green,
                Sampler.BorderColor.Blue,
                Sampler.BorderColor.Alpha
            };

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, Color);
        }

        private static bool IsCompressedTextureFormat(GalImageFormat Format)
        {
            switch (Format)
            {
                case GalImageFormat.BC6H_UFLOAT_BLOCK:
                case GalImageFormat.BC6H_SFLOAT_BLOCK:
                case GalImageFormat.BC7_UNORM_BLOCK:
                case GalImageFormat.BC1_RGBA_UNORM_BLOCK:
                case GalImageFormat.BC2_UNORM_BLOCK:
                case GalImageFormat.BC3_UNORM_BLOCK:
                case GalImageFormat.BC4_SNORM_BLOCK:
                case GalImageFormat.BC4_UNORM_BLOCK:
                case GalImageFormat.BC5_SNORM_BLOCK:
                case GalImageFormat.BC5_UNORM_BLOCK:
                    return true;
            }

            return false;
        }
    }
}
