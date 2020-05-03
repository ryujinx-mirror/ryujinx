using Mono.Unix.Native;
using System;
using System.Collections.Concurrent;

namespace Ryujinx.Memory
{
    static class MemoryManagementUnix
    {
        private static readonly ConcurrentDictionary<IntPtr, ulong> _allocations = new ConcurrentDictionary<IntPtr, ulong>();

        public static IntPtr Allocate(ulong size)
        {
            return AllocateInternal(size, MmapProts.PROT_READ | MmapProts.PROT_WRITE);
        }

        public static IntPtr Reserve(ulong size)
        {
            return AllocateInternal(size, MmapProts.PROT_NONE);
        }

        private static IntPtr AllocateInternal(ulong size, MmapProts prot)
        {
            const MmapFlags flags = MmapFlags.MAP_PRIVATE | MmapFlags.MAP_ANONYMOUS;

            IntPtr ptr = Syscall.mmap(IntPtr.Zero, size, prot, flags, -1, 0);

            if (ptr == new IntPtr(-1L))
            {
                throw new OutOfMemoryException();
            }

            if (!_allocations.TryAdd(ptr, size))
            {
                // This should be impossible, kernel shouldn't return an already mapped address.
                throw new InvalidOperationException();
            }

            return ptr;
        }

        public static bool Commit(IntPtr address, ulong size)
        {
            return Syscall.mprotect(address, size, MmapProts.PROT_READ | MmapProts.PROT_WRITE) == 0;
        }

        public static bool Reprotect(IntPtr address, ulong size, MemoryPermission permission)
        {
            return Syscall.mprotect(address, size, GetProtection(permission)) == 0;
        }

        private static MmapProts GetProtection(MemoryPermission permission)
        {
            return permission switch
            {
                MemoryPermission.None => MmapProts.PROT_NONE,
                MemoryPermission.Read => MmapProts.PROT_READ,
                MemoryPermission.ReadAndWrite => MmapProts.PROT_READ | MmapProts.PROT_WRITE,
                MemoryPermission.ReadAndExecute => MmapProts.PROT_READ | MmapProts.PROT_EXEC,
                MemoryPermission.ReadWriteExecute => MmapProts.PROT_READ | MmapProts.PROT_WRITE | MmapProts.PROT_EXEC,
                MemoryPermission.Execute => MmapProts.PROT_EXEC,
                _ => throw new MemoryProtectionException(permission)
            };
        }

        public static bool Free(IntPtr address)
        {
            if (_allocations.TryRemove(address, out ulong size))
            {
                return Syscall.munmap(address, size) == 0;
            }

            return false;
        }
    }
}