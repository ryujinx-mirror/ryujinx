using Ryujinx.Graphics.GAL.Multithreading.Model;
using Ryujinx.Graphics.GAL.Multithreading.Resources.Programs;

namespace Ryujinx.Graphics.GAL.Multithreading.Commands.Renderer
{
    struct CreateProgramCommand : IGALCommand, IGALCommand<CreateProgramCommand>
    {
        public readonly CommandType CommandType => CommandType.CreateProgram;
        private TableRef<IProgramRequest> _request;

        public void Set(TableRef<IProgramRequest> request)
        {
            _request = request;
        }

        public static void Run(ref CreateProgramCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            IProgramRequest request = command._request.Get(threaded);

            if (request.Threaded.Base == null)
            {
                request.Threaded.Base = request.Create(renderer);
            }

            threaded.Programs.ProcessQueue();
        }
    }
}
