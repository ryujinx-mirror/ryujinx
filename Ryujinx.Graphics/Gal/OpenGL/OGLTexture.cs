using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.Texture;
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

            GalImageFormat TypeLess = Image.Format & GalImageFormat.FormatMask;

            bool IsASTC = TypeLess >= GalImageFormat.ASTC_BEGIN && TypeLess <= GalImageFormat.ASTC_END;

            if (ImageUtils.IsCompressed(Image.Format) && !IsASTC)
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
                //TODO: Use KHR_texture_compression_astc_hdr when available
                if (IsASTC)
                {
                    int TextureBlockWidth  = GetAstcBlockWidth(Image.Format);
                    int TextureBlockHeight = GetAstcBlockHeight(Image.Format);

                    Data = ASTCDecoder.DecodeToRGBA8888(
                        Data,
                        TextureBlockWidth,
                        TextureBlockHeight, 1,
                        Image.Width,
                        Image.Height, 1);

                    Image.Format = GalImageFormat.A8B8G8R8 | GalImageFormat.Unorm;
                }
                else if (TypeLess == GalImageFormat.G8R8)
                {
                    Data = ImageConverter.G8R8ToR8G8(
                        Data,
                        Image.Width,
                        Image.Height,
                        1);

                    Image.Format = GalImageFormat.R8G8 | (Image.Format & GalImageFormat.TypeMask);
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
                case GalImageFormat.ASTC_4x4   | GalImageFormat.Unorm: return 4;
                case GalImageFormat.ASTC_5x5   | GalImageFormat.Unorm: return 5;
                case GalImageFormat.ASTC_6x6   | GalImageFormat.Unorm: return 6;
                case GalImageFormat.ASTC_8x8   | GalImageFormat.Unorm: return 8;
                case GalImageFormat.ASTC_10x10 | GalImageFormat.Unorm: return 10;
                case GalImageFormat.ASTC_12x12 | GalImageFormat.Unorm: return 12;
                case GalImageFormat.ASTC_5x4   | GalImageFormat.Unorm: return 5;
                case GalImageFormat.ASTC_6x5   | GalImageFormat.Unorm: return 6;
                case GalImageFormat.ASTC_8x6   | GalImageFormat.Unorm: return 8;
                case GalImageFormat.ASTC_10x8  | GalImageFormat.Unorm: return 10;
                case GalImageFormat.ASTC_12x10 | GalImageFormat.Unorm: return 12;
                case GalImageFormat.ASTC_8x5   | GalImageFormat.Unorm: return 8;
                case GalImageFormat.ASTC_10x5  | GalImageFormat.Unorm: return 10;
                case GalImageFormat.ASTC_10x6  | GalImageFormat.Unorm: return 10;
            }

            throw new ArgumentException(nameof(Format));
        }

        private static int GetAstcBlockHeight(GalImageFormat Format)
        {
            switch (Format)
            {
                case GalImageFormat.ASTC_4x4   | GalImageFormat.Unorm: return 4;
                case GalImageFormat.ASTC_5x5   | GalImageFormat.Unorm: return 5;
                case GalImageFormat.ASTC_6x6   | GalImageFormat.Unorm: return 6;
                case GalImageFormat.ASTC_8x8   | GalImageFormat.Unorm: return 8;
                case GalImageFormat.ASTC_10x10 | GalImageFormat.Unorm: return 10;
                case GalImageFormat.ASTC_12x12 | GalImageFormat.Unorm: return 12;
                case GalImageFormat.ASTC_5x4   | GalImageFormat.Unorm: return 4;
                case GalImageFormat.ASTC_6x5   | GalImageFormat.Unorm: return 5;
                case GalImageFormat.ASTC_8x6   | GalImageFormat.Unorm: return 6;
                case GalImageFormat.ASTC_10x8  | GalImageFormat.Unorm: return 8;
                case GalImageFormat.ASTC_12x10 | GalImageFormat.Unorm: return 10;
                case GalImageFormat.ASTC_8x5   | GalImageFormat.Unorm: return 5;
                case GalImageFormat.ASTC_10x5  | GalImageFormat.Unorm: return 5;
                case GalImageFormat.ASTC_10x6  | GalImageFormat.Unorm: return 6;
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
    }
}
