using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.Memory;
using Ryujinx.Memory.Range;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ryujinx.HLE.HOS.Kernel.Memory
{
    class KPageTableHostMapped : KPageTableBase
    {
        private const int CopyChunckSize = 0x100000;

        private readonly IVirtualMemoryManager _cpuMemory;

        public override bool SupportsMemoryAliasing => false;

        public KPageTableHostMapped(KernelContext context, IVirtualMemoryManager cpuMemory) : base(context)
        {
            _cpuMemory = cpuMemory;
        }

        /// <inheritdoc/>
        protected override IEnumerable<HostMemoryRange> GetPhysicalRegions(ulong va, ulong size)
        {
            return _cpuMemory.GetPhysicalRegions(va, size);
        }

        /// <inheritdoc/>
        protected override ReadOnlySpan<byte> GetSpan(ulong va, int size)
        {
            return _cpuMemory.GetSpan(va, size);
        }

        /// <inheritdoc/>
        protected override KernelResult MapMemory(ulong src, ulong dst, ulong pagesCount, KMemoryPermission oldSrcPermission, KMemoryPermission newDstPermission)
        {
            ulong size = pagesCount * PageSize;

            _cpuMemory.Map(dst, 0, size);

            ulong currentSize = size;
            while (currentSize > 0)
            {
                ulong copySize = Math.Min(currentSize, CopyChunckSize);
                _cpuMemory.Write(dst, _cpuMemory.GetSpan(src, (int)copySize));
                currentSize -= copySize;
            }

            return KernelResult.Success;
        }

        /// <inheritdoc/>
        protected override KernelResult UnmapMemory(ulong dst, ulong src, ulong pagesCount, KMemoryPermission oldDstPermission, KMemoryPermission newSrcPermission)
        {
            ulong size = pagesCount * PageSize;

            // TODO: Validation.

            ulong currentSize = size;
            while (currentSize > 0)
            {
                ulong copySize = Math.Min(currentSize, CopyChunckSize);
                _cpuMemory.Write(src, _cpuMemory.GetSpan(dst, (int)copySize));
                currentSize -= copySize;
            }

            _cpuMemory.Unmap(dst, size);
            return KernelResult.Success;
        }

        /// <inheritdoc/>
        protected override KernelResult MapPages(ulong dstVa, ulong pagesCount, ulong srcPa, KMemoryPermission permission)
        {
            _cpuMemory.Map(dstVa, 0, pagesCount * PageSize);
            return KernelResult.Success;
        }

        /// <inheritdoc/>
        protected override KernelResult MapPages(ulong address, KPageList pageList, KMemoryPermission permission)
        {
            _cpuMemory.Map(address, 0, pageList.GetPagesCount() * PageSize);
            return KernelResult.Success;
        }

        /// <inheritdoc/>
        protected override KernelResult MapPages(ulong address, IEnumerable<HostMemoryRange> ranges, KMemoryPermission permission)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        protected override KernelResult Unmap(ulong address, ulong pagesCount)
        {
            _cpuMemory.Unmap(address, pagesCount * PageSize);
            return KernelResult.Success;
        }

        /// <inheritdoc/>
        protected override KernelResult Reprotect(ulong address, ulong pagesCount, KMemoryPermission permission)
        {
            // TODO.
            return KernelResult.Success;
        }

        /// <inheritdoc/>
        protected override KernelResult ReprotectWithAttributes(ulong address, ulong pagesCount, KMemoryPermission permission)
        {
            // TODO.
            return KernelResult.Success;
        }

        /// <inheritdoc/>
        protected override void SignalMemoryTracking(ulong va, ulong size, bool write)
        {
            _cpuMemory.SignalMemoryTracking(va, size, write);
        }

        /// <inheritdoc/>
        protected override void Write(ulong va, ReadOnlySpan<byte> data)
        {
            _cpuMemory.Write(va, data);
        }
    }
}
