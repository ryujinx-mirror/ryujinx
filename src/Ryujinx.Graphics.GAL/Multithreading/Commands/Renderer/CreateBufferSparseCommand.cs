using Ryujinx.Graphics.GAL.Multithreading.Model;
using System;

namespace Ryujinx.Graphics.GAL.Multithreading.Commands.Renderer
{
    struct CreateBufferSparseCommand : IGALCommand, IGALCommand<CreateBufferSparseCommand>
    {
        public readonly CommandType CommandType => CommandType.CreateBufferSparse;
        private BufferHandle _threadedHandle;
        private SpanRef<BufferRange> _buffers;

        public void Set(BufferHandle threadedHandle, SpanRef<BufferRange> buffers)
        {
            _threadedHandle = threadedHandle;
            _buffers = buffers;
        }

        public static void Run(ref CreateBufferSparseCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            Span<BufferRange> buffers = command._buffers.Get(threaded);
            threaded.Buffers.AssignBuffer(command._threadedHandle, renderer.CreateBufferSparse(threaded.Buffers.MapBufferRanges(buffers)));
            command._buffers.Dispose(threaded);
        }
    }
}
