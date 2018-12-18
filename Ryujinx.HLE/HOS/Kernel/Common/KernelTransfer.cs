using Ryujinx.HLE.HOS.Kernel.Process;
using ChocolArm64.Memory;

namespace Ryujinx.HLE.HOS.Kernel.Common
{
    static class KernelTransfer
    {
        public static bool UserToKernelInt32(Horizon system, ulong address, out int value)
        {
            KProcess currentProcess = system.Scheduler.GetCurrentProcess();

            if (currentProcess.CpuMemory.IsMapped((long)address) &&
                currentProcess.CpuMemory.IsMapped((long)address + 3))
            {
                value = currentProcess.CpuMemory.ReadInt32((long)address);

                return true;
            }

            value = 0;

            return false;
        }

        public static bool UserToKernelString(Horizon system, ulong address, int size, out string value)
        {
            KProcess currentProcess = system.Scheduler.GetCurrentProcess();

            if (currentProcess.CpuMemory.IsMapped((long)address) &&
                currentProcess.CpuMemory.IsMapped((long)address + size - 1))
            {
                value = MemoryHelper.ReadAsciiString(currentProcess.CpuMemory, (long)address, size);

                return true;
            }

            value = null;

            return false;
        }

        public static bool KernelToUserInt32(Horizon system, ulong address, int value)
        {
            KProcess currentProcess = system.Scheduler.GetCurrentProcess();

            if (currentProcess.CpuMemory.IsMapped((long)address) &&
                currentProcess.CpuMemory.IsMapped((long)address + 3))
            {
                currentProcess.CpuMemory.WriteInt32ToSharedAddr((long)address, value);

                return true;
            }

            return false;
        }

        public static bool KernelToUserInt64(Horizon system, ulong address, long value)
        {
            KProcess currentProcess = system.Scheduler.GetCurrentProcess();

            if (currentProcess.CpuMemory.IsMapped((long)address) &&
                currentProcess.CpuMemory.IsMapped((long)address + 7))
            {
                currentProcess.CpuMemory.WriteInt64((long)address, value);

                return true;
            }

            return false;
        }
    }
}