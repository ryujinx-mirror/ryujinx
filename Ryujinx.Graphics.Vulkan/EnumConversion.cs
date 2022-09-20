using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Shader;
using Silk.NET.Vulkan;
using System;

namespace Ryujinx.Graphics.Vulkan
{
    static class EnumConversion
    {
        public static ShaderStageFlags Convert(this ShaderStage stage)
        {
            return stage switch
            {
                ShaderStage.Vertex => ShaderStageFlags.ShaderStageVertexBit,
                ShaderStage.Geometry => ShaderStageFlags.ShaderStageGeometryBit,
                ShaderStage.TessellationControl => ShaderStageFlags.ShaderStageTessellationControlBit,
                ShaderStage.TessellationEvaluation => ShaderStageFlags.ShaderStageTessellationEvaluationBit,
                ShaderStage.Fragment => ShaderStageFlags.ShaderStageFragmentBit,
                ShaderStage.Compute => ShaderStageFlags.ShaderStageComputeBit,
                _ => LogInvalidAndReturn(stage, nameof(ShaderStage), (ShaderStageFlags)0)
            };
        }

        public static PipelineStageFlags ConvertToPipelineStageFlags(this ShaderStage stage)
        {
            return stage switch
            {
                ShaderStage.Vertex => PipelineStageFlags.PipelineStageVertexShaderBit,
                ShaderStage.Geometry => PipelineStageFlags.PipelineStageGeometryShaderBit,
                ShaderStage.TessellationControl => PipelineStageFlags.PipelineStageTessellationControlShaderBit,
                ShaderStage.TessellationEvaluation => PipelineStageFlags.PipelineStageTessellationEvaluationShaderBit,
                ShaderStage.Fragment => PipelineStageFlags.PipelineStageFragmentShaderBit,
                ShaderStage.Compute => PipelineStageFlags.PipelineStageComputeShaderBit,
                _ => LogInvalidAndReturn(stage, nameof(ShaderStage), (PipelineStageFlags)0)
            };
        }

        public static SamplerAddressMode Convert(this AddressMode mode)
        {
            return mode switch
            {
                AddressMode.Clamp => SamplerAddressMode.ClampToEdge, // TODO: Should be clamp.
                AddressMode.Repeat => SamplerAddressMode.Repeat,
                AddressMode.MirrorClamp => SamplerAddressMode.ClampToEdge, // TODO: Should be mirror clamp.
                AddressMode.MirrorClampToEdge => SamplerAddressMode.MirrorClampToEdgeKhr,
                AddressMode.MirrorClampToBorder => SamplerAddressMode.ClampToBorder, // TODO: Should be mirror clamp to border.
                AddressMode.ClampToBorder => SamplerAddressMode.ClampToBorder,
                AddressMode.MirroredRepeat => SamplerAddressMode.MirroredRepeat,
                AddressMode.ClampToEdge => SamplerAddressMode.ClampToEdge,
                _ => LogInvalidAndReturn(mode, nameof(AddressMode), SamplerAddressMode.ClampToEdge)  // TODO: Should be clamp.
            };
        }

