using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Memory.WindowsShared
{
    static class WindowsApi
    {
        public static readonly IntPtr InvalidHandleValue = new IntPtr(-1);
        public static readonly IntPtr CurrentProcessHandle = new IntPtr(-1);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr VirtualAlloc(
            IntPtr lpAddress,
            IntPtr dwSize,
            AllocationType flAllocationType,
            MemoryProtection flProtect);

        [DllImport("KernelBase.dll", SetLastError = true)]
        public static extern IntPtr VirtualAlloc2(
            IntPtr process,
            IntPtr lpAddress,
            IntPtr dwSize,
            AllocationType flAllocationType,
            MemoryProtection flProtect,
            IntPtr extendedParameters,
            ulong parameterCount);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualProtect(
            IntPtr lpAddress,
            IntPtr dwSize,
            MemoryProtection flNewProtect,
            out MemoryProtection lpflOldProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualFree(IntPtr lpAddress, IntPtr dwSize, AllocationType dwFreeType);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateFileMapping(
            IntPtr hFile,
            IntPtr lpFileMappingAttributes,
            FileMapProtection flProtect,
            uint dwMaximumSizeHigh,
            uint dwMaximumSizeLow,
            [MarshalAs(UnmanagedType.LPWStr)] string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr MapViewOfFile(
            IntPtr hFileMappingObject,
            uint dwDesiredAccess,
            uint dwFileOffsetHigh,
            uint dwFileOffsetLow,
            IntPtr dwNumberOfBytesToMap);

        [DllImport("KernelBase.dll", SetLastError = true)]
        public static extern IntPtr MapViewOfFile3(
            IntPtr hFileMappingObject,
            IntPtr process,
            IntPtr baseAddress,
            ulong offset,
            IntPtr dwNumberOfBytesToMap,
            ulong allocationType,
            MemoryProtection dwDesiredAccess,
            IntPtr extendedParameters,
            ulong parameterCount);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

        [DllImport("KernelBase.dll", SetLastError = true)]
        public static extern bool UnmapViewOfFile2(IntPtr process, IntPtr lpBaseAddress, ulong unmapFlags);

        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();

        public static MemoryProtection GetProtection(MemoryPermission permission)
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
    }
}