namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct SetLogicOpStateCommand : IGALCommand, IGALCommand<SetLogicOpStateCommand>
    {
        public readonly CommandType CommandType => CommandType.SetLogicOpState;
        private bool _enable;
        private LogicalOp _op;

        public void Set(bool enable, LogicalOp op)
        {
            _enable = enable;
            _op = op;
        }

        public static void Run(ref SetLogicOpStateCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.SetLogicOpState(command._enable, command._op);
        }
    }
}
