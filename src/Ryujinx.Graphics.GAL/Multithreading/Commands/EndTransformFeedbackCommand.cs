namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct EndTransformFeedbackCommand : IGALCommand, IGALCommand<EndTransformFeedbackCommand>
    {
        public readonly CommandType CommandType => CommandType.EndTransformFeedback;

        public static void Run(ref EndTransformFeedbackCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.EndTransformFeedback();
        }
    }
}
