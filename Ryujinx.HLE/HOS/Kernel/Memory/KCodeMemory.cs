using Ryujinx.Common;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Process;
using System;
using System.Diagnostics;

namespace Ryujinx.HLE.HOS.Kernel.Memory
{
    class KCodeMemory : KAutoObject
    {
        public KProcess Owner { get; private set; }
        private readonly KPageList _pageList;
        private readonly object _lock;
        private ulong _address;
        private bool _isOwnerMapped;
        private bool _isMapped;

        public KCodeMemory(KernelContext context) : base(context)
        {
            _pageList = new KPageList();
            _lock = new object();
        }

        public KernelResult Initialize(ulong address, ulong size)
        {
            Owner = KernelStatic.GetCurrentProcess();

            KernelResult result = Owner.MemoryManager.BorrowCodeMemory(_pageList, address, size);

            if (result != KernelResult.Success)
            {
                return result;
            }

            Owner.CpuMemory.Fill(address, size, 0xff);
            Owner.IncrementReferenceCount();

            _address = address;
            _isMapped = false;
            _isOwnerMapped = false;

            return KernelResult.Success;
        }

        public KernelResult Map(ulong address, ulong size, KMemoryPermission perm)
        {
            if (_pageList.GetPagesCount() != BitUtils.DivRoundUp(size, KPageTableBase.PageSize))
            {
                return KernelResult.InvalidSize;
            }

            lock (_lock)
            {
                if (_isMapped)
                {
                    return KernelResult.InvalidState;
                }

                KProcess process = KernelStatic.GetCurrentProcess();

                KernelResult result = process.MemoryManager.MapPages(address, _pageList, MemoryState.CodeWritable, KMemoryPermission.ReadAndWrite);

                if (result != KernelResult.Success)
                {
                    return result;
                }

                _isMapped = true;
            }

            return KernelResult.Success;
        }

        public KernelResult MapToOwner(ulong address, ulong size, KMemoryPermission permission)
        {
            if (_pageList.GetPagesCount() != BitUtils.DivRoundUp(size, KPageTableBase.PageSize))
            {
                return KernelResult.InvalidSize;
            }

            lock (_lock)
            {
                if (_isOwnerMapped)
                {
                    return KernelResult.InvalidState;
                }

                Debug.Assert(permission == KMemoryPermission.Read || permission == KMemoryPermission.ReadAndExecute);

                KernelResult result = Owner.MemoryManager.MapPages(address, _pageList, MemoryState.CodeReadOnly, permission);

                if (result != KernelResult.Success)
                {
                    return result;
                }

                _isOwnerMapped = true;
            }

            return KernelResult.Success;
        }

        public KernelResult Unmap(ulong address, ulong size)
        {
            if (_pageList.GetPagesCount() != BitUtils.DivRoundUp(size, KPageTableBase.PageSize))
            {
                return KernelResult.InvalidSize;
            }

            lock (_lock)
            {
                KProcess process = KernelStatic.GetCurrentProcess();

                KernelResult result = process.MemoryManager.UnmapPages(address, _pageList, MemoryState.CodeWritable);

                if (result != KernelResult.Success)
                {
                    return result;
                }

                Debug.Assert(_isMapped);

                _isMapped = false;
            }

            return KernelResult.Success;
        }

        public KernelResult UnmapFromOwner(ulong address, ulong size)
        {
            if (_pageList.GetPagesCount() != BitUtils.DivRoundUp(size, KPageTableBase.PageSize))
            {
                return KernelResult.InvalidSize;
            }

            lock (_lock)
            {
                KernelResult result = Owner.MemoryManager.UnmapPages(address, _pageList, MemoryState.CodeReadOnly);

                if (result != KernelResult.Success)
                {
                    return result;
                }

                Debug.Assert(_isOwnerMapped);

                _isOwnerMapped = false;
            }

            return KernelResult.Success;
        }

        protected override void Destroy()
        {
            if (!_isMapped && !_isOwnerMapped)
            {
                ulong size = _pageList.GetPagesCount() * KPageTableBase.PageSize;

                if (Owner.MemoryManager.UnborrowCodeMemory(_address, size, _pageList) != KernelResult.Success)
                {
                    throw new InvalidOperationException("Unexpected failure restoring transfer memory attributes.");
                }
            }

            Owner.DecrementReferenceCount();
        }
    }
}