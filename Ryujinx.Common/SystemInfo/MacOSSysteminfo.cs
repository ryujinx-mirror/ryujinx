using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using Ryujinx.Common.Logging;

namespace Ryujinx.Common.SystemInfo
{
    [SupportedOSPlatform("macos")]
    internal class MacOSSysteminfo : SystemInfo
    {
        public override string CpuName { get; }
        public override ulong RamSize { get; }

        [DllImport("libSystem.dylib", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern int sysctlbyname(string name, IntPtr oldValue, ref ulong oldSize, IntPtr newValue, ulong newValueSize);

        private static int sysctlbyname(string name, IntPtr oldValue, ref ulong oldSize)
        {
            if (sysctlbyname(name, oldValue, ref oldSize, IntPtr.Zero, 0) == -1)
            {
                return Marshal.GetLastWin32Error();
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

        public MacOSSysteminfo()
        {
            ulong ramSize = 0;

            int res = sysctlbyname("hw.memsize", ref ramSize);

            if (res == 0)
            {
                RamSize = ramSize;
            }
            else
            {
                Logger.Error?.Print(LogClass.Application, $"Cannot get memory size, sysctlbyname error: {res}");

                RamSize = 0;
            }

            res = sysctlbyname("machdep.cpu.brand_string", out string cpuName);

            if (res == 0)
            {
                CpuName = cpuName;
            }
            else
            {
                Logger.Error?.Print(LogClass.Application, $"Cannot get CPU name, sysctlbyname error: {res}");

                CpuName = "Unknown";
            }
        }
    }
}