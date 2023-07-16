using Ryujinx.Common;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.Horizon.Common;
using Ryujinx.Memory;

namespace Ryujinx.HLE.HOS.Kernel.Memory
{
    class KSharedMemory : KAutoObject
    {
        private readonly KPageList _pageList;

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
            _pageList = storage.GetPageList();
            _ownerPid = ownerPid;
            _ownerPermission = ownerPermission;
            _userPermission = userPermission;
        }

        public Result MapIntoProcess(
            KPageTableBase memoryManager,
            ulong address,
            ulong size,
            KProcess process,
            KMemoryPermission permission)
        {
            if (_pageList.GetPagesCount() != BitUtils.DivRoundUp<ulong>(size, KPageTableBase.PageSize))
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

            // On platforms with page size > 4 KB, this can fail due to the address not being page aligned,
            // we can return an error to force the application to retry with a different address.

            try
            {
                return memoryManager.MapPages(address, _pageList, MemoryState.SharedMemory, permission);
            }
            catch (InvalidMemoryRegionException)
            {
                return KernelResult.InvalidMemState;
            }
        }

        public Result UnmapFromProcess(KPageTableBase memoryManager, ulong address, ulong size, KProcess process)
        {
            if (_pageList.GetPagesCount() != BitUtils.DivRoundUp<ulong>(size, KPageTableBase.PageSize))
            {
                return KernelResult.InvalidSize;
            }

            return memoryManager.UnmapPages(address, _pageList, MemoryState.SharedMemory);
        }
    }
}
