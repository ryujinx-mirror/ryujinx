using System;

namespace Ryujinx.Memory.WindowsShared
{
    [Flags]
    enum AllocationType : uint
    {
        CoalescePlaceholders = 0x1,
        PreservePlaceholder = 0x2,
        Commit = 0x1000,
        Reserve = 0x2000,
        Decommit = 0x4000,
        ReplacePlaceholder = Decommit,
        Release = 0x8000,
        ReservePlaceholder = 0x40000,
        Reset = 0x80000,
        Physical = 0x400000,
        TopDown = 0x100000,
        WriteWatch = 0x200000,
        LargePages = 0x20000000,
    }

    [Flags]
    enum MemoryProtection : uint
    {
        NoAccess = 0x01,
        ReadOnly = 0x02,
        ReadWrite = 0x04,
        WriteCopy = 0x08,
        Execute = 0x10,
        ExecuteRead = 0x20,
        ExecuteReadWrite = 0x40,
        ExecuteWriteCopy = 0x80,
        GuardModifierflag = 0x100,
        NoCacheModifierflag = 0x200,
        WriteCombineModifierflag = 0x400,
    }

    [Flags]
    enum FileMapProtection : uint
    {
        PageReadonly = 0x02,
        PageReadWrite = 0x04,
        PageWriteCopy = 0x08,
        PageExecuteRead = 0x20,
        PageExecuteReadWrite = 0x40,
        SectionCommit = 0x8000000,
        SectionImage = 0x1000000,
        SectionNoCache = 0x10000000,
        SectionReserve = 0x4000000,
    }
}
