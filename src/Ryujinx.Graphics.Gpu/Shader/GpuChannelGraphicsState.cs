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
        /// Indicates whether Y negate of the fragment coordinates is enabled.
        /// </summary>
        public bool YNegateEnabled;

        /// <summary>
        /// Creates a new graphics state from this state that can be used for shader generation.
        /// </summary>
        /// <param name="hostSupportsAlphaTest">Indicates if the host API supports alpha test operations</param>
        /// <param name="hostSupportsQuads">Indicates if the host API supports quad primitives</param>
        /// <param name="hasGeometryShader">Indicates if a geometry shader is used</param>
        /// <param name="originUpperLeft">If true, indicates that the fragment origin is the upper left corner of the viewport, otherwise it is the lower left corner</param>
        /// <returns>GPU graphics state that can be used for shader translation</returns>
        public readonly GpuGraphicsState CreateShaderGraphicsState(bool hostSupportsAlphaTest, bool hostSupportsQuads, bool hasGeometryShader, bool originUpperLeft)
        {
            AlphaTestOp alphaTestOp;

            if (hostSupportsAlphaTest || !AlphaTestEnable)
            {
                alphaTestOp = AlphaTestOp.Always;
            }
            else
            {
                alphaTestOp = AlphaTestCompare switch
                {
                    CompareOp.Never or CompareOp.NeverGl => AlphaTestOp.Never,
                    CompareOp.Less or CompareOp.LessGl => AlphaTestOp.Less,
                    CompareOp.Equal or CompareOp.EqualGl => AlphaTestOp.Equal,
                    CompareOp.LessOrEqual or CompareOp.LessOrEqualGl => AlphaTestOp.LessOrEqual,
                    CompareOp.Greater or CompareOp.GreaterGl => AlphaTestOp.Greater,
                    CompareOp.NotEqual or CompareOp.NotEqualGl => AlphaTestOp.NotEqual,
                    CompareOp.GreaterOrEqual or CompareOp.GreaterOrEqualGl => AlphaTestOp.GreaterOrEqual,
                    _ => AlphaTestOp.Always,
                };
            }

            bool isQuad = Topology == PrimitiveTopology.Quads || Topology == PrimitiveTopology.QuadStrip;
            bool halvePrimitiveId = !hostSupportsQuads && !hasGeometryShader && isQuad;

            return new GpuGraphicsState(
                EarlyZForce,
                ConvertToInputTopology(Topology, TessellationMode),
                TessellationMode.UnpackCw(),
                TessellationMode.UnpackPatchType(),
                TessellationMode.UnpackSpacing(),
                AlphaToCoverageEnable,
                AlphaToCoverageDitherEnable,
                ViewportTransformDisable,
                DepthMode,
                ProgramPointSizeEnable,
                PointSize,
                alphaTestOp,
                AlphaTestReference,
                in AttributeTypes,
                HasConstantBufferDrawParameters,
                in FragmentOutputTypes,
                DualSourceBlendEnable,
                YNegateEnabled,
                originUpperLeft,
                halvePrimitiveId);
        }

        /// <summary>
        /// Converts the Maxwell primitive topology to the shader translator topology.
        /// </summary>
        /// <param name="topology">Maxwell primitive topology</param>
        /// <param name="tessellationMode">Maxwell tessellation mode</param>
        /// <returns>Shader translator topology</returns>
        private static InputTopology ConvertToInputTopology(PrimitiveTopology topology, TessMode tessellationMode)
        {
            return topology switch
            {
                PrimitiveTopology.Points => InputTopology.Points,
                PrimitiveTopology.Lines or
                PrimitiveTopology.LineLoop or
                PrimitiveTopology.LineStrip => InputTopology.Lines,
                PrimitiveTopology.LinesAdjacency or
                PrimitiveTopology.LineStripAdjacency => InputTopology.LinesAdjacency,
                PrimitiveTopology.Triangles or
                PrimitiveTopology.TriangleStrip or
                PrimitiveTopology.TriangleFan => InputTopology.Triangles,
                PrimitiveTopology.TrianglesAdjacency or
                PrimitiveTopology.TriangleStripAdjacency => InputTopology.TrianglesAdjacency,
                PrimitiveTopology.Patches => tessellationMode.UnpackPatchType() == TessPatchType.Isolines
                    ? InputTopology.Lines
                    : InputTopology.Triangles,
                _ => InputTopology.Points,
            };
        }
    }
}
