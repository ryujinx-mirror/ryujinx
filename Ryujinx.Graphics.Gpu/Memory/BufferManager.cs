using Ryujinx.Common;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.State;
using Ryujinx.Graphics.Shader;
using System;

namespace Ryujinx.Graphics.Gpu.Memory
{
    /// <summary>
    /// Buffer manager.
    /// </summary>
    class BufferManager
    {
        private const int OverlapsBufferInitialCapacity = 10;
        private const int OverlapsBufferMaxCapacity     = 10000;

        private const ulong BufferAlignmentSize = 0x1000;
        private const ulong BufferAlignmentMask = BufferAlignmentSize - 1;

        private GpuContext _context;

        private RangeList<Buffer> _buffers;

        private Buffer[] _bufferOverlaps;

        private IndexBuffer _indexBuffer;

        private VertexBuffer[] _vertexBuffers;

        private class BuffersPerStage
        {
            public uint EnableMask { get; set; }

            public BufferBounds[] Buffers { get; }

            public BuffersPerStage(int count)
            {
                Buffers = new BufferBounds[count];
            }

            public void Bind(int index, ulong address, ulong size)
            {
                Buffers[index].Address = address;
                Buffers[index].Size    = size;
            }
        }

        private BuffersPerStage   _cpStorageBuffers;
        private BuffersPerStage   _cpUniformBuffers;
        private BuffersPerStage[] _gpStorageBuffers;
        private BuffersPerStage[] _gpUniformBuffers;

        private bool _gpStorageBuffersDirty;
        private bool _gpUniformBuffersDirty;

        private bool _indexBufferDirty;
        private bool _vertexBuffersDirty;
        private uint _vertexBuffersEnableMask;

        private bool _rebind;

        /// <summary>
        /// Creates a new instance of the buffer manager.
        /// </summary>
        /// <param name="context">The GPU context that the buffer manager belongs to</param>
        public BufferManager(GpuContext context)
        {
            _context = context;

            _buffers = new RangeList<Buffer>();

            _bufferOverlaps = new Buffer[OverlapsBufferInitialCapacity];

            _vertexBuffers = new VertexBuffer[Constants.TotalVertexBuffers];

            _cpStorageBuffers = new BuffersPerStage(Constants.TotalCpStorageBuffers);
            _cpUniformBuffers = new BuffersPerStage(Constants.TotalCpUniformBuffers);

            _gpStorageBuffers = new BuffersPerStage[Constants.TotalShaderStages];
            _gpUniformBuffers = new BuffersPerStage[Constants.TotalShaderStages];

            for (int index = 0; index < Constants.TotalShaderStages; index++)
            {
                _gpStorageBuffers[index] = new BuffersPerStage(Constants.TotalGpStorageBuffers);
                _gpUniformBuffers[index] = new BuffersPerStage(Constants.TotalGpUniformBuffers);
            }
        }

        /// <summary>
        /// Sets the memory range with the index buffer data, to be used for subsequent draw calls.
        /// </summary>
        /// <param name="gpuVa">Start GPU virtual address of the index buffer</param>
        /// <param name="size">Size, in bytes, of the index buffer</param>
        /// <param name="type">Type of each index buffer element</param>
        public void SetIndexBuffer(ulong gpuVa, ulong size, IndexType type)
        {
            ulong address = TranslateAndCreateBuffer(gpuVa, size);

            _indexBuffer.Address = address;
            _indexBuffer.Size    = size;
            _indexBuffer.Type    = type;

            _indexBufferDirty = true;
        }

