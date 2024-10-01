using System;

namespace Ryujinx.HLE.HOS.Kernel.Memory
{
    struct KScopedPageList : IDisposable
    {
        private readonly KMemoryManager _manager;
        private KPageList _pageList;

        public KScopedPageList(KMemoryManager manager, KPageList pageList)
        {
            _manager = manager;
            _pageList = pageList;
            pageList.IncrementPagesReferenceCount(manager);
        }

        public void SignalSuccess()
        {
            _pageList = null;
        }

        public readonly void Dispose()
        {
            _pageList?.DecrementPagesReferenceCount(_manager);
        }
    }
}
