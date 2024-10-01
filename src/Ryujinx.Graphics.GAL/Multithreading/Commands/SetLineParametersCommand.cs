namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct SetLineParametersCommand : IGALCommand, IGALCommand<SetLineParametersCommand>
    {
        public readonly CommandType CommandType => CommandType.SetLineParameters;
        private float _width;
        private bool _smooth;

        public void Set(float width, bool smooth)
        {
            _width = width;
            _smooth = smooth;
        }

        public static void Run(ref SetLineParametersCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.SetLineParameters(command._width, command._smooth);
        }
    }
}
