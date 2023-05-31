using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Engine.Threed;
using Ryujinx.Graphics.Gpu.Image;
using Ryujinx.Graphics.Shader;
using Ryujinx.Graphics.Shader.Translation;
using System;

namespace Ryujinx.Graphics.Gpu.Shader
{
    /// <summary>
    /// GPU accessor.
    /// </summary>
    class GpuAccessorBase
    {
        private readonly GpuContext _context;
        private readonly ResourceCounts _resourceCounts;
        private readonly int _stageIndex;

        /// <summary>
        /// Creates a new GPU accessor.
        /// </summary>
        /// <param name="context">GPU context</param>
        public GpuAccessorBase(GpuContext context, ResourceCounts resourceCounts, int stageIndex)
        {
            _context = context;
            _resourceCounts = resourceCounts;
            _stageIndex = stageIndex;
        }

        public int QueryBindingConstantBuffer(int index)
        {
            if (_context.Capabilities.Api == TargetApi.Vulkan)
            {
                // We need to start counting from 1 since binding 0 is reserved for the support uniform buffer.
                return GetBindingFromIndex(index, _context.Capabilities.MaximumUniformBuffersPerStage, "Uniform buffer") + 1;
            }
            else
            {
                return _resourceCounts.UniformBuffersCount++;
            }
        }

        public int QueryBindingStorageBuffer(int index)
        {
            if (_context.Capabilities.Api == TargetApi.Vulkan)
            {
                return GetBindingFromIndex(index, _context.Capabilities.MaximumStorageBuffersPerStage, "Storage buffer");
            }
            else
            {
                return _resourceCounts.StorageBuffersCount++;
            }
        }

        public int QueryBindingTexture(int index, bool isBuffer)
        {
            if (_context.Capabilities.Api == TargetApi.Vulkan)
            {
                if (isBuffer)
                {
                    index += (int)_context.Capabilities.MaximumTexturesPerStage;
                }

                return GetBindingFromIndex(index, _context.Capabilities.MaximumTexturesPerStage * 2, "Texture");
            }
            else
            {
                return _resourceCounts.TexturesCount++;
            }
        }

        public int QueryBindingImage(int index, bool isBuffer)
        {
            if (_context.Capabilities.Api == TargetApi.Vulkan)
            {
                if (isBuffer)
                {
                    index += (int)_context.Capabilities.MaximumImagesPerStage;
                }

                return GetBindingFromIndex(index, _context.Capabilities.MaximumImagesPerStage * 2, "Image");
            }
            else
            {
                return _resourceCounts.ImagesCount++;
            }
        }

        private int GetBindingFromIndex(int index, uint maxPerStage, string resourceName)
        {
            if ((uint)index >= maxPerStage)
            {
                Logger.Error?.Print(LogClass.Gpu, $"{resourceName} index {index} exceeds per stage limit of {maxPerStage}.");
            }

            return GetStageIndex(_stageIndex) * (int)maxPerStage + index;
        }

        public static int GetStageIndex(int stageIndex)
        {
            // This is just a simple remapping to ensure that most frequently used shader stages
            // have the lowest binding numbers.
            // This is useful because if we need to run on a system with a low limit on the bindings,
            // then we can still get most games working as the most common shaders will have low binding numbers.
            return stageIndex switch
            {
                4 => 1, // Fragment
                3 => 2, // Geometry
                1 => 3, // Tessellation control
                2 => 4, // Tessellation evaluation
                _ => 0 // Vertex/Compute
            };
        }

        public int QueryHostGatherBiasPrecision() => _context.Capabilities.GatherBiasPrecision;

        public bool QueryHostReducedPrecision() => _context.Capabilities.ReduceShaderPrecision;

        public bool QueryHostHasFrontFacingBug() => _context.Capabilities.HasFrontFacingBug;

        public bool QueryHostHasVectorIndexingBug() => _context.Capabilities.HasVectorIndexingBug;

        public int QueryHostStorageBufferOffsetAlignment() => _context.Capabilities.StorageBufferOffsetAlignment;

        public bool QueryHostSupportsBgraFormat() => _context.Capabilities.SupportsBgraFormat;

        public bool QueryHostSupportsFragmentShaderInterlock() => _context.Capabilities.SupportsFragmentShaderInterlock;

        public bool QueryHostSupportsFragmentShaderOrderingIntel() => _context.Capabilities.SupportsFragmentShaderOrderingIntel;

        public bool QueryHostSupportsGeometryShader() => _context.Capabilities.SupportsGeometryShader;

        public bool QueryHostSupportsGeometryShaderPassthrough() => _context.Capabilities.SupportsGeometryShaderPassthrough;

        public bool QueryHostSupportsImageLoadFormatted() => _context.Capabilities.SupportsImageLoadFormatted;

        public bool QueryHostSupportsLayerVertexTessellation() => _context.Capabilities.SupportsLayerVertexTessellation;

        public bool QueryHostSupportsNonConstantTextureOffset() => _context.Capabilities.SupportsNonConstantTextureOffset;

        public bool QueryHostSupportsShaderBallot() => _context.Capabilities.SupportsShaderBallot;

        public bool QueryHostSupportsSnormBufferTextureFormat() => _context.Capabilities.SupportsSnormBufferTextureFormat;

        public bool QueryHostSupportsTextureShadowLod() => _context.Capabilities.SupportsTextureShadowLod;

        public bool QueryHostSupportsViewportIndexVertexTessellation() => _context.Capabilities.SupportsViewportIndexVertexTessellation;

        public bool QueryHostSupportsViewportMask() => _context.Capabilities.SupportsViewportMask;

        public bool QueryHostSupportsDepthClipControl() => _context.Capabilities.SupportsDepthClipControl;

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
