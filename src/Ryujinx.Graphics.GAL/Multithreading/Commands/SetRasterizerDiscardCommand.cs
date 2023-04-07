namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct SetRasterizerDiscardCommand : IGALCommand, IGALCommand<SetRasterizerDiscardCommand>
    {
        public CommandType CommandType => CommandType.SetRasterizerDiscard;
        private bool _discard;

        public void Set(bool discard)
        {
            _discard = discard;
        }

        public static void Run(ref SetRasterizerDiscardCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.SetRasterizerDiscard(command._discard);
        }
    }
}
