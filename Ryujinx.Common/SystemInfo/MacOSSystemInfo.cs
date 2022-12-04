using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using Ryujinx.Common.Logging;

namespace Ryujinx.Common.SystemInfo
{
    [SupportedOSPlatform("macos")]
    class MacOSSystemInfo : SystemInfo
    {
        internal MacOSSystemInfo()
        {
            string cpuName = GetCpuidCpuName();

            if (cpuName == null && sysctlbyname("machdep.cpu.brand_string", out cpuName) != 0)
            {
                cpuName = "Unknown";
            }

            ulong totalRAM = 0;

            if (sysctlbyname("hw.memsize", ref totalRAM) != 0)  // Bytes
            {
                totalRAM = 0;
            }

            CpuName = $"{cpuName} ; {LogicalCoreCount} logical";
            RamTotal = totalRAM;
            RamAvailable = GetVMInfoAvailableMemory();
        }

        static ulong GetVMInfoAvailableMemory()
        {
            var port = mach_host_self();

            uint pageSize = 0;
            var result = host_page_size(port, ref pageSize);

            if (result != 0)
            {
                Logger.Error?.Print(LogClass.Application, $"Failed to query Available RAM. host_page_size() error = {result}");
                return 0;
            }

            const int flavor = 4; // HOST_VM_INFO64
            uint count = (uint)(Marshal.SizeOf<VMStatistics64>() / sizeof(int)); // HOST_VM_INFO64_COUNT
            VMStatistics64 stats = new();
            result = host_statistics64(port, flavor, ref stats, ref count);

            if (result != 0)
            {
                Logger.Error?.Print(LogClass.Application, $"Failed to query Available RAM. host_statistics64() error = {result}");
                return 0;
            }

            return (ulong)(stats.FreeCount + stats.InactiveCount) * pageSize;
        }

        private const string SystemLibraryName = "libSystem.dylib";

        [DllImport(SystemLibraryName, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern int sysctlbyname(string name, IntPtr oldValue, ref ulong oldSize, IntPtr newValue, ulong newValueSize);

        private static int sysctlbyname(string name, IntPtr oldValue, ref ulong oldSize)
        {
            if (sysctlbyname(name, oldValue, ref oldSize, IntPtr.Zero, 0) == -1)
            {
                int err = Marshal.GetLastWin32Error();

                Logger.Error?.Print(LogClass.Application, $"Cannot retrieve '{name}'. Error Code {err}");

                return err;
            }

            return 0;
        }

        private static int sysctlbyname<T>(string name, ref T oldValue)
        {
            unsafe
            {
                ulong oldValueSize = (ulong)Unsafe.SizeOf<T>();

                return sysctlbyname(name, (IntPtr)Unsafe.AsPointer(ref oldValue), ref oldValueSize);
            }
        }

        private static int sysctlbyname(string name, out string oldValue)
        {
            oldValue = default;

            ulong strSize = 0;

            int res = sysctlbyname(name, IntPtr.Zero, ref strSize);

            if (res == 0)
            {
                byte[] rawData = new byte[strSize];

                unsafe
                {
                    fixed (byte* rawDataPtr = rawData)
                    {
                        res = sysctlbyname(name, (IntPtr)rawDataPtr, ref strSize);
                    }

                    if (res == 0)
                    {
                        oldValue = Encoding.ASCII.GetString(rawData);
                    }
                }
            }

            return res;
        }

        [DllImport(SystemLibraryName, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern uint mach_host_self();

        [DllImport(SystemLibraryName, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern int host_page_size(uint host, ref uint out_page_size);

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        struct VMStatistics64
        {
            public uint FreeCount;
            public uint ActiveCount;
            public uint InactiveCount;
            public uint WireCount;
            public ulong ZeroFillCount;
            public ulong Reactivations;
            public ulong Pageins;
            public ulong Pageouts;
            public ulong Faults;
            public ulong CowFaults;
            public ulong Lookups;
            public ulong Hits;
            public ulong Purges;
            public uint PurgeableCount;
            public uint SpeculativeCount;
            public ulong Decompressions;
            public ulong Compressions;
            public ulong Swapins;
            public ulong Swapouts;
            public uint CompressorPageCount;
            public uint ThrottledCount;
            public uint ExternalPageCount;
            public uint InternalPageCount;
            public ulong TotalUncompressedPagesInCompressor;
        }

        [DllImport(SystemLibraryName, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern int host_statistics64(uint host_priv, int host_flavor, ref VMStatistics64 host_info64_out, ref uint host_info64_outCnt);
    }
}