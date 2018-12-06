using Ryujinx.Common;

namespace Ryujinx.HLE.HOS.Kernel
{
    class KMemoryRegionManager
    {
        private static readonly int[] BlockOrders = new int[] { 12, 16, 21, 22, 25, 29, 30 };

        public ulong Address { get; private set; }
        public ulong EndAddr { get; private set; }
        public ulong Size    { get; private set; }

        private int _blockOrdersCount;

        private KMemoryRegionBlock[] _blocks;

        public KMemoryRegionManager(ulong address, ulong size, ulong endAddr)
        {
            _blocks = new KMemoryRegionBlock[BlockOrders.Length];

            Address = address;
            Size    = size;
            EndAddr = endAddr;

            _blockOrdersCount = BlockOrders.Length;

            for (int blockIndex = 0; blockIndex < _blockOrdersCount; blockIndex++)
            {
                _blocks[blockIndex] = new KMemoryRegionBlock();

                _blocks[blockIndex].Order = BlockOrders[blockIndex];

                int nextOrder = blockIndex == _blockOrdersCount - 1 ? 0 : BlockOrders[blockIndex + 1];

                _blocks[blockIndex].NextOrder = nextOrder;

                int currBlockSize = 1 << BlockOrders[blockIndex];
                int nextBlockSize = currBlockSize;

                if (nextOrder != 0)
                {
                    nextBlockSize = 1 << nextOrder;
                }

                ulong startAligned   = BitUtils.AlignDown(address, nextBlockSize);
                ulong endAddrAligned = BitUtils.AlignDown(endAddr, currBlockSize);

                ulong sizeInBlocksTruncated = (endAddrAligned - startAligned) >> BlockOrders[blockIndex];

                ulong endAddrRounded = BitUtils.AlignUp(address + size, nextBlockSize);

                ulong sizeInBlocksRounded = (endAddrRounded - startAligned) >> BlockOrders[blockIndex];

                _blocks[blockIndex].StartAligned          = startAligned;
                _blocks[blockIndex].SizeInBlocksTruncated = sizeInBlocksTruncated;
                _blocks[blockIndex].SizeInBlocksRounded   = sizeInBlocksRounded;

                ulong currSizeInBlocks = sizeInBlocksRounded;

                int maxLevel = 0;

                do
                {
                    maxLevel++;
                }
                while ((currSizeInBlocks /= 64) != 0);

                _blocks[blockIndex].MaxLevel = maxLevel;

                _blocks[blockIndex].Masks = new long[maxLevel][];

                currSizeInBlocks = sizeInBlocksRounded;

                for (int level = maxLevel - 1; level >= 0; level--)
                {
                    currSizeInBlocks = (currSizeInBlocks + 63) / 64;

                    _blocks[blockIndex].Masks[level] = new long[currSizeInBlocks];
                }
            }

            if (size != 0)
            {
                FreePages(address, size / KMemoryManager.PageSize);
            }
        }

        public KernelResult AllocatePages(ulong pagesCount, bool backwards, out KPageList pageList)
        {
            lock (_blocks)
            {
                return AllocatePagesImpl(pagesCount, backwards, out pageList);
            }
        }

