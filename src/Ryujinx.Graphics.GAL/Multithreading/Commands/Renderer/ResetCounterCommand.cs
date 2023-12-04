namespace Ryujinx.Graphics.GAL.Multithreading.Commands.Renderer
{
    struct ResetCounterCommand : IGALCommand, IGALCommand<ResetCounterCommand>
    {
        public readonly CommandType CommandType => CommandType.ResetCounter;
        private CounterType _type;

        public void Set(CounterType type)
        {
            _type = type;
        }

        public static void Run(ref ResetCounterCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.ResetCounter(command._type);
        }
    }
}
