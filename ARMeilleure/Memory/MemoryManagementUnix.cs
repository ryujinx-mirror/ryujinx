using Mono.Unix.Native;
using System;

namespace ARMeilleure.Memory
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

        public static bool Commit(IntPtr address, ulong size)
        {
            return Syscall.mprotect(address, size, MmapProts.PROT_READ | MmapProts.PROT_WRITE) == 0;
        }

        public static bool Reprotect(IntPtr address, ulong size, Memory.MemoryProtection protection)
        {
            MmapProts prot = GetProtection(protection);

            return Syscall.mprotect(address, size, prot) == 0;
        }

        public static IntPtr Reserve(ulong size)
        {
            ulong pageSize = (ulong)Syscall.sysconf(SysconfName._SC_PAGESIZE);

            const MmapProts prot = MmapProts.PROT_NONE;

            const MmapFlags flags = MmapFlags.MAP_PRIVATE | MmapFlags.MAP_ANONYMOUS;

            IntPtr ptr = Syscall.mmap(IntPtr.Zero, size + pageSize, prot, flags, -1, 0);

            if (ptr == IntPtr.Zero)
            {
                throw new OutOfMemoryException();
            }

            return ptr;
        }

        private static MmapProts GetProtection(Memory.MemoryProtection protection)
        {
            switch (protection)
            {
                case Memory.MemoryProtection.None:             return MmapProts.PROT_NONE;
                case Memory.MemoryProtection.Read:             return MmapProts.PROT_READ;
                case Memory.MemoryProtection.ReadAndWrite:     return MmapProts.PROT_READ | MmapProts.PROT_WRITE;
                case Memory.MemoryProtection.ReadAndExecute:   return MmapProts.PROT_READ | MmapProts.PROT_EXEC;
                case Memory.MemoryProtection.ReadWriteExecute: return MmapProts.PROT_READ | MmapProts.PROT_WRITE | MmapProts.PROT_EXEC;
                case Memory.MemoryProtection.Execute:          return MmapProts.PROT_EXEC;

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