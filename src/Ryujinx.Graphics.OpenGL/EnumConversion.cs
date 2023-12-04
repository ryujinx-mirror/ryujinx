using OpenTK.Graphics.OpenGL;
using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Shader;

namespace Ryujinx.Graphics.OpenGL
{
    static class EnumConversion
    {
        public static TextureWrapMode Convert(this AddressMode mode)
        {
            switch (mode)
            {
                case AddressMode.Clamp:
                    return TextureWrapMode.Clamp;
                case AddressMode.Repeat:
                    return TextureWrapMode.Repeat;
                case AddressMode.MirrorClamp:
                    return (TextureWrapMode)ExtTextureMirrorClamp.MirrorClampExt;
                case AddressMode.MirrorClampToEdge:
                    return (TextureWrapMode)ExtTextureMirrorClamp.MirrorClampToEdgeExt;
                case AddressMode.MirrorClampToBorder:
                    return (TextureWrapMode)ExtTextureMirrorClamp.MirrorClampToBorderExt;
                case AddressMode.ClampToBorder:
                    return TextureWrapMode.ClampToBorder;
                case AddressMode.MirroredRepeat:
                    return TextureWrapMode.MirroredRepeat;
                case AddressMode.ClampToEdge:
                    return TextureWrapMode.ClampToEdge;
            }

            Logger.Debug?.Print(LogClass.Gpu, $"Invalid {nameof(AddressMode)} enum value: {mode}.");

            return TextureWrapMode.Clamp;
        }

        public static NvBlendEquationAdvanced Convert(this AdvancedBlendOp op)
        {
            switch (op)
            {
                case AdvancedBlendOp.Zero:
                    return NvBlendEquationAdvanced.Zero;
                case AdvancedBlendOp.Src:
                    return NvBlendEquationAdvanced.SrcNv;
                case AdvancedBlendOp.Dst:
                    return NvBlendEquationAdvanced.DstNv;
                case AdvancedBlendOp.SrcOver:
                    return NvBlendEquationAdvanced.SrcOverNv;
                case AdvancedBlendOp.DstOver:
                    return NvBlendEquationAdvanced.DstOverNv;
                case AdvancedBlendOp.SrcIn:
                    return NvBlendEquationAdvanced.SrcInNv;
                case AdvancedBlendOp.DstIn:
                    return NvBlendEquationAdvanced.DstInNv;
                case AdvancedBlendOp.SrcOut:
                    return NvBlendEquationAdvanced.SrcOutNv;
                case AdvancedBlendOp.DstOut:
                    return NvBlendEquationAdvanced.DstOutNv;
                case AdvancedBlendOp.SrcAtop:
                    return NvBlendEquationAdvanced.SrcAtopNv;
                case AdvancedBlendOp.DstAtop:
                    return NvBlendEquationAdvanced.DstAtopNv;
                case AdvancedBlendOp.Xor:
                    return NvBlendEquationAdvanced.XorNv;
                case AdvancedBlendOp.Plus:
                    return NvBlendEquationAdvanced.PlusNv;
                case AdvancedBlendOp.PlusClamped:
                    return NvBlendEquationAdvanced.PlusClampedNv;
                case AdvancedBlendOp.PlusClampedAlpha:
                    return NvBlendEquationAdvanced.PlusClampedAlphaNv;
                case AdvancedBlendOp.PlusDarker:
                    return NvBlendEquationAdvanced.PlusDarkerNv;
                case AdvancedBlendOp.Multiply:
                    return NvBlendEquationAdvanced.MultiplyNv;
                case AdvancedBlendOp.Screen:
                    return NvBlendEquationAdvanced.ScreenNv;
                case AdvancedBlendOp.Overlay:
                    return NvBlendEquationAdvanced.OverlayNv;
                case AdvancedBlendOp.Darken:
                    return NvBlendEquationAdvanced.DarkenNv;
                case AdvancedBlendOp.Lighten:
                    return NvBlendEquationAdvanced.LightenNv;
                case AdvancedBlendOp.ColorDodge:
                    return NvBlendEquationAdvanced.ColordodgeNv;
                case AdvancedBlendOp.ColorBurn:
                    return NvBlendEquationAdvanced.ColorburnNv;
                case AdvancedBlendOp.HardLight:
                    return NvBlendEquationAdvanced.HardlightNv;
                case AdvancedBlendOp.SoftLight:
                    return NvBlendEquationAdvanced.SoftlightNv;
                case AdvancedBlendOp.Difference:
                    return NvBlendEquationAdvanced.DifferenceNv;
                case AdvancedBlendOp.Minus:
                    return NvBlendEquationAdvanced.MinusNv;
                case AdvancedBlendOp.MinusClamped:
                    return NvBlendEquationAdvanced.MinusClampedNv;
                case AdvancedBlendOp.Exclusion:
                    return NvBlendEquationAdvanced.ExclusionNv;
                case AdvancedBlendOp.Contrast:
                    return NvBlendEquationAdvanced.ContrastNv;
                case AdvancedBlendOp.Invert:
                    return NvBlendEquationAdvanced.Invert;
                case AdvancedBlendOp.InvertRGB:
                    return NvBlendEquationAdvanced.InvertRgbNv;
                case AdvancedBlendOp.InvertOvg:
                    return NvBlendEquationAdvanced.InvertOvgNv;
                case AdvancedBlendOp.LinearDodge:
                    return NvBlendEquationAdvanced.LineardodgeNv;
                case AdvancedBlendOp.LinearBurn:
                    return NvBlendEquationAdvanced.LinearburnNv;
                case AdvancedBlendOp.VividLight:
                    return NvBlendEquationAdvanced.VividlightNv;
                case AdvancedBlendOp.LinearLight:
                    return NvBlendEquationAdvanced.LinearlightNv;
                case AdvancedBlendOp.PinLight:
                    return NvBlendEquationAdvanced.PinlightNv;
                case AdvancedBlendOp.HardMix:
                    return NvBlendEquationAdvanced.HardmixNv;
                case AdvancedBlendOp.Red:
                    return NvBlendEquationAdvanced.RedNv;
                case AdvancedBlendOp.Green:
                    return NvBlendEquationAdvanced.GreenNv;
                case AdvancedBlendOp.Blue:
                    return NvBlendEquationAdvanced.BlueNv;
                case AdvancedBlendOp.HslHue:
                    return NvBlendEquationAdvanced.HslHueNv;
                case AdvancedBlendOp.HslSaturation:
                    return NvBlendEquationAdvanced.HslSaturationNv;
                case AdvancedBlendOp.HslColor:
                    return NvBlendEquationAdvanced.HslColorNv;
                case AdvancedBlendOp.HslLuminosity:
                    return NvBlendEquationAdvanced.HslLuminosityNv;
            }

            Logger.Debug?.Print(LogClass.Gpu, $"Invalid {nameof(AdvancedBlendOp)} enum value: {op}.");

            return NvBlendEquationAdvanced.Zero;
        }

