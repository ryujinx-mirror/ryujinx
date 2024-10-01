namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct SetStencilTestCommand : IGALCommand, IGALCommand<SetStencilTestCommand>
    {
        public readonly CommandType CommandType => CommandType.SetStencilTest;
        private StencilTestDescriptor _stencilTest;

        public void Set(StencilTestDescriptor stencilTest)
        {
            _stencilTest = stencilTest;
        }

        public static void Run(ref SetStencilTestCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.SetStencilTest(command._stencilTest);
        }
    }
}
