using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.Gal.Texture;
using System;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    class OGLTexture
    {
        private int[] Textures;

        public OGLTexture()
        {
            Textures = new int[80];
        }

        public void Set(int Index, GalTexture Texture)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + Index);

            Bind(Index);

            const int Level  = 0; //TODO: Support mipmap textures.
            const int Border = 0;

            if (IsCompressedTextureFormat(Texture.Format))
            {
                PixelInternalFormat InternalFmt = OGLEnumConverter.GetCompressedTextureFormat(Texture.Format);

                GL.CompressedTexImage2D(
                    TextureTarget.Texture2D,
                    Level,
                    InternalFmt,
                    Texture.Width,
                    Texture.Height,
                    Border,
                    Texture.Data.Length,
                    Texture.Data);
            }
            else
            {
                if (Texture.Format >= GalTextureFormat.Astc2D4x4)
                {
                    Texture = ConvertAstcTextureToRgba(Texture);
                }

                const PixelInternalFormat InternalFmt = PixelInternalFormat.Rgba;

                (PixelFormat Format, PixelType Type) = OGLEnumConverter.GetTextureFormat(Texture.Format);

                GL.TexImage2D(
                    TextureTarget.Texture2D,
                    Level,
                    InternalFmt,
                    Texture.Width,
                    Texture.Height,
                    Border,
                    Format,
                    Type,
                    Texture.Data);
            }

            int SwizzleR = (int)OGLEnumConverter.GetTextureSwizzle(Texture.XSource);
            int SwizzleG = (int)OGLEnumConverter.GetTextureSwizzle(Texture.YSource);
            int SwizzleB = (int)OGLEnumConverter.GetTextureSwizzle(Texture.ZSource);
            int SwizzleA = (int)OGLEnumConverter.GetTextureSwizzle(Texture.WSource);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureSwizzleR, SwizzleR);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureSwizzleG, SwizzleG);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureSwizzleB, SwizzleB);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureSwizzleA, SwizzleA);
        }

        private static GalTexture ConvertAstcTextureToRgba(GalTexture Texture)
        {
            int TextureBlockWidth  = GetAstcBlockWidth(Texture.Format);
            int TextureBlockHeight = GetAstcBlockHeight(Texture.Format);

            Texture.Data = ASTCDecoder.DecodeToRGBA8888(
                Texture.Data,
                TextureBlockWidth,
                TextureBlockHeight, 1,
                Texture.Width,
                Texture.Height, 1);

            Texture.Format = GalTextureFormat.A8B8G8R8;

            return Texture;
        }

        private static int GetAstcBlockWidth(GalTextureFormat Format)
        {
            switch (Format)
            {
                case GalTextureFormat.Astc2D4x4:   return 4;
                case GalTextureFormat.Astc2D5x5:   return 5;
                case GalTextureFormat.Astc2D6x6:   return 6;
                case GalTextureFormat.Astc2D8x8:   return 8;
                case GalTextureFormat.Astc2D10x10: return 10;
                case GalTextureFormat.Astc2D12x12: return 12;
                case GalTextureFormat.Astc2D5x4:   return 5;
                case GalTextureFormat.Astc2D6x5:   return 6;
                case GalTextureFormat.Astc2D8x6:   return 8;
                case GalTextureFormat.Astc2D10x8:  return 10;
                case GalTextureFormat.Astc2D12x10: return 12;
                case GalTextureFormat.Astc2D8x5:   return 8;
                case GalTextureFormat.Astc2D10x5:  return 10;
                case GalTextureFormat.Astc2D10x6:  return 10;
            }

            throw new ArgumentException(nameof(Format));
        }

        private static int GetAstcBlockHeight(GalTextureFormat Format)
        {
            switch (Format)
            {
                case GalTextureFormat.Astc2D4x4:   return 4;
                case GalTextureFormat.Astc2D5x5:   return 5;
                case GalTextureFormat.Astc2D6x6:   return 6;
                case GalTextureFormat.Astc2D8x8:   return 8;
                case GalTextureFormat.Astc2D10x10: return 10;
                case GalTextureFormat.Astc2D12x12: return 12;
                case GalTextureFormat.Astc2D5x4:   return 4;
                case GalTextureFormat.Astc2D6x5:   return 5;
                case GalTextureFormat.Astc2D8x6:   return 6;
                case GalTextureFormat.Astc2D10x8:  return 8;
                case GalTextureFormat.Astc2D12x10: return 10;
                case GalTextureFormat.Astc2D8x5:   return 5;
                case GalTextureFormat.Astc2D10x5:  return 5;
                case GalTextureFormat.Astc2D10x6:  return 6;
            }

            throw new ArgumentException(nameof(Format));
        }

        public void Bind(int Index)
        {
            int Handle = EnsureTextureInitialized(Index);

            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }

        public static void Set(GalTextureSampler Sampler)
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

        private static bool IsCompressedTextureFormat(GalTextureFormat Format)
        {
            switch (Format)
            {
                case GalTextureFormat.BC1:
                case GalTextureFormat.BC2:
                case GalTextureFormat.BC3:
                case GalTextureFormat.BC4:
                case GalTextureFormat.BC5:
                    return true;
            }

            return false;
        }

        private int EnsureTextureInitialized(int TexIndex)
        {
            int Handle = Textures[TexIndex];

            if (Handle == 0)
            {
                Handle = Textures[TexIndex] = GL.GenTexture();
            }

            return Handle;
        }
    }
}