using OpenTK.Graphics.OpenGL;
using System;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    static class OGLEnumConverter
    {
        public static DepthFunction GetDepthFunc(GalComparisonOp Func)
        {
            if ((int)Func >= (int)DepthFunction.Never &&
                (int)Func <= (int)DepthFunction.Always)
            {
                return (DepthFunction)Func;
            }

            throw new ArgumentException(nameof(Func));
        }

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

        public static (PixelFormat, PixelType) GetTextureFormat(GalTextureFormat Format)
        {
            switch (Format)
            {
                case GalTextureFormat.R32G32B32A32: return (PixelFormat.Rgba, PixelType.Float);
                case GalTextureFormat.R16G16B16A16: return (PixelFormat.Rgba, PixelType.HalfFloat);
                case GalTextureFormat.A8B8G8R8:     return (PixelFormat.Rgba, PixelType.UnsignedByte);
                case GalTextureFormat.R32:          return (PixelFormat.Red,  PixelType.Float);
                case GalTextureFormat.A1B5G5R5:     return (PixelFormat.Rgba, PixelType.UnsignedShort5551);
                case GalTextureFormat.B5G6R5:       return (PixelFormat.Rgb,  PixelType.UnsignedShort565);
                case GalTextureFormat.G8R8:         return (PixelFormat.Rg,   PixelType.UnsignedByte);
                case GalTextureFormat.R16:          return (PixelFormat.Red,  PixelType.HalfFloat);
                case GalTextureFormat.R8:           return (PixelFormat.Red,  PixelType.UnsignedByte);
            }

            throw new NotImplementedException(Format.ToString());
        }

        public static InternalFormat GetCompressedTextureFormat(GalTextureFormat Format)
        {
            switch (Format)
            {
                case GalTextureFormat.BC7U: return InternalFormat.CompressedRgbaBptcUnorm;
                case GalTextureFormat.BC1:  return InternalFormat.CompressedRgbaS3tcDxt1Ext;
                case GalTextureFormat.BC2:  return InternalFormat.CompressedRgbaS3tcDxt3Ext;
                case GalTextureFormat.BC3:  return InternalFormat.CompressedRgbaS3tcDxt5Ext;
                case GalTextureFormat.BC4:  return InternalFormat.CompressedRedRgtc1;
                case GalTextureFormat.BC5:  return InternalFormat.CompressedRgRgtc2;
            }

            throw new NotImplementedException(Format.ToString());
        }

        public static All GetTextureSwizzle(GalTextureSource Source)
        {
            switch (Source)
            {
                case GalTextureSource.Zero:     return All.Zero;
                case GalTextureSource.Red:      return All.Red;
                case GalTextureSource.Green:    return All.Green;
                case GalTextureSource.Blue:     return All.Blue;
                case GalTextureSource.Alpha:    return All.Alpha;
                case GalTextureSource.OneInt:   return All.One;
                case GalTextureSource.OneFloat: return All.One;
            }

            throw new ArgumentException(nameof(Source));
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
            switch (BlendEquation)
            {
                case GalBlendEquation.FuncAdd:             return BlendEquationMode.FuncAdd;
                case GalBlendEquation.FuncSubtract:        return BlendEquationMode.FuncSubtract;
                case GalBlendEquation.FuncReverseSubtract: return BlendEquationMode.FuncReverseSubtract;
                case GalBlendEquation.Min:                 return BlendEquationMode.Min;
                case GalBlendEquation.Max:                 return BlendEquationMode.Max;
            }

            throw new ArgumentException(nameof(BlendEquation));
        }

        public static BlendingFactor GetBlendFactor(GalBlendFactor BlendFactor)
        {
            switch (BlendFactor)
            {
                case GalBlendFactor.Zero:                  return BlendingFactor.Zero;
                case GalBlendFactor.One:                   return BlendingFactor.One;
                case GalBlendFactor.SrcColor:              return BlendingFactor.SrcColor;
                case GalBlendFactor.OneMinusSrcColor:      return BlendingFactor.OneMinusSrcColor;
                case GalBlendFactor.DstColor:              return BlendingFactor.DstColor;
                case GalBlendFactor.OneMinusDstColor:      return BlendingFactor.OneMinusDstColor;
                case GalBlendFactor.SrcAlpha:              return BlendingFactor.SrcAlpha;
                case GalBlendFactor.OneMinusSrcAlpha:      return BlendingFactor.OneMinusSrcAlpha;
                case GalBlendFactor.DstAlpha:              return BlendingFactor.DstAlpha;
                case GalBlendFactor.OneMinusDstAlpha:      return BlendingFactor.OneMinusDstAlpha;
                case GalBlendFactor.OneMinusConstantColor: return BlendingFactor.OneMinusConstantColor;
                case GalBlendFactor.ConstantAlpha:         return BlendingFactor.ConstantAlpha;
                case GalBlendFactor.OneMinusConstantAlpha: return BlendingFactor.OneMinusConstantAlpha;
                case GalBlendFactor.SrcAlphaSaturate:      return BlendingFactor.SrcAlphaSaturate;
                case GalBlendFactor.Src1Color:             return BlendingFactor.Src1Color;
                case GalBlendFactor.OneMinusSrc1Color:     return (BlendingFactor)BlendingFactorSrc.OneMinusSrc1Color;
                case GalBlendFactor.Src1Alpha:             return BlendingFactor.Src1Alpha;
                case GalBlendFactor.OneMinusSrc1Alpha:     return (BlendingFactor)BlendingFactorSrc.OneMinusSrc1Alpha;

                case GalBlendFactor.ConstantColor:
                case GalBlendFactor.ConstantColorG80:
                    return BlendingFactor.ConstantColor;
            }

            throw new ArgumentException(nameof(BlendFactor));
        }
    }
}