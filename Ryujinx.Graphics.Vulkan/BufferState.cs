using System;

namespace Ryujinx.Graphics.Vulkan
{
    readonly struct BufferState : IDisposable
    {
        public static BufferState Null => new BufferState(null, 0, 0);

        private readonly int _offset;
        private readonly int _size;

        private readonly Auto<DisposableBuffer> _buffer;

        public BufferState(Auto<DisposableBuffer> buffer, int offset, int size)
        {
            _buffer = buffer;
            _offset = offset;
            _size = size;
            buffer?.IncrementReferenceCount();
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
