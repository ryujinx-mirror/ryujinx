using System;

namespace Ryujinx.HLE.HOS.Kernel.Memory
{
    class SharedMemoryStorage
    {
        private readonly KernelContext _context;
        private readonly KPageList _pageList;
        private readonly ulong _size;

        public SharedMemoryStorage(KernelContext context, KPageList pageList)
        {
            _context = context;
            _pageList = pageList;
            _size = pageList.GetPagesCount() * KPageTableBase.PageSize;

            foreach (KPageNode pageNode in pageList)
            {
                ulong address = pageNode.Address - DramMemoryMap.DramBase;
                ulong size = pageNode.PagesCount * KPageTableBase.PageSize;
                context.Memory.Commit(address, size);
            }
        }

        public void ZeroFill()
        {
            for (ulong offset = 0; offset < _size; offset += sizeof(ulong))
            {
                GetRef<ulong>(offset) = 0;
            }
        }

        public ref T GetRef<T>(ulong offset) where T : unmanaged
        {
            if (_pageList.Nodes.Count == 1)
            {
                ulong address = _pageList.Nodes.First.Value.Address - DramMemoryMap.DramBase;
                return ref _context.Memory.GetRef<T>(address + offset);
            }

            throw new NotImplementedException("Non-contiguous shared memory is not yet supported.");
        }

        public KPageList GetPageList()
        {
            return _pageList;
        }
    }
}
