using Ryujinx.Memory.WindowsShared;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Ryujinx.Memory
{
    static class MemoryManagementWindows
    {
        private static readonly IntPtr InvalidHandleValue = new IntPtr(-1);
        private static bool UseWin10Placeholders;

        private static object _emulatedHandleLock = new object();
        private static EmulatedSharedMemoryWindows[] _emulatedShared = new EmulatedSharedMemoryWindows[64];
        private static List<EmulatedSharedMemoryWindows> _emulatedSharedList = new List<EmulatedSharedMemoryWindows>();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAlloc(
            IntPtr lpAddress,
            IntPtr dwSize,
            AllocationType flAllocationType,
            MemoryProtection flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualProtect(
            IntPtr lpAddress,
            IntPtr dwSize,
            MemoryProtection flNewProtect,
            out MemoryProtection lpflOldProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualFree(IntPtr lpAddress, IntPtr dwSize, AllocationType dwFreeType);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateFileMapping(
            IntPtr hFile,
            IntPtr lpFileMappingAttributes,
            FileMapProtection flProtect,
            uint dwMaximumSizeHigh,
            uint dwMaximumSizeLow,
            [MarshalAs(UnmanagedType.LPWStr)] string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr MapViewOfFile(
            IntPtr hFileMappingObject,
            uint dwDesiredAccess,
            uint dwFileOffsetHigh,
            uint dwFileOffsetLow,
            IntPtr dwNumberOfBytesToMap);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint GetLastError();

        static MemoryManagementWindows()
        {
            Version version = Environment.OSVersion.Version;

            UseWin10Placeholders = (version.Major == 10 && version.Build >= 17134) || version.Major > 10;
        }

        public static IntPtr Allocate(IntPtr size)
        {
            return AllocateInternal(size, AllocationType.Reserve | AllocationType.Commit);
        }

        public static IntPtr Reserve(IntPtr size)
        {
            return AllocateInternal(size, AllocationType.Reserve);
        }

        private static IntPtr AllocateInternal(IntPtr size, AllocationType flags = 0)
        {
            IntPtr ptr = VirtualAlloc(IntPtr.Zero, size, flags, MemoryProtection.ReadWrite);

            if (ptr == IntPtr.Zero)
            {
                throw new OutOfMemoryException();
            }

            return ptr;
        }

        public static bool Commit(IntPtr location, IntPtr size)
        {
            if (UseWin10Placeholders)
            {
                lock (_emulatedSharedList)
                {
                    foreach (var shared in _emulatedSharedList)
                    {
                        if (shared.CommitMap(location, size))
                        {
                            return true;
                        }
                    }
                }
            }

            return VirtualAlloc(location, size, AllocationType.Commit, MemoryProtection.ReadWrite) != IntPtr.Zero;
        }

        public static bool Decommit(IntPtr location, IntPtr size)
        {
            if (UseWin10Placeholders)
            {
                lock (_emulatedSharedList)
                {
                    foreach (var shared in _emulatedSharedList)
                    {
                        if (shared.DecommitMap(location, size))
                        {
                            return true;
                        }
                    }
                }
            }

            return VirtualFree(location, size, AllocationType.Decommit);
        }

        public static bool Reprotect(IntPtr address, IntPtr size, MemoryPermission permission)
        {
            if (UseWin10Placeholders)
            {
                ulong uaddress = (ulong)address;
                ulong usize = (ulong)size;
                while (usize > 0)
                {
                    ulong nextGranular = (uaddress & ~EmulatedSharedMemoryWindows.MappingMask) + EmulatedSharedMemoryWindows.MappingGranularity;
                    ulong mapSize = Math.Min(usize, nextGranular - uaddress);

                    if (!VirtualProtect((IntPtr)uaddress, (IntPtr)mapSize, GetProtection(permission), out _))
                    {
                        return false;
                    }

                    uaddress = nextGranular;
                    usize -= mapSize;
                }

                return true;
            }
            else
            {
                return VirtualProtect(address, size, GetProtection(permission), out _);
            }
        }

        private static MemoryProtection GetProtection(MemoryPermission permission)
        {
            return permission switch
            {
                MemoryPermission.None => MemoryProtection.NoAccess,
                MemoryPermission.Read => MemoryProtection.ReadOnly,
                MemoryPermission.ReadAndWrite => MemoryProtection.ReadWrite,
                MemoryPermission.ReadAndExecute => MemoryProtection.ExecuteRead,
                MemoryPermission.ReadWriteExecute => MemoryProtection.ExecuteReadWrite,
                MemoryPermission.Execute => MemoryProtection.Execute,
                _ => throw new MemoryProtectionException(permission)
            };
        }

        public static bool Free(IntPtr address)
        {
            return VirtualFree(address, IntPtr.Zero, AllocationType.Release);
        }

        private static int GetEmulatedHandle()
        {
            // Assumes we have the handle lock.

            for (int i = 0; i < _emulatedShared.Length; i++)
            {
                if (_emulatedShared[i] == null)
                {
                    return i + 1;
                }
            }

            throw new InvalidProgramException("Too many shared memory handles were created.");
        }

        public static bool EmulatedHandleValid(ref int handle)
        {
            handle--;
            return handle >= 0 && handle < _emulatedShared.Length && _emulatedShared[handle] != null;
        }

        public static IntPtr CreateSharedMemory(IntPtr size, bool reserve)
        {
            if (UseWin10Placeholders && reserve)
            {
                lock (_emulatedHandleLock)
                {
                    int handle = GetEmulatedHandle();
                    _emulatedShared[handle - 1] = new EmulatedSharedMemoryWindows((ulong)size);
                    _emulatedSharedList.Add(_emulatedShared[handle - 1]);

                    return (IntPtr)handle;
                }
            }
            else
            {
                var prot = reserve ? FileMapProtection.SectionReserve : FileMapProtection.SectionCommit;

                IntPtr handle = CreateFileMapping(
                    InvalidHandleValue,
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
        }

        public static void DestroySharedMemory(IntPtr handle)
        {
            if (UseWin10Placeholders)
            {
                lock (_emulatedHandleLock)
                {
                    int iHandle = (int)(ulong)handle;

                    if (EmulatedHandleValid(ref iHandle))
                    {
                        _emulatedSharedList.Remove(_emulatedShared[iHandle]);
                        _emulatedShared[iHandle].Dispose();
                        _emulatedShared[iHandle] = null;

                        return;
                    }
                }
            }

            if (!CloseHandle(handle))
            {
                throw new ArgumentException("Invalid handle.", nameof(handle));
            }
        }

        public static IntPtr MapSharedMemory(IntPtr handle)
        {
            if (UseWin10Placeholders)
            {
                lock (_emulatedHandleLock)
                {
                    int iHandle = (int)(ulong)handle;

                    if (EmulatedHandleValid(ref iHandle))
                    {
                        return _emulatedShared[iHandle].Map();
                    }
                }
            }

            IntPtr ptr = MapViewOfFile(handle, 4 | 2, 0, 0, IntPtr.Zero);

            if (ptr == IntPtr.Zero)
            {
                throw new OutOfMemoryException();
            }

            return ptr;
        }

        public static void UnmapSharedMemory(IntPtr address)
        {
            if (UseWin10Placeholders)
            {
                lock (_emulatedHandleLock)
                {
                    foreach (EmulatedSharedMemoryWindows shared in _emulatedSharedList)
                    {
                        if (shared.Unmap((ulong)address))
                        {
                            return;
                        }
                    }
                }
            }

            if (!UnmapViewOfFile(address))
            {
                throw new ArgumentException("Invalid address.", nameof(address));
            }
        }
    }
}