        public static All Convert(this AdvancedBlendOverlap overlap)
        {
            switch (overlap)
            {
                case AdvancedBlendOverlap.Uncorrelated:
                    return All.UncorrelatedNv;
                case AdvancedBlendOverlap.Disjoint:
                    return All.DisjointNv;
                case AdvancedBlendOverlap.Conjoint:
                    return All.ConjointNv;
            }

            Logger.Debug?.Print(LogClass.Gpu, $"Invalid {nameof(AdvancedBlendOverlap)} enum value: {overlap}.");

            return All.UncorrelatedNv;
        }

        public static All Convert(this BlendFactor factor)
        {
            switch (factor)
            {
                case BlendFactor.Zero:
                case BlendFactor.ZeroGl:
                    return All.Zero;
                case BlendFactor.One:
                case BlendFactor.OneGl:
                    return All.One;
                case BlendFactor.SrcColor:
                case BlendFactor.SrcColorGl:
                    return All.SrcColor;
                case BlendFactor.OneMinusSrcColor:
                case BlendFactor.OneMinusSrcColorGl:
                    return All.OneMinusSrcColor;
                case BlendFactor.SrcAlpha:
                case BlendFactor.SrcAlphaGl:
                    return All.SrcAlpha;
                case BlendFactor.OneMinusSrcAlpha:
                case BlendFactor.OneMinusSrcAlphaGl:
                    return All.OneMinusSrcAlpha;
                case BlendFactor.DstAlpha:
                case BlendFactor.DstAlphaGl:
                    return All.DstAlpha;
                case BlendFactor.OneMinusDstAlpha:
                case BlendFactor.OneMinusDstAlphaGl:
                    return All.OneMinusDstAlpha;
                case BlendFactor.DstColor:
                case BlendFactor.DstColorGl:
                    return All.DstColor;
                case BlendFactor.OneMinusDstColor:
                case BlendFactor.OneMinusDstColorGl:
                    return All.OneMinusDstColor;
                case BlendFactor.SrcAlphaSaturate:
                case BlendFactor.SrcAlphaSaturateGl:
                    return All.SrcAlphaSaturate;
                case BlendFactor.Src1Color:
                case BlendFactor.Src1ColorGl:
                    return All.Src1Color;
                case BlendFactor.OneMinusSrc1Color:
                case BlendFactor.OneMinusSrc1ColorGl:
                    return All.OneMinusSrc1Color;
                case BlendFactor.Src1Alpha:
                case BlendFactor.Src1AlphaGl:
                    return All.Src1Alpha;
                case BlendFactor.OneMinusSrc1Alpha:
                case BlendFactor.OneMinusSrc1AlphaGl:
                    return All.OneMinusSrc1Alpha;
                case BlendFactor.ConstantColor:
                    return All.ConstantColor;
                case BlendFactor.OneMinusConstantColor:
                    return All.OneMinusConstantColor;
                case BlendFactor.ConstantAlpha:
                    return All.ConstantAlpha;
                case BlendFactor.OneMinusConstantAlpha:
                    return All.OneMinusConstantAlpha;
            }

            Logger.Debug?.Print(LogClass.Gpu, $"Invalid {nameof(BlendFactor)} enum value: {factor}.");

            return All.Zero;
        }

