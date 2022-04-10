using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Engine.Threed;
using Ryujinx.Graphics.Gpu.Image;
using Ryujinx.Graphics.Shader;

namespace Ryujinx.Graphics.Gpu.Shader
{
    /// <summary>
    /// GPU accessor.
    /// </summary>
    class GpuAccessorBase
    {
        private readonly GpuContext _context;

        /// <summary>
        /// Creates a new GPU accessor.
        /// </summary>
        /// <param name="context">GPU context</param>
        public GpuAccessorBase(GpuContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Queries host about the presence of the FrontFacing built-in variable bug.
        /// </summary>
        /// <returns>True if the bug is present on the host device used, false otherwise</returns>
        public bool QueryHostHasFrontFacingBug() => _context.Capabilities.HasFrontFacingBug;

        /// <summary>
        /// Queries host about the presence of the vector indexing bug.
        /// </summary>
        /// <returns>True if the bug is present on the host device used, false otherwise</returns>
        public bool QueryHostHasVectorIndexingBug() => _context.Capabilities.HasVectorIndexingBug;

        /// <summary>
        /// Queries host storage buffer alignment required.
        /// </summary>
        /// <returns>Host storage buffer alignment in bytes</returns>
        public int QueryHostStorageBufferOffsetAlignment() => _context.Capabilities.StorageBufferOffsetAlignment;

        /// <summary>
        /// Queries host support for texture formats with BGRA component order (such as BGRA8).
        /// </summary>
        /// <returns>True if BGRA formats are supported, false otherwise</returns>
        public bool QueryHostSupportsBgraFormat() => _context.Capabilities.SupportsBgraFormat;

        /// <summary>
        /// Queries host support for fragment shader ordering critical sections on the shader code.
        /// </summary>
        /// <returns>True if fragment shader interlock is supported, false otherwise</returns>
        public bool QueryHostSupportsFragmentShaderInterlock() => _context.Capabilities.SupportsFragmentShaderInterlock;

        /// <summary>
        /// Queries host support for fragment shader ordering scoped critical sections on the shader code.
        /// </summary>
        /// <returns>True if fragment shader ordering is supported, false otherwise</returns>
        public bool QueryHostSupportsFragmentShaderOrderingIntel() => _context.Capabilities.SupportsFragmentShaderOrderingIntel;

        /// <summary>
        /// Queries host support for readable images without a explicit format declaration on the shader.
        /// </summary>
        /// <returns>True if formatted image load is supported, false otherwise</returns>
        public bool QueryHostSupportsImageLoadFormatted() => _context.Capabilities.SupportsImageLoadFormatted;

        /// <summary>
        /// Queries host GPU non-constant texture offset support.
        /// </summary>
        /// <returns>True if the GPU and driver supports non-constant texture offsets, false otherwise</returns>
        public bool QueryHostSupportsNonConstantTextureOffset() => _context.Capabilities.SupportsNonConstantTextureOffset;

        /// <summary>
        /// Queries host GPU shader ballot support.
        /// </summary>
        /// <returns>True if the GPU and driver supports shader ballot, false otherwise</returns>
        public bool QueryHostSupportsShaderBallot() => _context.Capabilities.SupportsShaderBallot;

        /// <summary>
        /// Queries host GPU texture shadow LOD support.
        /// </summary>
        /// <returns>True if the GPU and driver supports texture shadow LOD, false otherwise</returns>
        public bool QueryHostSupportsTextureShadowLod() => _context.Capabilities.SupportsTextureShadowLod;

        /// <summary>
        /// Converts a packed Maxwell texture format to the shader translator texture format.
        /// </summary>
        /// <param name="format">Packed maxwell format</param>
        /// <param name="formatSrgb">Indicates if the format is sRGB</param>
        /// <returns>Shader translator texture format</returns>
        protected static TextureFormat ConvertToTextureFormat(uint format, bool formatSrgb)
        {
            if (!FormatTable.TryGetTextureFormat(format, formatSrgb, out FormatInfo formatInfo))
            {
                return TextureFormat.Unknown;
            }

            return formatInfo.Format switch
            {
                Format.R8Unorm           => TextureFormat.R8Unorm,
                Format.R8Snorm           => TextureFormat.R8Snorm,
                Format.R8Uint            => TextureFormat.R8Uint,
                Format.R8Sint            => TextureFormat.R8Sint,
                Format.R16Float          => TextureFormat.R16Float,
                Format.R16Unorm          => TextureFormat.R16Unorm,
                Format.R16Snorm          => TextureFormat.R16Snorm,
                Format.R16Uint           => TextureFormat.R16Uint,
                Format.R16Sint           => TextureFormat.R16Sint,
                Format.R32Float          => TextureFormat.R32Float,
                Format.R32Uint           => TextureFormat.R32Uint,
                Format.R32Sint           => TextureFormat.R32Sint,
                Format.R8G8Unorm         => TextureFormat.R8G8Unorm,
                Format.R8G8Snorm         => TextureFormat.R8G8Snorm,
                Format.R8G8Uint          => TextureFormat.R8G8Uint,
                Format.R8G8Sint          => TextureFormat.R8G8Sint,
                Format.R16G16Float       => TextureFormat.R16G16Float,
                Format.R16G16Unorm       => TextureFormat.R16G16Unorm,
                Format.R16G16Snorm       => TextureFormat.R16G16Snorm,
                Format.R16G16Uint        => TextureFormat.R16G16Uint,
                Format.R16G16Sint        => TextureFormat.R16G16Sint,
                Format.R32G32Float       => TextureFormat.R32G32Float,
                Format.R32G32Uint        => TextureFormat.R32G32Uint,
                Format.R32G32Sint        => TextureFormat.R32G32Sint,
                Format.R8G8B8A8Unorm     => TextureFormat.R8G8B8A8Unorm,
                Format.R8G8B8A8Snorm     => TextureFormat.R8G8B8A8Snorm,
                Format.R8G8B8A8Uint      => TextureFormat.R8G8B8A8Uint,
                Format.R8G8B8A8Sint      => TextureFormat.R8G8B8A8Sint,
                Format.R8G8B8A8Srgb      => TextureFormat.R8G8B8A8Unorm,
                Format.R16G16B16A16Float => TextureFormat.R16G16B16A16Float,
                Format.R16G16B16A16Unorm => TextureFormat.R16G16B16A16Unorm,
                Format.R16G16B16A16Snorm => TextureFormat.R16G16B16A16Snorm,
                Format.R16G16B16A16Uint  => TextureFormat.R16G16B16A16Uint,
                Format.R16G16B16A16Sint  => TextureFormat.R16G16B16A16Sint,
                Format.R32G32B32A32Float => TextureFormat.R32G32B32A32Float,
                Format.R32G32B32A32Uint  => TextureFormat.R32G32B32A32Uint,
                Format.R32G32B32A32Sint  => TextureFormat.R32G32B32A32Sint,
                Format.R10G10B10A2Unorm  => TextureFormat.R10G10B10A2Unorm,
                Format.R10G10B10A2Uint   => TextureFormat.R10G10B10A2Uint,
                Format.R11G11B10Float    => TextureFormat.R11G11B10Float,
                _                        => TextureFormat.Unknown
            };
        }

        /// <summary>
        /// Converts the Maxwell primitive topology to the shader translator topology.
        /// </summary>
        /// <param name="topology">Maxwell primitive topology</param>
        /// <param name="tessellationMode">Maxwell tessellation mode</param>
        /// <returns>Shader translator topology</returns>
        protected static InputTopology ConvertToInputTopology(PrimitiveTopology topology, TessMode tessellationMode)
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
                _ => InputTopology.Points
            };
        }
    }
}
