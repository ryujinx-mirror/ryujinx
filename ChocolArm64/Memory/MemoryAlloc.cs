using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ChocolArm64.Memory
{
    public static class MemoryAlloc
    {
        public static bool HasWriteWatchSupport => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static IntPtr Allocate(ulong size)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                IntPtr sizeNint = new IntPtr((long)size);

                return MemoryAllocWindows.Allocate(sizeNint);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                     RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return MemoryAllocUnix.Allocate(size);
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

                return MemoryAllocWindows.AllocateWriteTracked(sizeNint);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                     RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return MemoryAllocUnix.Allocate(size);
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

                result = MemoryAllocWindows.Reprotect(address, sizeNint, permission);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                     RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                result = MemoryAllocUnix.Reprotect(address, size, permission);
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
                return MemoryAllocWindows.Free(address);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                     RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return MemoryAllocUnix.Free(address);
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
            //This is only supported on windows, but returning
            //false (failed) is also valid for platforms without
            //write tracking support on the OS.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return MemoryAllocWindows.GetModifiedPages(address, size, addresses, out count);
            }
            else
            {
                count = 0;

                return false;
            }
        }
    }
}