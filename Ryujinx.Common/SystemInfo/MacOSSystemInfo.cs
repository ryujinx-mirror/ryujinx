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
            };

            CpuName = $"{cpuName} ; {LogicalCoreCount} logical";
            RamTotal = totalRAM;
        }

        [DllImport("libSystem.dylib", CharSet = CharSet.Ansi, SetLastError = true)]
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
    }
}