namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct BeginTransformFeedbackCommand : IGALCommand, IGALCommand<BeginTransformFeedbackCommand>
    {
        public readonly CommandType CommandType => CommandType.BeginTransformFeedback;
        private PrimitiveTopology _topology;

        public void Set(PrimitiveTopology topology)
        {
            _topology = topology;
        }

        public static void Run(ref BeginTransformFeedbackCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.BeginTransformFeedback(command._topology);
        }
    }
}