        public static Silk.NET.Vulkan.BlendFactor Convert(this GAL.BlendFactor factor)
        {
            return factor switch
            {
                GAL.BlendFactor.Zero or GAL.BlendFactor.ZeroGl => Silk.NET.Vulkan.BlendFactor.Zero,
                GAL.BlendFactor.One or GAL.BlendFactor.OneGl => Silk.NET.Vulkan.BlendFactor.One,
                GAL.BlendFactor.SrcColor or GAL.BlendFactor.SrcColorGl => Silk.NET.Vulkan.BlendFactor.SrcColor,
                GAL.BlendFactor.OneMinusSrcColor or GAL.BlendFactor.OneMinusSrcColorGl => Silk.NET.Vulkan.BlendFactor.OneMinusSrcColor,
                GAL.BlendFactor.SrcAlpha or GAL.BlendFactor.SrcAlphaGl => Silk.NET.Vulkan.BlendFactor.SrcAlpha,
                GAL.BlendFactor.OneMinusSrcAlpha or GAL.BlendFactor.OneMinusSrcAlphaGl => Silk.NET.Vulkan.BlendFactor.OneMinusSrcAlpha,
                GAL.BlendFactor.DstAlpha or GAL.BlendFactor.DstAlphaGl => Silk.NET.Vulkan.BlendFactor.DstAlpha,
                GAL.BlendFactor.OneMinusDstAlpha or GAL.BlendFactor.OneMinusDstAlphaGl => Silk.NET.Vulkan.BlendFactor.OneMinusDstAlpha,
                GAL.BlendFactor.DstColor or GAL.BlendFactor.DstColorGl => Silk.NET.Vulkan.BlendFactor.DstColor,
                GAL.BlendFactor.OneMinusDstColor or GAL.BlendFactor.OneMinusDstColorGl => Silk.NET.Vulkan.BlendFactor.OneMinusDstColor,
                GAL.BlendFactor.SrcAlphaSaturate or GAL.BlendFactor.SrcAlphaSaturateGl => Silk.NET.Vulkan.BlendFactor.SrcAlphaSaturate,
                GAL.BlendFactor.Src1Color or GAL.BlendFactor.Src1ColorGl => Silk.NET.Vulkan.BlendFactor.Src1Color,
                GAL.BlendFactor.OneMinusSrc1Color or GAL.BlendFactor.OneMinusSrc1ColorGl => Silk.NET.Vulkan.BlendFactor.OneMinusSrc1Color,
                GAL.BlendFactor.Src1Alpha or GAL.BlendFactor.Src1AlphaGl => Silk.NET.Vulkan.BlendFactor.Src1Alpha,
                GAL.BlendFactor.OneMinusSrc1Alpha or GAL.BlendFactor.OneMinusSrc1AlphaGl => Silk.NET.Vulkan.BlendFactor.OneMinusSrc1Alpha,
                GAL.BlendFactor.ConstantColor => Silk.NET.Vulkan.BlendFactor.ConstantColor,
                GAL.BlendFactor.OneMinusConstantColor => Silk.NET.Vulkan.BlendFactor.OneMinusConstantColor,
                GAL.BlendFactor.ConstantAlpha => Silk.NET.Vulkan.BlendFactor.ConstantAlpha,
                GAL.BlendFactor.OneMinusConstantAlpha => Silk.NET.Vulkan.BlendFactor.OneMinusConstantAlpha,
                _ => LogInvalidAndReturn(factor, nameof(GAL.BlendFactor), Silk.NET.Vulkan.BlendFactor.Zero)
            };
        }

        public static Silk.NET.Vulkan.BlendOp Convert(this GAL.BlendOp op)
        {
            return op switch
            {
                GAL.BlendOp.Add or GAL.BlendOp.AddGl => Silk.NET.Vulkan.BlendOp.Add,
                GAL.BlendOp.Subtract or GAL.BlendOp.SubtractGl => Silk.NET.Vulkan.BlendOp.Subtract,
                GAL.BlendOp.ReverseSubtract or GAL.BlendOp.ReverseSubtractGl => Silk.NET.Vulkan.BlendOp.ReverseSubtract,
                GAL.BlendOp.Minimum or GAL.BlendOp.MinimumGl => Silk.NET.Vulkan.BlendOp.Min,
                GAL.BlendOp.Maximum or GAL.BlendOp.MaximumGl => Silk.NET.Vulkan.BlendOp.Max,
                _ => LogInvalidAndReturn(op, nameof(GAL.BlendOp), Silk.NET.Vulkan.BlendOp.Add)
            };
        }

