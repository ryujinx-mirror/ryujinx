using OpenTK.Graphics.OpenGL;
using System;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    static class OGLEnumConverter
    {
        public static DrawElementsType GetDrawElementsType(GalIndexFormat Format)
        {
            switch (Format)
            {
                case GalIndexFormat.Byte:  return DrawElementsType.UnsignedByte;
                case GalIndexFormat.Int16: return DrawElementsType.UnsignedShort;
                case GalIndexFormat.Int32: return DrawElementsType.UnsignedInt;
            }

            throw new ArgumentException(nameof(Format));
        }

        public static PrimitiveType GetPrimitiveType(GalPrimitiveType Type)
        {
            switch (Type)
            {
                case GalPrimitiveType.Points:                 return PrimitiveType.Points;
                case GalPrimitiveType.Lines:                  return PrimitiveType.Lines;
                case GalPrimitiveType.LineLoop:               return PrimitiveType.LineLoop;
                case GalPrimitiveType.LineStrip:              return PrimitiveType.LineStrip;
                case GalPrimitiveType.Triangles:              return PrimitiveType.Triangles;
                case GalPrimitiveType.TriangleStrip:          return PrimitiveType.TriangleStrip;
                case GalPrimitiveType.TriangleFan:            return PrimitiveType.TriangleFan;
                case GalPrimitiveType.Quads:                  return PrimitiveType.Quads;
                case GalPrimitiveType.QuadStrip:              return PrimitiveType.QuadStrip;
                case GalPrimitiveType.Polygon:                return PrimitiveType.Polygon;
                case GalPrimitiveType.LinesAdjacency:         return PrimitiveType.LinesAdjacency;
                case GalPrimitiveType.LineStripAdjacency:     return PrimitiveType.LineStripAdjacency;
                case GalPrimitiveType.TrianglesAdjacency:     return PrimitiveType.TrianglesAdjacency;
                case GalPrimitiveType.TriangleStripAdjacency: return PrimitiveType.TriangleStripAdjacency;
                case GalPrimitiveType.Patches:                return PrimitiveType.Patches;
            }

            throw new ArgumentException(nameof(Type));
        }

        public static ShaderType GetShaderType(GalShaderType Type)
        {
            switch (Type)
            {
                case GalShaderType.Vertex:         return ShaderType.VertexShader;
                case GalShaderType.TessControl:    return ShaderType.TessControlShader;
                case GalShaderType.TessEvaluation: return ShaderType.TessEvaluationShader;
                case GalShaderType.Geometry:       return ShaderType.GeometryShader;
                case GalShaderType.Fragment:       return ShaderType.FragmentShader;
            }

            throw new ArgumentException(nameof(Type));
        }

        public static PixelInternalFormat GetCompressedTextureFormat(GalTextureFormat Format)
        {
            switch (Format)
            {
                case GalTextureFormat.BC1: return PixelInternalFormat.CompressedRgbaS3tcDxt1Ext;
                case GalTextureFormat.BC2: return PixelInternalFormat.CompressedRgbaS3tcDxt3Ext;
                case GalTextureFormat.BC3: return PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
            }

            throw new NotImplementedException(Format.ToString());
        }

        public static TextureWrapMode GetTextureWrapMode(GalTextureWrap Wrap)
        {
            switch (Wrap)
            {
                case GalTextureWrap.Repeat:              return TextureWrapMode.Repeat;
                case GalTextureWrap.MirroredRepeat:      return TextureWrapMode.MirroredRepeat;
                case GalTextureWrap.ClampToEdge:         return TextureWrapMode.ClampToEdge;
                case GalTextureWrap.ClampToBorder:       return TextureWrapMode.ClampToBorder;
                case GalTextureWrap.Clamp:               return TextureWrapMode.Clamp;

                //TODO: Those needs extensions (and are currently wrong).
                case GalTextureWrap.MirrorClampToEdge:   return TextureWrapMode.ClampToEdge;
                case GalTextureWrap.MirrorClampToBorder: return TextureWrapMode.ClampToBorder;
                case GalTextureWrap.MirrorClamp:         return TextureWrapMode.Clamp;
            }

            throw new ArgumentException(nameof(Wrap));
        }

        public static TextureMinFilter GetTextureMinFilter(
            GalTextureFilter    MinFilter,
            GalTextureMipFilter MipFilter)
        {
            //TODO: Mip (needs mipmap support first).
            switch (MinFilter)
            {
                case GalTextureFilter.Nearest: return TextureMinFilter.Nearest;
                case GalTextureFilter.Linear:  return TextureMinFilter.Linear;
            }

            throw new ArgumentException(nameof(MinFilter));
        }

        public static TextureMagFilter GetTextureMagFilter(GalTextureFilter Filter)
        {
            switch (Filter)
            {
                case GalTextureFilter.Nearest: return TextureMagFilter.Nearest;
                case GalTextureFilter.Linear:  return TextureMagFilter.Linear;
            }

            throw new ArgumentException(nameof(Filter));
        }

        public static BlendEquationMode GetBlendEquation(GalBlendEquation BlendEquation)
        {
            return (BlendEquationMode)BlendEquation;
        }

        public static BlendingFactorSrc GetBlendFactorSrc(GalBlendFactor BlendFactor)
        {
            return (BlendingFactorSrc)(BlendFactor - 0x4000);
        }

        public static BlendingFactorDest GetBlendFactorDst(GalBlendFactor BlendFactor)
        {
            return (BlendingFactorDest)(BlendFactor - 0x4000);
        }
    }
}