using System;

namespace Ryujinx.Memory
{
    public static class MemoryManagement
    {
        public static IntPtr Allocate(ulong size)
        {
            if (OperatingSystem.IsWindows())
            {
                IntPtr sizeNint = new IntPtr((long)size);

                return MemoryManagementWindows.Allocate(sizeNint);
            }
            else if (OperatingSystem.IsLinux() ||
                     OperatingSystem.IsMacOS())
            {
                return MemoryManagementUnix.Allocate(size);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public static IntPtr Reserve(ulong size)
        {
            if (OperatingSystem.IsWindows())
            {
                IntPtr sizeNint = new IntPtr((long)size);

                return MemoryManagementWindows.Reserve(sizeNint);
            }
            else if (OperatingSystem.IsLinux() ||
                     OperatingSystem.IsMacOS())
            {
                return MemoryManagementUnix.Reserve(size);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public static bool Commit(IntPtr address, ulong size)
        {
            if (OperatingSystem.IsWindows())
            {
                IntPtr sizeNint = new IntPtr((long)size);

                return MemoryManagementWindows.Commit(address, sizeNint);
            }
            else if (OperatingSystem.IsLinux() ||
                     OperatingSystem.IsMacOS())
            {
                return MemoryManagementUnix.Commit(address, size);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public static bool Decommit(IntPtr address, ulong size)
        {
            if (OperatingSystem.IsWindows())
            {
                IntPtr sizeNint = new IntPtr((long)size);

                return MemoryManagementWindows.Decommit(address, sizeNint);
            }
            else if (OperatingSystem.IsLinux() ||
                     OperatingSystem.IsMacOS())
            {
                return MemoryManagementUnix.Decommit(address, size);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public static void Reprotect(IntPtr address, ulong size, MemoryPermission permission, bool throwOnFail)
        {
            bool result;

            if (OperatingSystem.IsWindows())
            {
                IntPtr sizeNint = new IntPtr((long)size);

                result = MemoryManagementWindows.Reprotect(address, sizeNint, permission);
            }
            else if (OperatingSystem.IsLinux() ||
                     OperatingSystem.IsMacOS())
            {
                result = MemoryManagementUnix.Reprotect(address, size, permission);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            if (!result && throwOnFail)
            {
                throw new MemoryProtectionException(permission);
            }
        }

        public static bool Free(IntPtr address)
        {
            if (OperatingSystem.IsWindows())
            {
                return MemoryManagementWindows.Free(address);
            }
            else if (OperatingSystem.IsLinux() ||
                     OperatingSystem.IsMacOS())
            {
                return MemoryManagementUnix.Free(address);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public static IntPtr CreateSharedMemory(ulong size, bool reserve)
        {
            if (OperatingSystem.IsWindows())
            {
                IntPtr sizeNint = new IntPtr((long)size);

                return MemoryManagementWindows.CreateSharedMemory(sizeNint, reserve);
            }
            else if (OperatingSystem.IsLinux() ||
                     OperatingSystem.IsMacOS())
            {
                return MemoryManagementUnix.CreateSharedMemory(size, reserve);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public static void DestroySharedMemory(IntPtr handle)
        {
            if (OperatingSystem.IsWindows())
            {
                MemoryManagementWindows.DestroySharedMemory(handle);
            }
            else if (OperatingSystem.IsLinux() ||
                     OperatingSystem.IsMacOS())
            {
                MemoryManagementUnix.DestroySharedMemory(handle);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public static IntPtr MapSharedMemory(IntPtr handle)
        {
            if (OperatingSystem.IsWindows())
            {
                return MemoryManagementWindows.MapSharedMemory(handle);
            }
            else if (OperatingSystem.IsLinux() ||
                     OperatingSystem.IsMacOS())
            {
                return MemoryManagementUnix.MapSharedMemory(handle);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public static void UnmapSharedMemory(IntPtr address)
        {
            if (OperatingSystem.IsWindows())
            {
                MemoryManagementWindows.UnmapSharedMemory(address);
            }
            else if (OperatingSystem.IsLinux() ||
                     OperatingSystem.IsMacOS())
            {
                MemoryManagementUnix.UnmapSharedMemory(address);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public static IntPtr Remap(IntPtr target, IntPtr source, ulong size)
        {
            if (OperatingSystem.IsLinux() ||
                OperatingSystem.IsMacOS())
            {
                return MemoryManagementUnix.Remap(target, source, size);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }
    }
}