using ChocolArm64.Memory;
using System.Collections.Concurrent;

namespace Ryujinx.Graphics.Memory
{
    class NvGpuVmmCache
    {
        private const int PageBits = MemoryManager.PageBits;

        private const long PageSize = MemoryManager.PageSize;
        private const long PageMask = MemoryManager.PageMask;

        private ConcurrentDictionary<long, int>[] _cachedPages;

        private MemoryManager _memory;

        public NvGpuVmmCache(MemoryManager memory)
        {
            _memory = memory;

            _cachedPages = new ConcurrentDictionary<long, int>[1 << 20];
        }

        public bool IsRegionModified(long position, long size, NvGpuBufferType bufferType)
        {
            long va = position;

            long pa = _memory.GetPhysicalAddress(va);

            long endAddr = (va + size + PageMask) & ~PageMask;

            long addrTruncated = va & ~PageMask;

            bool modified = _memory.IsRegionModified(addrTruncated, endAddr - addrTruncated);

            int newBuffMask = 1 << (int)bufferType;

            long cachedPagesCount = 0;

            while (va < endAddr)
            {
                long page = _memory.GetPhysicalAddress(va) >> PageBits;

                ConcurrentDictionary<long, int> dictionary = _cachedPages[page];

                if (dictionary == null)
                {
                    dictionary = new ConcurrentDictionary<long, int>();

                    _cachedPages[page] = dictionary;
                }
                else if (modified)
                {
                    _cachedPages[page].Clear();
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

                va += PageSize;
            }

            return cachedPagesCount != (endAddr - addrTruncated) >> PageBits;
        }
    }
}