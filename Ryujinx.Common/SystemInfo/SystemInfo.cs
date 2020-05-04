using System.Runtime.InteropServices;

namespace Ryujinx.Common.SystemInfo
{
    public class SystemInfo
    {
        public virtual string OsDescription => $"{RuntimeInformation.OSDescription} ({RuntimeInformation.OSArchitecture})";
        public virtual string CpuName => "Unknown";
        public virtual ulong RamSize => 0;

        public string RamSizeInMB
        {
            get
            {
                if (RamSize == 0)
                {
                    return "Unknown";
                }

                return $"{RamSize / 1024 / 1024} MB";
            }
        }

        public static SystemInfo Instance { get; }

        static SystemInfo()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Instance = new WindowsSysteminfo();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Instance = new LinuxSysteminfo();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Instance = new MacOSSysteminfo();
            }
            else
            {
                Instance = new SystemInfo();
            }
        }
    }
}