        /// <summary>
        /// Sets the memory range with vertex buffer data, to be used for subsequent draw calls.
        /// </summary>
        /// <param name="index">Index of the vertex buffer (up to 16)</param>
        /// <param name="gpuVa">GPU virtual address of the buffer</param>
        /// <param name="size">Size in bytes of the buffer</param>
        /// <param name="stride">Stride of the buffer, defined as the number of bytes of each vertex</param>
        /// <param name="divisor">Vertex divisor of the buffer, for instanced draws</param>
        public void SetVertexBuffer(int index, ulong gpuVa, ulong size, int stride, int divisor)
        {
            ulong address = TranslateAndCreateBuffer(gpuVa, size);

            _vertexBuffers[index].Address = address;
            _vertexBuffers[index].Size    = size;
            _vertexBuffers[index].Stride  = stride;
            _vertexBuffers[index].Divisor = divisor;

            _vertexBuffersDirty = true;

            if (address != 0)
            {
                _vertexBuffersEnableMask |= 1u << index;
            }
            else
            {
                _vertexBuffersEnableMask &= ~(1u << index);
            }
        }

        /// <summary>
        /// Sets a storage buffer on the compute pipeline.
        /// Storage buffers can be read and written to on shaders.
        /// </summary>
        /// <param name="index">Index of the storage buffer</param>
        /// <param name="gpuVa">Start GPU virtual address of the buffer</param>
        /// <param name="size">Size in bytes of the storage buffer</param>
        public void SetComputeStorageBuffer(int index, ulong gpuVa, ulong size)
        {
            size += gpuVa & ((ulong)_context.Capabilities.StorageBufferOffsetAlignment - 1);

            gpuVa = BitUtils.AlignDown(gpuVa, _context.Capabilities.StorageBufferOffsetAlignment);

            ulong address = TranslateAndCreateBuffer(gpuVa, size);

            _cpStorageBuffers.Bind(index, address, size);
        }

        /// <summary>
        /// Sets a storage buffer on the graphics pipeline.
        /// Storage buffers can be read and written to on shaders.
        /// </summary>
        /// <param name="stage">Index of the shader stage</param>
        /// <param name="index">Index of the storage buffer</param>
        /// <param name="gpuVa">Start GPU virtual address of the buffer</param>
        /// <param name="size">Size in bytes of the storage buffer</param>
        public void SetGraphicsStorageBuffer(int stage, int index, ulong gpuVa, ulong size)
        {
            size += gpuVa & ((ulong)_context.Capabilities.StorageBufferOffsetAlignment - 1);

            gpuVa = BitUtils.AlignDown(gpuVa, _context.Capabilities.StorageBufferOffsetAlignment);

            ulong address = TranslateAndCreateBuffer(gpuVa, size);

            if (_gpStorageBuffers[stage].Buffers[index].Address != address ||
                _gpStorageBuffers[stage].Buffers[index].Size    != size)
            {
                _gpStorageBuffersDirty = true;
            }

            _gpStorageBuffers[stage].Bind(index, address, size);
        }

        /// <summary>
        /// Sets a uniform buffer on the compute pipeline.
        /// Uniform buffers are read-only from shaders, and have a small capacity.
        /// </summary>
        /// <param name="index">Index of the uniform buffer</param>
        /// <param name="gpuVa">Start GPU virtual address of the buffer</param>
        /// <param name="size">Size in bytes of the storage buffer</param>
        public void SetComputeUniformBuffer(int index, ulong gpuVa, ulong size)
        {
            ulong address = TranslateAndCreateBuffer(gpuVa, size);

            _cpUniformBuffers.Bind(index, address, size);
        }

        /// <summary>
        /// Sets a uniform buffer on the graphics pipeline.
        /// Uniform buffers are read-only from shaders, and have a small capacity.
        /// </summary>
        /// <param name="stage">Index of the shader stage</param>
        /// <param name="index">Index of the uniform buffer</param>
        /// <param name="gpuVa">Start GPU virtual address of the buffer</param>
        /// <param name="size">Size in bytes of the storage buffer</param>
        public void SetGraphicsUniformBuffer(int stage, int index, ulong gpuVa, ulong size)
        {
            ulong address = TranslateAndCreateBuffer(gpuVa, size);

            _gpUniformBuffers[stage].Bind(index, address, size);

            _gpUniformBuffersDirty = true;
        }

