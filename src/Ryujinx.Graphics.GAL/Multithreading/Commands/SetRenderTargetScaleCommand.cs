namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct SetRenderTargetScaleCommand : IGALCommand, IGALCommand<SetRenderTargetScaleCommand>
    {
        public readonly CommandType CommandType => CommandType.SetRenderTargetScale;
        private float _scale;

        public void Set(float scale)
        {
            _scale = scale;
        }

        public static void Run(ref SetRenderTargetScaleCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.SetRenderTargetScale(command._scale);
        }
    }
}
