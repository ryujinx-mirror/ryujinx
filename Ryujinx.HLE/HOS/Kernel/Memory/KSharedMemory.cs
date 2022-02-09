using Ryujinx.Common;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Process;

namespace Ryujinx.HLE.HOS.Kernel.Memory
{
    class KSharedMemory : KAutoObject
    {
        private readonly SharedMemoryStorage _storage;

        private readonly ulong _ownerPid;

        private readonly KMemoryPermission _ownerPermission;
        private readonly KMemoryPermission _userPermission;

        public KSharedMemory(
            KernelContext context,
            SharedMemoryStorage storage,
            ulong ownerPid,
            KMemoryPermission ownerPermission,
            KMemoryPermission userPermission) : base(context)
        {
            _storage = storage;
            _ownerPid = ownerPid;
            _ownerPermission = ownerPermission;
            _userPermission = userPermission;
        }

        public KernelResult MapIntoProcess(
            KPageTableBase memoryManager,
            ulong address,
            ulong size,
            KProcess process,
            KMemoryPermission permission)
        {
            ulong pagesCountRounded = BitUtils.DivRoundUp(size, KPageTableBase.PageSize);

            var pageList = _storage.GetPageList();
            if (pageList.GetPagesCount() != pagesCountRounded)
            {
                return KernelResult.InvalidSize;
            }

            KMemoryPermission expectedPermission = process.Pid == _ownerPid
                ? _ownerPermission
                : _userPermission;

            if (permission != expectedPermission)
            {
                return KernelResult.InvalidPermission;
            }

            KernelResult result = memoryManager.MapPages(address, pageList, MemoryState.SharedMemory, permission);

            if (result == KernelResult.Success && !memoryManager.SupportsMemoryAliasing)
            {
                _storage.Borrow(process, address);
            }

            return result;
        }

        public KernelResult UnmapFromProcess(
            KPageTableBase memoryManager,
            ulong address,
            ulong size,
            KProcess process)
        {
            ulong pagesCountRounded = BitUtils.DivRoundUp(size, KPageTableBase.PageSize);

            var pageList = _storage.GetPageList();
            ulong pagesCount = pageList.GetPagesCount();

            if (pagesCount != pagesCountRounded)
            {
                return KernelResult.InvalidSize;
            }

            var ranges = _storage.GetRanges();

            return memoryManager.UnmapPages(address, pagesCount, ranges, MemoryState.SharedMemory);
        }
    }
}