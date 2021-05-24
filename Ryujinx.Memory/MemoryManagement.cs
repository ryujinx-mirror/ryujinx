using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Memory
{
    public static class MemoryManagement
    {
        public static IntPtr Allocate(ulong size)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                IntPtr sizeNint = new IntPtr((long)size);

                return MemoryManagementWindows.Allocate(sizeNint);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                     RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                IntPtr sizeNint = new IntPtr((long)size);

                return MemoryManagementWindows.Reserve(sizeNint);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                     RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                IntPtr sizeNint = new IntPtr((long)size);

                return MemoryManagementWindows.Commit(address, sizeNint);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                     RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                IntPtr sizeNint = new IntPtr((long)size);

                return MemoryManagementWindows.Decommit(address, sizeNint);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                     RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
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

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                IntPtr sizeNint = new IntPtr((long)size);

                result = MemoryManagementWindows.Reprotect(address, sizeNint, permission);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                     RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return MemoryManagementWindows.Free(address);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                     RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                IntPtr sizeNint = new IntPtr((long)size);

                return MemoryManagementWindows.CreateSharedMemory(sizeNint, reserve);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                     RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                MemoryManagementWindows.DestroySharedMemory(handle);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                     RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return MemoryManagementWindows.MapSharedMemory(handle);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                     RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                MemoryManagementWindows.UnmapSharedMemory(address);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                     RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
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