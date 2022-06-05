using System;

namespace Ryujinx.Memory
{
    public static class MemoryManagement
    {
        public static IntPtr Allocate(ulong size)
        {
            if (OperatingSystem.IsWindows())
            {
                return MemoryManagementWindows.Allocate((IntPtr)size);
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

        public static IntPtr Reserve(ulong size, bool viewCompatible, bool force4KBMap)
        {
            if (OperatingSystem.IsWindows())
            {
                return MemoryManagementWindows.Reserve((IntPtr)size, viewCompatible, force4KBMap);
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
                return MemoryManagementWindows.Commit(address, (IntPtr)size);
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
                return MemoryManagementWindows.Decommit(address, (IntPtr)size);
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

        public static void MapView(IntPtr sharedMemory, ulong srcOffset, IntPtr address, ulong size, MemoryBlock owner)
        {
            if (OperatingSystem.IsWindows())
            {
                if (owner.ForceWindows4KBView)
                {
                    MemoryManagementWindows.MapView4KB(sharedMemory, srcOffset, address, (IntPtr)size);
                }
                else
                {
                    MemoryManagementWindows.MapView(sharedMemory, srcOffset, address, (IntPtr)size, owner);
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

        public static void UnmapView(IntPtr sharedMemory, IntPtr address, ulong size, MemoryBlock owner)
        {
            if (OperatingSystem.IsWindows())
            {
                if (owner.ForceWindows4KBView)
                {
                    MemoryManagementWindows.UnmapView4KB(address, (IntPtr)size);
                }
                else
                {
                    MemoryManagementWindows.UnmapView(sharedMemory, address, (IntPtr)size, owner);
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
                if (forView && force4KBMap)
                {
                    result = MemoryManagementWindows.Reprotect4KB(address, (IntPtr)size, permission, forView);
                }
                else
                {
                    result = MemoryManagementWindows.Reprotect(address, (IntPtr)size, permission, forView);
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

        public static bool Free(IntPtr address, ulong size, bool force4KBMap)
        {
            if (OperatingSystem.IsWindows())
            {
                return MemoryManagementWindows.Free(address, (IntPtr)size, force4KBMap);
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
                return MemoryManagementWindows.CreateSharedMemory((IntPtr)size, reserve);
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