using Ryujinx.Common;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Process;

namespace Ryujinx.HLE.HOS.Kernel.Memory
{
    class KSharedMemory : KAutoObject
    {
        private readonly KPageList _pageList;

        private readonly long _ownerPid;

        private readonly KMemoryPermission _ownerPermission;
        private readonly KMemoryPermission _userPermission;

        public KSharedMemory(
            KernelContext    context,
            KPageList        pageList,
            long             ownerPid,
            KMemoryPermission ownerPermission,
            KMemoryPermission userPermission) : base(context)
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
            KMemoryPermission permission)
        {
            ulong pagesCountRounded = BitUtils.DivRoundUp(size, KMemoryManager.PageSize);

            if (_pageList.GetPagesCount() != pagesCountRounded)
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