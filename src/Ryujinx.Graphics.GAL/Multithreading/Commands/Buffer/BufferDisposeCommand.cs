namespace Ryujinx.Graphics.GAL.Multithreading.Commands.Buffer
{
    struct BufferDisposeCommand : IGALCommand, IGALCommand<BufferDisposeCommand>
    {
        public readonly CommandType CommandType => CommandType.BufferDispose;
        private BufferHandle _buffer;

        public void Set(BufferHandle buffer)
        {
            _buffer = buffer;
        }

        public static void Run(ref BufferDisposeCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.DeleteBuffer(threaded.Buffers.MapBuffer(command._buffer));
            threaded.Buffers.UnassignBuffer(command._buffer);
        }
    }
}
