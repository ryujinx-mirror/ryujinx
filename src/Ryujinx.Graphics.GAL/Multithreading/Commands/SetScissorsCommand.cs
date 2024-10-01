using Ryujinx.Graphics.GAL.Multithreading.Model;

namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct SetScissorsCommand : IGALCommand, IGALCommand<SetScissorsCommand>
    {
        public readonly CommandType CommandType => CommandType.SetScissor;
        private SpanRef<Rectangle<int>> _scissors;

        public void Set(SpanRef<Rectangle<int>> scissors)
        {
            _scissors = scissors;
        }

        public static void Run(ref SetScissorsCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.SetScissors(command._scissors.Get(threaded));

            command._scissors.Dispose(threaded);
        }
    }
}
