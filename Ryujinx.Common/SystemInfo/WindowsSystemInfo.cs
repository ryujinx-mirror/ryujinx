using System.Management;

namespace Ryujinx.Common.SystemInfo
{
    internal class WindowsSysteminfo : SystemInfo
    {
        public override string CpuName { get; }
        public override ulong RamSize { get; }

        public WindowsSysteminfo()
        {
            foreach (ManagementBaseObject mObject in new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor").Get())
            {
                CpuName = mObject["Name"].ToString();
            }

            foreach (ManagementBaseObject mObject in new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_OperatingSystem").Get())
            {
                RamSize = ulong.Parse(mObject["TotalVisibleMemorySize"].ToString()) * 1024;
            }
        }
    }
}