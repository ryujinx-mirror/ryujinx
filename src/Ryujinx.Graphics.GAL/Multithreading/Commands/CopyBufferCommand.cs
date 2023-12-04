namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct CopyBufferCommand : IGALCommand, IGALCommand<CopyBufferCommand>
    {
        public readonly CommandType CommandType => CommandType.CopyBuffer;
        private BufferHandle _source;
        private BufferHandle _destination;
        private int _srcOffset;
        private int _dstOffset;
        private int _size;

        public void Set(BufferHandle source, BufferHandle destination, int srcOffset, int dstOffset, int size)
        {
            _source = source;
            _destination = destination;
            _srcOffset = srcOffset;
            _dstOffset = dstOffset;
            _size = size;
        }

        public static void Run(ref CopyBufferCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.CopyBuffer(threaded.Buffers.MapBuffer(command._source), threaded.Buffers.MapBuffer(command._destination), command._srcOffset, command._dstOffset, command._size);
        }
    }
}