        /// <summary>
        /// Sets the enabled storage buffers mask on the compute pipeline.
        /// Each bit set on the mask indicates that the respective buffer index is enabled.
        /// </summary>
        /// <param name="mask">Buffer enable mask</param>
        public void SetComputeStorageBufferEnableMask(uint mask)
        {
            _cpStorageBuffers.EnableMask = mask;
        }

        /// <summary>
        /// Sets the enabled storage buffers mask on the graphics pipeline.
        /// Each bit set on the mask indicates that the respective buffer index is enabled.
        /// </summary>
        /// <param name="stage">Index of the shader stage</param>
        /// <param name="mask">Buffer enable mask</param>
        public void SetGraphicsStorageBufferEnableMask(int stage, uint mask)
        {
            _gpStorageBuffers[stage].EnableMask = mask;

            _gpStorageBuffersDirty = true;
        }

        /// <summary>
        /// Sets the enabled uniform buffers mask on the compute pipeline.
        /// Each bit set on the mask indicates that the respective buffer index is enabled.
        /// </summary>
        /// <param name="mask">Buffer enable mask</param>
        public void SetComputeUniformBufferEnableMask(uint mask)
        {
            _cpUniformBuffers.EnableMask = mask;
        }

        /// <summary>
        /// Sets the enabled uniform buffers mask on the graphics pipeline.
        /// Each bit set on the mask indicates that the respective buffer index is enabled.
        /// </summary>
        /// <param name="stage">Index of the shader stage</param>
        /// <param name="mask">Buffer enable mask</param>
        public void SetGraphicsUniformBufferEnableMask(int stage, uint mask)
        {
            _gpUniformBuffers[stage].EnableMask = mask;

            _gpUniformBuffersDirty = true;
        }

        /// <summary>
        /// Performs address translation of the GPU virtual address, and creates a
        /// new buffer, if needed, for the specified range.
        /// </summary>
        /// <param name="gpuVa">Start GPU virtual address of the buffer</param>
        /// <param name="size">Size in bytes of the buffer</param>
        /// <returns>CPU virtual address of the buffer, after address translation</returns>
        private ulong TranslateAndCreateBuffer(ulong gpuVa, ulong size)
        {
            if (gpuVa == 0)
            {
                return 0;
            }

            ulong address = _context.MemoryManager.Translate(gpuVa);

            if (address == MemoryManager.BadAddress)
            {
                return 0;
            }

            ulong endAddress = address + size;

            ulong alignedAddress = address & ~BufferAlignmentMask;

            ulong alignedEndAddress = (endAddress + BufferAlignmentMask) & ~BufferAlignmentMask;

            // The buffer must have the size of at least one page.
            if (alignedEndAddress == alignedAddress)
            {
                alignedEndAddress += BufferAlignmentSize;
            }

            CreateBuffer(alignedAddress, alignedEndAddress - alignedAddress);

            return address;
        }

