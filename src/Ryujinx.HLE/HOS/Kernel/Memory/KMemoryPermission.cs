using Ryujinx.Memory;
using System;

namespace Ryujinx.HLE.HOS.Kernel.Memory
{
    [Flags]
    enum KMemoryPermission : uint
    {
        None = 0,
        UserMask = Read | Write | Execute,
        Mask = uint.MaxValue,

        Read = 1 << 0,
        Write = 1 << 1,
        Execute = 1 << 2,
        DontCare = 1 << 28,

        ReadAndWrite = Read | Write,
        ReadAndExecute = Read | Execute,
    }

    static class KMemoryPermissionExtensions
    {
        public static MemoryPermission Convert(this KMemoryPermission permission)
        {
            MemoryPermission output = MemoryPermission.None;

            if (permission.HasFlag(KMemoryPermission.Read))
            {
                output = MemoryPermission.Read;
            }

            if (permission.HasFlag(KMemoryPermission.Write))
            {
                output |= MemoryPermission.Write;
            }

            if (permission.HasFlag(KMemoryPermission.Execute))
            {
                output |= MemoryPermission.Execute;
            }

            return output;
        }
    }
}
