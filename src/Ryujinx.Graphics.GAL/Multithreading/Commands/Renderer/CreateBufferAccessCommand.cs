namespace Ryujinx.Graphics.GAL.Multithreading.Commands.Renderer
{
    struct CreateBufferAccessCommand : IGALCommand, IGALCommand<CreateBufferAccessCommand>
    {
        public readonly CommandType CommandType => CommandType.CreateBufferAccess;
        private BufferHandle _threadedHandle;
        private int _size;
        private BufferAccess _access;

        public void Set(BufferHandle threadedHandle, int size, BufferAccess access)
        {
            _threadedHandle = threadedHandle;
            _size = size;
            _access = access;
        }

        public static void Run(ref CreateBufferAccessCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            threaded.Buffers.AssignBuffer(command._threadedHandle, renderer.CreateBuffer(command._size, command._access));
        }
    }
}