        public static BlendEquationMode Convert(this BlendOp op)
        {
            switch (op)
            {
                case BlendOp.Add:
                case BlendOp.AddGl:
                    return BlendEquationMode.FuncAdd;
                case BlendOp.Minimum:
                case BlendOp.MinimumGl:
                    return BlendEquationMode.Min;
                case BlendOp.Maximum:
                case BlendOp.MaximumGl:
                    return BlendEquationMode.Max;
                case BlendOp.Subtract:
                case BlendOp.SubtractGl:
                    return BlendEquationMode.FuncSubtract;
                case BlendOp.ReverseSubtract:
                case BlendOp.ReverseSubtractGl:
                    return BlendEquationMode.FuncReverseSubtract;
            }

            Logger.Debug?.Print(LogClass.Gpu, $"Invalid {nameof(BlendOp)} enum value: {op}.");

            return BlendEquationMode.FuncAdd;
        }

        public static TextureCompareMode Convert(this CompareMode mode)
        {
            switch (mode)
            {
                case CompareMode.None:
                    return TextureCompareMode.None;
                case CompareMode.CompareRToTexture:
                    return TextureCompareMode.CompareRToTexture;
            }

            Logger.Debug?.Print(LogClass.Gpu, $"Invalid {nameof(CompareMode)} enum value: {mode}.");

            return TextureCompareMode.None;
        }

        public static All Convert(this CompareOp op)
        {
            switch (op)
            {
                case CompareOp.Never:
                case CompareOp.NeverGl:
                    return All.Never;
                case CompareOp.Less:
                case CompareOp.LessGl:
                    return All.Less;
                case CompareOp.Equal:
                case CompareOp.EqualGl:
                    return All.Equal;
                case CompareOp.LessOrEqual:
                case CompareOp.LessOrEqualGl:
                    return All.Lequal;
                case CompareOp.Greater:
                case CompareOp.GreaterGl:
                    return All.Greater;
                case CompareOp.NotEqual:
                case CompareOp.NotEqualGl:
                    return All.Notequal;
                case CompareOp.GreaterOrEqual:
                case CompareOp.GreaterOrEqualGl:
                    return All.Gequal;
                case CompareOp.Always:
                case CompareOp.AlwaysGl:
                    return All.Always;
            }

            Logger.Debug?.Print(LogClass.Gpu, $"Invalid {nameof(CompareOp)} enum value: {op}.");

            return All.Never;
        }

        public static ClipDepthMode Convert(this DepthMode mode)
        {
            switch (mode)
            {
                case DepthMode.MinusOneToOne:
                    return ClipDepthMode.NegativeOneToOne;
                case DepthMode.ZeroToOne:
                    return ClipDepthMode.ZeroToOne;
            }

            Logger.Debug?.Print(LogClass.Gpu, $"Invalid {nameof(DepthMode)} enum value: {mode}.");

            return ClipDepthMode.NegativeOneToOne;
        }

        public static All Convert(this DepthStencilMode mode)
        {
            switch (mode)
            {
                case DepthStencilMode.Depth:
                    return All.DepthComponent;
                case DepthStencilMode.Stencil:
                    return All.StencilIndex;
            }

            Logger.Debug?.Print(LogClass.Gpu, $"Invalid {nameof(DepthStencilMode)} enum value: {mode}.");

            return All.Depth;
        }

