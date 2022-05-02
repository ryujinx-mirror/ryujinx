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
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                return MemoryManagementUnix.Allocate(size);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public static IntPtr Reserve(ulong size, bool viewCompatible)
        {
            if (OperatingSystem.IsWindows())
            {
                IntPtr sizeNint = new IntPtr((long)size);

                return MemoryManagementWindows.Reserve(sizeNint, viewCompatible);
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
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
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
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
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                return MemoryManagementUnix.Decommit(address, size);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public static void MapView(IntPtr sharedMemory, ulong srcOffset, IntPtr address, ulong size, bool force4KBMap)
        {
            if (OperatingSystem.IsWindows())
            {
                IntPtr sizeNint = new IntPtr((long)size);

                if (force4KBMap)
                {
                    MemoryManagementWindows.MapView4KB(sharedMemory, srcOffset, address, sizeNint);
                }
                else
                {
                    MemoryManagementWindows.MapView(sharedMemory, srcOffset, address, sizeNint);
                }
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                MemoryManagementUnix.MapView(sharedMemory, srcOffset, address, size);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public static void UnmapView(IntPtr sharedMemory, IntPtr address, ulong size, bool force4KBMap)
        {
            if (OperatingSystem.IsWindows())
            {
                IntPtr sizeNint = new IntPtr((long)size);

                if (force4KBMap)
                {
                    MemoryManagementWindows.UnmapView4KB(address, sizeNint);
                }
                else
                {
                    MemoryManagementWindows.UnmapView(sharedMemory, address, sizeNint);
                }
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                MemoryManagementUnix.UnmapView(address, size);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public static void Reprotect(IntPtr address, ulong size, MemoryPermission permission, bool forView, bool force4KBMap, bool throwOnFail)
        {
            bool result;

            if (OperatingSystem.IsWindows())
            {
                IntPtr sizeNint = new IntPtr((long)size);

                if (forView && force4KBMap)
                {
                    result = MemoryManagementWindows.Reprotect4KB(address, sizeNint, permission, forView);
                }
                else
                {
                    result = MemoryManagementWindows.Reprotect(address, sizeNint, permission, forView);
                }
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
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
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
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
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
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
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                MemoryManagementUnix.DestroySharedMemory(handle);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public static IntPtr MapSharedMemory(IntPtr handle, ulong size)
        {
            if (OperatingSystem.IsWindows())
            {
                return MemoryManagementWindows.MapSharedMemory(handle);
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                return MemoryManagementUnix.MapSharedMemory(handle, size);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public static void UnmapSharedMemory(IntPtr address, ulong size)
        {
            if (OperatingSystem.IsWindows())
            {
                MemoryManagementWindows.UnmapSharedMemory(address);
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                MemoryManagementUnix.UnmapSharedMemory(address, size);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }
    }
}