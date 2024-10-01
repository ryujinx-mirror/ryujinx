namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct SetFaceCullingCommand : IGALCommand, IGALCommand<SetFaceCullingCommand>
    {
        public readonly CommandType CommandType => CommandType.SetFaceCulling;
        private bool _enable;
        private Face _face;

        public void Set(bool enable, Face face)
        {
            _enable = enable;
            _face = face;
        }

        public static void Run(ref SetFaceCullingCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.SetFaceCulling(command._enable, command._face);
        }
    }
}