        private KernelResult AllocatePagesImpl(ulong pagesCount, bool backwards, out KPageList pageList)
        {
            pageList = new KPageList();

            if (_blockOrdersCount > 0)
            {
                if (GetFreePagesImpl() < pagesCount)
                {
                    return KernelResult.OutOfMemory;
                }
            }
            else if (pagesCount != 0)
            {
                return KernelResult.OutOfMemory;
            }

            for (int blockIndex = _blockOrdersCount - 1; blockIndex >= 0; blockIndex--)
            {
                KMemoryRegionBlock block = _blocks[blockIndex];

                ulong bestFitBlockSize = 1UL << block.Order;

                ulong blockPagesCount = bestFitBlockSize / KMemoryManager.PageSize;

                //Check if this is the best fit for this page size.
                //If so, try allocating as much requested pages as possible.
                while (blockPagesCount <= pagesCount)
                {
                    ulong address = 0;

                    for (int currBlockIndex = blockIndex;
                             currBlockIndex < _blockOrdersCount && address == 0;
                             currBlockIndex++)
                    {
                        block = _blocks[currBlockIndex];

                        int index = 0;

                        bool zeroMask = false;

                        for (int level = 0; level < block.MaxLevel; level++)
                        {
                            long mask = block.Masks[level][index];

                            if (mask == 0)
                            {
                                zeroMask = true;

                                break;
                            }

                            if (backwards)
                            {
                                index = (index * 64 + 63) - BitUtils.CountLeadingZeros64(mask);
                            }
                            else
                            {
                                index = index * 64 + BitUtils.CountLeadingZeros64(BitUtils.ReverseBits64(mask));
                            }
                        }

                        if (block.SizeInBlocksTruncated <= (ulong)index || zeroMask)
                        {
                            continue;
                        }

                        block.FreeCount--;

                        int tempIdx = index;

                        for (int level = block.MaxLevel - 1; level >= 0; level--, tempIdx /= 64)
                        {
                            block.Masks[level][tempIdx / 64] &= ~(1L << (tempIdx & 63));

                            if (block.Masks[level][tempIdx / 64] != 0)
                            {
                                break;
                            }
                        }

                        address = block.StartAligned + ((ulong)index << block.Order);
                    }

                    for (int currBlockIndex = blockIndex;
                             currBlockIndex < _blockOrdersCount && address == 0;
                             currBlockIndex++)
                    {
                        block = _blocks[currBlockIndex];

                        int index = 0;

                        bool zeroMask = false;

                        for (int level = 0; level < block.MaxLevel; level++)
                        {
                            long mask = block.Masks[level][index];

                            if (mask == 0)
                            {
                                zeroMask = true;

                                break;
                            }

                            if (backwards)
                            {
                                index = index * 64 + BitUtils.CountLeadingZeros64(BitUtils.ReverseBits64(mask));
                            }
                            else
                            {
                                index = (index * 64 + 63) - BitUtils.CountLeadingZeros64(mask);
                            }
                        }

                        if (block.SizeInBlocksTruncated <= (ulong)index || zeroMask)
                        {
                            continue;
                        }

                        block.FreeCount--;

                        int tempIdx = index;

                        for (int level = block.MaxLevel - 1; level >= 0; level--, tempIdx /= 64)
                        {
                            block.Masks[level][tempIdx / 64] &= ~(1L << (tempIdx & 63));

                            if (block.Masks[level][tempIdx / 64] != 0)
                            {
                                break;
                            }
                        }

                        address = block.StartAligned + ((ulong)index << block.Order);
                    }

                    //The address being zero means that no free space was found on that order,
                    //just give up and try with the next one.
                    if (address == 0)
                    {
                        break;
                    }

                    //If we are using a larger order than best fit, then we should
                    //split it into smaller blocks.
                    ulong firstFreeBlockSize = 1UL << block.Order;

                    if (firstFreeBlockSize > bestFitBlockSize)
                    {
                        FreePages(address + bestFitBlockSize, (firstFreeBlockSize - bestFitBlockSize) / KMemoryManager.PageSize);
                    }

                    //Add new allocated page(s) to the pages list.
                    //If an error occurs, then free all allocated pages and fail.
                    KernelResult result = pageList.AddRange(address, blockPagesCount);

                    if (result != KernelResult.Success)
                    {
                        FreePages(address, blockPagesCount);

                        foreach (KPageNode pageNode in pageList)
                        {
                            FreePages(pageNode.Address, pageNode.PagesCount);
                        }

                        return result;
                    }

                    pagesCount -= blockPagesCount;
                }
            }

            //Success case, all requested pages were allocated successfully.
            if (pagesCount == 0)
            {
                return KernelResult.Success;
            }

            //Error case, free allocated pages and return out of memory.
            foreach (KPageNode pageNode in pageList)
            {
                FreePages(pageNode.Address, pageNode.PagesCount);
            }

            pageList = null;

            return KernelResult.OutOfMemory;
        }

