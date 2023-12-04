using Ryujinx.Horizon.Sdk.Sf.Hipc;

namespace Ryujinx.Horizon.Sdk.Sf
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

    readonly struct CommandArg
    {
        public CommandArgType Type { get; }
        public HipcBufferFlags BufferFlags { get; }
        public ushort BufferFixedSize { get; }
        public int ArgSize { get; }
        public int ArgAlignment { get; }

        public CommandArg(CommandArgType type)
        {
            Type = type;
            BufferFlags = default;
            BufferFixedSize = 0;
            ArgSize = 0;
            ArgAlignment = 0;
        }

        public CommandArg(CommandArgType type, int argSize, int argAlignment)
        {
            Type = type;
            BufferFlags = default;
            BufferFixedSize = 0;
            ArgSize = argSize;
            ArgAlignment = argAlignment;
        }

        public CommandArg(HipcBufferFlags flags, ushort fixedSize = 0)
        {
            Type = CommandArgType.Buffer;
            BufferFlags = flags;
            BufferFixedSize = fixedSize;
            ArgSize = 0;
            ArgAlignment = 0;
        }
    }
}
