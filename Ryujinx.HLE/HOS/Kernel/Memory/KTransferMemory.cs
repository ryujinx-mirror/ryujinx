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

        public ulong Address { get; private set; }
        public ulong Size { get; private set; }

        public KMemoryPermission Permission { get; private set; }

        private bool _hasBeenInitialized;
        private bool _isMapped;

        public KTransferMemory(KernelContext context) : base(context)
        {
            _ranges = new List<HostMemoryRange>();
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