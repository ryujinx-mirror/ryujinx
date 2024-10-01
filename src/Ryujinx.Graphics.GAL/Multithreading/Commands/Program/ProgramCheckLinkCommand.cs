using Ryujinx.Graphics.GAL.Multithreading.Model;
using Ryujinx.Graphics.GAL.Multithreading.Resources;

namespace Ryujinx.Graphics.GAL.Multithreading.Commands.Program
{
    struct ProgramCheckLinkCommand : IGALCommand, IGALCommand<ProgramCheckLinkCommand>
    {
        public readonly CommandType CommandType => CommandType.ProgramCheckLink;
        private TableRef<ThreadedProgram> _program;
        private bool _blocking;
        private TableRef<ResultBox<ProgramLinkStatus>> _result;

        public void Set(TableRef<ThreadedProgram> program, bool blocking, TableRef<ResultBox<ProgramLinkStatus>> result)
        {
            _program = program;
            _blocking = blocking;
            _result = result;
        }

        public static void Run(ref ProgramCheckLinkCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            ProgramLinkStatus result = command._program.Get(threaded).Base.CheckProgramLink(command._blocking);

            command._result.Get(threaded).Result = result;
        }
    }
}
