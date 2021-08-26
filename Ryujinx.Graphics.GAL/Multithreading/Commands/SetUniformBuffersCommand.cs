using Ryujinx.Graphics.GAL.Multithreading.Model;
using System;

namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct SetUniformBuffersCommand : IGALCommand
    {
        public CommandType CommandType => CommandType.SetUniformBuffers;
        private int _first;
        private SpanRef<BufferRange> _buffers;

        public void Set(int first, SpanRef<BufferRange> buffers)
        {
            _first = first;
            _buffers = buffers;
        }

        public static void Run(ref SetUniformBuffersCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            Span<BufferRange> buffers = command._buffers.Get(threaded);
            renderer.Pipeline.SetUniformBuffers(command._first, threaded.Buffers.MapBufferRanges(buffers));
            command._buffers.Dispose(threaded);
        }
    }
}