        public static Silk.NET.Vulkan.CompareOp Convert(this GAL.CompareOp op)
        {
            return op switch
            {
                GAL.CompareOp.Never or GAL.CompareOp.NeverGl => Silk.NET.Vulkan.CompareOp.Never,
                GAL.CompareOp.Less or GAL.CompareOp.LessGl => Silk.NET.Vulkan.CompareOp.Less,
                GAL.CompareOp.Equal or GAL.CompareOp.EqualGl => Silk.NET.Vulkan.CompareOp.Equal,
                GAL.CompareOp.LessOrEqual or GAL.CompareOp.LessOrEqualGl => Silk.NET.Vulkan.CompareOp.LessOrEqual,
                GAL.CompareOp.Greater or GAL.CompareOp.GreaterGl => Silk.NET.Vulkan.CompareOp.Greater,
                GAL.CompareOp.NotEqual or GAL.CompareOp.NotEqualGl => Silk.NET.Vulkan.CompareOp.NotEqual,
                GAL.CompareOp.GreaterOrEqual or GAL.CompareOp.GreaterOrEqualGl => Silk.NET.Vulkan.CompareOp.GreaterOrEqual,
                GAL.CompareOp.Always or GAL.CompareOp.AlwaysGl => Silk.NET.Vulkan.CompareOp.Always,
                _ => LogInvalidAndReturn(op, nameof(GAL.CompareOp), Silk.NET.Vulkan.CompareOp.Never)
            };
        }

        public static CullModeFlags Convert(this Face face)
        {
            return face switch
            {
                Face.Back => CullModeFlags.CullModeBackBit,
                Face.Front => CullModeFlags.CullModeFrontBit,
                Face.FrontAndBack => CullModeFlags.CullModeFrontAndBack,
                _ => LogInvalidAndReturn(face, nameof(Face), CullModeFlags.CullModeBackBit)
            };
        }

        public static Silk.NET.Vulkan.FrontFace Convert(this GAL.FrontFace frontFace)
        {
            // Flipped to account for origin differences.
            return frontFace switch
            {
                GAL.FrontFace.Clockwise => Silk.NET.Vulkan.FrontFace.CounterClockwise,
                GAL.FrontFace.CounterClockwise => Silk.NET.Vulkan.FrontFace.Clockwise,
                _ => LogInvalidAndReturn(frontFace, nameof(GAL.FrontFace), Silk.NET.Vulkan.FrontFace.Clockwise)
            };
        }

        public static Silk.NET.Vulkan.IndexType Convert(this GAL.IndexType type)
        {
            return type switch
            {
                GAL.IndexType.UByte => Silk.NET.Vulkan.IndexType.Uint8Ext,
                GAL.IndexType.UShort => Silk.NET.Vulkan.IndexType.Uint16,
                GAL.IndexType.UInt => Silk.NET.Vulkan.IndexType.Uint32,
                _ => LogInvalidAndReturn(type, nameof(GAL.IndexType), Silk.NET.Vulkan.IndexType.Uint16)
            };
        }

        public static Filter Convert(this MagFilter filter)
        {
            return filter switch
            {
                MagFilter.Nearest => Filter.Nearest,
                MagFilter.Linear => Filter.Linear,
                _ => LogInvalidAndReturn(filter, nameof(MagFilter), Filter.Nearest)
            };
        }

        public static (Filter, SamplerMipmapMode) Convert(this MinFilter filter)
        {
            return filter switch
            {
                MinFilter.Nearest => (Filter.Nearest, SamplerMipmapMode.Nearest),
                MinFilter.Linear => (Filter.Linear, SamplerMipmapMode.Nearest),
                MinFilter.NearestMipmapNearest => (Filter.Nearest, SamplerMipmapMode.Nearest),
                MinFilter.LinearMipmapNearest => (Filter.Linear, SamplerMipmapMode.Nearest),
                MinFilter.NearestMipmapLinear => (Filter.Nearest, SamplerMipmapMode.Linear),
                MinFilter.LinearMipmapLinear => (Filter.Linear, SamplerMipmapMode.Linear),
                _ => LogInvalidAndReturn(filter, nameof(MinFilter), (Filter.Nearest, SamplerMipmapMode.Nearest))
            };
        }

