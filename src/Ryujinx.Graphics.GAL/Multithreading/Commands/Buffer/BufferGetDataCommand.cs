using Ryujinx.Graphics.GAL.Multithreading.Model;

namespace Ryujinx.Graphics.GAL.Multithreading.Commands.Buffer
{
    struct BufferGetDataCommand : IGALCommand, IGALCommand<BufferGetDataCommand>
    {
        public readonly CommandType CommandType => CommandType.BufferGetData;
        private BufferHandle _buffer;
        private int _offset;
        private int _size;
        private TableRef<ResultBox<PinnedSpan<byte>>> _result;

        public void Set(BufferHandle buffer, int offset, int size, TableRef<ResultBox<PinnedSpan<byte>>> result)
        {
            _buffer = buffer;
            _offset = offset;
            _size = size;
            _result = result;
        }

        public static void Run(ref BufferGetDataCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            PinnedSpan<byte> result = renderer.GetBufferData(threaded.Buffers.MapBuffer(command._buffer), command._offset, command._size);

            command._result.Get(threaded).Result = result;
        }
    }
}
