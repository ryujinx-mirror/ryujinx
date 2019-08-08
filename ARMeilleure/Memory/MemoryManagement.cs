using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ARMeilleure.Memory
{
    public static class MemoryManagement
    {
        public static bool HasWriteWatchSupport => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

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

        public static IntPtr AllocateWriteTracked(ulong size)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                IntPtr sizeNint = new IntPtr((long)size);

                return MemoryManagementWindows.AllocateWriteTracked(sizeNint);
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

        public static void Reprotect(IntPtr address, ulong size, MemoryProtection permission)
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

            if (!result)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetModifiedPages(
            IntPtr    address,
            IntPtr    size,
            IntPtr[]  addresses,
            out ulong count)
        {
            // This is only supported on windows, but returning
            // false (failed) is also valid for platforms without
            // write tracking support on the OS.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return MemoryManagementWindows.GetModifiedPages(address, size, addresses, out count);
            }
            else
            {
                count = 0;

                return false;
            }
        }
    }
}