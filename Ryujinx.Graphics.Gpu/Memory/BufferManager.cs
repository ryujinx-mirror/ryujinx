using Ryujinx.Common;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.State;
using Ryujinx.Graphics.Shader;
using Ryujinx.Memory.Range;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ryujinx.Graphics.Gpu.Memory
{
    /// <summary>
    /// Buffer manager.
    /// </summary>
    class BufferManager
    {
        private const int StackToHeapThreshold = 16;

        private const int OverlapsBufferInitialCapacity = 10;
        private const int OverlapsBufferMaxCapacity     = 10000;

        private const ulong BufferAlignmentSize = 0x1000;
        private const ulong BufferAlignmentMask = BufferAlignmentSize - 1;

        private GpuContext _context;

        private RangeList<Buffer> _buffers;

        private Buffer[] _bufferOverlaps;

        private IndexBuffer _indexBuffer;
        private VertexBuffer[] _vertexBuffers;
        private BufferBounds[] _transformFeedbackBuffers;

        /// <summary>
        /// Holds shader stage buffer state and binding information.
        /// </summary>
        private class BuffersPerStage
        {
            /// <summary>
            /// Shader buffer binding information.
            /// </summary>
            public BufferDescriptor[] Bindings { get; }

            /// <summary>
            /// Buffer regions.
            /// </summary>
            public BufferBounds[] Buffers { get; }

            /// <summary>
            /// Total amount of buffers used on the shader.
            /// </summary>
            public int Count { get; private set; }

            /// <summary>
            /// Creates a new instance of the shader stage buffer information.
            /// </summary>
            /// <param name="count">Maximum amount of buffers that the shader stage can use</param>
            public BuffersPerStage(int count)
            {
                Bindings = new BufferDescriptor[count];
                Buffers = new BufferBounds[count];
            }

            /// <summary>
            /// Sets the region of a buffer at a given slot.
            /// </summary>
            /// <param name="index">Buffer slot</param>
            /// <param name="address">Region virtual address</param>
            /// <param name="size">Region size in bytes</param>
            /// <param name="flags">Buffer usage flags</param>
            public void SetBounds(int index, ulong address, ulong size, BufferUsageFlags flags = BufferUsageFlags.None)
            {
                Buffers[index] = new BufferBounds(address, size, flags);
            }

            /// <summary>
            /// Sets shader buffer binding information.
            /// </summary>
            /// <param name="descriptors">Buffer binding information</param>
            public void SetBindings(ReadOnlyCollection<BufferDescriptor> descriptors)
            {
                if (descriptors == null)
                {
                    Count = 0;
                    return;
                }

                descriptors.CopyTo(Bindings, 0);
                Count = descriptors.Count;
            }
        }

        private BuffersPerStage   _cpStorageBuffers;
        private BuffersPerStage   _cpUniformBuffers;
        private BuffersPerStage[] _gpStorageBuffers;
        private BuffersPerStage[] _gpUniformBuffers;

        private int _cpStorageBufferBindings;
        private int _cpUniformBufferBindings;
        private int _gpStorageBufferBindings;
        private int _gpUniformBufferBindings;

        private bool _gpStorageBuffersDirty;
        private bool _gpUniformBuffersDirty;

        private bool _indexBufferDirty;
        private bool _vertexBuffersDirty;
        private uint _vertexBuffersEnableMask;
        private bool _transformFeedbackBuffersDirty;

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

            _transformFeedbackBuffers = new BufferBounds[Constants.TotalTransformFeedbackBuffers];

            _cpStorageBuffers = new BuffersPerStage(Constants.TotalCpStorageBuffers);
            _cpUniformBuffers = new BuffersPerStage(Constants.TotalCpUniformBuffers);

            _gpStorageBuffers = new BuffersPerStage[Constants.ShaderStages];
            _gpUniformBuffers = new BuffersPerStage[Constants.ShaderStages];

            for (int index = 0; index < Constants.ShaderStages; index++)
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
        /// Sets a new index buffer that overrides the one set on the call to <see cref="CommitGraphicsBindings"/>.
        /// </summary>
        /// <param name="buffer">Buffer to be used as index buffer</param>
        /// <param name="type">Type of each index buffer element</param>
        public void SetIndexBuffer(BufferRange buffer, IndexType type)
        {
            _context.Renderer.Pipeline.SetIndexBuffer(buffer, type);

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
        /// Sets a transform feedback buffer on the graphics pipeline.
        /// The output from the vertex transformation stages are written into the feedback buffer.
        /// </summary>
        /// <param name="index">Index of the transform feedback buffer</param>
        /// <param name="gpuVa">Start GPU virtual address of the buffer</param>
        /// <param name="size">Size in bytes of the transform feedback buffer</param>
        public void SetTransformFeedbackBuffer(int index, ulong gpuVa, ulong size)
        {
            ulong address = TranslateAndCreateBuffer(gpuVa, size);

            _transformFeedbackBuffers[index] = new BufferBounds(address, size);
            _transformFeedbackBuffersDirty = true;
        }

        /// <summary>
        /// Sets a storage buffer on the compute pipeline.
        /// Storage buffers can be read and written to on shaders.
        /// </summary>
        /// <param name="index">Index of the storage buffer</param>
        /// <param name="gpuVa">Start GPU virtual address of the buffer</param>
        /// <param name="size">Size in bytes of the storage buffer</param>
        /// <param name="flags">Buffer usage flags</param>
        public void SetComputeStorageBuffer(int index, ulong gpuVa, ulong size, BufferUsageFlags flags)
        {
            size += gpuVa & ((ulong)_context.Capabilities.StorageBufferOffsetAlignment - 1);

            gpuVa = BitUtils.AlignDown(gpuVa, _context.Capabilities.StorageBufferOffsetAlignment);

            ulong address = TranslateAndCreateBuffer(gpuVa, size);

            _cpStorageBuffers.SetBounds(index, address, size, flags);
        }

        /// <summary>
        /// Sets a storage buffer on the graphics pipeline.
        /// Storage buffers can be read and written to on shaders.
        /// </summary>
        /// <param name="stage">Index of the shader stage</param>
        /// <param name="index">Index of the storage buffer</param>
        /// <param name="gpuVa">Start GPU virtual address of the buffer</param>
        /// <param name="size">Size in bytes of the storage buffer</param>
        /// <param name="flags">Buffer usage flags</param>
        public void SetGraphicsStorageBuffer(int stage, int index, ulong gpuVa, ulong size, BufferUsageFlags flags)
        {
            size += gpuVa & ((ulong)_context.Capabilities.StorageBufferOffsetAlignment - 1);

            gpuVa = BitUtils.AlignDown(gpuVa, _context.Capabilities.StorageBufferOffsetAlignment);

            ulong address = TranslateAndCreateBuffer(gpuVa, size);

            if (_gpStorageBuffers[stage].Buffers[index].Address != address ||
                _gpStorageBuffers[stage].Buffers[index].Size    != size)
            {
                _gpStorageBuffersDirty = true;
            }

            _gpStorageBuffers[stage].SetBounds(index, address, size, flags);
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

            _cpUniformBuffers.SetBounds(index, address, size);
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

            _gpUniformBuffers[stage].SetBounds(index, address, size);
            _gpUniformBuffersDirty = true;
        }

        /// <summary>
        /// Sets the binding points for the storage buffers bound on the compute pipeline.
        /// </summary>
        /// <param name="descriptors">Buffer descriptors with the binding point values</param>
        public void SetComputeStorageBufferBindings(ReadOnlyCollection<BufferDescriptor> descriptors)
        {
            _cpStorageBuffers.SetBindings(descriptors);
            _cpStorageBufferBindings = descriptors.Count != 0 ? descriptors.Max(x => x.Binding) + 1 : 0;
        }

        /// <summary>
        /// Sets the binding points for the storage buffers bound on the graphics pipeline.
        /// </summary>
        /// <param name="stage">Index of the shader stage</param>
        /// <param name="descriptors">Buffer descriptors with the binding point values</param>
        public void SetGraphicsStorageBufferBindings(int stage, ReadOnlyCollection<BufferDescriptor> descriptors)
        {
            _gpStorageBuffers[stage].SetBindings(descriptors);
            _gpStorageBuffersDirty = true;
        }

        /// <summary>
        /// Sets the total number of storage buffer bindings used.
        /// </summary>
        /// <param name="count">Number of storage buffer bindings used</param>
        public void SetGraphicsStorageBufferBindingsCount(int count)
        {
            _gpStorageBufferBindings = count;
        }

        /// <summary>
        /// Sets the binding points for the uniform buffers bound on the compute pipeline.
        /// </summary>
        /// <param name="descriptors">Buffer descriptors with the binding point values</param>
        public void SetComputeUniformBufferBindings(ReadOnlyCollection<BufferDescriptor> descriptors)
        {
            _cpUniformBuffers.SetBindings(descriptors);
            _cpUniformBufferBindings = descriptors.Count != 0 ? descriptors.Max(x => x.Binding) + 1 : 0;
        }

        /// <summary>
        /// Sets the enabled uniform buffers mask on the graphics pipeline.
        /// Each bit set on the mask indicates that the respective buffer index is enabled.
        /// </summary>
        /// <param name="stage">Index of the shader stage</param>
        /// <param name="descriptors">Buffer descriptors with the binding point values</param>
        public void SetGraphicsUniformBufferBindings(int stage, ReadOnlyCollection<BufferDescriptor> descriptors)
        {
            _gpUniformBuffers[stage].SetBindings(descriptors);
            _gpUniformBuffersDirty = true;
        }

        /// <summary>
        /// Sets the total number of uniform buffer bindings used.
        /// </summary>
        /// <param name="count">Number of uniform buffer bindings used</param>
        public void SetGraphicsUniformBufferBindingsCount(int count)
        {
            _gpUniformBufferBindings = count;
        }

        /// <summary>
        /// Gets a bit mask indicating which compute uniform buffers are currently bound.
        /// </summary>
        /// <returns>Mask where each bit set indicates a bound constant buffer</returns>
        public uint GetComputeUniformBufferUseMask()
        {
            uint mask = 0;

            for (int i = 0; i < _cpUniformBuffers.Buffers.Length; i++)
            {
                if (_cpUniformBuffers.Buffers[i].Address != 0)
                {
                    mask |= 1u << i;
                }
            }

            return mask;
        }

        /// <summary>
        /// Gets a bit mask indicating which graphics uniform buffers are currently bound.
        /// </summary>
        /// <param name="stage">Index of the shader stage</param>
        /// <returns>Mask where each bit set indicates a bound constant buffer</returns>
        public uint GetGraphicsUniformBufferUseMask(int stage)
        {
            uint mask = 0;

            for (int i = 0; i < _gpUniformBuffers[stage].Buffers.Length; i++)
            {
                if (_gpUniformBuffers[stage].Buffers[i].Address != 0)
                {
                    mask |= 1u << i;
                }
            }

            return mask;
        }

        /// <summary>
        /// Handles removal of buffers written to a memory region being unmapped.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        public void MemoryUnmappedHandler(object sender, UnmapEventArgs e)
        {
            Buffer[] overlaps = new Buffer[10];
            int overlapCount;

            ulong address = _context.MemoryManager.Translate(e.Address);
            ulong size = e.Size;

            lock (_buffers)
            {
                overlapCount = _buffers.FindOverlaps(address, size, ref overlaps);
            }

            for (int i = 0; i < overlapCount; i++)
            {
                overlaps[i].Unmapped(address, size);
            }
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

            if (address == MemoryManager.PteUnmapped)
            {
                return 0;
            }

            CreateBuffer(address, size);

            return address;
        }

        /// <summary>
        /// Creates a new buffer for the specified range, if it does not yet exist.
        /// This can be used to ensure the existance of a buffer.
        /// </summary>
        /// <param name="address">Address of the buffer in memory</param>
        /// <param name="size">Size of the buffer in bytes</param>
        public void CreateBuffer(ulong address, ulong size)
        {
            ulong endAddress = address + size;

            ulong alignedAddress = address & ~BufferAlignmentMask;

            ulong alignedEndAddress = (endAddress + BufferAlignmentMask) & ~BufferAlignmentMask;

            // The buffer must have the size of at least one page.
            if (alignedEndAddress == alignedAddress)
            {
                alignedEndAddress += BufferAlignmentSize;
            }

            CreateBufferAligned(alignedAddress, alignedEndAddress - alignedAddress);
        }

        /// <summary>
        /// Creates a new buffer for the specified range, if needed.
        /// If a buffer where this range can be fully contained already exists,
        /// then the creation of a new buffer is not necessary.
        /// </summary>
        /// <param name="address">Address of the buffer in guest memory</param>
        /// <param name="size">Size in bytes of the buffer</param>
        private void CreateBufferAligned(ulong address, ulong size)
        {
            int overlapsCount;

            lock (_buffers)
            {
                overlapsCount = _buffers.FindOverlapsNonOverlapping(address, size, ref _bufferOverlaps);
            }

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

                        lock (_buffers)
                        {
                            _buffers.Remove(buffer);
                        }
                    }

                    Buffer newBuffer = new Buffer(_context, address, endAddress - address);
                    newBuffer.SynchronizeMemory(address, endAddress - address);

                    lock (_buffers)
                    {
                        _buffers.Add(newBuffer);
                    }

                    for (int index = 0; index < overlapsCount; index++)
                    {
                        Buffer buffer = _bufferOverlaps[index];

                        int dstOffset = (int)(buffer.Address - newBuffer.Address);

                        buffer.SynchronizeMemory(buffer.Address, buffer.Size);

                        buffer.CopyTo(newBuffer, dstOffset);
                        newBuffer.InheritModifiedRanges(buffer);

                        buffer.Dispose();
                    }

                    // Existing buffers were modified, we need to rebind everything.
                    _rebind = true;
                }
            }
            else
            {
                // No overlap, just create a new buffer.
                Buffer buffer = new Buffer(_context, address, size);

                lock (_buffers)
                {
                    _buffers.Add(buffer);
                }
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
        /// <returns>The uniform buffer address, or an undefined value if the buffer is not currently bound</returns>
        public ulong GetComputeUniformBufferAddress(int index)
        {
            return _cpUniformBuffers.Buffers[index].Address;
        }

        /// <summary>
        /// Gets the address of the graphics uniform buffer currently bound at the given index.
        /// </summary>
        /// <param name="stage">Index of the shader stage</param>
        /// <param name="index">Index of the uniform buffer binding</param>
        /// <returns>The uniform buffer address, or an undefined value if the buffer is not currently bound</returns>
        public ulong GetGraphicsUniformBufferAddress(int stage, int index)
        {
            return _gpUniformBuffers[stage].Buffers[index].Address;
        }

        /// <summary>
        /// Ensures that the compute engine bindings are visible to the host GPU.
        /// Note: this actually performs the binding using the host graphics API.
        /// </summary>
        public void CommitComputeBindings()
        {
            int sCount = _cpStorageBufferBindings;

            Span<BufferRange> sRanges = sCount < StackToHeapThreshold ? stackalloc BufferRange[sCount] : new BufferRange[sCount];

            for (int index = 0; index < _cpStorageBuffers.Count; index++)
            {
                ref var bindingInfo = ref _cpStorageBuffers.Bindings[index];

                BufferBounds bounds = _cpStorageBuffers.Buffers[bindingInfo.Slot];

                if (bounds.Address != 0)
                {
                    // The storage buffer size is not reliable (it might be lower than the actual size),
                    // so we bind the entire buffer to allow otherwise out of range accesses to work.
                    sRanges[bindingInfo.Binding] = GetBufferRangeTillEnd(
                        bounds.Address,
                        bounds.Size,
                        bounds.Flags.HasFlag(BufferUsageFlags.Write));
                }
            }

            _context.Renderer.Pipeline.SetStorageBuffers(sRanges);

            int uCount = _cpUniformBufferBindings;

            Span<BufferRange> uRanges = uCount < StackToHeapThreshold ? stackalloc BufferRange[uCount] : new BufferRange[uCount];

            for (int index = 0; index < _cpUniformBuffers.Count; index++)
            {
                ref var bindingInfo = ref _cpUniformBuffers.Bindings[index];

                BufferBounds bounds = _cpUniformBuffers.Buffers[bindingInfo.Slot];

                if (bounds.Address != 0)
                {
                    uRanges[bindingInfo.Binding] = GetBufferRange(bounds.Address, bounds.Size);
                }
            }

            _context.Renderer.Pipeline.SetUniformBuffers(uRanges);

            // Force rebind after doing compute work.
            _rebind = true;
        }

        /// <summary>
        /// Ensures that the graphics engine bindings are visible to the host GPU.
        /// Note: this actually performs the binding using the host graphics API.
        /// </summary>
        public void CommitGraphicsBindings()
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

                Span<VertexBufferDescriptor> vertexBuffers = stackalloc VertexBufferDescriptor[Constants.TotalVertexBuffers];

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

            if (_transformFeedbackBuffersDirty || _rebind)
            {
                _transformFeedbackBuffersDirty = false;

                Span<BufferRange> tfbs = stackalloc BufferRange[Constants.TotalTransformFeedbackBuffers];

                for (int index = 0; index < Constants.TotalTransformFeedbackBuffers; index++)
                {
                    BufferBounds tfb = _transformFeedbackBuffers[index];

                    if (tfb.Address == 0)
                    {
                        tfbs[index] = BufferRange.Empty;
                        continue;
                    }

                    tfbs[index] = GetBufferRange(tfb.Address, tfb.Size);
                }

                _context.Renderer.Pipeline.SetTransformFeedbackBuffers(tfbs);
            }
            else
            {
                for (int index = 0; index < Constants.TotalTransformFeedbackBuffers; index++)
                {
                    BufferBounds tfb = _transformFeedbackBuffers[index];

                    if (tfb.Address == 0)
                    {
                        continue;
                    }

                    SynchronizeBufferRange(tfb.Address, tfb.Size);
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
            int count = isStorage ? _gpStorageBufferBindings : _gpUniformBufferBindings;

            Span<BufferRange> ranges = count < StackToHeapThreshold ? stackalloc BufferRange[count] : new BufferRange[count];

            for (ShaderStage stage = ShaderStage.Vertex; stage <= ShaderStage.Fragment; stage++)
            {
                ref var buffers = ref bindings[(int)stage - 1];

                for (int index = 0; index < buffers.Count; index++)
                {
                    ref var bindingInfo = ref buffers.Bindings[index];

                    BufferBounds bounds = buffers.Buffers[bindingInfo.Slot];

                    if (bounds.Address != 0)
                    {
                        ranges[bindingInfo.Binding] = isStorage
                            ? GetBufferRangeTillEnd(bounds.Address, bounds.Size, bounds.Flags.HasFlag(BufferUsageFlags.Write))
                            : GetBufferRange(bounds.Address, bounds.Size, bounds.Flags.HasFlag(BufferUsageFlags.Write));
                    }
                }
            }

            if (isStorage)
            {
                _context.Renderer.Pipeline.SetStorageBuffers(ranges);
            }
            else
            {
                _context.Renderer.Pipeline.SetUniformBuffers(ranges);
            }
        }

        /// <summary>
        /// Updates data for the already bound buffer bindings.
        /// </summary>
        /// <param name="bindings">Bindings to update</param>
        private void UpdateBuffers(BuffersPerStage[] bindings)
        {
            for (ShaderStage stage = ShaderStage.Vertex; stage <= ShaderStage.Fragment; stage++)
            {
                ref var buffers = ref bindings[(int)stage - 1];

                for (int index = 0; index < buffers.Count; index++)
                {
                    ref var binding = ref buffers.Bindings[index];

                    BufferBounds bounds = buffers.Buffers[binding.Slot];

                    if (bounds.Address == 0)
                    {
                        continue;
                    }

                    SynchronizeBufferRange(bounds.Address, bounds.Size);
                }
            }
        }

        /// <summary>
        /// Sets the buffer storage of a buffer texture.
        /// </summary>
        /// <param name="texture">Buffer texture</param>
        /// <param name="address">Address of the buffer in memory</param>
        /// <param name="size">Size of the buffer in bytes</param>
        /// <param name="compute">Indicates if the buffer texture belongs to the compute or graphics pipeline</param>
        public void SetBufferTextureStorage(ITexture texture, ulong address, ulong size, bool compute)
        {
            CreateBuffer(address, size);

            if (_rebind)
            {
                // We probably had to modify existing buffers to create the texture buffer,
                // so rebind everything to ensure we're using the new buffers for all bound resources.
                if (compute)
                {
                    CommitComputeBindings();
                }
                else
                {
                    CommitGraphicsBindings();
                }
            }

            texture.SetStorage(GetBufferRange(address, size));
        }

        /// <summary>
        /// Copy a buffer data from a given address to another.
        /// </summary>
        /// <remarks>
        /// This does a GPU side copy.
        /// </remarks>
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

            _context.Renderer.Pipeline.CopyBuffer(
                srcBuffer.Handle,
                dstBuffer.Handle,
                srcOffset,
                dstOffset,
                (int)size);

            if (srcBuffer.IsModified(srcAddress, size))
            {
                dstBuffer.SignalModified(dstAddress, size);
            }
            else
            {
                // Optimization: If the data being copied is already in memory, then copy it directly instead of flushing from GPU.

                dstBuffer.ClearModified(dstAddress, size);
                _context.PhysicalMemory.WriteUntracked(dstAddress, _context.PhysicalMemory.GetSpan(srcAddress, (int)size));
            }
        }

        /// <summary>
        /// Clears a buffer at a given address with the specified value.
        /// </summary>
        /// <remarks>
        /// Both the address and size must be aligned to 4 bytes.
        /// </remarks>
        /// <param name="gpuVa">GPU virtual address of the region to clear</param>
        /// <param name="size">Number of bytes to clear</param>
        /// <param name="value">Value to be written into the buffer</param>
        public void ClearBuffer(GpuVa gpuVa, ulong size, uint value)
        {
            ulong address = TranslateAndCreateBuffer(gpuVa.Pack(), size);

            Buffer buffer = GetBuffer(address, size);

            int offset = (int)(address - buffer.Address);

            _context.Renderer.Pipeline.ClearBuffer(buffer.Handle, offset, (int)size, value);

            buffer.SignalModified(address, size);
        }

        /// <summary>
        /// Gets a buffer sub-range starting at a given memory address.
        /// </summary>
        /// <param name="address">Start address of the memory range</param>
        /// <param name="size">Size in bytes of the memory range</param>
        /// <param name="write">Whether the buffer will be written to by this use</param>
        /// <returns>The buffer sub-range starting at the given memory address</returns>
        private BufferRange GetBufferRangeTillEnd(ulong address, ulong size, bool write = false)
        {
            return GetBuffer(address, size, write).GetRange(address);
        }

        /// <summary>
        /// Gets a buffer sub-range for a given memory range.
        /// </summary>
        /// <param name="address">Start address of the memory range</param>
        /// <param name="size">Size in bytes of the memory range</param>
        /// <param name="write">Whether the buffer will be written to by this use</param>
        /// <returns>The buffer sub-range for the given range</returns>
        private BufferRange GetBufferRange(ulong address, ulong size, bool write = false)
        {
            return GetBuffer(address, size, write).GetRange(address, size);
        }

        /// <summary>
        /// Gets a buffer for a given memory range.
        /// A buffer overlapping with the specified range is assumed to already exist on the cache.
        /// </summary>
        /// <param name="address">Start address of the memory range</param>
        /// <param name="size">Size in bytes of the memory range</param>
        /// <param name="write">Whether the buffer will be written to by this use</param>
        /// <returns>The buffer where the range is fully contained</returns>
        private Buffer GetBuffer(ulong address, ulong size, bool write = false)
        {
            Buffer buffer;

            if (size != 0)
            {
                lock (_buffers)
                {
                    buffer = _buffers.FindFirstOverlap(address, size);
                }

                buffer.SynchronizeMemory(address, size);

                if (write)
                {
                    buffer.SignalModified(address, size);
                }
            }
            else
            {
                lock (_buffers)
                {
                    buffer = _buffers.FindFirstOverlap(address, 1);
                }
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
                Buffer buffer;

                lock (_buffers)
                {
                    buffer = _buffers.FindFirstOverlap(address, size);
                }

                buffer.SynchronizeMemory(address, size);
            }
        }

        /// <summary>
        /// Disposes all buffers in the cache.
        /// It's an error to use the buffer manager after disposal.
        /// </summary>
        public void Dispose()
        {
            lock (_buffers)
            {
                foreach (Buffer buffer in _buffers)
                {
                    buffer.Dispose();
                }
            }
        }
    }
}