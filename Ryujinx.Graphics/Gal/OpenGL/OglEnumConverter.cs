using OpenTK.Graphics.OpenGL;
using System;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    static class OglEnumConverter
    {
        public static FrontFaceDirection GetFrontFace(GalFrontFace frontFace)
        {
            switch (frontFace)
            {
                case GalFrontFace.Cw:  return FrontFaceDirection.Cw;
                case GalFrontFace.Ccw: return FrontFaceDirection.Ccw;
            }

            throw new ArgumentException(nameof(frontFace) + " \"" + frontFace + "\" is not valid!");
        }

        public static CullFaceMode GetCullFace(GalCullFace cullFace)
        {
            switch (cullFace)
            {
                case GalCullFace.Front:        return CullFaceMode.Front;
                case GalCullFace.Back:         return CullFaceMode.Back;
                case GalCullFace.FrontAndBack: return CullFaceMode.FrontAndBack;
            }

            throw new ArgumentException(nameof(cullFace) + " \"" + cullFace + "\" is not valid!");
        }

        public static StencilOp GetStencilOp(GalStencilOp op)
        {
            switch (op)
            {
                case GalStencilOp.Keep:     return StencilOp.Keep;
                case GalStencilOp.Zero:     return StencilOp.Zero;
                case GalStencilOp.Replace:  return StencilOp.Replace;
                case GalStencilOp.Incr:     return StencilOp.Incr;
                case GalStencilOp.Decr:     return StencilOp.Decr;
                case GalStencilOp.Invert:   return StencilOp.Invert;
                case GalStencilOp.IncrWrap: return StencilOp.IncrWrap;
                case GalStencilOp.DecrWrap: return StencilOp.DecrWrap;
            }

            throw new ArgumentException(nameof(op) + " \"" + op + "\" is not valid!");
        }

        public static DepthFunction GetDepthFunc(GalComparisonOp func)
        {
            return (DepthFunction)GetFunc(func);
        }

        public static StencilFunction GetStencilFunc(GalComparisonOp func)
        {
            return (StencilFunction)GetFunc(func);
        }

        private static All GetFunc(GalComparisonOp func)
        {
            if ((int)func >= (int)All.Never &&
                (int)func <= (int)All.Always)
            {
                return (All)func;
            }

            switch (func)
            {
                case GalComparisonOp.Never:    return All.Never;
                case GalComparisonOp.Less:     return All.Less;
                case GalComparisonOp.Equal:    return All.Equal;
                case GalComparisonOp.Lequal:   return All.Lequal;
                case GalComparisonOp.Greater:  return All.Greater;
                case GalComparisonOp.NotEqual: return All.Notequal;
                case GalComparisonOp.Gequal:   return All.Gequal;
                case GalComparisonOp.Always:   return All.Always;
            }

            throw new ArgumentException(nameof(func) + " \"" + func + "\" is not valid!");
        }

        public static DrawElementsType GetDrawElementsType(GalIndexFormat format)
        {
            switch (format)
            {
                case GalIndexFormat.Byte:  return DrawElementsType.UnsignedByte;
                case GalIndexFormat.Int16: return DrawElementsType.UnsignedShort;
                case GalIndexFormat.Int32: return DrawElementsType.UnsignedInt;
            }

            throw new ArgumentException(nameof(format) + " \"" + format + "\" is not valid!");
        }

        public static PrimitiveType GetPrimitiveType(GalPrimitiveType type)
        {
            switch (type)
            {
                case GalPrimitiveType.Points:                 return PrimitiveType.Points;
                case GalPrimitiveType.Lines:                  return PrimitiveType.Lines;
                case GalPrimitiveType.LineLoop:               return PrimitiveType.LineLoop;
                case GalPrimitiveType.LineStrip:              return PrimitiveType.LineStrip;
                case GalPrimitiveType.Triangles:              return PrimitiveType.Triangles;
                case GalPrimitiveType.TriangleStrip:          return PrimitiveType.TriangleStrip;
                case GalPrimitiveType.TriangleFan:            return PrimitiveType.TriangleFan;
                case GalPrimitiveType.Polygon:                return PrimitiveType.Polygon;
                case GalPrimitiveType.LinesAdjacency:         return PrimitiveType.LinesAdjacency;
                case GalPrimitiveType.LineStripAdjacency:     return PrimitiveType.LineStripAdjacency;
                case GalPrimitiveType.TrianglesAdjacency:     return PrimitiveType.TrianglesAdjacency;
                case GalPrimitiveType.TriangleStripAdjacency: return PrimitiveType.TriangleStripAdjacency;
                case GalPrimitiveType.Patches:                return PrimitiveType.Patches;
            }

            throw new ArgumentException(nameof(type) + " \"" + type + "\" is not valid!");
        }

        public static ShaderType GetShaderType(GalShaderType type)
        {
            switch (type)
            {
                case GalShaderType.Vertex:         return ShaderType.VertexShader;
                case GalShaderType.TessControl:    return ShaderType.TessControlShader;
                case GalShaderType.TessEvaluation: return ShaderType.TessEvaluationShader;
                case GalShaderType.Geometry:       return ShaderType.GeometryShader;
                case GalShaderType.Fragment:       return ShaderType.FragmentShader;
            }

            throw new ArgumentException(nameof(type) + " \"" + type + "\" is not valid!");
        }

        public static (PixelInternalFormat, PixelFormat, PixelType) GetImageFormat(GalImageFormat format)
        {
            switch (format)
            {
                case GalImageFormat.Rgba32    | GalImageFormat.Float: return (PixelInternalFormat.Rgba32f,      PixelFormat.Rgba,        PixelType.Float);
                case GalImageFormat.Rgba32    | GalImageFormat.Sint:  return (PixelInternalFormat.Rgba32i,      PixelFormat.RgbaInteger, PixelType.Int);
                case GalImageFormat.Rgba32    | GalImageFormat.Uint:  return (PixelInternalFormat.Rgba32ui,     PixelFormat.RgbaInteger, PixelType.UnsignedInt);
                case GalImageFormat.Rgba16    | GalImageFormat.Float: return (PixelInternalFormat.Rgba16f,      PixelFormat.Rgba,        PixelType.HalfFloat);
                case GalImageFormat.Rgba16    | GalImageFormat.Sint:  return (PixelInternalFormat.Rgba16i,      PixelFormat.RgbaInteger, PixelType.Short);
                case GalImageFormat.Rgba16    | GalImageFormat.Uint:  return (PixelInternalFormat.Rgba16ui,     PixelFormat.RgbaInteger, PixelType.UnsignedShort);
                case GalImageFormat.Rgba16    | GalImageFormat.Unorm: return (PixelInternalFormat.Rgba16,       PixelFormat.Rgba,        PixelType.UnsignedShort);
                case GalImageFormat.Rg32      | GalImageFormat.Float: return (PixelInternalFormat.Rg32f,        PixelFormat.Rg,          PixelType.Float);
                case GalImageFormat.Rg32      | GalImageFormat.Sint:  return (PixelInternalFormat.Rg32i,        PixelFormat.RgInteger,   PixelType.Int);
                case GalImageFormat.Rg32      | GalImageFormat.Uint:  return (PixelInternalFormat.Rg32ui,       PixelFormat.RgInteger,   PixelType.UnsignedInt);
                case GalImageFormat.Rgbx8     | GalImageFormat.Unorm: return (PixelInternalFormat.Rgb8,         PixelFormat.Rgba,        PixelType.UnsignedByte);
                case GalImageFormat.Rgba8     | GalImageFormat.Snorm: return (PixelInternalFormat.Rgba8Snorm,   PixelFormat.Rgba,        PixelType.Byte);
                case GalImageFormat.Rgba8     | GalImageFormat.Unorm: return (PixelInternalFormat.Rgba8,        PixelFormat.Rgba,        PixelType.UnsignedByte);
                case GalImageFormat.Rgba8     | GalImageFormat.Sint:  return (PixelInternalFormat.Rgba8i,       PixelFormat.RgbaInteger, PixelType.Byte);
                case GalImageFormat.Rgba8     | GalImageFormat.Uint:  return (PixelInternalFormat.Rgba8ui,      PixelFormat.RgbaInteger, PixelType.UnsignedByte);
                case GalImageFormat.Rgba8     | GalImageFormat.Srgb:  return (PixelInternalFormat.Srgb8Alpha8,  PixelFormat.Rgba,        PixelType.UnsignedByte);
                case GalImageFormat.Bgra8     | GalImageFormat.Unorm: return (PixelInternalFormat.Rgba8,        PixelFormat.Bgra,        PixelType.UnsignedByte);
                case GalImageFormat.Bgra8     | GalImageFormat.Srgb:  return (PixelInternalFormat.Srgb8Alpha8,  PixelFormat.Bgra,        PixelType.UnsignedByte);
                case GalImageFormat.Rgba4     | GalImageFormat.Unorm: return (PixelInternalFormat.Rgba4,        PixelFormat.Rgba,        PixelType.UnsignedShort4444Reversed);
                case GalImageFormat.Rgb10A2   | GalImageFormat.Uint:  return (PixelInternalFormat.Rgb10A2ui,    PixelFormat.RgbaInteger, PixelType.UnsignedInt2101010Reversed);
                case GalImageFormat.Rgb10A2   | GalImageFormat.Unorm: return (PixelInternalFormat.Rgb10A2,      PixelFormat.Rgba,        PixelType.UnsignedInt2101010Reversed);
                case GalImageFormat.R32       | GalImageFormat.Float: return (PixelInternalFormat.R32f,         PixelFormat.Red,         PixelType.Float);
                case GalImageFormat.R32       | GalImageFormat.Sint:  return (PixelInternalFormat.R32i,         PixelFormat.Red,         PixelType.Int);
                case GalImageFormat.R32       | GalImageFormat.Uint:  return (PixelInternalFormat.R32ui,        PixelFormat.Red,         PixelType.UnsignedInt);
                case GalImageFormat.Bgr5A1    | GalImageFormat.Unorm: return (PixelInternalFormat.Rgb5A1,       PixelFormat.Rgba,        PixelType.UnsignedShort5551);
                case GalImageFormat.Rgb5A1    | GalImageFormat.Unorm: return (PixelInternalFormat.Rgb5A1,       PixelFormat.Rgba,        PixelType.UnsignedShort1555Reversed);
                case GalImageFormat.Rgb565    | GalImageFormat.Unorm: return (PixelInternalFormat.Rgba,         PixelFormat.Rgb,         PixelType.UnsignedShort565Reversed);
                case GalImageFormat.Bgr565    | GalImageFormat.Unorm: return (PixelInternalFormat.Rgba,         PixelFormat.Rgb,         PixelType.UnsignedShort565);
                case GalImageFormat.Rg16      | GalImageFormat.Float: return (PixelInternalFormat.Rg16f,        PixelFormat.Rg,          PixelType.HalfFloat);
                case GalImageFormat.Rg16      | GalImageFormat.Sint:  return (PixelInternalFormat.Rg16i,        PixelFormat.RgInteger,   PixelType.Short);
                case GalImageFormat.Rg16      | GalImageFormat.Snorm: return (PixelInternalFormat.Rg16Snorm,    PixelFormat.Rg,          PixelType.Short);
                case GalImageFormat.Rg16      | GalImageFormat.Uint:  return (PixelInternalFormat.Rg16ui,       PixelFormat.RgInteger,   PixelType.UnsignedShort);
                case GalImageFormat.Rg16      | GalImageFormat.Unorm: return (PixelInternalFormat.Rg16,         PixelFormat.Rg,          PixelType.UnsignedShort);
                case GalImageFormat.Rg8       | GalImageFormat.Sint:  return (PixelInternalFormat.Rg8i,         PixelFormat.RgInteger,   PixelType.Byte);
                case GalImageFormat.Rg8       | GalImageFormat.Snorm: return (PixelInternalFormat.Rg8Snorm,     PixelFormat.Rg,          PixelType.Byte);
                case GalImageFormat.Rg8       | GalImageFormat.Uint:  return (PixelInternalFormat.Rg8ui,        PixelFormat.RgInteger,   PixelType.UnsignedByte);
                case GalImageFormat.Rg8       | GalImageFormat.Unorm: return (PixelInternalFormat.Rg8,          PixelFormat.Rg,          PixelType.UnsignedByte);
                case GalImageFormat.R16       | GalImageFormat.Float: return (PixelInternalFormat.R16f,         PixelFormat.Red,         PixelType.HalfFloat);
                case GalImageFormat.R16       | GalImageFormat.Sint:  return (PixelInternalFormat.R16i,         PixelFormat.RedInteger,  PixelType.Short);
                case GalImageFormat.R16       | GalImageFormat.Snorm: return (PixelInternalFormat.R16Snorm,     PixelFormat.Red,         PixelType.Short);
                case GalImageFormat.R16       | GalImageFormat.Uint:  return (PixelInternalFormat.R16ui,        PixelFormat.RedInteger,  PixelType.UnsignedShort);
                case GalImageFormat.R16       | GalImageFormat.Unorm: return (PixelInternalFormat.R16,          PixelFormat.Red,         PixelType.UnsignedShort);
                case GalImageFormat.R8        | GalImageFormat.Sint:  return (PixelInternalFormat.R8i,          PixelFormat.RedInteger,  PixelType.Byte);
                case GalImageFormat.R8        | GalImageFormat.Snorm: return (PixelInternalFormat.R8Snorm,      PixelFormat.Red,         PixelType.Byte);
                case GalImageFormat.R8        | GalImageFormat.Uint:  return (PixelInternalFormat.R8ui,         PixelFormat.RedInteger,  PixelType.UnsignedByte);
                case GalImageFormat.R8        | GalImageFormat.Unorm: return (PixelInternalFormat.R8,           PixelFormat.Red,         PixelType.UnsignedByte);
                case GalImageFormat.R11G11B10 | GalImageFormat.Float: return (PixelInternalFormat.R11fG11fB10f, PixelFormat.Rgb,         PixelType.UnsignedInt10F11F11FRev);

                case GalImageFormat.D16   | GalImageFormat.Unorm: return (PixelInternalFormat.DepthComponent16,  PixelFormat.DepthComponent, PixelType.UnsignedShort);
                case GalImageFormat.D24   | GalImageFormat.Unorm: return (PixelInternalFormat.DepthComponent24,  PixelFormat.DepthComponent, PixelType.UnsignedInt);
                case GalImageFormat.D24S8 | GalImageFormat.Uint:  return (PixelInternalFormat.Depth24Stencil8,   PixelFormat.DepthStencil,   PixelType.UnsignedInt248);
                case GalImageFormat.D24S8 | GalImageFormat.Unorm: return (PixelInternalFormat.Depth24Stencil8,   PixelFormat.DepthStencil,   PixelType.UnsignedInt248);
                case GalImageFormat.D32   | GalImageFormat.Float: return (PixelInternalFormat.DepthComponent32f, PixelFormat.DepthComponent, PixelType.Float);
                case GalImageFormat.D32S8 | GalImageFormat.Float: return (PixelInternalFormat.Depth32fStencil8,  PixelFormat.DepthStencil,   PixelType.Float32UnsignedInt248Rev);
            }

            throw new NotImplementedException($"{format & GalImageFormat.FormatMask} {format & GalImageFormat.TypeMask}");
        }

        public static All GetDepthCompareFunc(DepthCompareFunc depthCompareFunc)
        {
            switch (depthCompareFunc)
            {
                case DepthCompareFunc.LEqual:
                    return All.Lequal;
                case DepthCompareFunc.GEqual:
                    return All.Gequal;
                case DepthCompareFunc.Less:
                    return All.Less;
                case DepthCompareFunc.Greater:
                    return All.Greater;
                case DepthCompareFunc.Equal:
                    return All.Equal;
                case DepthCompareFunc.NotEqual:
                    return All.Notequal;
                case DepthCompareFunc.Always:
                    return All.Always;
                case DepthCompareFunc.Never:
                    return All.Never;
                default:
                    throw new ArgumentException(nameof(depthCompareFunc) + " \"" + depthCompareFunc + "\" is not valid!");
            }
        }

        public static InternalFormat GetCompressedImageFormat(GalImageFormat format)
        {
            switch (format)
            {
                case GalImageFormat.BptcSfloat | GalImageFormat.Float: return InternalFormat.CompressedRgbBptcSignedFloat;
                case GalImageFormat.BptcUfloat | GalImageFormat.Float: return InternalFormat.CompressedRgbBptcUnsignedFloat;
                case GalImageFormat.BptcUnorm  | GalImageFormat.Unorm: return InternalFormat.CompressedRgbaBptcUnorm;
                case GalImageFormat.BptcUnorm  | GalImageFormat.Srgb:  return InternalFormat.CompressedSrgbAlphaBptcUnorm;
                case GalImageFormat.BC1        | GalImageFormat.Unorm: return InternalFormat.CompressedRgbaS3tcDxt1Ext;
                case GalImageFormat.BC1        | GalImageFormat.Srgb:  return InternalFormat.CompressedSrgbAlphaS3tcDxt1Ext;
                case GalImageFormat.BC2        | GalImageFormat.Unorm: return InternalFormat.CompressedRgbaS3tcDxt3Ext;
                case GalImageFormat.BC2        | GalImageFormat.Srgb:  return InternalFormat.CompressedSrgbAlphaS3tcDxt3Ext;
                case GalImageFormat.BC3        | GalImageFormat.Unorm: return InternalFormat.CompressedRgbaS3tcDxt5Ext;
                case GalImageFormat.BC3        | GalImageFormat.Srgb:  return InternalFormat.CompressedSrgbAlphaS3tcDxt5Ext;
                case GalImageFormat.BC4        | GalImageFormat.Snorm: return InternalFormat.CompressedSignedRedRgtc1;
                case GalImageFormat.BC4        | GalImageFormat.Unorm: return InternalFormat.CompressedRedRgtc1;
                case GalImageFormat.BC5        | GalImageFormat.Snorm: return InternalFormat.CompressedSignedRgRgtc2;
                case GalImageFormat.BC5        | GalImageFormat.Unorm: return InternalFormat.CompressedRgRgtc2;
            }

            throw new NotImplementedException($"{format & GalImageFormat.FormatMask} {format & GalImageFormat.TypeMask}");
        }

        public static All GetTextureSwizzle(GalTextureSource source)
        {
            switch (source)
            {
                case GalTextureSource.Zero:     return All.Zero;
                case GalTextureSource.Red:      return All.Red;
                case GalTextureSource.Green:    return All.Green;
                case GalTextureSource.Blue:     return All.Blue;
                case GalTextureSource.Alpha:    return All.Alpha;
                case GalTextureSource.OneInt:   return All.One;
                case GalTextureSource.OneFloat: return All.One;
            }

            throw new ArgumentException(nameof(source) + " \"" + source + "\" is not valid!");
        }

        public static TextureWrapMode GetTextureWrapMode(GalTextureWrap wrap)
        {
            switch (wrap)
            {
                case GalTextureWrap.Repeat:         return TextureWrapMode.Repeat;
                case GalTextureWrap.MirroredRepeat: return TextureWrapMode.MirroredRepeat;
                case GalTextureWrap.ClampToEdge:    return TextureWrapMode.ClampToEdge;
                case GalTextureWrap.ClampToBorder:  return TextureWrapMode.ClampToBorder;
                case GalTextureWrap.Clamp:          return TextureWrapMode.Clamp;
            }

            if (OglExtension.TextureMirrorClamp)
            {
                switch (wrap)
                {
                    case GalTextureWrap.MirrorClampToEdge:   return (TextureWrapMode)ExtTextureMirrorClamp.MirrorClampToEdgeExt;
                    case GalTextureWrap.MirrorClampToBorder: return (TextureWrapMode)ExtTextureMirrorClamp.MirrorClampToBorderExt;
                    case GalTextureWrap.MirrorClamp:         return (TextureWrapMode)ExtTextureMirrorClamp.MirrorClampExt;
                }
            }
            else
            {
                //Fallback to non-mirrored clamps
                switch (wrap)
                {
                    case GalTextureWrap.MirrorClampToEdge:   return TextureWrapMode.ClampToEdge;
                    case GalTextureWrap.MirrorClampToBorder: return TextureWrapMode.ClampToBorder;
                    case GalTextureWrap.MirrorClamp:         return TextureWrapMode.Clamp;
                }
            }

            throw new ArgumentException(nameof(wrap) + " \"" + wrap + "\" is not valid!");
        }

        public static TextureMinFilter GetTextureMinFilter(
            GalTextureFilter    minFilter,
            GalTextureMipFilter mipFilter)
        {
            //TODO: Mip (needs mipmap support first).
            switch (minFilter)
            {
                case GalTextureFilter.Nearest: return TextureMinFilter.Nearest;
                case GalTextureFilter.Linear:  return TextureMinFilter.Linear;
            }

            throw new ArgumentException(nameof(minFilter) + " \"" + minFilter + "\" is not valid!");
        }

        public static TextureMagFilter GetTextureMagFilter(GalTextureFilter filter)
        {
            switch (filter)
            {
                case GalTextureFilter.Nearest: return TextureMagFilter.Nearest;
                case GalTextureFilter.Linear:  return TextureMagFilter.Linear;
            }

            throw new ArgumentException(nameof(filter) + " \"" + filter + "\" is not valid!");
        }

        public static BlendEquationMode GetBlendEquation(GalBlendEquation blendEquation)
        {
            switch (blendEquation)
            {
                case GalBlendEquation.FuncAdd:
                case GalBlendEquation.FuncAddGl:
                    return BlendEquationMode.FuncAdd;

                case GalBlendEquation.FuncSubtract:
                case GalBlendEquation.FuncSubtractGl:
                    return BlendEquationMode.FuncSubtract;

                case GalBlendEquation.FuncReverseSubtract:
                case GalBlendEquation.FuncReverseSubtractGl:
                    return BlendEquationMode.FuncReverseSubtract;

                case GalBlendEquation.Min:
                case GalBlendEquation.MinGl:
                    return BlendEquationMode.Min;

                case GalBlendEquation.Max:
                case GalBlendEquation.MaxGl:
                    return BlendEquationMode.Max;
            }

            throw new ArgumentException(nameof(blendEquation) + " \"" + blendEquation + "\" is not valid!");
        }

        public static BlendingFactor GetBlendFactor(GalBlendFactor blendFactor)
        {
            switch (blendFactor)
            {
                case GalBlendFactor.Zero:
                case GalBlendFactor.ZeroGl:
                    return BlendingFactor.Zero;

                case GalBlendFactor.One:
                case GalBlendFactor.OneGl:
                    return BlendingFactor.One;

                case GalBlendFactor.SrcColor:
                case GalBlendFactor.SrcColorGl:
                    return BlendingFactor.SrcColor;

                case GalBlendFactor.OneMinusSrcColor:
                case GalBlendFactor.OneMinusSrcColorGl:
                    return BlendingFactor.OneMinusSrcColor;

                case GalBlendFactor.DstColor:
                case GalBlendFactor.DstColorGl:
                    return BlendingFactor.DstColor;

                case GalBlendFactor.OneMinusDstColor:
                case GalBlendFactor.OneMinusDstColorGl:
                    return BlendingFactor.OneMinusDstColor;

                case GalBlendFactor.SrcAlpha:
                case GalBlendFactor.SrcAlphaGl:
                    return BlendingFactor.SrcAlpha;

                case GalBlendFactor.OneMinusSrcAlpha:
                case GalBlendFactor.OneMinusSrcAlphaGl:
                    return BlendingFactor.OneMinusSrcAlpha;

                case GalBlendFactor.DstAlpha:
                case GalBlendFactor.DstAlphaGl:
                    return BlendingFactor.DstAlpha;

                case GalBlendFactor.OneMinusDstAlpha:
                case GalBlendFactor.OneMinusDstAlphaGl:
                    return BlendingFactor.OneMinusDstAlpha;

                case GalBlendFactor.OneMinusConstantColor:
                case GalBlendFactor.OneMinusConstantColorGl:
                    return BlendingFactor.OneMinusConstantColor;

                case GalBlendFactor.ConstantAlpha:
                case GalBlendFactor.ConstantAlphaGl:
                    return BlendingFactor.ConstantAlpha;

                case GalBlendFactor.OneMinusConstantAlpha:
                case GalBlendFactor.OneMinusConstantAlphaGl:
                    return BlendingFactor.OneMinusConstantAlpha;

                case GalBlendFactor.SrcAlphaSaturate:
                case GalBlendFactor.SrcAlphaSaturateGl:
                    return BlendingFactor.SrcAlphaSaturate;

                case GalBlendFactor.Src1Color:
                case GalBlendFactor.Src1ColorGl:
                    return BlendingFactor.Src1Color;

                case GalBlendFactor.OneMinusSrc1Color:
                case GalBlendFactor.OneMinusSrc1ColorGl:
                    return (BlendingFactor)BlendingFactorSrc.OneMinusSrc1Color;

                case GalBlendFactor.Src1Alpha:
                case GalBlendFactor.Src1AlphaGl:
                    return BlendingFactor.Src1Alpha;

                case GalBlendFactor.OneMinusSrc1Alpha:
                case GalBlendFactor.OneMinusSrc1AlphaGl:
                    return (BlendingFactor)BlendingFactorSrc.OneMinusSrc1Alpha;

                case GalBlendFactor.ConstantColor:
                case GalBlendFactor.ConstantColorGl:
                    return BlendingFactor.ConstantColor;
            }

            throw new ArgumentException(nameof(blendFactor) + " \"" + blendFactor + "\" is not valid!");
        }
    }
}
