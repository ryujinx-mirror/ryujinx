namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct SetFrontFaceCommand : IGALCommand, IGALCommand<SetFrontFaceCommand>
    {
        public readonly CommandType CommandType => CommandType.SetFrontFace;
        private FrontFace _frontFace;

        public void Set(FrontFace frontFace)
        {
            _frontFace = frontFace;
        }

        public static void Run(ref SetFrontFaceCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.SetFrontFace(command._frontFace);
        }
    }
}
