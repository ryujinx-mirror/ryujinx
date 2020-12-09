using Ryujinx.Cpu;
using Ryujinx.HLE.HOS.Kernel.Process;
using System;

namespace Ryujinx.HLE.HOS.Kernel.Common
{
    static class KernelTransfer
    {
        public static bool UserToKernelInt32(KernelContext context, ulong address, out int value)
        {
            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            if (currentProcess.CpuMemory.IsMapped(address) &&
                currentProcess.CpuMemory.IsMapped(address + 3))
            {
                value = currentProcess.CpuMemory.Read<int>(address);

                return true;
            }

            value = 0;

            return false;
        }

        public static bool UserToKernelInt32Array(KernelContext context, ulong address, Span<int> values)
        {
            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            for (int index = 0; index < values.Length; index++, address += 4)
            {
                if (currentProcess.CpuMemory.IsMapped(address) &&
                    currentProcess.CpuMemory.IsMapped(address + 3))
                {
                    values[index]= currentProcess.CpuMemory.Read<int>(address);
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public static bool UserToKernelString(KernelContext context, ulong address, int size, out string value)
        {
            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            if (currentProcess.CpuMemory.IsMapped(address) &&
                currentProcess.CpuMemory.IsMapped(address + (ulong)size - 1))
            {
                value = MemoryHelper.ReadAsciiString(currentProcess.CpuMemory, (long)address, size);

                return true;
            }

            value = null;

            return false;
        }

        public static bool KernelToUserInt32(KernelContext context, ulong address, int value)
        {
            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            if (currentProcess.CpuMemory.IsMapped(address) &&
                currentProcess.CpuMemory.IsMapped(address + 3))
            {
                currentProcess.CpuMemory.Write(address, value);

                return true;
            }

            return false;
        }

        public static bool KernelToUserInt64(KernelContext context, ulong address, long value)
        {
            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            if (currentProcess.CpuMemory.IsMapped(address) &&
                currentProcess.CpuMemory.IsMapped(address + 7))
            {
                currentProcess.CpuMemory.Write(address, value);

                return true;
            }

            return false;
        }
    }
}