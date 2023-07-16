using Ryujinx.Cpu;
using Ryujinx.HLE.HOS.Kernel.Process;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Kernel.Common
{
    static class KernelTransfer
    {
        public static bool UserToKernel<T>(out T value, ulong address) where T : unmanaged
        {
            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            if (currentProcess.CpuMemory.IsRangeMapped(address, (ulong)Unsafe.SizeOf<T>()))
            {
                value = currentProcess.CpuMemory.Read<T>(address);

                return true;
            }

            value = default;

            return false;
        }

        public static bool UserToKernelArray<T>(ulong address, Span<T> values) where T : unmanaged
        {
            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            Span<byte> data = MemoryMarshal.Cast<T, byte>(values);

            if (currentProcess.CpuMemory.IsRangeMapped(address, (ulong)data.Length))
            {
                currentProcess.CpuMemory.Read(address, data);

                return true;
            }

            return false;
        }

        public static bool UserToKernelString(out string value, ulong address, uint size)
        {
            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            if (currentProcess.CpuMemory.IsRangeMapped(address, size))
            {
                value = MemoryHelper.ReadAsciiString(currentProcess.CpuMemory, address, size);

                return true;
            }

            value = null;

            return false;
        }

        public static bool KernelToUser<T>(ulong address, T value) where T : unmanaged
        {
            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            if (currentProcess.CpuMemory.IsRangeMapped(address, (ulong)Unsafe.SizeOf<T>()))
            {
                currentProcess.CpuMemory.Write(address, value);

                return true;
            }

            return false;
        }
    }
}