        /// <summary>
        /// Creates a new buffer for the specified range, if needed.
        /// If a buffer where this range can be fully contained already exists,
        /// then the creation of a new buffer is not necessary.
        /// </summary>
        /// <param name="address">Address of the buffer in guest memory</param>
        /// <param name="size">Size in bytes of the buffer</param>
        private void CreateBuffer(ulong address, ulong size)
        {
            int overlapsCount = _buffers.FindOverlapsNonOverlapping(address, size, ref _bufferOverlaps);

            if (overlapsCount != 0)
            {
                // The buffer already exists. We can just return the existing buffer
                // if the buffer we need is fully contained inside the overlapping buffer.
                // Otherwise, we must delete the overlapping buffers and create a bigger buffer
                // that fits all the data we need. We also need to copy the contents from the
                // old buffer(s) to the new buffer.
                ulong endAddress = address + size;

                if (_bufferOverlaps[0].Address > address || _bufferOverlaps[0].EndAddress < endAddress)
                {
                    for (int index = 0; index < overlapsCount; index++)
                    {
                        Buffer buffer = _bufferOverlaps[index];

                        address    = Math.Min(address,    buffer.Address);
                        endAddress = Math.Max(endAddress, buffer.EndAddress);

                        buffer.SynchronizeMemory(buffer.Address, buffer.Size);

                        _buffers.Remove(buffer);
                    }

                    Buffer newBuffer = new Buffer(_context, address, endAddress - address);

                    _buffers.Add(newBuffer);

                    for (int index = 0; index < overlapsCount; index++)
                    {
                        Buffer buffer = _bufferOverlaps[index];

                        int dstOffset = (int)(buffer.Address - newBuffer.Address);

                        buffer.CopyTo(newBuffer, dstOffset);

                        buffer.Dispose();
                    }

                    _rebind = true;
                }
            }
            else
            {
                // No overlap, just create a new buffer.
                Buffer buffer = new Buffer(_context, address, size);

                _buffers.Add(buffer);
            }

            ShrinkOverlapsBufferIfNeeded();
        }

        /// <summary>
        /// Resizes the temporary buffer used for range list intersection results, if it has grown too much.
        /// </summary>
        private void ShrinkOverlapsBufferIfNeeded()
        {
            if (_bufferOverlaps.Length > OverlapsBufferMaxCapacity)
            {
                Array.Resize(ref _bufferOverlaps, OverlapsBufferMaxCapacity);
            }
        }

        /// <summary>
        /// Gets the address of the compute uniform buffer currently bound at the given index.
        /// </summary>
        /// <param name="index">Index of the uniform buffer binding</param>
        /// <returns>The uniform buffer address, or a undefined value if the buffer is not currently bound</returns>
        public ulong GetComputeUniformBufferAddress(int index)
        {
            return _cpUniformBuffers.Buffers[index].Address;
        }

        /// <summary>
        /// Gets the address of the graphics uniform buffer currently bound at the given index.
        /// </summary>
        /// <param name="stage">Index of the shader stage</param>
        /// <param name="index">Index of the uniform buffer binding</param>
        /// <returns>The uniform buffer address, or a undefined value if the buffer is not currently bound</returns>
        public ulong GetGraphicsUniformBufferAddress(int stage, int index)
        {
            return _gpUniformBuffers[stage].Buffers[index].Address;
        }

        /// <summary>
        /// Ensures that the compute engine bindings are visible to the host GPU.
        /// This actually performs the binding using the host graphics API.
        /// </summary>
        public void CommitComputeBindings()
        {
            uint enableMask = _cpStorageBuffers.EnableMask;

            for (int index = 0; (enableMask >> index) != 0; index++)
            {
                if ((enableMask & (1u << index)) == 0)
                {
                    continue;
                }

                BufferBounds bounds = _cpStorageBuffers.Buffers[index];

                if (bounds.Address == 0)
                {
                    continue;
                }

                BufferRange buffer = GetBufferRange(bounds.Address, bounds.Size);

                _context.Renderer.Pipeline.SetStorageBuffer(index, ShaderStage.Compute, buffer);
            }

            enableMask = _cpUniformBuffers.EnableMask;

            for (int index = 0; (enableMask >> index) != 0; index++)
            {
                if ((enableMask & (1u << index)) == 0)
                {
                    continue;
                }

                BufferBounds bounds = _cpUniformBuffers.Buffers[index];

                if (bounds.Address == 0)
                {
                    continue;
                }

                BufferRange buffer = GetBufferRange(bounds.Address, bounds.Size);

                _context.Renderer.Pipeline.SetUniformBuffer(index, ShaderStage.Compute, buffer);
            }

            // Force rebind after doing compute work.
            _rebind = true;
        }