        public static CullFaceMode Convert(this Face face)
        {
            switch (face)
            {
                case Face.Back:
                    return CullFaceMode.Back;
                case Face.Front:
                    return CullFaceMode.Front;
                case Face.FrontAndBack:
                    return CullFaceMode.FrontAndBack;
            }

            Logger.Debug?.Print(LogClass.Gpu, $"Invalid {nameof(Face)} enum value: {face}.");

            return CullFaceMode.Back;
        }

        public static FrontFaceDirection Convert(this FrontFace frontFace)
        {
            switch (frontFace)
            {
                case FrontFace.Clockwise:
                    return FrontFaceDirection.Cw;
                case FrontFace.CounterClockwise:
                    return FrontFaceDirection.Ccw;
            }

            Logger.Debug?.Print(LogClass.Gpu, $"Invalid {nameof(FrontFace)} enum value: {frontFace}.");

            return FrontFaceDirection.Cw;
        }

        public static DrawElementsType Convert(this IndexType type)
        {
            switch (type)
            {
                case IndexType.UByte:
                    return DrawElementsType.UnsignedByte;
                case IndexType.UShort:
                    return DrawElementsType.UnsignedShort;
                case IndexType.UInt:
                    return DrawElementsType.UnsignedInt;
            }

            Logger.Debug?.Print(LogClass.Gpu, $"Invalid {nameof(IndexType)} enum value: {type}.");

            return DrawElementsType.UnsignedByte;
        }

        public static TextureMagFilter Convert(this MagFilter filter)
        {
            switch (filter)
            {
                case MagFilter.Nearest:
                    return TextureMagFilter.Nearest;
                case MagFilter.Linear:
                    return TextureMagFilter.Linear;
            }

            Logger.Debug?.Print(LogClass.Gpu, $"Invalid {nameof(MagFilter)} enum value: {filter}.");

            return TextureMagFilter.Nearest;
        }

        public static TextureMinFilter Convert(this MinFilter filter)
        {
            switch (filter)
            {
                case MinFilter.Nearest:
                    return TextureMinFilter.Nearest;
                case MinFilter.Linear:
                    return TextureMinFilter.Linear;
                case MinFilter.NearestMipmapNearest:
                    return TextureMinFilter.NearestMipmapNearest;
                case MinFilter.LinearMipmapNearest:
                    return TextureMinFilter.LinearMipmapNearest;
                case MinFilter.NearestMipmapLinear:
                    return TextureMinFilter.NearestMipmapLinear;
                case MinFilter.LinearMipmapLinear:
                    return TextureMinFilter.LinearMipmapLinear;
            }

            Logger.Debug?.Print(LogClass.Gpu, $"Invalid {nameof(MinFilter)} enum value: {filter}.");

            return TextureMinFilter.Nearest;
        }

        public static OpenTK.Graphics.OpenGL.PolygonMode Convert(this GAL.PolygonMode mode)
        {
            switch (mode)
            {
                case GAL.PolygonMode.Point:
                    return OpenTK.Graphics.OpenGL.PolygonMode.Point;
                case GAL.PolygonMode.Line:
                    return OpenTK.Graphics.OpenGL.PolygonMode.Line;
                case GAL.PolygonMode.Fill:
                    return OpenTK.Graphics.OpenGL.PolygonMode.Fill;
            }

            Logger.Debug?.Print(LogClass.Gpu, $"Invalid {nameof(GAL.PolygonMode)} enum value: {mode}.");

            return OpenTK.Graphics.OpenGL.PolygonMode.Fill;
        }

