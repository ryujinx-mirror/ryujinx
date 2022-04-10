namespace Ryujinx.Graphics.GAL.Multithreading.Commands.Renderer
{
    struct CreateBufferCommand : IGALCommand
    {
        public CommandType CommandType => CommandType.CreateBuffer;
        private BufferHandle _threadedHandle;
        private int _size;

        public void Set(BufferHandle threadedHandle, int size)
        {
            _threadedHandle = threadedHandle;
            _size = size;
        }

        public static void Run(ref CreateBufferCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            threaded.Buffers.AssignBuffer(command._threadedHandle, renderer.CreateBuffer(command._size));
        }
    }
}
