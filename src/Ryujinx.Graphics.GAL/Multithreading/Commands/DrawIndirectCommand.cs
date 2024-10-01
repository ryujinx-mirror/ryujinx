namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct DrawIndirectCommand : IGALCommand, IGALCommand<DrawIndirectCommand>
    {
        public readonly CommandType CommandType => CommandType.DrawIndirect;
        private BufferRange _indirectBuffer;

        public void Set(BufferRange indirectBuffer)
        {
            _indirectBuffer = indirectBuffer;
        }

        public static void Run(ref DrawIndirectCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.DrawIndirect(threaded.Buffers.MapBufferRange(command._indirectBuffer));
        }
    }
}
