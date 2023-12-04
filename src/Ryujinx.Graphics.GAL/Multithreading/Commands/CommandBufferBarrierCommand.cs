namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct CommandBufferBarrierCommand : IGALCommand, IGALCommand<CommandBufferBarrierCommand>
    {
        public readonly CommandType CommandType => CommandType.CommandBufferBarrier;

        public static void Run(ref CommandBufferBarrierCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.CommandBufferBarrier();
        }
    }
}
