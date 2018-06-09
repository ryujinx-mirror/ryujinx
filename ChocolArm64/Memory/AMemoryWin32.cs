using System;
using System.Runtime.InteropServices;

namespace ChocolArm64.Memory
{
    static class AMemoryWin32
    {
        private const int MEM_COMMIT      = 0x00001000;
        private const int MEM_RESERVE     = 0x00002000;
        private const int MEM_WRITE_WATCH = 0x00200000;

        private const int PAGE_READWRITE = 0x04;

        private const int MEM_RELEASE = 0x8000;

        private const int WRITE_WATCH_FLAG_RESET = 1;

        [DllImport("kernel32.dll")]
        private static extern IntPtr VirtualAlloc(IntPtr lpAddress, IntPtr dwSize, int flAllocationType, int flProtect);

        [DllImport("kernel32.dll")]
        private static extern bool VirtualFree(IntPtr lpAddress, IntPtr dwSize, int dwFreeType);

        [DllImport("kernel32.dll")]
        private unsafe static extern int GetWriteWatch(
            int      dwFlags,
            IntPtr   lpBaseAddress,
            IntPtr   dwRegionSize,
            IntPtr[] lpAddresses,
            long*    lpdwCount,
            long*    lpdwGranularity);

        public static IntPtr Allocate(IntPtr Size)
        {
            const int Flags = MEM_COMMIT | MEM_RESERVE | MEM_WRITE_WATCH;

            IntPtr Address = VirtualAlloc(IntPtr.Zero, Size, Flags, PAGE_READWRITE);

            if (Address == IntPtr.Zero)
            {
                throw new InvalidOperationException();
            }

            return Address;
        }

        public static void Free(IntPtr Address)
        {
            VirtualFree(Address, IntPtr.Zero, MEM_RELEASE);
        }

        public unsafe static long IsRegionModified(IntPtr Address, IntPtr Size, bool Reset)
        {
            IntPtr[] Addresses = new IntPtr[1];

            long Count = Addresses.Length;

            long Granularity;

            int Flags = Reset ? WRITE_WATCH_FLAG_RESET : 0;

            GetWriteWatch(
                Flags,
                Address,
                Size,
                Addresses,
                &Count,
                &Granularity);

            return Count != 0 ? Granularity : 0;
        }
    }
}