        public static PrimitiveType Convert(this PrimitiveTopology topology)
        {
            switch (topology)
            {
                case PrimitiveTopology.Points:
                    return PrimitiveType.Points;
                case PrimitiveTopology.Lines:
                    return PrimitiveType.Lines;
                case PrimitiveTopology.LineLoop:
                    return PrimitiveType.LineLoop;
                case PrimitiveTopology.LineStrip:
                    return PrimitiveType.LineStrip;
                case PrimitiveTopology.Triangles:
                    return PrimitiveType.Triangles;
                case PrimitiveTopology.TriangleStrip:
                    return PrimitiveType.TriangleStrip;
                case PrimitiveTopology.TriangleFan:
                    return PrimitiveType.TriangleFan;
                case PrimitiveTopology.Quads:
                    return PrimitiveType.Quads;
                case PrimitiveTopology.QuadStrip:
                    return PrimitiveType.QuadStrip;
                case PrimitiveTopology.Polygon:
                    return PrimitiveType.TriangleFan;
                case PrimitiveTopology.LinesAdjacency:
                    return PrimitiveType.LinesAdjacency;
                case PrimitiveTopology.LineStripAdjacency:
                    return PrimitiveType.LineStripAdjacency;
                case PrimitiveTopology.TrianglesAdjacency:
                    return PrimitiveType.TrianglesAdjacency;
                case PrimitiveTopology.TriangleStripAdjacency:
                    return PrimitiveType.TriangleStripAdjacency;
                case PrimitiveTopology.Patches:
                    return PrimitiveType.Patches;
            }

            Logger.Debug?.Print(LogClass.Gpu, $"Invalid {nameof(PrimitiveTopology)} enum value: {topology}.");

            return PrimitiveType.Points;
        }

        public static TransformFeedbackPrimitiveType ConvertToTfType(this PrimitiveTopology topology)
        {
            switch (topology)
            {
                case PrimitiveTopology.Points:
                    return TransformFeedbackPrimitiveType.Points;
                case PrimitiveTopology.Lines:
                case PrimitiveTopology.LineLoop:
                case PrimitiveTopology.LineStrip:
                case PrimitiveTopology.LinesAdjacency:
                case PrimitiveTopology.LineStripAdjacency:
                    return TransformFeedbackPrimitiveType.Lines;
                case PrimitiveTopology.Triangles:
                case PrimitiveTopology.TriangleStrip:
                case PrimitiveTopology.TriangleFan:
                case PrimitiveTopology.TrianglesAdjacency:
                case PrimitiveTopology.TriangleStripAdjacency:
                    return TransformFeedbackPrimitiveType.Triangles;
            }

            Logger.Debug?.Print(LogClass.Gpu, $"Invalid {nameof(PrimitiveTopology)} enum value: {topology}.");

            return TransformFeedbackPrimitiveType.Points;
        }

        public static OpenTK.Graphics.OpenGL.StencilOp Convert(this GAL.StencilOp op)
        {
            switch (op)
            {
                case GAL.StencilOp.Keep:
                case GAL.StencilOp.KeepGl:
                    return OpenTK.Graphics.OpenGL.StencilOp.Keep;
                case GAL.StencilOp.Zero:
                case GAL.StencilOp.ZeroGl:
                    return OpenTK.Graphics.OpenGL.StencilOp.Zero;
                case GAL.StencilOp.Replace:
                case GAL.StencilOp.ReplaceGl:
                    return OpenTK.Graphics.OpenGL.StencilOp.Replace;
                case GAL.StencilOp.IncrementAndClamp:
                case GAL.StencilOp.IncrementAndClampGl:
                    return OpenTK.Graphics.OpenGL.StencilOp.Incr;
                case GAL.StencilOp.DecrementAndClamp:
                case GAL.StencilOp.DecrementAndClampGl:
                    return OpenTK.Graphics.OpenGL.StencilOp.Decr;
                case GAL.StencilOp.Invert:
                case GAL.StencilOp.InvertGl:
                    return OpenTK.Graphics.OpenGL.StencilOp.Invert;
                case GAL.StencilOp.IncrementAndWrap:
                case GAL.StencilOp.IncrementAndWrapGl:
                    return OpenTK.Graphics.OpenGL.StencilOp.IncrWrap;
                case GAL.StencilOp.DecrementAndWrap:
                case GAL.StencilOp.DecrementAndWrapGl:
                    return OpenTK.Graphics.OpenGL.StencilOp.DecrWrap;
            }

            Logger.Debug?.Print(LogClass.Gpu, $"Invalid {nameof(GAL.StencilOp)} enum value: {op}.");

            return OpenTK.Graphics.OpenGL.StencilOp.Keep;
        }

        public static All Convert(this SwizzleComponent swizzleComponent)
        {
            switch (swizzleComponent)
            {
                case SwizzleComponent.Zero:
                    return All.Zero;
                case SwizzleComponent.One:
                    return All.One;
                case SwizzleComponent.Red:
                    return All.Red;
                case SwizzleComponent.Green:
                    return All.Green;
                case SwizzleComponent.Blue:
                    return All.Blue;
                case SwizzleComponent.Alpha:
                    return All.Alpha;
            }

            Logger.Debug?.Print(LogClass.Gpu, $"Invalid {nameof(SwizzleComponent)} enum value: {swizzleComponent}.");

            return All.Zero;
        }

