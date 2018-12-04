using Ryujinx.Common;

namespace Ryujinx.HLE.HOS.Kernel
{
    class KSharedMemory
    {
        private KPageList _pageList;

        private long _ownerPid;

        private MemoryPermission _ownerPermission;
        private MemoryPermission _userPermission;

        public KSharedMemory(
            KPageList        pageList,
            long             ownerPid,
            MemoryPermission ownerPermission,
            MemoryPermission userPermission)
        {
            _pageList        = pageList;
            _ownerPid        = ownerPid;
            _ownerPermission = ownerPermission;
            _userPermission  = userPermission;
        }

        public KernelResult MapIntoProcess(
            KMemoryManager   memoryManager,
            ulong            address,
            ulong            size,
            KProcess         process,
            MemoryPermission permission)
        {
            ulong pagesCountRounded = BitUtils.DivRoundUp(size, KMemoryManager.PageSize);

            if (_pageList.GetPagesCount() != pagesCountRounded)
            {
                return KernelResult.InvalidSize;
            }

            MemoryPermission expectedPermission = process.Pid == _ownerPid
                ? _ownerPermission
                : _userPermission;

            if (permission != expectedPermission)
            {
                return KernelResult.InvalidPermission;
            }

            return memoryManager.MapPages(address, _pageList, MemoryState.SharedMemory, permission);
        }

        public KernelResult UnmapFromProcess(
            KMemoryManager   memoryManager,
            ulong            address,
            ulong            size,
            KProcess         process)
        {
            ulong pagesCountRounded = BitUtils.DivRoundUp(size, KMemoryManager.PageSize);

            if (_pageList.GetPagesCount() != pagesCountRounded)
            {
                return KernelResult.InvalidSize;
            }

            return memoryManager.UnmapPages(address, _pageList, MemoryState.SharedMemory);
        }
    }
}