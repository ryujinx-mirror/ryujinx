namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct SetDepthClampCommand : IGALCommand, IGALCommand<SetDepthClampCommand>
    {
        public readonly CommandType CommandType => CommandType.SetDepthClamp;
        private bool _clamp;

        public void Set(bool clamp)
        {
            _clamp = clamp;
        }

        public static void Run(ref SetDepthClampCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.SetDepthClamp(command._clamp);
        }
    }
}
