using BufferHandle = Ryujinx.Graphics.GAL.BufferHandle;

namespace Ryujinx.Graphics.Vulkan
{
    internal struct VertexBufferState
    {
        public static VertexBufferState Null => new VertexBufferState(null, 0, 0, 0);

        private readonly int _offset;
        private readonly int _size;
        private readonly int _stride;

        private readonly BufferHandle _handle;
        private Auto<DisposableBuffer> _buffer;

        internal readonly int DescriptorIndex;
        internal int AttributeScalarAlignment;

        public VertexBufferState(Auto<DisposableBuffer> buffer, int descriptorIndex, int offset, int size, int stride = 0)
        {
            _buffer = buffer;
            _handle = BufferHandle.Null;

            _offset = offset;
            _size = size;
            _stride = stride;

            DescriptorIndex = descriptorIndex;
            AttributeScalarAlignment = 1;

            buffer?.IncrementReferenceCount();
        }

        public VertexBufferState(BufferHandle handle, int descriptorIndex, int offset, int size, int stride = 0)
        {
            // This buffer state may be rewritten at bind time, so it must be retrieved on bind.

            _buffer = null;
            _handle = handle;

            _offset = offset;
            _size = size;
            _stride = stride;

            DescriptorIndex = descriptorIndex;
            AttributeScalarAlignment = 1;
        }

        public void BindVertexBuffer(VulkanRenderer gd, CommandBufferScoped cbs, uint binding, ref PipelineState state)
        {
            var autoBuffer = _buffer;

            if (_handle != BufferHandle.Null)
            {
                // May need to restride the vertex buffer.

                if (gd.NeedsVertexBufferAlignment(AttributeScalarAlignment, out int alignment) && (_stride % alignment) != 0)
                {
                    autoBuffer = gd.BufferManager.GetAlignedVertexBuffer(cbs, _handle, _offset, _size, _stride, alignment);

                    if (autoBuffer != null)
                    {
                        int stride = (_stride + (alignment - 1)) & -alignment;
                        int newSize = (_size / _stride) * stride;

                        var buffer = autoBuffer.Get(cbs, 0, newSize).Value;

                        if (gd.Capabilities.SupportsExtendedDynamicState)
                        {
                            gd.ExtendedDynamicStateApi.CmdBindVertexBuffers2(
                                cbs.CommandBuffer,
                                binding,
                                1,
                                buffer,
                                0,
                                (ulong)newSize,
                                (ulong)stride);
                        }
                        else
                        {
                            gd.Api.CmdBindVertexBuffers(cbs.CommandBuffer, binding, 1, buffer, 0);
                        }

                        _buffer = autoBuffer;
                    }

                    state.Internal.VertexBindingDescriptions[DescriptorIndex].Stride = (uint)_stride;

                    return;
                }
                else
                {
                    autoBuffer = gd.BufferManager.GetBuffer(cbs.CommandBuffer, _handle, false, out int size);

                    // The original stride must be reapplied in case it was rewritten.
                    state.Internal.VertexBindingDescriptions[DescriptorIndex].Stride = (uint)_stride;

                    if (_offset >= size)
                    {
                        autoBuffer = null;
                    }
                }
            }

            if (autoBuffer != null)
            {
                var buffer = autoBuffer.Get(cbs, _offset, _size).Value;

                if (gd.Capabilities.SupportsExtendedDynamicState)
                {
                    gd.ExtendedDynamicStateApi.CmdBindVertexBuffers2(
                        cbs.CommandBuffer,
                        binding,
                        1,
                        buffer,
                        (ulong)_offset,
                        (ulong)_size,
                        (ulong)_stride);
                }
                else
                {
                    gd.Api.CmdBindVertexBuffers(cbs.CommandBuffer, binding, 1, buffer, (ulong)_offset);
                }
            }
        }

        public bool BoundEquals(Auto<DisposableBuffer> buffer)
        {
            return _buffer == buffer;
        }

        public void Dispose()
        {
            // Only dispose if this buffer is not refetched on each bind.

            if (_handle == BufferHandle.Null)
            {
                _buffer?.DecrementReferenceCount();
            }
        }
    }
}
