using Silk.NET.Vulkan;
using System;

namespace Ryujinx.Graphics.Vulkan
{
    struct BufferState : IDisposable
    {
        public static BufferState Null => new BufferState(null, 0, 0);

        private readonly Auto<DisposableBuffer> _buffer;
        private readonly int _offset;
        private readonly int _size;
        private readonly ulong _stride;
        private readonly IndexType _type;

        public BufferState(Auto<DisposableBuffer> buffer, int offset, int size, IndexType type)
        {
            _buffer = buffer;
            _offset = offset;
            _size = size;
            _stride = 0;
            _type = type;
            buffer?.IncrementReferenceCount();
        }

        public BufferState(Auto<DisposableBuffer> buffer, int offset, int size, ulong stride = 0UL)
        {
            _buffer = buffer;
            _offset = offset;
            _size = size;
            _stride = stride;
            _type = IndexType.Uint16;
            buffer?.IncrementReferenceCount();
        }

        public void BindIndexBuffer(Vk api, CommandBufferScoped cbs)
        {
            if (_buffer != null)
            {
                api.CmdBindIndexBuffer(cbs.CommandBuffer, _buffer.Get(cbs, _offset, _size).Value, (ulong)_offset, _type);
            }
        }

        public void BindTransformFeedbackBuffer(VulkanRenderer gd, CommandBufferScoped cbs, uint binding)
        {
            if (_buffer != null)
            {
                var buffer = _buffer.Get(cbs, _offset, _size).Value;

                gd.TransformFeedbackApi.CmdBindTransformFeedbackBuffers(cbs.CommandBuffer, binding, 1, buffer, (ulong)_offset, (ulong)_size);
            }
        }

        public void BindVertexBuffer(VulkanRenderer gd, CommandBufferScoped cbs, uint binding)
        {
            if (_buffer != null)
            {
                var buffer = _buffer.Get(cbs, _offset, _size).Value;

                if (gd.Capabilities.SupportsExtendedDynamicState)
                {
                    gd.ExtendedDynamicStateApi.CmdBindVertexBuffers2(
                        cbs.CommandBuffer,
                        binding,
                        1,
                        buffer,
                        (ulong)_offset,
                        (ulong)_size,
                        _stride);
                }
                else
                {
                    gd.Api.CmdBindVertexBuffers(cbs.CommandBuffer, binding, 1, buffer, (ulong)_offset);
                }
            }
        }

        public void Dispose()
        {
            _buffer?.DecrementReferenceCount();
        }
    }
}
