namespace Ryujinx.Horizon.Generators.Hipc
{
    enum CommandArgType : byte
    {
        Invalid,

        Buffer,
        InArgument,
        InCopyHandle,
        InMoveHandle,
        InObject,
        OutArgument,
        OutCopyHandle,
        OutMoveHandle,
        OutObject,
        ProcessId,
    }
}
