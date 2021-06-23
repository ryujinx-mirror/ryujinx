using Ryujinx.Common;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.Memory.Range;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Kernel.Memory
{
    class KTransferMemory : KAutoObject
    {
        private KProcess _creator;

        // TODO: Remove when we no longer need to read it from the owner directly.
        public KProcess Creator => _creator;

        private readonly List<HostMemoryRange> _ranges;

        private readonly SharedMemoryStorage _storage;

        public ulong Address { get; private set; }
        public ulong Size { get; private set; }

        public KMemoryPermission Permission { get; private set; }

        private bool _hasBeenInitialized;
        private bool _isMapped;

        public KTransferMemory(KernelContext context) : base(context)
        {
            _ranges = new List<HostMemoryRange>();
        }

        public KTransferMemory(KernelContext context, SharedMemoryStorage storage) : base(context)
        {
            _storage = storage;
            Permission = KMemoryPermission.ReadAndWrite;

            _hasBeenInitialized = true;
            _isMapped = false;
        }

        public KernelResult Initialize(ulong address, ulong size, KMemoryPermission permission)
        {
            KProcess creator = KernelStatic.GetCurrentProcess();

            _creator = creator;

            KernelResult result = creator.MemoryManager.BorrowTransferMemory(_ranges, address, size, permission);

            if (result != KernelResult.Success)
            {
                return result;
            }

            creator.IncrementReferenceCount();

            Permission = permission;
            Address = address;
            Size = size;
            _hasBeenInitialized = true;
            _isMapped = false;

            return result;
        }

        public KernelResult MapIntoProcess(
            KPageTableBase memoryManager,
            ulong address,
            ulong size,
            KProcess process,
            KMemoryPermission permission)
        {
            if (_storage == null)
            {
                throw new NotImplementedException();
            }

            ulong pagesCountRounded = BitUtils.DivRoundUp(size, KPageTableBase.PageSize);

            var pageList = _storage.GetPageList();
            if (pageList.GetPagesCount() != pagesCountRounded)
            {
                return KernelResult.InvalidSize;
            }

            if (permission != Permission || _isMapped)
            {
                return KernelResult.InvalidState;
            }

            MemoryState state = Permission == KMemoryPermission.None ? MemoryState.TransferMemoryIsolated : MemoryState.TransferMemory;

            KernelResult result = memoryManager.MapPages(address, pageList, state, KMemoryPermission.ReadAndWrite);

            if (result == KernelResult.Success)
            {
                _isMapped = true;

                if (!memoryManager.SupportsMemoryAliasing)
                {
                    _storage.Borrow(process, address);
                }
            }

            return result;
        }

        public KernelResult UnmapFromProcess(
            KPageTableBase memoryManager,
            ulong address,
            ulong size,
            KProcess process)
        {
            if (_storage == null)
            {
                throw new NotImplementedException();
            }

            ulong pagesCountRounded = BitUtils.DivRoundUp(size, KPageTableBase.PageSize);

            var pageList = _storage.GetPageList();
            ulong pagesCount = pageList.GetPagesCount();

            if (pagesCount != pagesCountRounded)
            {
                return KernelResult.InvalidSize;
            }

            var ranges = _storage.GetRanges();

            MemoryState state = Permission == KMemoryPermission.None ? MemoryState.TransferMemoryIsolated : MemoryState.TransferMemory;

            KernelResult result = memoryManager.UnmapPages(address, pagesCount, ranges, state);

            if (result == KernelResult.Success)
            {
                _isMapped = false;
            }

            return result;
        }

        protected override void Destroy()
        {
            if (_hasBeenInitialized)
            {
                if (!_isMapped && _creator.MemoryManager.UnborrowTransferMemory(Address, Size, _ranges) != KernelResult.Success)
                {
                    throw new InvalidOperationException("Unexpected failure restoring transfer memory attributes.");
                }

                _creator.ResourceLimit?.Release(LimitableResource.TransferMemory, 1);
                _creator.DecrementReferenceCount();
            }
        }
    }
}