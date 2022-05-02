using System;
using System.Collections.Concurrent;
using System.Runtime.Versioning;
using System.Text;

using static Ryujinx.Memory.MemoryManagerUnixHelper;

namespace Ryujinx.Memory
{
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
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

        private static IntPtr AllocateInternal(ulong size, MmapProts prot, bool shared = false)
        {
            MmapFlags flags = MmapFlags.MAP_ANONYMOUS;

            if (shared)
            {
                flags |= MmapFlags.MAP_SHARED | MmapFlags.MAP_UNLOCKED;
            }
            else
            {
                flags |= MmapFlags.MAP_PRIVATE;
            }

            if (prot == MmapProts.PROT_NONE)
            {
                flags |= MmapFlags.MAP_NORESERVE;
            }

            IntPtr ptr = mmap(IntPtr.Zero, size, prot, flags, -1, 0);

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
            return mprotect(address, size, MmapProts.PROT_READ | MmapProts.PROT_WRITE) == 0;
        }

        public static bool Decommit(IntPtr address, ulong size)
        {
            // Must be writable for madvise to work properly.
            mprotect(address, size, MmapProts.PROT_READ | MmapProts.PROT_WRITE);

            madvise(address, size, MADV_REMOVE);

            return mprotect(address, size, MmapProts.PROT_NONE) == 0;
        }

        public static bool Reprotect(IntPtr address, ulong size, MemoryPermission permission)
        {
            return mprotect(address, size, GetProtection(permission)) == 0;
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
                return munmap(address, size) == 0;
            }

            return false;
        }

        public static bool Unmap(IntPtr address, ulong size)
        {
            return munmap(address, size) == 0;
        }

        public unsafe static IntPtr CreateSharedMemory(ulong size, bool reserve)
        {
            int fd;

            if (OperatingSystem.IsMacOS())
            {
                byte[] memName = Encoding.ASCII.GetBytes("Ryujinx-XXXXXX");

                fixed (byte* pMemName = memName)
                {
                    fd = shm_open((IntPtr)pMemName, 0x2 | 0x200 | 0x800 | 0x400, 384); // O_RDWR | O_CREAT | O_EXCL | O_TRUNC, 0600
                    if (fd == -1)
                    {
                        throw new OutOfMemoryException();
                    }

                    if (shm_unlink((IntPtr)pMemName) != 0)
                    {
                        throw new OutOfMemoryException();
                    }
                }
            }
            else
            {
                byte[] fileName = Encoding.ASCII.GetBytes("/dev/shm/Ryujinx-XXXXXX");

                fixed (byte* pFileName = fileName)
                {
                    fd = mkstemp((IntPtr)pFileName);
                    if (fd == -1)
                    {
                        throw new OutOfMemoryException();
                    }

                    if (unlink((IntPtr)pFileName) != 0)
                    {
                        throw new OutOfMemoryException();
                    }
                }
            }

            if (ftruncate(fd, (IntPtr)size) != 0)
            {
                throw new OutOfMemoryException();
            }

            return (IntPtr)fd;
        }

        public static void DestroySharedMemory(IntPtr handle)
        {
            close((int)handle);
        }

        public static IntPtr MapSharedMemory(IntPtr handle, ulong size)
        {
            return mmap(IntPtr.Zero, size, MmapProts.PROT_READ | MmapProts.PROT_WRITE, MmapFlags.MAP_SHARED, (int)handle, 0);
        }

        public static void UnmapSharedMemory(IntPtr address, ulong size)
        {
            munmap(address, size);
        }

        public static void MapView(IntPtr sharedMemory, ulong srcOffset, IntPtr location, ulong size)
        {
            mmap(location, size, MmapProts.PROT_READ | MmapProts.PROT_WRITE, MmapFlags.MAP_FIXED | MmapFlags.MAP_SHARED, (int)sharedMemory, (long)srcOffset);
        }

        public static void UnmapView(IntPtr location, ulong size)
        {
            mmap(location, size, MmapProts.PROT_NONE, MmapFlags.MAP_FIXED, -1, 0);
        }
    }
}