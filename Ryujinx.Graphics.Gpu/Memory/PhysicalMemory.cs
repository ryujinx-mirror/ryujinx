using Ryujinx.Cpu;
using Ryujinx.Cpu.Tracking;
using Ryujinx.Memory;
using Ryujinx.Memory.Range;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Memory
{
    /// <summary>
    /// Represents physical memory, accessible from the GPU.
    /// This is actually working CPU virtual addresses, of memory mapped on the application process.
    /// </summary>
    class PhysicalMemory
    {
        public const int PageSize = 0x1000;

        private readonly Cpu.MemoryManager _cpuMemory;

        /// <summary>
        /// Creates a new instance of the physical memory.
        /// </summary>
        /// <param name="cpuMemory">CPU memory manager of the application process</param>
        public PhysicalMemory(Cpu.MemoryManager cpuMemory)
        {
            _cpuMemory = cpuMemory;
        }

        /// <summary>
        /// Gets a span of data from the application process.
        /// </summary>
        /// <param name="address">Start address of the range</param>
        /// <param name="size">Size in bytes to be range</param>
        /// <param name="tracked">True if read tracking is triggered on the span</param>
        /// <returns>A read only span of the data at the specified memory location</returns>
        public ReadOnlySpan<byte> GetSpan(ulong address, int size, bool tracked = false)
        {
            return _cpuMemory.GetSpan(address, size, tracked);
        }

        /// <summary>
        /// Gets a span of data from the application process.
        /// </summary>
        /// <param name="range">Ranges of physical memory where the data is located</param>
        /// <param name="tracked">True if read tracking is triggered on the span</param>
        /// <returns>A read only span of the data at the specified memory location</returns>
        public ReadOnlySpan<byte> GetSpan(MultiRange range, bool tracked = false)
        {
            if (range.Count == 1)
            {
                var singleRange = range.GetSubRange(0);
                return _cpuMemory.GetSpan(singleRange.Address, (int)singleRange.Size, tracked);
            }
            else
            {
                Span<byte> data = new byte[range.GetSize()];

                int offset = 0;

                for (int i = 0; i < range.Count; i++)
                {
                    var currentRange = range.GetSubRange(i);
                    int size = (int)currentRange.Size;
                    _cpuMemory.GetSpan(currentRange.Address, size, tracked).CopyTo(data.Slice(offset, size));
                    offset += size;
                }

                return data;
            }
        }

        /// <summary>
        /// Gets a writable region from the application process.
        /// </summary>
        /// <param name="address">Start address of the range</param>
        /// <param name="size">Size in bytes to be range</param>
        /// <returns>A writable region with the data at the specified memory location</returns>
        public WritableRegion GetWritableRegion(ulong address, int size)
        {
            return _cpuMemory.GetWritableRegion(address, size);
        }

        /// <summary>
        /// Reads data from the application process.
        /// </summary>
        /// <typeparam name="T">Type of the structure</typeparam>
        /// <param name="gpuVa">Address to read from</param>
        /// <returns>The data at the specified memory location</returns>
        public T Read<T>(ulong address) where T : unmanaged
        {
            return MemoryMarshal.Cast<byte, T>(GetSpan(address, Unsafe.SizeOf<T>()))[0];
        }

        /// <summary>
        /// Writes data to the application process.
        /// </summary>
        /// <param name="address">Address to write into</param>
        /// <param name="data">Data to be written</param>
        public void Write(ulong address, ReadOnlySpan<byte> data)
        {
            _cpuMemory.Write(address, data);
        }

        /// <summary>
        /// Writes data to the application process.
        /// </summary>
        /// <param name="range">Ranges of physical memory where the data is located</param>
        /// <param name="data">Data to be written</param>
        public void Write(MultiRange range, ReadOnlySpan<byte> data)
        {
            WriteImpl(range, data, _cpuMemory.Write);
        }

        /// <summary>
        /// Writes data to the application process, without any tracking.
        /// </summary>
        /// <param name="address">Address to write into</param>
        /// <param name="data">Data to be written</param>
        public void WriteUntracked(ulong address, ReadOnlySpan<byte> data)
        {
            _cpuMemory.WriteUntracked(address, data);
        }

        /// <summary>
        /// Writes data to the application process, without any tracking.
        /// </summary>
        /// <param name="range">Ranges of physical memory where the data is located</param>
        /// <param name="data">Data to be written</param>
        public void WriteUntracked(MultiRange range, ReadOnlySpan<byte> data)
        {
            WriteImpl(range, data, _cpuMemory.WriteUntracked);
        }

        private delegate void WriteCallback(ulong address, ReadOnlySpan<byte> data);

        /// <summary>
        /// Writes data to the application process, using the supplied callback method.
        /// </summary>
        /// <param name="range">Ranges of physical memory where the data is located</param>
        /// <param name="data">Data to be written</param>
        /// <param name="writeCallback">Callback method that will perform the write</param>
        private void WriteImpl(MultiRange range, ReadOnlySpan<byte> data, WriteCallback writeCallback)
        {
            if (range.Count == 1)
            {
                var singleRange = range.GetSubRange(0);
                writeCallback(singleRange.Address, data);
            }
            else
            {
                int offset = 0;

                for (int i = 0; i < range.Count; i++)
                {
                    var currentRange = range.GetSubRange(i);
                    int size = (int)currentRange.Size;
                    writeCallback(currentRange.Address, data.Slice(offset, size));
                    offset += size;
                }
            }
        }

        /// <summary>
        /// Obtains a memory tracking handle for the given virtual region. This should be disposed when finished with.
        /// </summary>
        /// <param name="address">CPU virtual address of the region</param>
        /// <param name="size">Size of the region</param>
        /// <returns>The memory tracking handle</returns>
        public CpuRegionHandle BeginTracking(ulong address, ulong size)
        {
            return _cpuMemory.BeginTracking(address, size);
        }

        /// <summary>
        /// Obtains a memory tracking handle for the given virtual region. This should be disposed when finished with.
        /// </summary>
        /// <param name="range">Ranges of physical memory where the data is located</param>
        /// <returns>The memory tracking handle</returns>
        public GpuRegionHandle BeginTracking(MultiRange range)
        {
            var cpuRegionHandles = new CpuRegionHandle[range.Count];

            for (int i = 0; i < range.Count; i++)
            {
                var currentRange = range.GetSubRange(i);
                cpuRegionHandles[i] = _cpuMemory.BeginTracking(currentRange.Address, currentRange.Size);
            }

            return new GpuRegionHandle(cpuRegionHandles);
        }

        /// <summary>
        /// Obtains a memory tracking handle for the given virtual region, with a specified granularity. This should be disposed when finished with.
        /// </summary>
        /// <param name="address">CPU virtual address of the region</param>
        /// <param name="size">Size of the region</param>
        /// <param name="granularity">Desired granularity of write tracking</param>
        /// <returns>The memory tracking handle</returns>
        public CpuMultiRegionHandle BeginGranularTracking(ulong address, ulong size, ulong granularity = 4096)
        {
            return _cpuMemory.BeginGranularTracking(address, size, granularity);
        }

        /// <summary>
        /// Obtains a smart memory tracking handle for the given virtual region, with a specified granularity. This should be disposed when finished with.
        /// </summary>
        /// <param name="address">CPU virtual address of the region</param>
        /// <param name="size">Size of the region</param>
        /// <param name="granularity">Desired granularity of write tracking</param>
        /// <returns>The memory tracking handle</returns>
        public CpuSmartMultiRegionHandle BeginSmartGranularTracking(ulong address, ulong size, ulong granularity = 4096)
        {
            return _cpuMemory.BeginSmartGranularTracking(address, size, granularity);
        }
    }
}