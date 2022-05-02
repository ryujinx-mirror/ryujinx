using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.Memory;
using System;
using System.Diagnostics;

namespace Ryujinx.HLE.HOS.Kernel.Memory
{
    class KPageTable : KPageTableBase
    {
        private readonly IVirtualMemoryManager _cpuMemory;

        public KPageTable(KernelContext context, IVirtualMemoryManager cpuMemory) : base(context)
        {
            _cpuMemory = cpuMemory;
        }

        /// <inheritdoc/>
        protected override void GetPhysicalRegions(ulong va, ulong size, KPageList pageList)
        {
            var ranges = _cpuMemory.GetPhysicalRegions(va, size);
            foreach (var range in ranges)
            {
                pageList.AddRange(range.Address + DramMemoryMap.DramBase, range.Size / PageSize);
            }
        }

        /// <inheritdoc/>
        protected override ReadOnlySpan<byte> GetSpan(ulong va, int size)
        {
            return _cpuMemory.GetSpan(va, size);
        }

        /// <inheritdoc/>
        protected override KernelResult MapMemory(ulong src, ulong dst, ulong pagesCount, KMemoryPermission oldSrcPermission, KMemoryPermission newDstPermission)
        {
            KPageList pageList = new KPageList();
            GetPhysicalRegions(src, pagesCount * PageSize, pageList);

            KernelResult result = Reprotect(src, pagesCount, KMemoryPermission.None);

            if (result != KernelResult.Success)
            {
                return result;
            }

            result = MapPages(dst, pageList, newDstPermission, false, 0);

            if (result != KernelResult.Success)
            {
                KernelResult reprotectResult = Reprotect(src, pagesCount, oldSrcPermission);
                Debug.Assert(reprotectResult == KernelResult.Success);
            }

            return result;
        }

        /// <inheritdoc/>
        protected override KernelResult UnmapMemory(ulong dst, ulong src, ulong pagesCount, KMemoryPermission oldDstPermission, KMemoryPermission newSrcPermission)
        {
            ulong size = pagesCount * PageSize;

            KPageList srcPageList = new KPageList();
            KPageList dstPageList = new KPageList();

            GetPhysicalRegions(src, size, srcPageList);
            GetPhysicalRegions(dst, size, dstPageList);

            if (!dstPageList.IsEqual(srcPageList))
            {
                return KernelResult.InvalidMemRange;
            }

            KernelResult result = Unmap(dst, pagesCount);

            if (result != KernelResult.Success)
            {
                return result;
            }

            result = Reprotect(src, pagesCount, newSrcPermission);

            if (result != KernelResult.Success)
            {
                KernelResult mapResult = MapPages(dst, dstPageList, oldDstPermission, false, 0);
                Debug.Assert(mapResult == KernelResult.Success);
            }

            return result;
        }

        /// <inheritdoc/>
        protected override KernelResult MapPages(ulong dstVa, ulong pagesCount, ulong srcPa, KMemoryPermission permission, bool shouldFillPages, byte fillValue)
        {
            ulong size = pagesCount * PageSize;

            Context.Memory.Commit(srcPa - DramMemoryMap.DramBase, size);

            _cpuMemory.Map(dstVa, srcPa - DramMemoryMap.DramBase, size);

            if (DramMemoryMap.IsHeapPhysicalAddress(srcPa))
            {
                Context.MemoryManager.IncrementPagesReferenceCount(srcPa, pagesCount);
            }

            if (shouldFillPages)
            {
                _cpuMemory.Fill(dstVa, size, fillValue);
            }

            return KernelResult.Success;
        }

        /// <inheritdoc/>
        protected override KernelResult MapPages(ulong address, KPageList pageList, KMemoryPermission permission, bool shouldFillPages, byte fillValue)
        {
            using var scopedPageList = new KScopedPageList(Context.MemoryManager, pageList);

            ulong currentVa = address;

            foreach (var pageNode in pageList)
            {
                ulong addr = pageNode.Address - DramMemoryMap.DramBase;
                ulong size = pageNode.PagesCount * PageSize;

                Context.Memory.Commit(addr, size);

                _cpuMemory.Map(currentVa, addr, size);

                if (shouldFillPages)
                {
                    _cpuMemory.Fill(currentVa, size, fillValue);
                }

                currentVa += size;
            }

            scopedPageList.SignalSuccess();

            return KernelResult.Success;
        }

        /// <inheritdoc/>
        protected override KernelResult Unmap(ulong address, ulong pagesCount)
        {
            KPageList pagesToClose = new KPageList();

            var regions = _cpuMemory.GetPhysicalRegions(address, pagesCount * PageSize);

            foreach (var region in regions)
            {
                ulong pa = region.Address + DramMemoryMap.DramBase;
                if (DramMemoryMap.IsHeapPhysicalAddress(pa))
                {
                    pagesToClose.AddRange(pa, region.Size / PageSize);
                }
            }

            _cpuMemory.Unmap(address, pagesCount * PageSize);

            pagesToClose.DecrementPagesReferenceCount(Context.MemoryManager);

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
