using ChocolArm64.Events;
using ChocolArm64.Memory;
using System.Collections.Concurrent;

namespace Ryujinx.Graphics.Memory
{
    class NvGpuVmmCache
    {
        private const int PageBits = MemoryManager.PageBits;

        private const long PageSize = MemoryManager.PageSize;
        private const long PageMask = MemoryManager.PageMask;

        private ConcurrentDictionary<long, int>[] CachedPages;

        private MemoryManager _memory;

        public NvGpuVmmCache(MemoryManager memory)
        {
            _memory = memory;

            _memory.ObservedAccess += MemoryAccessHandler;

            CachedPages = new ConcurrentDictionary<long, int>[1 << 20];
        }

        private void MemoryAccessHandler(object sender, MemoryAccessEventArgs e)
        {
            long pa = _memory.GetPhysicalAddress(e.Position);

            CachedPages[pa >> PageBits]?.Clear();
        }

        public bool IsRegionModified(long position, long size, NvGpuBufferType bufferType)
        {
            long pa = _memory.GetPhysicalAddress(position);

            long addr = pa;

            long endAddr = (addr + size + PageMask) & ~PageMask;

            int newBuffMask = 1 << (int)bufferType;

            _memory.StartObservingRegion(position, size);

            long cachedPagesCount = 0;

            while (addr < endAddr)
            {
                long page = addr >> PageBits;

                ConcurrentDictionary<long, int> dictionary = CachedPages[page];

                if (dictionary == null)
                {
                    dictionary = new ConcurrentDictionary<long, int>();

                    CachedPages[page] = dictionary;
                }

                if (dictionary.TryGetValue(pa, out int currBuffMask))
                {
                    if ((currBuffMask & newBuffMask) != 0)
                    {
                        cachedPagesCount++;
                    }
                    else
                    {
                        dictionary[pa] |= newBuffMask;
                    }
                }
                else
                {
                    dictionary[pa] = newBuffMask;
                }

                addr += PageSize;
            }

            return cachedPagesCount != (endAddr - pa + PageMask) >> PageBits;
        }
    }
}