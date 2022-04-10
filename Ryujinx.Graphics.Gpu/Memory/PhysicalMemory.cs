using Ryujinx.Cpu;
using Ryujinx.Cpu.Tracking;
using Ryujinx.Graphics.Gpu.Image;
using Ryujinx.Graphics.Gpu.Shader;
using Ryujinx.Memory;
using Ryujinx.Memory.Range;
using Ryujinx.Memory.Tracking;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Ryujinx.Graphics.Gpu.Memory
{
    /// <summary>
    /// Represents physical memory, accessible from the GPU.
    /// This is actually working CPU virtual addresses, of memory mapped on the application process.
    /// </summary>
    class PhysicalMemory : IDisposable
    {
        private readonly GpuContext _context;
        private IVirtualMemoryManagerTracked _cpuMemory;
        private int _referenceCount;

        /// <summary>
        /// In-memory shader cache.
        /// </summary>
        public ShaderCache ShaderCache { get; }

        /// <summary>
        /// GPU buffer manager.
        /// </summary>
        public BufferCache BufferCache { get; }

        /// <summary>
        /// GPU texture manager.
        /// </summary>
        public TextureCache TextureCache { get; }

        /// <summary>
        /// Creates a new instance of the physical memory.
        /// </summary>
        /// <param name="context">GPU context that the physical memory belongs to</param>
        /// <param name="cpuMemory">CPU memory manager of the application process</param>
        public PhysicalMemory(GpuContext context, IVirtualMemoryManagerTracked cpuMemory)
        {
            _context = context;
            _cpuMemory = cpuMemory;
            ShaderCache = new ShaderCache(context);
            BufferCache = new BufferCache(context, this);
            TextureCache = new TextureCache(context, this);

            if (cpuMemory is IRefCounted rc)
            {
                rc.IncrementReferenceCount();
            }

            _referenceCount = 1;
        }

        /// <summary>
        /// Increments the memory reference count.
        /// </summary>
        public void IncrementReferenceCount()
        {
            Interlocked.Increment(ref _referenceCount);
        }

        /// <summary>
        /// Decrements the memory reference count.
        /// </summary>
        public void DecrementReferenceCount()
        {
            if (Interlocked.Decrement(ref _referenceCount) == 0 && _cpuMemory is IRefCounted rc)
            {
                rc.DecrementReferenceCount();
            }
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
                if (singleRange.Address != MemoryManager.PteUnmapped)
                {
                    return _cpuMemory.GetSpan(singleRange.Address, (int)singleRange.Size, tracked);
                }
            }

            Span<byte> data = new byte[range.GetSize()];

            int offset = 0;

            for (int i = 0; i < range.Count; i++)
            {
                var currentRange = range.GetSubRange(i);
                int size = (int)currentRange.Size;
                if (currentRange.Address != MemoryManager.PteUnmapped)
                {
                    _cpuMemory.GetSpan(currentRange.Address, size, tracked).CopyTo(data.Slice(offset, size));
                }
                offset += size;
            }

            return data;
        }

        /// <summary>
        /// Gets a writable region from the application process.
        /// </summary>
        /// <param name="address">Start address of the range</param>
        /// <param name="size">Size in bytes to be range</param>
        /// <param name="tracked">True if write tracking is triggered on the span</param>
        /// <returns>A writable region with the data at the specified memory location</returns>
        public WritableRegion GetWritableRegion(ulong address, int size, bool tracked = false)
        {
            return _cpuMemory.GetWritableRegion(address, size, tracked);
        }

        /// <summary>
        /// Gets a writable region from GPU mapped memory.
        /// </summary>
        /// <param name="range">Range</param>
        /// <param name="tracked">True if write tracking is triggered on the span</param>
        /// <returns>A writable region with the data at the specified memory location</returns>
        public WritableRegion GetWritableRegion(MultiRange range, bool tracked = false)
        {
            if (range.Count == 1)
            {
                MemoryRange subrange = range.GetSubRange(0);

                return GetWritableRegion(subrange.Address, (int)subrange.Size, tracked);
            }
            else
            {
                Memory<byte> memory = new byte[range.GetSize()];

                int offset = 0;
                for (int i = 0; i < range.Count; i++)
                {
                    var currentRange = range.GetSubRange(i);
                    int size = (int)currentRange.Size;
                    if (currentRange.Address != MemoryManager.PteUnmapped)
                    {
                        GetSpan(currentRange.Address, size).CopyTo(memory.Span.Slice(offset, size));
                    }
                    offset += size;
                }

                return new WritableRegion(new MultiRangeWritableBlock(range, this), 0, memory, tracked);
            }
        }

        /// <summary>
        /// Reads data from the application process.
        /// </summary>
        /// <typeparam name="T">Type of the structure</typeparam>
        /// <param name="address">Address to read from</param>
        /// <returns>The data at the specified memory location</returns>
        public T Read<T>(ulong address) where T : unmanaged
        {
            return _cpuMemory.Read<T>(address);
        }

        /// <summary>
        /// Reads data from the application process, with write tracking.
        /// </summary>
        /// <typeparam name="T">Type of the structure</typeparam>
        /// <param name="address">Address to read from</param>
        /// <returns>The data at the specified memory location</returns>
        public T ReadTracked<T>(ulong address) where T : unmanaged
        {
            return _cpuMemory.ReadTracked<T>(address);
        }

        /// <summary>
        /// Writes data to the application process, triggering a precise memory tracking event.
        /// </summary>
        /// <param name="address">Address to write into</param>
        /// <param name="data">Data to be written</param>
        public void WriteTrackedResource(ulong address, ReadOnlySpan<byte> data)
        {
            _cpuMemory.SignalMemoryTracking(address, (ulong)data.Length, true, precise: true);
            _cpuMemory.WriteUntracked(address, data);
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
        private static void WriteImpl(MultiRange range, ReadOnlySpan<byte> data, WriteCallback writeCallback)
        {
            if (range.Count == 1)
            {
                var singleRange = range.GetSubRange(0);
                if (singleRange.Address != MemoryManager.PteUnmapped)
                {
                    writeCallback(singleRange.Address, data);
                }
            }
            else
            {
                int offset = 0;

                for (int i = 0; i < range.Count; i++)
                {
                    var currentRange = range.GetSubRange(i);
                    int size = (int)currentRange.Size;
                    if (currentRange.Address != MemoryManager.PteUnmapped)
                    {
                        writeCallback(currentRange.Address, data.Slice(offset, size));
                    }
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
            int count = 0;

            for (int i = 0; i < range.Count; i++)
            {
                var currentRange = range.GetSubRange(i);
                if (currentRange.Address != MemoryManager.PteUnmapped)
                {
                    cpuRegionHandles[count++] = _cpuMemory.BeginTracking(currentRange.Address, currentRange.Size);
                }
            }

            if (count != range.Count)
            {
                Array.Resize(ref cpuRegionHandles, count);
            }

            return new GpuRegionHandle(cpuRegionHandles);
        }

        /// <summary>
        /// Obtains a memory tracking handle for the given virtual region, with a specified granularity. This should be disposed when finished with.
        /// </summary>
        /// <param name="address">CPU virtual address of the region</param>
        /// <param name="size">Size of the region</param>
        /// <param name="handles">Handles to inherit state from or reuse</param>
        /// <param name="granularity">Desired granularity of write tracking</param>
        /// <returns>The memory tracking handle</returns>
        public CpuMultiRegionHandle BeginGranularTracking(ulong address, ulong size, IEnumerable<IRegionHandle> handles = null, ulong granularity = 4096)
        {
            return _cpuMemory.BeginGranularTracking(address, size, handles, granularity);
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

        /// <summary>
        /// Checks if a given memory page is mapped.
        /// </summary>
        /// <param name="address">CPU virtual address of the page</param>
        /// <returns>True if mapped, false otherwise</returns>
        public bool IsMapped(ulong address)
        {
            return _cpuMemory.IsMapped(address);
        }

        /// <summary>
        /// Release our reference to the CPU memory manager.
        /// </summary>
        public void Dispose()
        {
            _context.DeferredActions.Enqueue(Destroy);
        }

        /// <summary>
        /// Performs disposal of the host GPU caches with resources mapped on this physical memory.
        /// This must only be called from the render thread.
        /// </summary>
        private void Destroy()
        {
            ShaderCache.Dispose();
            BufferCache.Dispose();
            TextureCache.Dispose();

            DecrementReferenceCount();
        }
    }
}