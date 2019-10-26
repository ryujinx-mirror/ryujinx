using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.GAL.InputAssembler;
using Ryujinx.Graphics.Gpu.State;
using Ryujinx.Graphics.Shader;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Memory
{
    class BufferManager
    {
        private const ulong BufferAlignmentSize = 0x1000;
        private const ulong BufferAlignmentMask = BufferAlignmentSize - 1;

        private GpuContext _context;

        private RangeList<Buffer> _buffers;

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

        private bool _rebind;

        public BufferManager(GpuContext context)
        {
            _context = context;

            _buffers = new RangeList<Buffer>();

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

        public void SetIndexBuffer(ulong gpuVa, ulong size, IndexType type)
        {
            ulong address = TranslateAndCreateBuffer(gpuVa, size);

            _indexBuffer.Address = address;
            _indexBuffer.Size    = size;
            _indexBuffer.Type    = type;

            _indexBufferDirty = true;
        }

        public void SetVertexBuffer(int index, ulong gpuVa, ulong size, int stride, int divisor)
        {
            ulong address = TranslateAndCreateBuffer(gpuVa, size);

            _vertexBuffers[index].Address = address;
            _vertexBuffers[index].Size    = size;
            _vertexBuffers[index].Stride  = stride;
            _vertexBuffers[index].Divisor = divisor;

            _vertexBuffersDirty = true;
        }

        public void SetComputeStorageBuffer(int index, ulong gpuVa, ulong size)
        {
            // TODO: Improve
            size += gpuVa & 0x3fUL;

            gpuVa &= ~0x3fUL;

            ulong address = TranslateAndCreateBuffer(gpuVa, size);

            _cpStorageBuffers.Bind(index, address, size);
        }

        public void SetGraphicsStorageBuffer(int stage, int index, ulong gpuVa, ulong size)
        {
            // TODO: Improve
            size += gpuVa & 0x3fUL;

            gpuVa &= ~0x3fUL;

            ulong address = TranslateAndCreateBuffer(gpuVa, size);

            if (_gpStorageBuffers[stage].Buffers[index].Address != address ||
                _gpStorageBuffers[stage].Buffers[index].Size    != size)
            {
                _gpStorageBuffersDirty = true;
            }

            _gpStorageBuffers[stage].Bind(index, address, size);
        }

        public void SetComputeUniformBuffer(int index, ulong gpuVa, ulong size)
        {
            ulong address = TranslateAndCreateBuffer(gpuVa, size);

            _cpUniformBuffers.Bind(index, address, size);
        }

        public void SetGraphicsUniformBuffer(int stage, int index, ulong gpuVa, ulong size)
        {
            ulong address = TranslateAndCreateBuffer(gpuVa, size);

            _gpUniformBuffers[stage].Bind(index, address, size);

            _gpUniformBuffersDirty = true;
        }

        public void SetComputeStorageBufferEnableMask(uint mask)
        {
            _cpStorageBuffers.EnableMask = mask;
        }

        public void SetGraphicsStorageBufferEnableMask(int stage, uint mask)
        {
            _gpStorageBuffers[stage].EnableMask = mask;

            _gpStorageBuffersDirty = true;
        }

        public void SetComputeUniformBufferEnableMask(uint mask)
        {
            _cpUniformBuffers.EnableMask = mask;
        }

        public void SetGraphicsUniformBufferEnableMask(int stage, uint mask)
        {
            _gpUniformBuffers[stage].EnableMask = mask;

            _gpUniformBuffersDirty = true;
        }

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

        private void CreateBuffer(ulong address, ulong size)
        {
            Buffer[] overlaps = _buffers.FindOverlaps(address, size);

            if (overlaps.Length != 0)
            {
                // The buffer already exists. We can just return the existing buffer
                // if the buffer we need is fully contained inside the overlapping buffer.
                // Otherwise, we must delete the overlapping buffers and create a bigger buffer
                // that fits all the data we need. We also need to copy the contents from the
                // old buffer(s) to the new buffer.
                ulong endAddress = address + size;

                if (overlaps[0].Address > address || overlaps[0].EndAddress < endAddress)
                {
                    foreach (Buffer buffer in overlaps)
                    {
                        address    = Math.Min(address,    buffer.Address);
                        endAddress = Math.Max(endAddress, buffer.EndAddress);

                        buffer.SynchronizeMemory(buffer.Address, buffer.Size);

                        _buffers.Remove(buffer);
                    }

                    Buffer newBuffer = new Buffer(_context, address, endAddress - address);

                    _buffers.Add(newBuffer);

                    foreach (Buffer buffer in overlaps)
                    {
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
        }

        public ulong GetComputeUniformBufferAddress(int index)
        {
            return _cpUniformBuffers.Buffers[index].Address;
        }

        public ulong GetGraphicsUniformBufferAddress(int stage, int index)
        {
            return _gpUniformBuffers[stage].Buffers[index].Address;
        }

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

                _context.Renderer.Pipeline.BindStorageBuffer(index, ShaderStage.Compute, buffer);
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

                _context.Renderer.Pipeline.BindUniformBuffer(index, ShaderStage.Compute, buffer);

                if (index == 0)
                {
                    // TODO: Improve
                    Span<byte> data = _context.PhysicalMemory.Read(bounds.Address + 0x310, 0x100);

                    Span<int> words = MemoryMarshal.Cast<byte, int>(data);

                    for (int offset = 0; offset < 0x40; offset += 4)
                    {
                        words[offset] &= 0x3f;
                    }

                    buffer = GetBufferRange(bounds.Address + 0x310, 0x100);

                    buffer.Buffer.SetData(buffer.Offset, data);
                }
            }

            // Force rebind after doing compute work.
            _rebind = true;
        }

        public void CommitBindings()
        {
            if (_indexBufferDirty || _rebind)
            {
                _indexBufferDirty = false;

                if (_indexBuffer.Address != 0)
                {
                    BufferRange buffer = GetBufferRange(_indexBuffer.Address, _indexBuffer.Size);

                    _context.Renderer.Pipeline.BindIndexBuffer(buffer, _indexBuffer.Type);
                }
            }
            else if (_indexBuffer.Address != 0)
            {
                SynchronizeBufferRange(_indexBuffer.Address, _indexBuffer.Size);
            }

            if (_vertexBuffersDirty || _rebind)
            {
                _vertexBuffersDirty = false;

                VertexBufferDescriptor[] vertexBuffers = new VertexBufferDescriptor[Constants.TotalVertexBuffers];

                for (int index = 0; index < Constants.TotalVertexBuffers; index++)
                {
                    VertexBuffer vb = _vertexBuffers[index];

                    if (vb.Address == 0)
                    {
                        continue;
                    }

                    BufferRange buffer = GetBufferRange(vb.Address, vb.Size);

                    vertexBuffers[index] = new VertexBufferDescriptor(buffer, vb.Stride, vb.Divisor);
                }

                _context.Renderer.Pipeline.BindVertexBuffers(vertexBuffers);
            }
            else
            {
                for (int index = 0; index < Constants.TotalVertexBuffers; index++)
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

        private void BindBuffers(BuffersPerStage[] bindings, bool isStorage)
        {
            BindOrUpdateBuffers(bindings, bind: true, isStorage);
        }

        private void UpdateBuffers(BuffersPerStage[] bindings)
        {
            BindOrUpdateBuffers(bindings, bind: false);
        }

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

        private void BindBuffer(int index, ShaderStage stage, BufferBounds bounds, bool isStorage)
        {
            BufferRange buffer = GetBufferRange(bounds.Address, bounds.Size);

            if (isStorage)
            {
                _context.Renderer.Pipeline.BindStorageBuffer(index, stage, buffer);
            }
            else
            {
                _context.Renderer.Pipeline.BindUniformBuffer(index, stage, buffer);
            }

            if (!isStorage && index == 0)
            {
                // TODO: Improve
                Span<byte> data = _context.PhysicalMemory.Read(bounds.Address + 0x110, 0x100);

                Span<int> words = MemoryMarshal.Cast<byte, int>(data);

                for (int offset = 0; offset < 0x40; offset += 4)
                {
                    words[offset] &= 0x3f;
                }

                buffer = GetBufferRange(bounds.Address + 0x110, 0x100);

                buffer.Buffer.SetData(buffer.Offset, data);
            }
        }

        public void CopyBuffer(GpuVa srcVa, GpuVa dstVa, ulong size)
        {
            ulong srcAddress = TranslateAndCreateBuffer(srcVa.Pack(), size);
            ulong dstAddress = TranslateAndCreateBuffer(dstVa.Pack(), size);

            BufferRange srcBuffer = GetBufferRange(srcAddress, size);
            BufferRange dstBuffer = GetBufferRange(dstAddress, size);

            srcBuffer.Buffer.CopyTo(
                dstBuffer.Buffer,
                srcBuffer.Offset,
                dstBuffer.Offset,
                (int)size);
        }

        private BufferRange GetBufferRange(ulong address, ulong size)
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

            return buffer.GetRange(address, size);
        }

        private void SynchronizeBufferRange(ulong address, ulong size)
        {
            if (size != 0)
            {
                Buffer buffer = _buffers.FindFirstOverlap(address, size);

                buffer.SynchronizeMemory(address, size);
            }
        }

        public void InvalidateRange(ulong address, ulong size)
        {
            Buffer[] overlappingBuffers = _buffers.FindOverlaps(address, size);

            foreach (Buffer buffer in overlappingBuffers)
            {
                buffer.Invalidate();
            }
        }
    }
}