        /// <summary>
        /// Ensures that the graphics engine bindings are visible to the host GPU.
        /// This actually performs the binding using the host graphics API.
        /// </summary>
        public void CommitBindings()
        {
            if (_indexBufferDirty || _rebind)
            {
                _indexBufferDirty = false;

                if (_indexBuffer.Address != 0)
                {
                    BufferRange buffer = GetBufferRange(_indexBuffer.Address, _indexBuffer.Size);

                    _context.Renderer.Pipeline.SetIndexBuffer(buffer, _indexBuffer.Type);
                }
            }
            else if (_indexBuffer.Address != 0)
            {
                SynchronizeBufferRange(_indexBuffer.Address, _indexBuffer.Size);
            }

            uint vbEnableMask = _vertexBuffersEnableMask;

            if (_vertexBuffersDirty || _rebind)
            {
                _vertexBuffersDirty = false;

                VertexBufferDescriptor[] vertexBuffers = new VertexBufferDescriptor[Constants.TotalVertexBuffers];

                for (int index = 0; (vbEnableMask >> index) != 0; index++)
                {
                    VertexBuffer vb = _vertexBuffers[index];

                    if (vb.Address == 0)
                    {
                        continue;
                    }

                    BufferRange buffer = GetBufferRange(vb.Address, vb.Size);

                    vertexBuffers[index] = new VertexBufferDescriptor(buffer, vb.Stride, vb.Divisor);
                }

                _context.Renderer.Pipeline.SetVertexBuffers(vertexBuffers);
            }
            else
            {
                for (int index = 0; (vbEnableMask >> index) != 0; index++)
                {
                    VertexBuffer vb = _vertexBuffers[index];

                    if (vb.Address == 0)
                    {
                        continue;
                    }

                    SynchronizeBufferRange(vb.Address, vb.Size);
                }
            }

            if (_gpStorageBuffersDirty || _rebind)
            {
                _gpStorageBuffersDirty = false;

                BindBuffers(_gpStorageBuffers, isStorage: true);
            }
            else
            {
                UpdateBuffers(_gpStorageBuffers);
            }

            if (_gpUniformBuffersDirty || _rebind)
            {
                _gpUniformBuffersDirty = false;

                BindBuffers(_gpUniformBuffers, isStorage: false);
            }
            else
            {
                UpdateBuffers(_gpUniformBuffers);
            }

            _rebind = false;
        }

        /// <summary>
        /// Bind respective buffer bindings on the host API.
        /// </summary>
        /// <param name="bindings">Bindings to bind</param>
        /// <param name="isStorage">True to bind as storage buffer, false to bind as uniform buffers</param>
        private void BindBuffers(BuffersPerStage[] bindings, bool isStorage)
        {
            BindOrUpdateBuffers(bindings, bind: true, isStorage);
        }

        /// <summary>
        /// Updates data for the already bound buffer bindings.
        /// </summary>
        /// <param name="bindings">Bindings to update</param>
        private void UpdateBuffers(BuffersPerStage[] bindings)
        {
            BindOrUpdateBuffers(bindings, bind: false);
        }

        /// <summary>
        /// This binds buffer into the host API, or updates data for already bound buffers.
        /// </summary>
        /// <param name="bindings">Bindings to bind or update</param>
        /// <param name="bind">True to bind, false to update</param>
        /// <param name="isStorage">True to bind as storage buffer, false to bind as uniform buffers</param>
        private void BindOrUpdateBuffers(BuffersPerStage[] bindings, bool bind, bool isStorage = false)
        {
            for (ShaderStage stage = ShaderStage.Vertex; stage <= ShaderStage.Fragment; stage++)
            {
                uint enableMask = bindings[(int)stage - 1].EnableMask;

                if (enableMask == 0)
                {
                    continue;
                }

                for (int index = 0; (enableMask >> index) != 0; index++)
                {
                    if ((enableMask & (1u << index)) == 0)
                    {
                        continue;
                    }

                    BufferBounds bounds = bindings[(int)stage - 1].Buffers[index];

                    if (bounds.Address == 0)
                    {
                        continue;
                    }

                    if (bind)
                    {
                        BindBuffer(index, stage, bounds, isStorage);
                    }
                    else
                    {
                        SynchronizeBufferRange(bounds.Address, bounds.Size);
                    }
                }
            }
        }

