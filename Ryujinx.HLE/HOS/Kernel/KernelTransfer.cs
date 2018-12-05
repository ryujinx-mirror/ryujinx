using ChocolArm64.Memory;

namespace Ryujinx.HLE.HOS.Kernel
{
    static class KernelTransfer
    {
        public static bool UserToKernelInt32(Horizon System, long Address, out int Value)
        {
            KProcess CurrentProcess = System.Scheduler.GetCurrentProcess();

            if (CurrentProcess.CpuMemory.IsMapped(Address) &&
                CurrentProcess.CpuMemory.IsMapped(Address + 3))
            {
                Value = CurrentProcess.CpuMemory.ReadInt32(Address);

                return true;
            }

            Value = 0;

            return false;
        }

        public static bool UserToKernelString(Horizon System, long Address, int Size, out string Value)
        {
            KProcess CurrentProcess = System.Scheduler.GetCurrentProcess();

            if (CurrentProcess.CpuMemory.IsMapped(Address) &&
                CurrentProcess.CpuMemory.IsMapped(Address + Size - 1))
            {
                Value = MemoryHelper.ReadAsciiString(CurrentProcess.CpuMemory, Address, Size);

                return true;
            }

            Value = null;

            return false;
        }

        public static bool KernelToUserInt32(Horizon System, long Address, int Value)
        {
            KProcess CurrentProcess = System.Scheduler.GetCurrentProcess();

            if (CurrentProcess.CpuMemory.IsMapped(Address) &&
                CurrentProcess.CpuMemory.IsMapped(Address + 3))
            {
                CurrentProcess.CpuMemory.WriteInt32ToSharedAddr(Address, Value);

                return true;
            }

            return false;
        }

        public static bool KernelToUserInt64(Horizon System, long Address, long Value)
        {
            KProcess CurrentProcess = System.Scheduler.GetCurrentProcess();

            if (CurrentProcess.CpuMemory.IsMapped(Address) &&
                CurrentProcess.CpuMemory.IsMapped(Address + 7))
            {
                CurrentProcess.CpuMemory.WriteInt64(Address, Value);

                return true;
            }

            return false;
        }
    }
}