namespace Ryujinx.Graphics.GAL.Multithreading.Commands.Renderer
{
    struct UpdateCountersCommand : IGALCommand, IGALCommand<UpdateCountersCommand>
    {
        public CommandType CommandType => CommandType.UpdateCounters;

        public static void Run(ref UpdateCountersCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.UpdateCounters();
        }
    }
}
