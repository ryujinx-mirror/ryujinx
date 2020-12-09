using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Process;
using System;

namespace Ryujinx.HLE.HOS.Kernel.Memory
{
    class KTransferMemory : KAutoObject
    {
        private KProcess _creator;

        // TODO: Remove when we no longer need to read it from the owner directly.
        public KProcess Creator => _creator;

        private readonly KPageList _pageList;

        public ulong Address { get; private set; }
        public ulong Size => _pageList.GetPagesCount() * KMemoryManager.PageSize;

        public KMemoryPermission Permission { get; private set; }

        private bool _hasBeenInitialized;
        private bool _isMapped;

        public KTransferMemory(KernelContext context) : base(context)
        {
            _pageList = new KPageList();
        }

        public KernelResult Initialize(ulong address, ulong size, KMemoryPermission permission)
        {
            KProcess creator = KernelStatic.GetCurrentProcess();

            _creator = creator;

            KernelResult result = creator.MemoryManager.BorrowTransferMemory(_pageList, address, size, permission);

            if (result != KernelResult.Success)
            {
                return result;
            }

            creator.IncrementReferenceCount();

            Permission = permission;
            Address = address;
            _hasBeenInitialized = true;
            _isMapped = false;

            return result;
        }

        protected override void Destroy()
        {
            if (_hasBeenInitialized)
            {
                if (!_isMapped && _creator.MemoryManager.UnborrowTransferMemory(Address, Size, _pageList) != KernelResult.Success)
                {
                    throw new InvalidOperationException("Unexpected failure restoring transfer memory attributes.");
                }

                _creator.ResourceLimit?.Release(LimitableResource.TransferMemory, 1);
                _creator.DecrementReferenceCount();
            }
        }
    }
}