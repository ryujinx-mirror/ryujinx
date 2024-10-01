using Ryujinx.Common.Memory;

namespace Ryujinx.Graphics.Shader
{
    /// <summary>
    /// GPU graphics state that the shader depends on.
    /// </summary>
    public readonly struct GpuGraphicsState
    {
        /// <summary>
        /// Early Z force enable.
        /// </summary>
        public readonly bool EarlyZForce;

        /// <summary>
        /// Primitive topology of current draw.
        /// </summary>
        public readonly InputTopology Topology;

        /// <summary>
        /// Tessellation winding order.
        /// </summary>
        public readonly bool TessCw;

        /// <summary>
        /// Tessellation patch type.
        /// </summary>
        public readonly TessPatchType TessPatchType;

        /// <summary>
        /// Tessellation spacing.
        /// </summary>
        public readonly TessSpacing TessSpacing;

        /// <summary>
        /// Indicates whether alpha-to-coverage is enabled.
        /// </summary>
        public readonly bool AlphaToCoverageEnable;

        /// <summary>
        /// Indicates whether alpha-to-coverage dithering is enabled.
        /// </summary>
        public readonly bool AlphaToCoverageDitherEnable;

        /// <summary>
        /// Indicates whether the viewport transform is disabled.
        /// </summary>
        public readonly bool ViewportTransformDisable;

        /// <summary>
        /// Depth mode zero to one or minus one to one.
        /// </summary>
        public readonly bool DepthMode;

        /// <summary>
        /// Indicates if the point size is set on the shader or is fixed.
        /// </summary>
        public readonly bool ProgramPointSizeEnable;

        /// <summary>
        /// Point size used if <see cref="ProgramPointSizeEnable" /> is false.
        /// </summary>
        public readonly float PointSize;

        /// <summary>
        /// When alpha test is enabled, indicates the comparison that decides if the fragment should be discarded.
        /// </summary>
        public readonly AlphaTestOp AlphaTestCompare;

        /// <summary>
        /// When alpha test is enabled, indicates the value to compare with the fragment output alpha.
        /// </summary>
        public readonly float AlphaTestReference;

        /// <summary>
        /// Type of the vertex attributes consumed by the shader.
        /// </summary>
        public readonly Array32<AttributeType> AttributeTypes;

        /// <summary>
        /// Indicates that the draw is writing the base vertex, base instance and draw index to Constant Buffer 0.
        /// </summary>
        public readonly bool HasConstantBufferDrawParameters;

        /// <summary>
        /// Type of the fragment shader outputs.
        /// </summary>
        public readonly Array8<AttributeType> FragmentOutputTypes;

        /// <summary>
        /// Indicates whether dual source blend is enabled.
        /// </summary>
        public readonly bool DualSourceBlendEnable;

        /// <summary>
        /// Indicates if negation of the viewport Y axis is enabled.
        /// </summary>
        public readonly bool YNegateEnabled;

        /// <summary>
        /// If true, indicates that the fragment origin is the upper left corner of the viewport, otherwise it is the lower left corner.
        /// </summary>
        public readonly bool OriginUpperLeft;

        /// <summary>
        /// Indicates that the primitive ID values on the shader should be halved due to quad to triangles conversion.
        /// </summary>
        public readonly bool HalvePrimitiveId;

        /// <summary>
        /// Creates a new GPU graphics state.
        /// </summary>
        /// <param name="earlyZForce">Early Z force enable</param>
        /// <param name="topology">Primitive topology</param>
        /// <param name="tessCw">Tessellation winding order (clockwise or counter-clockwise)</param>
        /// <param name="tessPatchType">Tessellation patch type</param>
        /// <param name="tessSpacing">Tessellation spacing</param>
        /// <param name="alphaToCoverageEnable">Indicates whether alpha-to-coverage is enabled</param>
        /// <param name="alphaToCoverageDitherEnable">Indicates whether alpha-to-coverage dithering is enabled</param>
        /// <param name="viewportTransformDisable">Indicates whether the viewport transform is disabled</param>
        /// <param name="depthMode">Depth mode zero to one or minus one to one</param>
        /// <param name="programPointSizeEnable">Indicates if the point size is set on the shader or is fixed</param>
        /// <param name="pointSize">Point size if not set from shader</param>
        /// <param name="alphaTestCompare">When alpha test is enabled, indicates the comparison that decides if the fragment should be discarded</param>
        /// <param name="alphaTestReference">When alpha test is enabled, indicates the value to compare with the fragment output alpha</param>
        /// <param name="attributeTypes">Type of the vertex attributes consumed by the shader</param>
        /// <param name="hasConstantBufferDrawParameters">Indicates that the draw is writing the base vertex, base instance and draw index to Constant Buffer 0</param>
        /// <param name="fragmentOutputTypes">Type of the fragment shader outputs</param>
        /// <param name="dualSourceBlendEnable">Indicates whether dual source blend is enabled</param>
        /// <param name="yNegateEnabled">Indicates if negation of the viewport Y axis is enabled</param>
        /// <param name="originUpperLeft">If true, indicates that the fragment origin is the upper left corner of the viewport, otherwise it is the lower left corner</param>
        /// <param name="halvePrimitiveId">Indicates that the primitive ID values on the shader should be halved due to quad to triangles conversion</param>
        public GpuGraphicsState(
            bool earlyZForce,
            InputTopology topology,
            bool tessCw,
            TessPatchType tessPatchType,
            TessSpacing tessSpacing,
            bool alphaToCoverageEnable,
            bool alphaToCoverageDitherEnable,
            bool viewportTransformDisable,
            bool depthMode,
            bool programPointSizeEnable,
            float pointSize,
            AlphaTestOp alphaTestCompare,
            float alphaTestReference,
            in Array32<AttributeType> attributeTypes,
            bool hasConstantBufferDrawParameters,
            in Array8<AttributeType> fragmentOutputTypes,
            bool dualSourceBlendEnable,
            bool yNegateEnabled,
            bool originUpperLeft,
            bool halvePrimitiveId)
        {
            EarlyZForce = earlyZForce;
            Topology = topology;
            TessCw = tessCw;
            TessPatchType = tessPatchType;
            TessSpacing = tessSpacing;
            AlphaToCoverageEnable = alphaToCoverageEnable;
            AlphaToCoverageDitherEnable = alphaToCoverageDitherEnable;
            ViewportTransformDisable = viewportTransformDisable;
            DepthMode = depthMode;
            ProgramPointSizeEnable = programPointSizeEnable;
            PointSize = pointSize;
            AlphaTestCompare = alphaTestCompare;
            AlphaTestReference = alphaTestReference;
            AttributeTypes = attributeTypes;
            HasConstantBufferDrawParameters = hasConstantBufferDrawParameters;
            FragmentOutputTypes = fragmentOutputTypes;
            DualSourceBlendEnable = dualSourceBlendEnable;
            YNegateEnabled = yNegateEnabled;
            OriginUpperLeft = originUpperLeft;
            HalvePrimitiveId = halvePrimitiveId;
        }
    }
}
