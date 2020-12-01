using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.State;
using Ryujinx.Graphics.Shader;

namespace Ryujinx.Graphics.Gpu.Shader
{
    /// <summary>
    /// Represents a GPU state and memory accessor.
    /// </summary>
    class GpuAccessor : TextureDescriptorCapableGpuAccessor, IGpuAccessor
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
        public override T MemoryRead<T>(ulong address)
        {
            return _context.MemoryManager.Read<T>(address);
        }

        /// <summary>
        /// Checks if a given memory address is mapped.
        /// </summary>
        /// <param name="address">GPU virtual address to be checked</param>
        /// <returns>True if the address is mapped, false otherwise</returns>
        public bool MemoryMapped(ulong address)
        {
            return _context.MemoryManager.IsMapped(address);
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
        /// Queries Constant Buffer usage information.
        /// </summary>
        /// <returns>A mask where each bit set indicates a bound constant buffer</returns>
        public uint QueryConstantBufferUse()
        {
            return _compute
                ? _context.Methods.BufferManager.GetComputeUniformBufferUseMask()
                : _context.Methods.BufferManager.GetGraphicsUniformBufferUseMask(_stageIndex);
        }

        /// <summary>
        /// Queries current primitive topology for geometry shaders.
        /// </summary>
        /// <returns>Current primitive topology</returns>
        public InputTopology QueryPrimitiveTopology()
        {
            switch (_context.Methods.Topology)
            {
                case PrimitiveTopology.Points:
                    return InputTopology.Points;
                case PrimitiveTopology.Lines:
                case PrimitiveTopology.LineLoop:
                case PrimitiveTopology.LineStrip:
                    return InputTopology.Lines;
                case PrimitiveTopology.LinesAdjacency:
                case PrimitiveTopology.LineStripAdjacency:
                    return InputTopology.LinesAdjacency;
                case PrimitiveTopology.Triangles:
                case PrimitiveTopology.TriangleStrip:
                case PrimitiveTopology.TriangleFan:
                    return InputTopology.Triangles;
                case PrimitiveTopology.TrianglesAdjacency:
                case PrimitiveTopology.TriangleStripAdjacency:
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
        /// Gets the texture descriptor for a given texture on the pool.
        /// </summary>
        /// <param name="handle">Index of the texture (this is the word offset of the handle in the constant buffer)</param>
        /// <returns>Texture descriptor</returns>
        public override Image.ITextureDescriptor GetTextureDescriptor(int handle)
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

        /// <summary>
        /// Queries if host state forces early depth testing.
        /// </summary>
        /// <returns>True if early depth testing is forced</returns>
        public bool QueryEarlyZForce()
        {
            return _state.Get<bool>(MethodOffset.EarlyZForce);
        }
    }
}
