using Ryujinx.Graphics.Gpu.Memory;
using Ryujinx.Graphics.Gpu.Shader.HashTable;
using System;

namespace Ryujinx.Graphics.Gpu.Shader
{
    /// <summary>
    /// Shader code accessor.
    /// </summary>
    readonly struct ShaderCodeAccessor : IDataAccessor
    {
        private readonly MemoryManager _memoryManager;
        private readonly ulong _baseAddress;

        /// <summary>
        /// Creates a new shader code accessor.
        /// </summary>
        /// <param name="memoryManager">Memory manager used to access the shader code</param>
        /// <param name="baseAddress">Base address of the shader in memory</param>
        public ShaderCodeAccessor(MemoryManager memoryManager, ulong baseAddress)
        {
            _memoryManager = memoryManager;
            _baseAddress = baseAddress;
        }

        /// <inheritdoc/>
        public ReadOnlySpan<byte> GetSpan(int offset, int length)
        {
            return _memoryManager.GetSpanMapped(_baseAddress + (ulong)offset, length);
        }
    }
}
