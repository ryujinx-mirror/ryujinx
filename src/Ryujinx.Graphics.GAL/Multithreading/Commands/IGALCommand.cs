namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    interface IGALCommand
    {
        CommandType CommandType { get; }
    }

    interface IGALCommand<T> where T : IGALCommand
    {
        abstract static void Run(ref T command, ThreadedRenderer threaded, IRenderer renderer);
    }
}
