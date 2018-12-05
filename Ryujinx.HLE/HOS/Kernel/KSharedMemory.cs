using Ryujinx.Common;

namespace Ryujinx.HLE.HOS.Kernel
{
    class KSharedMemory
    {
        private KPageList PageList;

        private long OwnerPid;

        private MemoryPermission OwnerPermission;
        private MemoryPermission UserPermission;

        public KSharedMemory(
            KPageList        PageList,
            long             OwnerPid,
            MemoryPermission OwnerPermission,
            MemoryPermission UserPermission)
        {
            this.PageList        = PageList;
            this.OwnerPid        = OwnerPid;
            this.OwnerPermission = OwnerPermission;
            this.UserPermission  = UserPermission;
        }

        public KernelResult MapIntoProcess(
            KMemoryManager   MemoryManager,
            ulong            Address,
            ulong            Size,
            KProcess         Process,
            MemoryPermission Permission)
        {
            ulong PagesCountRounded = BitUtils.DivRoundUp(Size, KMemoryManager.PageSize);

            if (PageList.GetPagesCount() != PagesCountRounded)
            {
                return KernelResult.InvalidSize;
            }

            MemoryPermission ExpectedPermission = Process.Pid == OwnerPid
                ? OwnerPermission
                : UserPermission;

            if (Permission != ExpectedPermission)
            {
                return KernelResult.InvalidPermission;
            }

            return MemoryManager.MapPages(Address, PageList, MemoryState.SharedMemory, Permission);
        }

        public KernelResult UnmapFromProcess(
            KMemoryManager   MemoryManager,
            ulong            Address,
            ulong            Size,
            KProcess         Process)
        {
            ulong PagesCountRounded = BitUtils.DivRoundUp(Size, KMemoryManager.PageSize);

            if (PageList.GetPagesCount() != PagesCountRounded)
            {
                return KernelResult.InvalidSize;
            }

            return MemoryManager.UnmapPages(Address, PageList, MemoryState.SharedMemory);
        }
    }
}