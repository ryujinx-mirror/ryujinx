using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Image;
using Ryujinx.Graphics.Gpu.State;
using Ryujinx.Graphics.Shader;
using System;

namespace Ryujinx.Graphics.Gpu.Shader
{
    /// <summary>
    /// Represents a GPU state and memory accessor.
    /// </summary>
    class GpuAccessor : IGpuAccessor
    {
        private readonly GpuContext _context;
        private readonly GpuState _state;
        private readonly int _stageIndex;
        private readonly bool _compute;
        private readonly int _localSizeX;
        private readonly int _localSizeY;
        private readonly int _localSizeZ;
        private readonly int _localMemorySize;
        private readonly int _sharedMemorySize;

        /// <summary>
        /// Creates a new instance of the GPU state accessor for graphics shader translation.
        /// </summary>
        /// <param name="context">GPU context</param>
        /// <param name="state">Current GPU state</param>
        /// <param name="stageIndex">Graphics shader stage index (0 = Vertex, 4 = Fragment)</param>
        public GpuAccessor(GpuContext context, GpuState state, int stageIndex)
        {
            _context = context;
            _state = state;
            _stageIndex = stageIndex;
        }

        /// <summary>
        /// Creates a new instance of the GPU state accessor for compute shader translation.
        /// </summary>
        /// <param name="context">GPU context</param>
        /// <param name="state">Current GPU state</param>
        /// <param name="localSizeX">Local group size X of the compute shader</param>
        /// <param name="localSizeY">Local group size Y of the compute shader</param>
        /// <param name="localSizeZ">Local group size Z of the compute shader</param>
        /// <param name="localMemorySize">Local memory size of the compute shader</param>
        /// <param name="sharedMemorySize">Shared memory size of the compute shader</param>
        public GpuAccessor(
            GpuContext context,
            GpuState state,
            int localSizeX,
            int localSizeY,
            int localSizeZ,
            int localMemorySize,
            int sharedMemorySize)
        {
            _context = context;
            _state = state;
            _compute = true;
            _localSizeX = localSizeX;
            _localSizeY = localSizeY;
            _localSizeZ = localSizeZ;
            _localMemorySize = localMemorySize;
            _sharedMemorySize = sharedMemorySize;
        }

        /// <summary>
        /// Prints a log message.
        /// </summary>
        /// <param name="message">Message to print</param>
        public void Log(string message)
        {
            Logger.Warning?.Print(LogClass.Gpu, $"Shader translator: {message}");
        }

        /// <summary>
        /// Reads data from GPU memory.
        /// </summary>
        /// <typeparam name="T">Type of the data to be read</typeparam>
        /// <param name="address">GPU virtual address of the data</param>
        /// <returns>Data at the memory location</returns>
        public T MemoryRead<T>(ulong address) where T : unmanaged
        {
            return _context.MemoryManager.Read<T>(address);
        }

        /// <summary>
        /// Queries Local Size X for compute shaders.
        /// </summary>
        /// <returns>Local Size X</returns>
        public int QueryComputeLocalSizeX() => _localSizeX;

        /// <summary>
        /// Queries Local Size Y for compute shaders.
        /// </summary>
        /// <returns>Local Size Y</returns>
        public int QueryComputeLocalSizeY() => _localSizeY;

        /// <summary>
        /// Queries Local Size Z for compute shaders.
        /// </summary>
        /// <returns>Local Size Z</returns>
        public int QueryComputeLocalSizeZ() => _localSizeZ;

        /// <summary>
        /// Queries Local Memory size in bytes for compute shaders.
        /// </summary>
        /// <returns>Local Memory size in bytes</returns>
        public int QueryComputeLocalMemorySize() => _localMemorySize;

        /// <summary>
        /// Queries Shared Memory size in bytes for compute shaders.
        /// </summary>
        /// <returns>Shared Memory size in bytes</returns>
        public int QueryComputeSharedMemorySize() => _sharedMemorySize;

        /// <summary>
        /// Queries texture target information.
        /// </summary>
        /// <param name="handle">Texture handle</param>
        /// <returns>True if the texture is a buffer texture, false otherwise</returns>
        public bool QueryIsTextureBuffer(int handle)
        {
            return GetTextureDescriptor(handle).UnpackTextureTarget() == TextureTarget.TextureBuffer;
        }

        /// <summary>
        /// Queries texture target information.
        /// </summary>
        /// <param name="handle">Texture handle</param>
        /// <returns>True if the texture is a rectangle texture, false otherwise</returns>
        public bool QueryIsTextureRectangle(int handle)
        {
            var descriptor = GetTextureDescriptor(handle);

            TextureTarget target = descriptor.UnpackTextureTarget();

            bool is2DTexture = target == TextureTarget.Texture2D ||
                               target == TextureTarget.Texture2DRect;

            return !descriptor.UnpackTextureCoordNormalized() && is2DTexture;
        }

