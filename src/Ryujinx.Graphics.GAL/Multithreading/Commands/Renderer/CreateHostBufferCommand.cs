namespace Ryujinx.Graphics.GAL.Multithreading.Commands.Renderer
{
    struct CreateHostBufferCommand : IGALCommand, IGALCommand<CreateHostBufferCommand>
    {
        public readonly CommandType CommandType => CommandType.CreateHostBuffer;
        private BufferHandle _threadedHandle;
        private nint _pointer;
        private int _size;

        public void Set(BufferHandle threadedHandle, nint pointer, int size)
        {
            _threadedHandle = threadedHandle;
            _pointer = pointer;
            _size = size;
        }

        public static void Run(ref CreateHostBufferCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            threaded.Buffers.AssignBuffer(command._threadedHandle, renderer.CreateBuffer(command._pointer, command._size));
        }
    }
}
