namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct EndHostConditionalRenderingCommand : IGALCommand
    {
        public CommandType CommandType => CommandType.EndHostConditionalRendering;

        public static void Run(IRenderer renderer)
        {
            renderer.Pipeline.EndHostConditionalRendering();
        }
    }
}
