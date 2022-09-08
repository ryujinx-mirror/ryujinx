using Silk.NET.Vulkan;
using System;

namespace Ryujinx.Graphics.Vulkan
{
    struct BufferState : IDisposable
    {
        public static BufferState Null => new BufferState(null, 0, 0);

        private readonly int _offset;
        private readonly int _size;
        private readonly IndexType _type;

        private readonly Auto<DisposableBuffer> _buffer;

        public BufferState(Auto<DisposableBuffer> buffer, int offset, int size, IndexType type)
        {
            _buffer = buffer;

            _offset = offset;
            _size = size;
            _type = type;
            buffer?.IncrementReferenceCount();
        }

        public BufferState(Auto<DisposableBuffer> buffer, int offset, int size)
        {
            _buffer = buffer;

            _offset = offset;
            _size = size;
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

        public void Dispose()
        {
            _buffer?.DecrementReferenceCount();
        }
    }
}
