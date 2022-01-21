using Ryujinx.Common.Logging;
using Ryujinx.Graphics.Gpu.Shader.Cache.Definition;
using Ryujinx.Graphics.Shader;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Shader
{
    class CachedGpuAccessor : TextureDescriptorCapableGpuAccessor, IGpuAccessor
    {
        private readonly ReadOnlyMemory<byte> _data;
        private readonly ReadOnlyMemory<byte> _cb1Data;
        private readonly GuestGpuAccessorHeader _header;
        private readonly Dictionary<int, GuestTextureDescriptor> _textureDescriptors;
        private readonly TransformFeedbackDescriptor[] _tfd;

        /// <summary>
        /// Creates a new instance of the cached GPU state accessor for shader translation.
        /// </summary>
        /// <param name="context">GPU context</param>
        /// <param name="data">The data of the shader</param>
        /// <param name="cb1Data">The constant buffer 1 data of the shader</param>
        /// <param name="header">The cache of the GPU accessor</param>
        /// <param name="guestTextureDescriptors">The cache of the texture descriptors</param>
        public CachedGpuAccessor(
            GpuContext context,
            ReadOnlyMemory<byte> data,
            ReadOnlyMemory<byte> cb1Data,
            GuestGpuAccessorHeader header,
            IReadOnlyDictionary<int, GuestTextureDescriptor> guestTextureDescriptors,
            TransformFeedbackDescriptor[] tfd) : base(context)
        {
            _data = data;
            _cb1Data = cb1Data;
            _header = header;
            _textureDescriptors = new Dictionary<int, GuestTextureDescriptor>();

            foreach (KeyValuePair<int, GuestTextureDescriptor> guestTextureDescriptor in guestTextureDescriptors)
            {
                _textureDescriptors.Add(guestTextureDescriptor.Key, guestTextureDescriptor.Value);
            }

            _tfd = tfd;
        }

        /// <summary>
        /// Reads data from the constant buffer 1.
        /// </summary>
        /// <param name="offset">Offset in bytes to read from</param>
        /// <returns>Value at the given offset</returns>
        public uint ConstantBuffer1Read(int offset)
        {
            return MemoryMarshal.Cast<byte, uint>(_cb1Data.Span.Slice(offset))[0];
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
        /// Gets a span of the specified memory location, containing shader code.
        /// </summary>
        /// <param name="address">GPU virtual address of the data</param>
        /// <param name="minimumSize">Minimum size that the returned span may have</param>
        /// <returns>Span of the memory location</returns>
        public override ReadOnlySpan<ulong> GetCode(ulong address, int minimumSize)
        {
            return MemoryMarshal.Cast<byte, ulong>(_data.Span.Slice((int)address));
        }

        /// <summary>
        /// Checks if a given memory address is mapped.
        /// </summary>
        /// <param name="address">GPU virtual address to be checked</param>
        /// <returns>True if the address is mapped, false otherwise</returns>
        public bool MemoryMapped(ulong address)
        {
            return address < (ulong)_data.Length;
        }

        /// <summary>
        /// Queries Local Size X for compute shaders.
        /// </summary>
        /// <returns>Local Size X</returns>
        public int QueryComputeLocalSizeX()
        {
            return _header.ComputeLocalSizeX;
        }

        /// <summary>
        /// Queries Local Size Y for compute shaders.
        /// </summary>
        /// <returns>Local Size Y</returns>
        public int QueryComputeLocalSizeY()
        {
            return _header.ComputeLocalSizeY;
        }

        /// <summary>
        /// Queries Local Size Z for compute shaders.
        /// </summary>
        /// <returns>Local Size Z</returns>
        public int QueryComputeLocalSizeZ()
        {
            return _header.ComputeLocalSizeZ;
        }

        /// <summary>
        /// Queries Local Memory size in bytes for compute shaders.
        /// </summary>
        /// <returns>Local Memory size in bytes</returns>
        public int QueryComputeLocalMemorySize()
        {
            return _header.ComputeLocalMemorySize;
        }

        /// <summary>
        /// Queries Shared Memory size in bytes for compute shaders.
        /// </summary>
        /// <returns>Shared Memory size in bytes</returns>
        public int QueryComputeSharedMemorySize()
        {
            return _header.ComputeSharedMemorySize;
        }

        /// <summary>
        /// Queries current primitive topology for geometry shaders.
        /// </summary>
        /// <returns>Current primitive topology</returns>
        public InputTopology QueryPrimitiveTopology()
        {
            return _header.PrimitiveTopology;
        }

        /// <summary>
        /// Queries the tessellation evaluation shader primitive winding order.
        /// </summary>
        /// <returns>True if the primitive winding order is clockwise, false if counter-clockwise</returns>
        public bool QueryTessCw()
        {
            return (_header.TessellationModePacked & 0x10) != 0;
        }

        /// <summary>
        /// Queries the tessellation evaluation shader abstract patch type.
        /// </summary>
        /// <returns>Abstract patch type</returns>
        public TessPatchType QueryTessPatchType()
        {
            return (TessPatchType)(_header.TessellationModePacked & 3);
        }

        /// <summary>
        /// Queries the tessellation evaluation shader spacing between tessellated vertices of the patch.
        /// </summary>
        /// <returns>Spacing between tessellated vertices of the patch</returns>
        public TessSpacing QueryTessSpacing()
        {
            return (TessSpacing)((_header.TessellationModePacked >> 2) & 3);
        }

        /// <summary>
        /// Gets the texture descriptor for a given texture on the pool.
        /// </summary>
        /// <param name="handle">Index of the texture (this is the word offset of the handle in the constant buffer)</param>
        /// <param name="cbufSlot">Constant buffer slot for the texture handle</param>
        /// <returns>Texture descriptor</returns>
        public override Image.ITextureDescriptor GetTextureDescriptor(int handle, int cbufSlot)
        {
            if (!_textureDescriptors.TryGetValue(handle, out GuestTextureDescriptor textureDescriptor))
            {
                throw new ArgumentException();
            }

            return textureDescriptor;
        }

        /// <summary>
        /// Queries transform feedback enable state.
        /// </summary>
        /// <returns>True if the shader uses transform feedback, false otherwise</returns>
        public bool QueryTransformFeedbackEnabled()
        {
            return _tfd != null;
        }

        /// <summary>
        /// Queries the varying locations that should be written to the transform feedback buffer.
        /// </summary>
        /// <param name="bufferIndex">Index of the transform feedback buffer</param>
        /// <returns>Varying locations for the specified buffer</returns>
        public ReadOnlySpan<byte> QueryTransformFeedbackVaryingLocations(int bufferIndex)
        {
            return _tfd[bufferIndex].VaryingLocations;
        }

        /// <summary>
        /// Queries the stride (in bytes) of the per vertex data written into the transform feedback buffer.
        /// </summary>
        /// <param name="bufferIndex">Index of the transform feedback buffer</param>
        /// <returns>Stride for the specified buffer</returns>
        public int QueryTransformFeedbackStride(int bufferIndex)
        {
            return _tfd[bufferIndex].Stride;
        }

        /// <summary>
        /// Queries if host state forces early depth testing.
        /// </summary>
        /// <returns>True if early depth testing is forced</returns>
        public bool QueryEarlyZForce()
        {
            return (_header.StateFlags & GuestGpuStateFlags.EarlyZForce) != 0;
        }
    }
}