        /// <summary>
        /// Binds a buffer on the host API.
        /// </summary>
        /// <param name="index">Index to bind the buffer into</param>
        /// <param name="stage">Shader stage to bind the buffer into</param>
        /// <param name="bounds">Buffer address and size</param>
        /// <param name="isStorage">True to bind as storage buffer, false to bind as uniform buffer</param>
        private void BindBuffer(int index, ShaderStage stage, BufferBounds bounds, bool isStorage)
        {
            BufferRange buffer = GetBufferRange(bounds.Address, bounds.Size);

            if (isStorage)
            {
                _context.Renderer.Pipeline.SetStorageBuffer(index, stage, buffer);
            }
            else
            {
                _context.Renderer.Pipeline.SetUniformBuffer(index, stage, buffer);
            }
        }

        /// <summary>
        /// Copy a buffer data from a given address to another.
        /// This does a GPU side copy.
        /// </summary>
        /// <param name="srcVa">GPU virtual address of the copy source</param>
        /// <param name="dstVa">GPU virtual address of the copy destination</param>
        /// <param name="size">Size in bytes of the copy</param>
        public void CopyBuffer(GpuVa srcVa, GpuVa dstVa, ulong size)
        {
            ulong srcAddress = TranslateAndCreateBuffer(srcVa.Pack(), size);
            ulong dstAddress = TranslateAndCreateBuffer(dstVa.Pack(), size);

            Buffer srcBuffer = GetBuffer(srcAddress, size);
            Buffer dstBuffer = GetBuffer(dstAddress, size);

            int srcOffset = (int)(srcAddress - srcBuffer.Address);
            int dstOffset = (int)(dstAddress - dstBuffer.Address);

            srcBuffer.HostBuffer.CopyTo(
                dstBuffer.HostBuffer,
                srcOffset,
                dstOffset,
                (int)size);

            dstBuffer.Flush(dstAddress, size);
        }

        /// <summary>
        /// Gets a buffer sub-range for a given memory range.
        /// </summary>
        /// <param name="address">Start address of the memory range</param>
        /// <param name="size">Size in bytes of the memory range</param>
        /// <returns>The buffer sub-range for the given range</returns>
        private BufferRange GetBufferRange(ulong address, ulong size)
        {
            return GetBuffer(address, size).GetRange(address, size);
        }

        /// <summary>
        /// Gets a buffer for a given memory range.
        /// A buffer overlapping with the specified range is assumed to already exist on the cache.
        /// </summary>
        /// <param name="address">Start address of the memory range</param>
        /// <param name="size">Size in bytes of the memory range</param>
        /// <returns>The buffer where the range is fully contained</returns>
        private Buffer GetBuffer(ulong address, ulong size)
        {
            Buffer buffer;

            if (size != 0)
            {
                buffer = _buffers.FindFirstOverlap(address, size);

                buffer.SynchronizeMemory(address, size);
            }
            else
            {
                buffer = _buffers.FindFirstOverlap(address, 1);
            }

            return buffer;
        }

        /// <summary>
        /// Performs guest to host memory synchronization of a given memory range.
        /// </summary>
        /// <param name="address">Start address of the memory range</param>
        /// <param name="size">Size in bytes of the memory range</param>
        private void SynchronizeBufferRange(ulong address, ulong size)
        {
            if (size != 0)
            {
                Buffer buffer = _buffers.FindFirstOverlap(address, size);

                buffer.SynchronizeMemory(address, size);
            }
        }
    }
}