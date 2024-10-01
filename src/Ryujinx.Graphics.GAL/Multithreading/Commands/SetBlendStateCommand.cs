namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct SetBlendStateCommand : IGALCommand, IGALCommand<SetBlendStateCommand>
    {
        public readonly CommandType CommandType => CommandType.SetBlendState;
        private int _index;
        private BlendDescriptor _blend;

        public void Set(int index, BlendDescriptor blend)
        {
            _index = index;
            _blend = blend;
        }

        public static void Run(ref SetBlendStateCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.SetBlendState(command._index, command._blend);
        }
    }
}