        public static ImageTarget ConvertToImageTarget(this Target target)
        {
            return (ImageTarget)target.Convert();
        }

        public static TextureTarget Convert(this Target target)
        {
            switch (target)
            {
                case Target.Texture1D:
                    return TextureTarget.Texture1D;
                case Target.Texture2D:
                    return TextureTarget.Texture2D;
                case Target.Texture3D:
                    return TextureTarget.Texture3D;
                case Target.Texture1DArray:
                    return TextureTarget.Texture1DArray;
                case Target.Texture2DArray:
                    return TextureTarget.Texture2DArray;
                case Target.Texture2DMultisample:
                    return TextureTarget.Texture2DMultisample;
                case Target.Texture2DMultisampleArray:
                    return TextureTarget.Texture2DMultisampleArray;
                case Target.Cubemap:
                    return TextureTarget.TextureCubeMap;
                case Target.CubemapArray:
                    return TextureTarget.TextureCubeMapArray;
                case Target.TextureBuffer:
                    return TextureTarget.TextureBuffer;
            }

            Logger.Debug?.Print(LogClass.Gpu, $"Invalid {nameof(Target)} enum value: {target}.");

            return TextureTarget.Texture2D;
        }

        public static NvViewportSwizzle Convert(this ViewportSwizzle swizzle)
        {
            switch (swizzle)
            {
                case ViewportSwizzle.PositiveX:
                    return NvViewportSwizzle.ViewportSwizzlePositiveXNv;
                case ViewportSwizzle.PositiveY:
                    return NvViewportSwizzle.ViewportSwizzlePositiveYNv;
                case ViewportSwizzle.PositiveZ:
                    return NvViewportSwizzle.ViewportSwizzlePositiveZNv;
                case ViewportSwizzle.PositiveW:
                    return NvViewportSwizzle.ViewportSwizzlePositiveWNv;
                case ViewportSwizzle.NegativeX:
                    return NvViewportSwizzle.ViewportSwizzleNegativeXNv;
                case ViewportSwizzle.NegativeY:
                    return NvViewportSwizzle.ViewportSwizzleNegativeYNv;
                case ViewportSwizzle.NegativeZ:
                    return NvViewportSwizzle.ViewportSwizzleNegativeZNv;
                case ViewportSwizzle.NegativeW:
                    return NvViewportSwizzle.ViewportSwizzleNegativeWNv;
            }

            Logger.Debug?.Print(LogClass.Gpu, $"Invalid {nameof(ViewportSwizzle)} enum value: {swizzle}.");

            return NvViewportSwizzle.ViewportSwizzlePositiveXNv;
        }

        public static All Convert(this LogicalOp op)
        {
            switch (op)
            {
                case LogicalOp.Clear:
                    return All.Clear;
                case LogicalOp.And:
                    return All.And;
                case LogicalOp.AndReverse:
                    return All.AndReverse;
                case LogicalOp.Copy:
                    return All.Copy;
                case LogicalOp.AndInverted:
                    return All.AndInverted;
                case LogicalOp.Noop:
                    return All.Noop;
                case LogicalOp.Xor:
                    return All.Xor;
                case LogicalOp.Or:
                    return All.Or;
                case LogicalOp.Nor:
                    return All.Nor;
                case LogicalOp.Equiv:
                    return All.Equiv;
                case LogicalOp.Invert:
                    return All.Invert;
                case LogicalOp.OrReverse:
                    return All.OrReverse;
                case LogicalOp.CopyInverted:
                    return All.CopyInverted;
                case LogicalOp.OrInverted:
                    return All.OrInverted;
                case LogicalOp.Nand:
                    return All.Nand;
                case LogicalOp.Set:
                    return All.Set;
            }

            Logger.Debug?.Print(LogClass.Gpu, $"Invalid {nameof(LogicalOp)} enum value: {op}.");

            return All.Never;
        }

        public static ShaderType Convert(this ShaderStage stage)
        {
            return stage switch
            {
                ShaderStage.Compute => ShaderType.ComputeShader,
                ShaderStage.Vertex => ShaderType.VertexShader,
                ShaderStage.TessellationControl => ShaderType.TessControlShader,
                ShaderStage.TessellationEvaluation => ShaderType.TessEvaluationShader,
                ShaderStage.Geometry => ShaderType.GeometryShader,
                ShaderStage.Fragment => ShaderType.FragmentShader,
                _ => ShaderType.VertexShader,
            };
        }
    }
}
