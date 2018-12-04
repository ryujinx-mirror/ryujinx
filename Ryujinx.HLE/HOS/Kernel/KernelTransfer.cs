using ChocolArm64.Memory;

namespace Ryujinx.HLE.HOS.Kernel
{
    static class KernelTransfer
    {
        public static bool UserToKernelInt32(Horizon system, long address, out int value)
        {
            KProcess currentProcess = system.Scheduler.GetCurrentProcess();

            if (currentProcess.CpuMemory.IsMapped(address) &&
                currentProcess.CpuMemory.IsMapped(address + 3))
            {
                value = currentProcess.CpuMemory.ReadInt32(address);

                return true;
            }

            value = 0;

            return false;
        }

        public static bool UserToKernelString(Horizon system, long address, int size, out string value)
        {
            KProcess currentProcess = system.Scheduler.GetCurrentProcess();

            if (currentProcess.CpuMemory.IsMapped(address) &&
                currentProcess.CpuMemory.IsMapped(address + size - 1))
            {
                value = MemoryHelper.ReadAsciiString(currentProcess.CpuMemory, address, size);

                return true;
            }

            value = null;

            return false;
        }

        public static bool KernelToUserInt32(Horizon system, long address, int value)
        {
            KProcess currentProcess = system.Scheduler.GetCurrentProcess();

            if (currentProcess.CpuMemory.IsMapped(address) &&
                currentProcess.CpuMemory.IsMapped(address + 3))
            {
                currentProcess.CpuMemory.WriteInt32ToSharedAddr(address, value);

                return true;
            }

            return false;
        }

        public static bool KernelToUserInt64(Horizon system, long address, long value)
        {
            KProcess currentProcess = system.Scheduler.GetCurrentProcess();

            if (currentProcess.CpuMemory.IsMapped(address) &&
                currentProcess.CpuMemory.IsMapped(address + 7))
            {
                currentProcess.CpuMemory.WriteInt64(address, value);

                return true;
            }

            return false;
        }
    }
}