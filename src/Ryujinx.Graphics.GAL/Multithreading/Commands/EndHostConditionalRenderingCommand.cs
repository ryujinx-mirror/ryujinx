namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct EndHostConditionalRenderingCommand : IGALCommand, IGALCommand<EndHostConditionalRenderingCommand>
    {
        public readonly CommandType CommandType => CommandType.EndHostConditionalRendering;

        public static void Run(ref EndHostConditionalRenderingCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.EndHostConditionalRendering();
        }
    }
}
