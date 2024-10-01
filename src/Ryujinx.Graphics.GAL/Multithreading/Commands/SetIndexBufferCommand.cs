namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct SetIndexBufferCommand : IGALCommand, IGALCommand<SetIndexBufferCommand>
    {
        public readonly CommandType CommandType => CommandType.SetIndexBuffer;
        private BufferRange _buffer;
        private IndexType _type;

        public void Set(BufferRange buffer, IndexType type)
        {
            _buffer = buffer;
            _type = type;
        }

        public static void Run(ref SetIndexBufferCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            BufferRange range = threaded.Buffers.MapBufferRange(command._buffer);
            renderer.Pipeline.SetIndexBuffer(range, command._type);
        }
    }
}
