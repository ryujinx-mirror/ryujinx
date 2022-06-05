using Ryujinx.Memory.WindowsShared;
using System;
using System.Runtime.Versioning;

namespace Ryujinx.Memory
{
    [SupportedOSPlatform("windows")]
    static class MemoryManagementWindows
    {
        public const int PageSize = 0x1000;

        private static readonly PlaceholderManager _placeholders = new PlaceholderManager();
        private static readonly PlaceholderManager4KB _placeholders4KB = new PlaceholderManager4KB();

        public static IntPtr Allocate(IntPtr size)
        {
            return AllocateInternal(size, AllocationType.Reserve | AllocationType.Commit);
        }

        public static IntPtr Reserve(IntPtr size, bool viewCompatible, bool force4KBMap)
        {
            if (viewCompatible)
            {
                IntPtr baseAddress = AllocateInternal2(size, AllocationType.Reserve | AllocationType.ReservePlaceholder);

                if (!force4KBMap)
                {
                    _placeholders.ReserveRange((ulong)baseAddress, (ulong)size);
                }

                return baseAddress;
            }

            return AllocateInternal(size, AllocationType.Reserve);
        }

        private static IntPtr AllocateInternal(IntPtr size, AllocationType flags = 0)
        {
            IntPtr ptr = WindowsApi.VirtualAlloc(IntPtr.Zero, size, flags, MemoryProtection.ReadWrite);

            if (ptr == IntPtr.Zero)
            {
                throw new OutOfMemoryException();
            }

            return ptr;
        }

        private static IntPtr AllocateInternal2(IntPtr size, AllocationType flags = 0)
        {
            IntPtr ptr = WindowsApi.VirtualAlloc2(WindowsApi.CurrentProcessHandle, IntPtr.Zero, size, flags, MemoryProtection.NoAccess, IntPtr.Zero, 0);

            if (ptr == IntPtr.Zero)
            {
                throw new OutOfMemoryException();
            }

            return ptr;
        }

        public static bool Commit(IntPtr location, IntPtr size)
        {
            return WindowsApi.VirtualAlloc(location, size, AllocationType.Commit, MemoryProtection.ReadWrite) != IntPtr.Zero;
        }

        public static bool Decommit(IntPtr location, IntPtr size)
        {
            return WindowsApi.VirtualFree(location, size, AllocationType.Decommit);
        }

        public static void MapView(IntPtr sharedMemory, ulong srcOffset, IntPtr location, IntPtr size, MemoryBlock owner)
        {
            _placeholders.MapView(sharedMemory, srcOffset, location, size, owner);
        }

        public static void MapView4KB(IntPtr sharedMemory, ulong srcOffset, IntPtr location, IntPtr size)
        {
            _placeholders4KB.UnmapAndMarkRangeAsMapped(location, size);

            ulong uaddress = (ulong)location;
            ulong usize = (ulong)size;
            IntPtr endLocation = (IntPtr)(uaddress + usize);

            while (location != endLocation)
            {
                WindowsApi.VirtualFree(location, (IntPtr)PageSize, AllocationType.Release | AllocationType.PreservePlaceholder);

                var ptr = WindowsApi.MapViewOfFile3(
                    sharedMemory,
                    WindowsApi.CurrentProcessHandle,
                    location,
                    srcOffset,
                    (IntPtr)PageSize,
                    0x4000,
                    MemoryProtection.ReadWrite,
                    IntPtr.Zero,
                    0);

                if (ptr == IntPtr.Zero)
                {
                    throw new WindowsApiException("MapViewOfFile3");
                }

                location += PageSize;
                srcOffset += PageSize;
            }
        }

        public static void UnmapView(IntPtr sharedMemory, IntPtr location, IntPtr size, MemoryBlock owner)
        {
            _placeholders.UnmapView(sharedMemory, location, size, owner);
        }

        public static void UnmapView4KB(IntPtr location, IntPtr size)
        {
            _placeholders4KB.UnmapView(location, size);
        }

        public static bool Reprotect(IntPtr address, IntPtr size, MemoryPermission permission, bool forView)
        {
            if (forView)
            {
                return _placeholders.ReprotectView(address, size, permission);
            }
            else
            {
                return WindowsApi.VirtualProtect(address, size, WindowsApi.GetProtection(permission), out _);
            }
        }

        public static bool Reprotect4KB(IntPtr address, IntPtr size, MemoryPermission permission, bool forView)
        {
            ulong uaddress = (ulong)address;
            ulong usize = (ulong)size;
            while (usize > 0)
            {
                if (!WindowsApi.VirtualProtect((IntPtr)uaddress, (IntPtr)PageSize, WindowsApi.GetProtection(permission), out _))
                {
                    return false;
                }

                uaddress += PageSize;
                usize -= PageSize;
            }

            return true;
        }

        public static bool Free(IntPtr address, IntPtr size, bool force4KBMap)
        {
            if (force4KBMap)
            {
                _placeholders4KB.UnmapRange(address, size);
            }
            else
            {
                _placeholders.UnreserveRange((ulong)address, (ulong)size);
            }

            return WindowsApi.VirtualFree(address, IntPtr.Zero, AllocationType.Release);
        }

        public static IntPtr CreateSharedMemory(IntPtr size, bool reserve)
        {
            var prot = reserve ? FileMapProtection.SectionReserve : FileMapProtection.SectionCommit;

            IntPtr handle = WindowsApi.CreateFileMapping(
                WindowsApi.InvalidHandleValue,
                IntPtr.Zero,
                FileMapProtection.PageReadWrite | prot,
                (uint)(size.ToInt64() >> 32),
                (uint)size.ToInt64(),
                null);

            if (handle == IntPtr.Zero)
            {
                throw new OutOfMemoryException();
            }

            return handle;
        }

        public static void DestroySharedMemory(IntPtr handle)
        {
            if (!WindowsApi.CloseHandle(handle))
            {
                throw new ArgumentException("Invalid handle.", nameof(handle));
            }
        }

        public static IntPtr MapSharedMemory(IntPtr handle)
        {
            IntPtr ptr = WindowsApi.MapViewOfFile(handle, 4 | 2, 0, 0, IntPtr.Zero);

            if (ptr == IntPtr.Zero)
            {
                throw new OutOfMemoryException();
            }

            return ptr;
        }

        public static void UnmapSharedMemory(IntPtr address)
        {
            if (!WindowsApi.UnmapViewOfFile(address))
            {
                throw new ArgumentException("Invalid address.", nameof(address));
            }
        }

        public static bool RetryFromAccessViolation()
        {
            return _placeholders.RetryFromAccessViolation();
        }
    }
}