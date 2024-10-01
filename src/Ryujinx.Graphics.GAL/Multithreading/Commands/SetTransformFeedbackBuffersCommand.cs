using Ryujinx.Graphics.GAL.Multithreading.Model;
using System;

namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct SetTransformFeedbackBuffersCommand : IGALCommand, IGALCommand<SetTransformFeedbackBuffersCommand>
    {
        public readonly CommandType CommandType => CommandType.SetTransformFeedbackBuffers;
        private SpanRef<BufferRange> _buffers;

        public void Set(SpanRef<BufferRange> buffers)
        {
            _buffers = buffers;
        }

        public static void Run(ref SetTransformFeedbackBuffersCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            Span<BufferRange> buffers = command._buffers.Get(threaded);
            renderer.Pipeline.SetTransformFeedbackBuffers(threaded.Buffers.MapBufferRanges(buffers));
            command._buffers.Dispose(threaded);
        }
    }
}
