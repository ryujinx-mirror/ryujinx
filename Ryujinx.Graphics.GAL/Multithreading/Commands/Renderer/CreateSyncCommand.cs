namespace Ryujinx.Graphics.GAL.Multithreading.Commands.Renderer
{
    struct CreateSyncCommand : IGALCommand
    {
        public CommandType CommandType => CommandType.CreateSync;
        private ulong _id;

        public void Set(ulong id)
        {
            _id = id;
        }

        public static void Run(ref CreateSyncCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.CreateSync(command._id);

            threaded.Sync.AssignSync(command._id);
        }
    }
}
