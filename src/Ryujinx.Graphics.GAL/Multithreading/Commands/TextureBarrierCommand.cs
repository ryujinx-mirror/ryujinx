namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct TextureBarrierCommand : IGALCommand, IGALCommand<TextureBarrierCommand>
    {
        public readonly CommandType CommandType => CommandType.TextureBarrier;

        public static void Run(ref TextureBarrierCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.TextureBarrier();
        }
    }
}
