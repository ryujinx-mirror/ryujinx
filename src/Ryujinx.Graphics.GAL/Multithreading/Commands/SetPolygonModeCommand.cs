namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct SetPolygonModeCommand : IGALCommand, IGALCommand<SetPolygonModeCommand>
    {
        public readonly CommandType CommandType => CommandType.SetPolygonMode;
        private PolygonMode _frontMode;
        private PolygonMode _backMode;

        public void Set(PolygonMode frontMode, PolygonMode backMode)
        {
            _frontMode = frontMode;
            _backMode = backMode;
        }

        public static void Run(ref SetPolygonModeCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.SetPolygonMode(command._frontMode, command._backMode);
        }
    }
}
