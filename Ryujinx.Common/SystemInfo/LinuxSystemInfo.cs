using System.IO;
using System.Linq;

namespace Ryujinx.Common.SystemInfo
{
    internal class LinuxSysteminfo : SystemInfo
    {
        public override string CpuName { get; }
        public override ulong RamSize { get; }

        public LinuxSysteminfo()
        {
            CpuName = File.ReadAllLines("/proc/cpuinfo").Where(line => line.StartsWith("model name")).ToList()[0].Split(":")[1].Trim();
            RamSize = ulong.Parse(File.ReadAllLines("/proc/meminfo")[0].Split(":")[1].Trim().Split(" ")[0]) * 1024;
        }
    }
}