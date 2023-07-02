using Ryujinx.Common.Memory;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Engine.Threed;
using Ryujinx.Graphics.Shader;

namespace Ryujinx.Graphics.Gpu.Shader
{
    /// <summary>
    /// State used by the <see cref="GpuAccessor"/>.
    /// </summary>
    struct GpuChannelGraphicsState
    {
        // New fields should be added to the end of the struct to keep disk shader cache compatibility.

        /// <summary>
        /// Early Z force enable.
        /// </summary>
        public bool EarlyZForce;

        /// <summary>
        /// Primitive topology of current draw.
        /// </summary>
        public PrimitiveTopology Topology;

        /// <summary>
        /// Tessellation mode.
        /// </summary>
        public TessMode TessellationMode;

        /// <summary>
        /// Indicates whether alpha-to-coverage is enabled.
        /// </summary>
        public bool AlphaToCoverageEnable;

        /// <summary>
        /// Indicates whether alpha-to-coverage dithering is enabled.
        /// </summary>
        public bool AlphaToCoverageDitherEnable;

        /// <summary>
        /// Indicates whether the viewport transform is disabled.
        /// </summary>
        public bool ViewportTransformDisable;

        /// <summary>
        /// Depth mode zero to one or minus one to one.
        /// </summary>
        public bool DepthMode;

        /// <summary>
        /// Indicates if the point size is set on the shader or is fixed.
        /// </summary>
        public bool ProgramPointSizeEnable;

        /// <summary>
        /// Point size used if <see cref="ProgramPointSizeEnable" /> is false.
        /// </summary>
        public float PointSize;

        /// <summary>
        /// Indicates whether alpha test is enabled.
        /// </summary>
        public bool AlphaTestEnable;

        /// <summary>
        /// When alpha test is enabled, indicates the comparison that decides if the fragment should be discarded.
        /// </summary>
        public CompareOp AlphaTestCompare;

        /// <summary>
        /// When alpha test is enabled, indicates the value to compare with the fragment output alpha.
        /// </summary>
        public float AlphaTestReference;

        /// <summary>
        /// Type of the vertex attributes consumed by the shader.
        /// </summary>
        public Array32<AttributeType> AttributeTypes;

        /// <summary>
        /// Indicates that the draw is writing the base vertex, base instance and draw index to Constant Buffer 0.
        /// </summary>
        public bool HasConstantBufferDrawParameters;

        /// <summary>
        /// Indicates that any storage buffer use is unaligned.
        /// </summary>
        public bool HasUnalignedStorageBuffer;

        /// <summary>
        /// Type of the fragment shader outputs.
        /// </summary>
        public Array8<AttributeType> FragmentOutputTypes;

        /// <summary>
        /// Indicates whether dual source blend is enabled.
        /// </summary>
        public bool DualSourceBlendEnable;

        /// <summary>
        /// Creates a new GPU graphics state.
        /// </summary>
        /// <param name="earlyZForce">Early Z force enable</param>
        /// <param name="topology">Primitive topology</param>
        /// <param name="tessellationMode">Tessellation mode</param>
        /// <param name="alphaToCoverageEnable">Indicates whether alpha-to-coverage is enabled</param>
        /// <param name="alphaToCoverageDitherEnable">Indicates whether alpha-to-coverage dithering is enabled</param>
        /// <param name="viewportTransformDisable">Indicates whether the viewport transform is disabled</param>
        /// <param name="depthMode">Depth mode zero to one or minus one to one</param>
        /// <param name="programPointSizeEnable">Indicates if the point size is set on the shader or is fixed</param>
        /// <param name="pointSize">Point size if not set from shader</param>
        /// <param name="alphaTestEnable">Indicates whether alpha test is enabled</param>
        /// <param name="alphaTestCompare">When alpha test is enabled, indicates the comparison that decides if the fragment should be discarded</param>
        /// <param name="alphaTestReference">When alpha test is enabled, indicates the value to compare with the fragment output alpha</param>
        /// <param name="attributeTypes">Type of the vertex attributes consumed by the shader</param>
        /// <param name="hasConstantBufferDrawParameters">Indicates that the draw is writing the base vertex, base instance and draw index to Constant Buffer 0</param>
        /// <param name="hasUnalignedStorageBuffer">Indicates that any storage buffer use is unaligned</param>
        /// <param name="fragmentOutputTypes">Type of the fragment shader outputs</param>
        /// <param name="dualSourceBlendEnable">Type of the vertex attributes consumed by the shader</param>
        public GpuChannelGraphicsState(
            bool earlyZForce,
            PrimitiveTopology topology,
            TessMode tessellationMode,
            bool alphaToCoverageEnable,
            bool alphaToCoverageDitherEnable,
            bool viewportTransformDisable,
            bool depthMode,
            bool programPointSizeEnable,
            float pointSize,
            bool alphaTestEnable,
            CompareOp alphaTestCompare,
            float alphaTestReference,
            ref Array32<AttributeType> attributeTypes,
            bool hasConstantBufferDrawParameters,
            bool hasUnalignedStorageBuffer,
            ref Array8<AttributeType> fragmentOutputTypes,
            bool dualSourceBlendEnable)
        {
            EarlyZForce = earlyZForce;
            Topology = topology;
            TessellationMode = tessellationMode;
            AlphaToCoverageEnable = alphaToCoverageEnable;
            AlphaToCoverageDitherEnable = alphaToCoverageDitherEnable;
            ViewportTransformDisable = viewportTransformDisable;
            DepthMode = depthMode;
            ProgramPointSizeEnable = programPointSizeEnable;
            PointSize = pointSize;
            AlphaTestEnable = alphaTestEnable;
            AlphaTestCompare = alphaTestCompare;
            AlphaTestReference = alphaTestReference;
            AttributeTypes = attributeTypes;
            HasConstantBufferDrawParameters = hasConstantBufferDrawParameters;
            HasUnalignedStorageBuffer = hasUnalignedStorageBuffer;
            FragmentOutputTypes = fragmentOutputTypes;
            DualSourceBlendEnable = dualSourceBlendEnable;
        }
    }
}