        public static Silk.NET.Vulkan.PrimitiveTopology Convert(this GAL.PrimitiveTopology topology)
        {
            return topology switch
            {
                GAL.PrimitiveTopology.Points => Silk.NET.Vulkan.PrimitiveTopology.PointList,
                GAL.PrimitiveTopology.Lines => Silk.NET.Vulkan.PrimitiveTopology.LineList,
                GAL.PrimitiveTopology.LineStrip => Silk.NET.Vulkan.PrimitiveTopology.LineStrip,
                GAL.PrimitiveTopology.Triangles => Silk.NET.Vulkan.PrimitiveTopology.TriangleList,
                GAL.PrimitiveTopology.TriangleStrip => Silk.NET.Vulkan.PrimitiveTopology.TriangleStrip,
                GAL.PrimitiveTopology.TriangleFan => Silk.NET.Vulkan.PrimitiveTopology.TriangleFan,
                GAL.PrimitiveTopology.LinesAdjacency => Silk.NET.Vulkan.PrimitiveTopology.LineListWithAdjacency,
                GAL.PrimitiveTopology.LineStripAdjacency => Silk.NET.Vulkan.PrimitiveTopology.LineStripWithAdjacency,
                GAL.PrimitiveTopology.TrianglesAdjacency => Silk.NET.Vulkan.PrimitiveTopology.TriangleListWithAdjacency,
                GAL.PrimitiveTopology.TriangleStripAdjacency => Silk.NET.Vulkan.PrimitiveTopology.TriangleStripWithAdjacency,
                GAL.PrimitiveTopology.Patches => Silk.NET.Vulkan.PrimitiveTopology.PatchList,
                GAL.PrimitiveTopology.Quads => throw new NotSupportedException("Quad topology is not available in Vulkan."),
                GAL.PrimitiveTopology.QuadStrip => throw new NotSupportedException("QuadStrip topology is not available in Vulkan."),
                _ => LogInvalidAndReturn(topology, nameof(GAL.PrimitiveTopology), Silk.NET.Vulkan.PrimitiveTopology.TriangleList)
            };
        }

        public static Silk.NET.Vulkan.StencilOp Convert(this GAL.StencilOp op)
        {
            return op switch
            {
                GAL.StencilOp.Keep or GAL.StencilOp.KeepGl => Silk.NET.Vulkan.StencilOp.Keep,
                GAL.StencilOp.Zero or GAL.StencilOp.ZeroGl => Silk.NET.Vulkan.StencilOp.Zero,
                GAL.StencilOp.Replace or GAL.StencilOp.ReplaceGl => Silk.NET.Vulkan.StencilOp.Replace,
                GAL.StencilOp.IncrementAndClamp or GAL.StencilOp.IncrementAndClampGl => Silk.NET.Vulkan.StencilOp.IncrementAndClamp,
                GAL.StencilOp.DecrementAndClamp or GAL.StencilOp.DecrementAndClampGl => Silk.NET.Vulkan.StencilOp.DecrementAndClamp,
                GAL.StencilOp.Invert or GAL.StencilOp.InvertGl => Silk.NET.Vulkan.StencilOp.Invert,
                GAL.StencilOp.IncrementAndWrap or GAL.StencilOp.IncrementAndWrapGl => Silk.NET.Vulkan.StencilOp.IncrementAndWrap,
                GAL.StencilOp.DecrementAndWrap or GAL.StencilOp.DecrementAndWrapGl => Silk.NET.Vulkan.StencilOp.DecrementAndWrap,
                _ => LogInvalidAndReturn(op, nameof(GAL.StencilOp), Silk.NET.Vulkan.StencilOp.Keep)
            };
        }

        public static ComponentSwizzle Convert(this SwizzleComponent swizzleComponent)
        {
            return swizzleComponent switch
            {
                SwizzleComponent.Zero => ComponentSwizzle.Zero,
                SwizzleComponent.One => ComponentSwizzle.One,
                SwizzleComponent.Red => ComponentSwizzle.R,
                SwizzleComponent.Green => ComponentSwizzle.G,
                SwizzleComponent.Blue => ComponentSwizzle.B,
                SwizzleComponent.Alpha => ComponentSwizzle.A,
                _ => LogInvalidAndReturn(swizzleComponent, nameof(SwizzleComponent), ComponentSwizzle.Zero)
            };
        }