        public void FreePages(KPageList pageList)
        {
            lock (_blocks)
            {
                foreach (KPageNode pageNode in pageList)
                {
                    FreePages(pageNode.Address, pageNode.PagesCount);
                }
            }
        }

        private void FreePages(ulong address, ulong pagesCount)
        {
            ulong endAddr = address + pagesCount * KMemoryManager.PageSize;

            int blockIndex = _blockOrdersCount - 1;

            ulong addressRounded   = 0;
            ulong endAddrTruncated = 0;

            for (; blockIndex >= 0; blockIndex--)
            {
                KMemoryRegionBlock allocInfo = _blocks[blockIndex];

                int blockSize = 1 << allocInfo.Order;

                addressRounded   = BitUtils.AlignUp  (address, blockSize);
                endAddrTruncated = BitUtils.AlignDown(endAddr, blockSize);

                if (addressRounded < endAddrTruncated)
                {
                    break;
                }
            }

            void FreeRegion(ulong currAddress)
            {
                for (int currBlockIndex = blockIndex;
                         currBlockIndex < _blockOrdersCount && currAddress != 0;
                         currBlockIndex++)
                {
                    KMemoryRegionBlock block = _blocks[currBlockIndex];

                    block.FreeCount++;

                    ulong freedBlocks = (currAddress - block.StartAligned) >> block.Order;

                    int index = (int)freedBlocks;

                    for (int level = block.MaxLevel - 1; level >= 0; level--, index /= 64)
                    {
                        long mask = block.Masks[level][index / 64];

                        block.Masks[level][index / 64] = mask | (1L << (index & 63));

                        if (mask != 0)
                        {
                            break;
                        }
                    }

                    int blockSizeDelta = 1 << (block.NextOrder - block.Order);

                    int freedBlocksTruncated = BitUtils.AlignDown((int)freedBlocks, blockSizeDelta);

                    if (!block.TryCoalesce(freedBlocksTruncated, blockSizeDelta))
                    {
                        break;
                    }

                    currAddress = block.StartAligned + ((ulong)freedBlocksTruncated << block.Order);
                }
            }

            //Free inside aligned region.
            ulong baseAddress = addressRounded;

            while (baseAddress < endAddrTruncated)
            {
                ulong blockSize = 1UL << _blocks[blockIndex].Order;

                FreeRegion(baseAddress);

                baseAddress += blockSize;
            }

            int nextBlockIndex = blockIndex - 1;

            //Free region between Address and aligned region start.
            baseAddress = addressRounded;

            for (blockIndex = nextBlockIndex; blockIndex >= 0; blockIndex--)
            {
                ulong blockSize = 1UL << _blocks[blockIndex].Order;

                while (baseAddress - blockSize >= address)
                {
                    baseAddress -= blockSize;

                    FreeRegion(baseAddress);
                }
            }

            //Free region between aligned region end and End Address.
            baseAddress = endAddrTruncated;

            for (blockIndex = nextBlockIndex; blockIndex >= 0; blockIndex--)
            {
                ulong blockSize = 1UL << _blocks[blockIndex].Order;

                while (baseAddress + blockSize <= endAddr)
                {
                    FreeRegion(baseAddress);

                    baseAddress += blockSize;
                }
            }
        }

        public ulong GetFreePages()
        {
            lock (_blocks)
            {
                return GetFreePagesImpl();
            }
        }

        private ulong GetFreePagesImpl()
        {
            ulong availablePages = 0;

            for (int blockIndex = 0; blockIndex < _blockOrdersCount; blockIndex++)
            {
                KMemoryRegionBlock block = _blocks[blockIndex];

                ulong blockPagesCount = (1UL << block.Order) / KMemoryManager.PageSize;

                availablePages += blockPagesCount * block.FreeCount;
            }

            return availablePages;
        }
    }
}