        /// <summary>
        /// Queries current primitive topology for geometry shaders.
        /// </summary>
        /// <returns>Current primitive topology</returns>
        public InputTopology QueryPrimitiveTopology()
        {
            switch (_context.Methods.PrimitiveType)
            {
                case PrimitiveType.Points:
                    return InputTopology.Points;
                case PrimitiveType.Lines:
                case PrimitiveType.LineLoop:
                case PrimitiveType.LineStrip:
                    return InputTopology.Lines;
                case PrimitiveType.LinesAdjacency:
                case PrimitiveType.LineStripAdjacency:
                    return InputTopology.LinesAdjacency;
                case PrimitiveType.Triangles:
                case PrimitiveType.TriangleStrip:
                case PrimitiveType.TriangleFan:
                    return InputTopology.Triangles;
                case PrimitiveType.TrianglesAdjacency:
                case PrimitiveType.TriangleStripAdjacency:
                    return InputTopology.TrianglesAdjacency;
            }

            return InputTopology.Points;
        }

        /// <summary>
        /// Queries host storage buffer alignment required.
        /// </summary>
        /// <returns>Host storage buffer alignment in bytes</returns>
        public int QueryStorageBufferOffsetAlignment() => _context.Capabilities.StorageBufferOffsetAlignment;

        /// <summary>
        /// Queries host support for readable images without a explicit format declaration on the shader.
        /// </summary>
        /// <returns>True if formatted image load is supported, false otherwise</returns>
        public bool QuerySupportsImageLoadFormatted() => _context.Capabilities.SupportsImageLoadFormatted;

        /// <summary>
        /// Queries host GPU non-constant texture offset support.
        /// </summary>
        /// <returns>True if the GPU and driver supports non-constant texture offsets, false otherwise</returns>
        public bool QuerySupportsNonConstantTextureOffset() => _context.Capabilities.SupportsNonConstantTextureOffset;

        /// <summary>
        /// Queries host GPU viewport swizzle support.
        /// </summary>
        /// <returns>True if the GPU and driver supports viewport swizzle, false otherwise</returns>
        public bool QuerySupportsViewportSwizzle() => _context.Capabilities.SupportsViewportSwizzle;

        /// <summary>
        /// Queries texture format information, for shaders using image load or store.
        /// </summary>
        /// <remarks>
        /// This only returns non-compressed color formats.
        /// If the format of the texture is a compressed, depth or unsupported format, then a default value is returned.
        /// </remarks>
        /// <param name="handle">Texture handle</param>
        /// <returns>Color format of the non-compressed texture</returns>
        public TextureFormat QueryTextureFormat(int handle)
        {
            var descriptor = GetTextureDescriptor(handle);

            if (!FormatTable.TryGetTextureFormat(descriptor.UnpackFormat(), descriptor.UnpackSrgb(), out FormatInfo formatInfo))
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

        public int QueryViewportSwizzle(int component)
        {
            YControl yControl = _state.Get<YControl>(MethodOffset.YControl);

            bool flipY = yControl.HasFlag(YControl.NegateY) ^ !yControl.HasFlag(YControl.TriangleRastFlip);

            ViewportTransform transform = _state.Get<ViewportTransform>(MethodOffset.ViewportTransform, 0);

            return component switch
            {
                0 => (int)(transform.UnpackSwizzleX() ^ (transform.ScaleX < 0 ? ViewportSwizzle.NegativeFlag : 0)),
                1 => (int)(transform.UnpackSwizzleY() ^ (transform.ScaleY < 0 ? ViewportSwizzle.NegativeFlag : 0) ^ (flipY ? ViewportSwizzle.NegativeFlag : 0)),
                2 => (int)(transform.UnpackSwizzleZ() ^ (transform.ScaleZ < 0 ? ViewportSwizzle.NegativeFlag : 0)),
                3 => (int)transform.UnpackSwizzleW(),
                _ => throw new ArgumentOutOfRangeException(nameof(component))
            };
        }

        /// <summary>
        /// Gets the texture descriptor for a given texture on the pool.
        /// </summary>
        /// <param name="handle">Index of the texture (this is the shader "fake" handle)</param>
        /// <returns>Texture descriptor</returns>
        private Image.TextureDescriptor GetTextureDescriptor(int handle)
        {
            if (_compute)
            {
                return _context.Methods.TextureManager.GetComputeTextureDescriptor(_state, handle);
            }
            else
            {
                return _context.Methods.TextureManager.GetGraphicsTextureDescriptor(_state, _stageIndex, handle);
            }
        }
    }
}
