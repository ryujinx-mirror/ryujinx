using Mono.Unix.Native;
using System;

namespace ChocolArm64.Memory
{
    static class MemoryManagementUnix
    {
        public static IntPtr Allocate(ulong size)
        {
            ulong pageSize = (ulong)Syscall.sysconf(SysconfName._SC_PAGESIZE);

            const MmapProts prot = MmapProts.PROT_READ | MmapProts.PROT_WRITE;

            const MmapFlags flags = MmapFlags.MAP_PRIVATE | MmapFlags.MAP_ANONYMOUS;

            IntPtr ptr = Syscall.mmap(IntPtr.Zero, size + pageSize, prot, flags, -1, 0);

            if (ptr == IntPtr.Zero)
            {
                throw new OutOfMemoryException();
            }

            unsafe
            {
                ptr = new IntPtr(ptr.ToInt64() + (long)pageSize);

                *((ulong*)ptr - 1) = size;
            }

            return ptr;
        }

        public static bool Reprotect(IntPtr address, ulong size, MemoryProtection protection)
        {
            MmapProts prot = GetProtection(protection);

            return Syscall.mprotect(address, size, prot) == 0;
        }

        private static MmapProts GetProtection(MemoryProtection protection)
        {
            switch (protection)
            {
                case MemoryProtection.None:           return MmapProts.PROT_NONE;
                case MemoryProtection.Read:           return MmapProts.PROT_READ;
                case MemoryProtection.ReadAndWrite:   return MmapProts.PROT_READ | MmapProts.PROT_WRITE;
                case MemoryProtection.ReadAndExecute: return MmapProts.PROT_READ | MmapProts.PROT_EXEC;
                case MemoryProtection.Execute:        return MmapProts.PROT_EXEC;

                default: throw new ArgumentException($"Invalid permission \"{protection}\".");
            }
        }

        public static bool Free(IntPtr address)
        {
            ulong pageSize = (ulong)Syscall.sysconf(SysconfName._SC_PAGESIZE);

            ulong size;

            unsafe
            {
                size = *((ulong*)address - 1);

                address = new IntPtr(address.ToInt64() - (long)pageSize);
            }

            return Syscall.munmap(address, size + pageSize) == 0;
        }
    }
}