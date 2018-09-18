using OpenTK.Graphics.OpenGL;
using System;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    static class OGLEnumConverter
    {
        public static FrontFaceDirection GetFrontFace(GalFrontFace FrontFace)
        {
            switch (FrontFace)
            {
                case GalFrontFace.CW:  return FrontFaceDirection.Cw;
                case GalFrontFace.CCW: return FrontFaceDirection.Ccw;
            }

            throw new ArgumentException(nameof(FrontFace));
        }

        public static CullFaceMode GetCullFace(GalCullFace CullFace)
        {
            switch (CullFace)
            {
                case GalCullFace.Front:        return CullFaceMode.Front;
                case GalCullFace.Back:         return CullFaceMode.Back;
                case GalCullFace.FrontAndBack: return CullFaceMode.FrontAndBack;
            }

            throw new ArgumentException(nameof(CullFace));
        }

        public static StencilOp GetStencilOp(GalStencilOp Op)
        {
            switch (Op)
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

            throw new ArgumentException(nameof(Op));
        }

        public static DepthFunction GetDepthFunc(GalComparisonOp Func)
        {
            //Looks like the GPU can take it's own values (described in GalComparisonOp) and OpenGL values alike
            if ((int)Func >= (int)DepthFunction.Never &&
                (int)Func <= (int)DepthFunction.Always)
            {
                return (DepthFunction)Func;
            }

            switch (Func)
            {
                case GalComparisonOp.Never:    return DepthFunction.Never;
                case GalComparisonOp.Less:     return DepthFunction.Less;
                case GalComparisonOp.Equal:    return DepthFunction.Equal;
                case GalComparisonOp.Lequal:   return DepthFunction.Lequal;
                case GalComparisonOp.Greater:  return DepthFunction.Greater;
                case GalComparisonOp.NotEqual: return DepthFunction.Notequal;
                case GalComparisonOp.Gequal:   return DepthFunction.Gequal;
                case GalComparisonOp.Always:   return DepthFunction.Always;
            }

            throw new ArgumentException(nameof(Func));
        }

        public static StencilFunction GetStencilFunc(GalComparisonOp Func)
        {
            //OGL comparison values match, it's just an enum cast
            return (StencilFunction)GetDepthFunc(Func);
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

        public static (PixelInternalFormat, PixelFormat, PixelType) GetImageFormat(GalImageFormat Format)
        {
            switch (Format)
            {
                case GalImageFormat.R32G32B32A32 | GalImageFormat.Sfloat: return (PixelInternalFormat.Rgba32f,      PixelFormat.Rgba,        PixelType.Float);
                case GalImageFormat.R32G32B32A32 | GalImageFormat.Sint:   return (PixelInternalFormat.Rgba32i,      PixelFormat.RgbaInteger, PixelType.Int);
                case GalImageFormat.R32G32B32A32 | GalImageFormat.Uint:   return (PixelInternalFormat.Rgba32ui,     PixelFormat.RgbaInteger, PixelType.UnsignedInt);
                case GalImageFormat.R16G16B16A16 | GalImageFormat.Sfloat: return (PixelInternalFormat.Rgba16f,      PixelFormat.Rgba,        PixelType.HalfFloat);
                case GalImageFormat.R16G16B16A16 | GalImageFormat.Sint:   return (PixelInternalFormat.Rgba16i,      PixelFormat.RgbaInteger, PixelType.Short);
                case GalImageFormat.R16G16B16A16 | GalImageFormat.Uint:   return (PixelInternalFormat.Rgba16ui,     PixelFormat.RgbaInteger, PixelType.UnsignedShort);
                case GalImageFormat.R32G32       | GalImageFormat.Sfloat: return (PixelInternalFormat.Rg32f,        PixelFormat.Rg,          PixelType.Float);
                case GalImageFormat.R32G32       | GalImageFormat.Sint:   return (PixelInternalFormat.Rg32i,        PixelFormat.RgInteger,   PixelType.Int);
                case GalImageFormat.R32G32       | GalImageFormat.Uint:   return (PixelInternalFormat.Rg32ui,       PixelFormat.RgInteger,   PixelType.UnsignedInt);
                case GalImageFormat.A8B8G8R8     | GalImageFormat.Snorm:  return (PixelInternalFormat.Rgba8Snorm,   PixelFormat.Rgba,        PixelType.Byte);
                case GalImageFormat.A8B8G8R8     | GalImageFormat.Unorm:  return (PixelInternalFormat.Rgba8,        PixelFormat.Rgba,        PixelType.UnsignedByte);
                case GalImageFormat.A8B8G8R8     | GalImageFormat.Sint:   return (PixelInternalFormat.Rgba8i,       PixelFormat.RgbaInteger, PixelType.Byte);
                case GalImageFormat.A8B8G8R8     | GalImageFormat.Uint:   return (PixelInternalFormat.Rgba8ui,      PixelFormat.RgbaInteger, PixelType.UnsignedByte);
                case GalImageFormat.A8B8G8R8_SRGB:                        return (PixelInternalFormat.Srgb8Alpha8,  PixelFormat.Rgba,        PixelType.UnsignedByte);
                case GalImageFormat.A4B4G4R4     | GalImageFormat.Unorm:  return (PixelInternalFormat.Rgba4,        PixelFormat.Rgba,        PixelType.UnsignedShort4444Reversed);
                case GalImageFormat.A2B10G10R10  | GalImageFormat.Uint:   return (PixelInternalFormat.Rgb10A2ui,    PixelFormat.RgbaInteger, PixelType.UnsignedInt2101010Reversed);
                case GalImageFormat.A2B10G10R10  | GalImageFormat.Unorm:  return (PixelInternalFormat.Rgb10A2,      PixelFormat.Rgba,        PixelType.UnsignedInt2101010Reversed);
                case GalImageFormat.R32          | GalImageFormat.Sfloat: return (PixelInternalFormat.R32f,         PixelFormat.Red,         PixelType.Float);
                case GalImageFormat.R32          | GalImageFormat.Sint:   return (PixelInternalFormat.R32i,         PixelFormat.Red,         PixelType.Int);
                case GalImageFormat.R32          | GalImageFormat.Uint:   return (PixelInternalFormat.R32ui,        PixelFormat.Red,         PixelType.UnsignedInt);
                case GalImageFormat.A1R5G5B5     | GalImageFormat.Unorm:  return (PixelInternalFormat.Rgb5A1,       PixelFormat.Rgba,        PixelType.UnsignedShort1555Reversed);
                case GalImageFormat.B5G6R5       | GalImageFormat.Unorm:  return (PixelInternalFormat.Rgba,         PixelFormat.Rgb,         PixelType.UnsignedShort565Reversed);
                case GalImageFormat.R16G16       | GalImageFormat.Sfloat: return (PixelInternalFormat.Rg16f,        PixelFormat.Rg,          PixelType.HalfFloat);
                case GalImageFormat.R16G16       | GalImageFormat.Sint:   return (PixelInternalFormat.Rg16i,        PixelFormat.RgInteger,   PixelType.Short);
                case GalImageFormat.R16G16       | GalImageFormat.Snorm:  return (PixelInternalFormat.Rg16Snorm,    PixelFormat.Rg,          PixelType.Byte);
                case GalImageFormat.R16G16       | GalImageFormat.Unorm:  return (PixelInternalFormat.Rg16,         PixelFormat.Rg,          PixelType.UnsignedShort);
                case GalImageFormat.R8G8         | GalImageFormat.Sint:   return (PixelInternalFormat.Rg8i,         PixelFormat.RgInteger,   PixelType.Byte);
                case GalImageFormat.R8G8         | GalImageFormat.Snorm:  return (PixelInternalFormat.Rg8Snorm,     PixelFormat.Rg,          PixelType.Byte);
                case GalImageFormat.R8G8         | GalImageFormat.Uint:   return (PixelInternalFormat.Rg8ui,        PixelFormat.RgInteger,   PixelType.UnsignedByte);
                case GalImageFormat.R8G8         | GalImageFormat.Unorm:  return (PixelInternalFormat.Rg8,          PixelFormat.Rg,          PixelType.UnsignedByte);
                case GalImageFormat.R16          | GalImageFormat.Sfloat: return (PixelInternalFormat.R16f,         PixelFormat.Red,         PixelType.HalfFloat);
                case GalImageFormat.R16          | GalImageFormat.Sint:   return (PixelInternalFormat.R16i,         PixelFormat.RedInteger,  PixelType.Short);
                case GalImageFormat.R16          | GalImageFormat.Snorm:  return (PixelInternalFormat.R16Snorm,     PixelFormat.Red,         PixelType.Byte);
                case GalImageFormat.R16          | GalImageFormat.Uint:   return (PixelInternalFormat.R16ui,        PixelFormat.RedInteger,  PixelType.UnsignedShort);
                case GalImageFormat.R16          | GalImageFormat.Unorm:  return (PixelInternalFormat.R16,          PixelFormat.Red,         PixelType.UnsignedShort);
                case GalImageFormat.R8           | GalImageFormat.Sint:   return (PixelInternalFormat.R8i,          PixelFormat.RedInteger,  PixelType.Byte);
                case GalImageFormat.R8           | GalImageFormat.Snorm:  return (PixelInternalFormat.R8Snorm,      PixelFormat.Red,         PixelType.Byte);
                case GalImageFormat.R8           | GalImageFormat.Uint:   return (PixelInternalFormat.R8ui,         PixelFormat.RedInteger,  PixelType.UnsignedByte);
                case GalImageFormat.R8           | GalImageFormat.Unorm:  return (PixelInternalFormat.R8,           PixelFormat.Red,         PixelType.UnsignedByte);
                case GalImageFormat.B10G11R11    | GalImageFormat.Sfloat: return (PixelInternalFormat.R11fG11fB10f, PixelFormat.Rgb,         PixelType.UnsignedInt10F11F11FRev);

                case GalImageFormat.D24_S8 | GalImageFormat.Unorm:  return (PixelInternalFormat.Depth24Stencil8,   PixelFormat.DepthStencil,   PixelType.UnsignedInt248);
                case GalImageFormat.D32    | GalImageFormat.Sfloat: return (PixelInternalFormat.DepthComponent32f, PixelFormat.DepthComponent, PixelType.Float);
                case GalImageFormat.D16    | GalImageFormat.Unorm:  return (PixelInternalFormat.DepthComponent16,  PixelFormat.DepthComponent, PixelType.UnsignedShort);
                case GalImageFormat.D32_S8 | GalImageFormat.Uint:   return (PixelInternalFormat.Depth32fStencil8,  PixelFormat.DepthStencil,   PixelType.Float32UnsignedInt248Rev);
            }

            throw new NotImplementedException($"{Format & GalImageFormat.FormatMask} {Format & GalImageFormat.TypeMask}");
        }

        public static InternalFormat GetCompressedImageFormat(GalImageFormat Format)
        {
            switch (Format)
            {
                case GalImageFormat.BC6H_UF16 | GalImageFormat.Sfloat: return InternalFormat.CompressedRgbBptcUnsignedFloat;
                case GalImageFormat.BC6H_SF16 | GalImageFormat.Unorm:  return InternalFormat.CompressedRgbBptcSignedFloat;
                case GalImageFormat.BC7       | GalImageFormat.Unorm:  return InternalFormat.CompressedRgbaBptcUnorm;
                case GalImageFormat.BC1_RGBA  | GalImageFormat.Unorm:  return InternalFormat.CompressedRgbaS3tcDxt1Ext;
                case GalImageFormat.BC2       | GalImageFormat.Unorm:  return InternalFormat.CompressedRgbaS3tcDxt3Ext;
                case GalImageFormat.BC3       | GalImageFormat.Unorm:  return InternalFormat.CompressedRgbaS3tcDxt5Ext;
                case GalImageFormat.BC4       | GalImageFormat.Snorm:  return InternalFormat.CompressedSignedRedRgtc1;
                case GalImageFormat.BC4       | GalImageFormat.Unorm:  return InternalFormat.CompressedRedRgtc1;
                case GalImageFormat.BC5       | GalImageFormat.Snorm:  return InternalFormat.CompressedSignedRgRgtc2;
                case GalImageFormat.BC5       | GalImageFormat.Unorm:  return InternalFormat.CompressedRgRgtc2;
            }

            throw new NotImplementedException($"{Format & GalImageFormat.FormatMask} {Format & GalImageFormat.TypeMask}");
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
                case GalTextureWrap.Repeat:         return TextureWrapMode.Repeat;
                case GalTextureWrap.MirroredRepeat: return TextureWrapMode.MirroredRepeat;
                case GalTextureWrap.ClampToEdge:    return TextureWrapMode.ClampToEdge;
                case GalTextureWrap.ClampToBorder:  return TextureWrapMode.ClampToBorder;
                case GalTextureWrap.Clamp:          return TextureWrapMode.Clamp;
            }

            if (OGLExtension.HasTextureMirrorClamp())
            {
                switch (Wrap)
                {
                    case GalTextureWrap.MirrorClampToEdge:   return (TextureWrapMode)ExtTextureMirrorClamp.MirrorClampToEdgeExt;
                    case GalTextureWrap.MirrorClampToBorder: return (TextureWrapMode)ExtTextureMirrorClamp.MirrorClampToBorderExt;
                    case GalTextureWrap.MirrorClamp:         return (TextureWrapMode)ExtTextureMirrorClamp.MirrorClampExt;
                }
            }
            else
            {
                //Fallback to non-mirrored clamps
                switch (Wrap)
                {
                    case GalTextureWrap.MirrorClampToEdge:   return TextureWrapMode.ClampToEdge;
                    case GalTextureWrap.MirrorClampToBorder: return TextureWrapMode.ClampToBorder;
                    case GalTextureWrap.MirrorClamp:         return TextureWrapMode.Clamp;
                }
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
