namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct SetDepthTestCommand : IGALCommand, IGALCommand<SetDepthTestCommand>
    {
        public readonly CommandType CommandType => CommandType.SetDepthTest;
        private DepthTestDescriptor _depthTest;

        public void Set(DepthTestDescriptor depthTest)
        {
            _depthTest = depthTest;
        }

        public static void Run(ref SetDepthTestCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.SetDepthTest(command._depthTest);
        }
    }
}
