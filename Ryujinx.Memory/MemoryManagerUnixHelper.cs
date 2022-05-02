using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Memory
{
    public static class MemoryManagerUnixHelper
    {
        [Flags]
        public enum MmapProts : uint
        {
            PROT_NONE = 0,
            PROT_READ = 1,
            PROT_WRITE = 2,
            PROT_EXEC = 4
        }

        [Flags]
        public enum MmapFlags : uint
        {
            MAP_SHARED = 1,
            MAP_PRIVATE = 2,
            MAP_ANONYMOUS = 4,
            MAP_NORESERVE = 8,
            MAP_FIXED = 16,
            MAP_UNLOCKED = 32
        }

        [Flags]
        public enum OpenFlags : uint
        {
            O_RDONLY = 0,
            O_WRONLY = 1,
            O_RDWR = 2,
            O_CREAT = 4,
            O_EXCL = 8,
            O_NOCTTY = 16,
            O_TRUNC = 32,
            O_APPEND = 64,
            O_NONBLOCK = 128,
            O_SYNC = 256,
        }

        private const int MAP_ANONYMOUS_LINUX_GENERIC = 0x20;
        private const int MAP_NORESERVE_LINUX_GENERIC = 0x4000;
        private const int MAP_UNLOCKED_LINUX_GENERIC = 0x80000;

        private const int MAP_NORESERVE_DARWIN = 0x40;
        private const int MAP_JIT_DARWIN = 0x800;
        private const int MAP_ANONYMOUS_DARWIN = 0x1000;

        public const int MADV_DONTNEED = 4;
        public const int MADV_REMOVE = 9;

        [DllImport("libc", EntryPoint = "mmap", SetLastError = true)]
        private static extern IntPtr Internal_mmap(IntPtr address, ulong length, MmapProts prot, int flags, int fd, long offset);

        [DllImport("libc", SetLastError = true)]
        public static extern int mprotect(IntPtr address, ulong length, MmapProts prot);

        [DllImport("libc", SetLastError = true)]
        public static extern int munmap(IntPtr address, ulong length);

        [DllImport("libc", SetLastError = true)]
        public static extern IntPtr mremap(IntPtr old_address, ulong old_size, ulong new_size, int flags, IntPtr new_address);

        [DllImport("libc", SetLastError = true)]
        public static extern int madvise(IntPtr address, ulong size, int advice);

        [DllImport("libc", SetLastError = true)]
        public static extern int mkstemp(IntPtr template);

        [DllImport("libc", SetLastError = true)]
        public static extern int unlink(IntPtr pathname);

        [DllImport("libc", SetLastError = true)]
        public static extern int ftruncate(int fildes, IntPtr length);

        [DllImport("libc", SetLastError = true)]
        public static extern int close(int fd);

        [DllImport("libc", SetLastError = true)]
        public static extern int shm_open(IntPtr name, int oflag, uint mode);

        [DllImport("libc", SetLastError = true)]
        public static extern int shm_unlink(IntPtr name);

        private static int MmapFlagsToSystemFlags(MmapFlags flags)
        {
            int result = 0;

            if (flags.HasFlag(MmapFlags.MAP_SHARED))
            {
                result |= (int)MmapFlags.MAP_SHARED;
            }

            if (flags.HasFlag(MmapFlags.MAP_PRIVATE))
            {
                result |= (int)MmapFlags.MAP_PRIVATE;
            }

            if (flags.HasFlag(MmapFlags.MAP_FIXED))
            {
                result |= (int)MmapFlags.MAP_FIXED;
            }

            if (flags.HasFlag(MmapFlags.MAP_ANONYMOUS))
            {
                if (OperatingSystem.IsLinux())
                {
                    result |= MAP_ANONYMOUS_LINUX_GENERIC;
                }
                else if (OperatingSystem.IsMacOS())
                {
                    result |= MAP_ANONYMOUS_DARWIN;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            if (flags.HasFlag(MmapFlags.MAP_NORESERVE))
            {
                if (OperatingSystem.IsLinux())
                {
                    result |= MAP_NORESERVE_LINUX_GENERIC;
                }
                else if (OperatingSystem.IsMacOS())
                {
                    result |= MAP_NORESERVE_DARWIN;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            if (flags.HasFlag(MmapFlags.MAP_UNLOCKED))
            {
                if (OperatingSystem.IsLinux())
                {
                    result |= MAP_UNLOCKED_LINUX_GENERIC;
                }
                else if (OperatingSystem.IsMacOS())
                {
                    // FIXME: Doesn't exist on Darwin
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            if (OperatingSystem.IsMacOSVersionAtLeast(10, 14))
            {
                result |= MAP_JIT_DARWIN;
            }

            return result;
        }

        public static IntPtr mmap(IntPtr address, ulong length, MmapProts prot, MmapFlags flags, int fd, long offset)
        {
            return Internal_mmap(address, length, prot, MmapFlagsToSystemFlags(flags), fd, offset);
        }
    }
}