namespace Ryujinx.Graphics.GAL.Multithreading.Commands.Renderer
{
    struct CreateSyncCommand : IGALCommand, IGALCommand<CreateSyncCommand>
    {
        public readonly CommandType CommandType => CommandType.CreateSync;
        private ulong _id;
        private bool _strict;

        public void Set(ulong id, bool strict)
        {
            _id = id;
            _strict = strict;
        }

        public static void Run(ref CreateSyncCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.CreateSync(command._id, command._strict);

            threaded.Sync.AssignSync(command._id);
        }
    }
}
