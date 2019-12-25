using System;

namespace Ryujinx.Graphics.Gpu.Memory
{
    using CpuMemoryManager = ARMeilleure.Memory.MemoryManager;

    class PhysicalMemory
    {
        private readonly CpuMemoryManager _cpuMemory;

        public PhysicalMemory(CpuMemoryManager cpuMemory)
        {
            _cpuMemory = cpuMemory;
        }

        public Span<byte> Read(ulong address, ulong size)
        {
            return _cpuMemory.ReadBytes((long)address, (long)size);
        }

        public void Write(ulong address, Span<byte> data)
        {
            _cpuMemory.WriteBytes((long)address, data.ToArray());
        }

        public (ulong, ulong)[] GetModifiedRanges(ulong address, ulong size, ResourceName name)
        {
            return _cpuMemory.GetModifiedRanges(address, size, (int)name);
        }
    }
}