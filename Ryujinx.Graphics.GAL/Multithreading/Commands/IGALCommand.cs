namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    interface IGALCommand
    {
        CommandType CommandType { get; }
    }
}
