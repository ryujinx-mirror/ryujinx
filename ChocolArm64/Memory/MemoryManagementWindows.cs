using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ChocolArm64.Memory
{
    static class MemoryManagementWindows
    {
        [Flags]
        private enum AllocationType : uint
        {
            Commit     = 0x1000,
            Reserve    = 0x2000,
            Decommit   = 0x4000,
            Release    = 0x8000,
            Reset      = 0x80000,
            Physical   = 0x400000,
            TopDown    = 0x100000,
            WriteWatch = 0x200000,
            LargePages = 0x20000000
        }

        [Flags]
        private enum MemoryProtection : uint
        {
            NoAccess                 = 0x01,
            ReadOnly                 = 0x02,
            ReadWrite                = 0x04,
            WriteCopy                = 0x08,
            Execute                  = 0x10,
            ExecuteRead              = 0x20,
            ExecuteReadWrite         = 0x40,
            ExecuteWriteCopy         = 0x80,
            GuardModifierflag        = 0x100,
            NoCacheModifierflag      = 0x200,
            WriteCombineModifierflag = 0x400
        }

        private enum WriteWatchFlags : uint
        {
            None  = 0,
            Reset = 1
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr VirtualAlloc(
            IntPtr           lpAddress,
            IntPtr           dwSize,
            AllocationType   flAllocationType,
            MemoryProtection flProtect);

        [DllImport("kernel32.dll")]
        private static extern bool VirtualProtect(
            IntPtr               lpAddress,
            IntPtr               dwSize,
            MemoryProtection     flNewProtect,
            out MemoryProtection lpflOldProtect);

        [DllImport("kernel32.dll")]
        private static extern bool VirtualFree(
            IntPtr         lpAddress,
            IntPtr         dwSize,
            AllocationType dwFreeType);

        [DllImport("kernel32.dll")]
        private static extern int GetWriteWatch(
            WriteWatchFlags dwFlags,
            IntPtr          lpBaseAddress,
            IntPtr          dwRegionSize,
            IntPtr[]        lpAddresses,
            ref ulong       lpdwCount,
            out uint        lpdwGranularity);

        public static IntPtr Allocate(IntPtr size)
        {
            const AllocationType flags =
                AllocationType.Reserve |
                AllocationType.Commit;

            IntPtr ptr = VirtualAlloc(IntPtr.Zero, size, flags, MemoryProtection.ReadWrite);

            if (ptr == IntPtr.Zero)
            {
                throw new OutOfMemoryException();
            }

            return ptr;
        }

        public static IntPtr AllocateWriteTracked(IntPtr size)
        {
            const AllocationType flags =
                AllocationType.Reserve |
                AllocationType.Commit  |
                AllocationType.WriteWatch;

            IntPtr ptr = VirtualAlloc(IntPtr.Zero, size, flags, MemoryProtection.ReadWrite);

            if (ptr == IntPtr.Zero)
            {
                throw new OutOfMemoryException();
            }

            return ptr;
        }

        public static bool Reprotect(IntPtr address, IntPtr size, Memory.MemoryProtection protection)
        {
            MemoryProtection prot = GetProtection(protection);

            return VirtualProtect(address, size, prot, out _);
        }

        private static MemoryProtection GetProtection(Memory.MemoryProtection protection)
        {
            switch (protection)
            {
                case Memory.MemoryProtection.None:           return MemoryProtection.NoAccess;
                case Memory.MemoryProtection.Read:           return MemoryProtection.ReadOnly;
                case Memory.MemoryProtection.ReadAndWrite:   return MemoryProtection.ReadWrite;
                case Memory.MemoryProtection.ReadAndExecute: return MemoryProtection.ExecuteRead;
                case Memory.MemoryProtection.Execute:        return MemoryProtection.Execute;

                default: throw new ArgumentException($"Invalid permission \"{protection}\".");
            }
        }

        public static bool Free(IntPtr address)
        {
            return VirtualFree(address, IntPtr.Zero, AllocationType.Release);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetModifiedPages(
            IntPtr    address,
            IntPtr    size,
            IntPtr[]  addresses,
            out ulong count)
        {
            ulong pagesCount = (ulong)addresses.Length;

            int result = GetWriteWatch(
                WriteWatchFlags.Reset,
                address,
                size,
                addresses,
                ref pagesCount,
                out uint granularity);

            count = pagesCount;

            return result == 0;
        }
    }
}