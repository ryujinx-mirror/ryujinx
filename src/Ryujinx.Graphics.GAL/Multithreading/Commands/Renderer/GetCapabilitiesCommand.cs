using Ryujinx.Graphics.GAL.Multithreading.Model;

namespace Ryujinx.Graphics.GAL.Multithreading.Commands.Renderer
{
    struct GetCapabilitiesCommand : IGALCommand, IGALCommand<GetCapabilitiesCommand>
    {
        public readonly CommandType CommandType => CommandType.GetCapabilities;
        private TableRef<ResultBox<Capabilities>> _result;

        public void Set(TableRef<ResultBox<Capabilities>> result)
        {
            _result = result;
        }

        public static void Run(ref GetCapabilitiesCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            command._result.Get(threaded).Result = renderer.GetCapabilities();
        }
    }
}