        public static ImageType Convert(this Target target)
        {
            return target switch
            {
                Target.Texture1D or
                Target.Texture1DArray or
                Target.TextureBuffer => ImageType.ImageType1D,
                Target.Texture2D or
                Target.Texture2DArray or
                Target.Texture2DMultisample or
                Target.Cubemap or
                Target.CubemapArray => ImageType.ImageType2D,
                Target.Texture3D => ImageType.ImageType3D,
                _ => LogInvalidAndReturn(target, nameof(Target), ImageType.ImageType2D)
            };
        }

        public static ImageViewType ConvertView(this Target target)
        {
            return target switch
            {
                Target.Texture1D => ImageViewType.ImageViewType1D,
                Target.Texture2D or Target.Texture2DMultisample => ImageViewType.ImageViewType2D,
                Target.Texture3D => ImageViewType.ImageViewType3D,
                Target.Texture1DArray => ImageViewType.ImageViewType1DArray,
                Target.Texture2DArray => ImageViewType.ImageViewType2DArray,
                Target.Cubemap => ImageViewType.Cube,
                Target.CubemapArray => ImageViewType.CubeArray,
                _ => LogInvalidAndReturn(target, nameof(Target), ImageViewType.ImageViewType2D)
            };
        }

        public static ImageAspectFlags ConvertAspectFlags(this GAL.Format format)
        {
            return format switch
            {
                GAL.Format.D16Unorm or GAL.Format.D32Float => ImageAspectFlags.ImageAspectDepthBit,
                GAL.Format.S8Uint => ImageAspectFlags.ImageAspectStencilBit,
                GAL.Format.D24UnormS8Uint or
                GAL.Format.D32FloatS8Uint or
                GAL.Format.S8UintD24Unorm    => ImageAspectFlags.ImageAspectDepthBit | ImageAspectFlags.ImageAspectStencilBit,
                _ => ImageAspectFlags.ImageAspectColorBit
            };
        }

        public static ImageAspectFlags ConvertAspectFlags(this GAL.Format format, DepthStencilMode depthStencilMode)
        {
            return format switch
            {
                GAL.Format.D16Unorm or GAL.Format.D32Float => ImageAspectFlags.ImageAspectDepthBit,
                GAL.Format.S8Uint => ImageAspectFlags.ImageAspectStencilBit,
                GAL.Format.D24UnormS8Uint or
                GAL.Format.D32FloatS8Uint or
                GAL.Format.S8UintD24Unorm => depthStencilMode == DepthStencilMode.Stencil ? ImageAspectFlags.ImageAspectStencilBit : ImageAspectFlags.ImageAspectDepthBit,
                _ => ImageAspectFlags.ImageAspectColorBit
            };
        }

        public static LogicOp Convert(this LogicalOp op)
        {
            return op switch
            {
                LogicalOp.Clear => LogicOp.Clear,
                LogicalOp.And => LogicOp.And,
                LogicalOp.AndReverse => LogicOp.AndReverse,
                LogicalOp.Copy => LogicOp.Copy,
                LogicalOp.AndInverted => LogicOp.AndInverted,
                LogicalOp.Noop => LogicOp.NoOp,
                LogicalOp.Xor => LogicOp.Xor,
                LogicalOp.Or => LogicOp.Or,
                LogicalOp.Nor => LogicOp.Nor,
                LogicalOp.Equiv => LogicOp.Equivalent,
                LogicalOp.Invert => LogicOp.Invert,
                LogicalOp.OrReverse => LogicOp.OrReverse,
                LogicalOp.CopyInverted => LogicOp.CopyInverted,
                LogicalOp.OrInverted => LogicOp.OrInverted,
                LogicalOp.Nand => LogicOp.Nand,
                LogicalOp.Set => LogicOp.Set,
                _ => LogInvalidAndReturn(op, nameof(LogicalOp), LogicOp.Copy)
            };
        }

        private static T2 LogInvalidAndReturn<T1, T2>(T1 value, string name, T2 defaultValue = default)
        {
            Logger.Debug?.Print(LogClass.Gpu, $"Invalid {name} enum value: {value}.");

            return defaultValue;
        }
    }
}
