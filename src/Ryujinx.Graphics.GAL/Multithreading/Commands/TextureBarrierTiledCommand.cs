namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct TextureBarrierTiledCommand : IGALCommand, IGALCommand<TextureBarrierTiledCommand>
    {
        public readonly CommandType CommandType => CommandType.TextureBarrierTiled;

        public static void Run(ref TextureBarrierTiledCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.TextureBarrierTiled();
        }
    }
}
