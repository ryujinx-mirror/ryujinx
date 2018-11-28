using Ryujinx.Common;

namespace Ryujinx.HLE.HOS.Kernel
{
    class KMemoryRegionManager
    {
        private static readonly int[] BlockOrders = new int[] { 12, 16, 21, 22, 25, 29, 30 };

        public ulong Address { get; private set; }
        public ulong EndAddr { get; private set; }
        public ulong Size    { get; private set; }

        private int BlockOrdersCount;

        private KMemoryRegionBlock[] Blocks;

        public KMemoryRegionManager(ulong Address, ulong Size, ulong EndAddr)
        {
            Blocks = new KMemoryRegionBlock[BlockOrders.Length];

            this.Address = Address;
            this.Size    = Size;
            this.EndAddr = EndAddr;

            BlockOrdersCount = BlockOrders.Length;

            for (int BlockIndex = 0; BlockIndex < BlockOrdersCount; BlockIndex++)
            {
                Blocks[BlockIndex] = new KMemoryRegionBlock();

                Blocks[BlockIndex].Order = BlockOrders[BlockIndex];

                int NextOrder = BlockIndex == BlockOrdersCount - 1 ? 0 : BlockOrders[BlockIndex + 1];

                Blocks[BlockIndex].NextOrder = NextOrder;

                int CurrBlockSize = 1 << BlockOrders[BlockIndex];
                int NextBlockSize = CurrBlockSize;

                if (NextOrder != 0)
                {
                    NextBlockSize = 1 << NextOrder;
                }

                ulong StartAligned   = BitUtils.AlignDown(Address, NextBlockSize);
                ulong EndAddrAligned = BitUtils.AlignDown(EndAddr, CurrBlockSize);

                ulong SizeInBlocksTruncated = (EndAddrAligned - StartAligned) >> BlockOrders[BlockIndex];

                ulong EndAddrRounded = BitUtils.AlignUp(Address + Size, NextBlockSize);

                ulong SizeInBlocksRounded = (EndAddrRounded - StartAligned) >> BlockOrders[BlockIndex];

                Blocks[BlockIndex].StartAligned          = StartAligned;
                Blocks[BlockIndex].SizeInBlocksTruncated = SizeInBlocksTruncated;
                Blocks[BlockIndex].SizeInBlocksRounded   = SizeInBlocksRounded;

                ulong CurrSizeInBlocks = SizeInBlocksRounded;

                int MaxLevel = 0;

                do
                {
                    MaxLevel++;
                }
                while ((CurrSizeInBlocks /= 64) != 0);

                Blocks[BlockIndex].MaxLevel = MaxLevel;

                Blocks[BlockIndex].Masks = new long[MaxLevel][];

                CurrSizeInBlocks = SizeInBlocksRounded;

                for (int Level = MaxLevel - 1; Level >= 0; Level--)
                {
                    CurrSizeInBlocks = (CurrSizeInBlocks + 63) / 64;

                    Blocks[BlockIndex].Masks[Level] = new long[CurrSizeInBlocks];
                }
            }

            if (Size != 0)
            {
                FreePages(Address, Size / KMemoryManager.PageSize);
            }
        }

        public KernelResult AllocatePages(ulong PagesCount, bool Backwards, out KPageList PageList)
        {
            lock (Blocks)
            {
                return AllocatePagesImpl(PagesCount, Backwards, out PageList);
            }
        }

        private KernelResult AllocatePagesImpl(ulong PagesCount, bool Backwards, out KPageList PageList)
        {
            PageList = new KPageList();

            if (BlockOrdersCount > 0)
            {
                if (GetFreePagesImpl() < PagesCount)
                {
                    return KernelResult.OutOfMemory;
                }
            }
            else if (PagesCount != 0)
            {
                return KernelResult.OutOfMemory;
            }

            for (int BlockIndex = BlockOrdersCount - 1; BlockIndex >= 0; BlockIndex--)
            {
                KMemoryRegionBlock Block = Blocks[BlockIndex];

                ulong BestFitBlockSize = 1UL << Block.Order;

                ulong BlockPagesCount = BestFitBlockSize / KMemoryManager.PageSize;

                //Check if this is the best fit for this page size.
                //If so, try allocating as much requested pages as possible.
                while (BlockPagesCount <= PagesCount)
                {
                    ulong Address = 0;

                    for (int CurrBlockIndex = BlockIndex;
                             CurrBlockIndex < BlockOrdersCount && Address == 0;
                             CurrBlockIndex++)
                    {
                        Block = Blocks[CurrBlockIndex];

                        int Index = 0;

                        bool ZeroMask = false;

                        for (int Level = 0; Level < Block.MaxLevel; Level++)
                        {
                            long Mask = Block.Masks[Level][Index];

                            if (Mask == 0)
                            {
                                ZeroMask = true;

                                break;
                            }

                            if (Backwards)
                            {
                                Index = (Index * 64 + 63) - BitUtils.CountLeadingZeros64(Mask);
                            }
                            else
                            {
                                Index = Index * 64 + BitUtils.CountLeadingZeros64(BitUtils.ReverseBits64(Mask));
                            }
                        }

                        if (Block.SizeInBlocksTruncated <= (ulong)Index || ZeroMask)
                        {
                            continue;
                        }

                        Block.FreeCount--;

                        int TempIdx = Index;

                        for (int Level = Block.MaxLevel - 1; Level >= 0; Level--, TempIdx /= 64)
                        {
                            Block.Masks[Level][TempIdx / 64] &= ~(1L << (TempIdx & 63));

                            if (Block.Masks[Level][TempIdx / 64] != 0)
                            {
                                break;
                            }
                        }

                        Address = Block.StartAligned + ((ulong)Index << Block.Order);
                    }

                    for (int CurrBlockIndex = BlockIndex;
                             CurrBlockIndex < BlockOrdersCount && Address == 0;
                             CurrBlockIndex++)
                    {
                        Block = Blocks[CurrBlockIndex];

                        int Index = 0;

                        bool ZeroMask = false;

                        for (int Level = 0; Level < Block.MaxLevel; Level++)
                        {
                            long Mask = Block.Masks[Level][Index];

                            if (Mask == 0)
                            {
                                ZeroMask = true;

                                break;
                            }

                            if (Backwards)
                            {
                                Index = Index * 64 + BitUtils.CountLeadingZeros64(BitUtils.ReverseBits64(Mask));
                            }
                            else
                            {
                                Index = (Index * 64 + 63) - BitUtils.CountLeadingZeros64(Mask);
                            }
                        }

                        if (Block.SizeInBlocksTruncated <= (ulong)Index || ZeroMask)
                        {
                            continue;
                        }

                        Block.FreeCount--;

                        int TempIdx = Index;

                        for (int Level = Block.MaxLevel - 1; Level >= 0; Level--, TempIdx /= 64)
                        {
                            Block.Masks[Level][TempIdx / 64] &= ~(1L << (TempIdx & 63));

                            if (Block.Masks[Level][TempIdx / 64] != 0)
                            {
                                break;
                            }
                        }

                        Address = Block.StartAligned + ((ulong)Index << Block.Order);
                    }

                    //The address being zero means that no free space was found on that order,
                    //just give up and try with the next one.
                    if (Address == 0)
                    {
                        break;
                    }

                    //If we are using a larger order than best fit, then we should
                    //split it into smaller blocks.
                    ulong FirstFreeBlockSize = 1UL << Block.Order;

                    if (FirstFreeBlockSize > BestFitBlockSize)
                    {
                        FreePages(Address + BestFitBlockSize, (FirstFreeBlockSize - BestFitBlockSize) / KMemoryManager.PageSize);
                    }

                    //Add new allocated page(s) to the pages list.
                    //If an error occurs, then free all allocated pages and fail.
                    KernelResult Result = PageList.AddRange(Address, BlockPagesCount);

                    if (Result != KernelResult.Success)
                    {
                        FreePages(Address, BlockPagesCount);

                        foreach (KPageNode PageNode in PageList)
                        {
                            FreePages(PageNode.Address, PageNode.PagesCount);
                        }

                        return Result;
                    }

                    PagesCount -= BlockPagesCount;
                }
            }

            //Success case, all requested pages were allocated successfully.
            if (PagesCount == 0)
            {
                return KernelResult.Success;
            }

            //Error case, free allocated pages and return out of memory.
            foreach (KPageNode PageNode in PageList)
            {
                FreePages(PageNode.Address, PageNode.PagesCount);
            }

            PageList = null;

            return KernelResult.OutOfMemory;
        }

        public void FreePages(KPageList PageList)
        {
            lock (Blocks)
            {
                foreach (KPageNode PageNode in PageList)
                {
                    FreePages(PageNode.Address, PageNode.PagesCount);
                }
            }
        }

        private void FreePages(ulong Address, ulong PagesCount)
        {
            ulong EndAddr = Address + PagesCount * KMemoryManager.PageSize;

            int BlockIndex = BlockOrdersCount - 1;

            ulong AddressRounded   = 0;
            ulong EndAddrTruncated = 0;

            for (; BlockIndex >= 0; BlockIndex--)
            {
                KMemoryRegionBlock AllocInfo = Blocks[BlockIndex];

                int BlockSize = 1 << AllocInfo.Order;

                AddressRounded   = BitUtils.AlignUp  (Address, BlockSize);
                EndAddrTruncated = BitUtils.AlignDown(EndAddr, BlockSize);

                if (AddressRounded < EndAddrTruncated)
                {
                    break;
                }
            }

            void FreeRegion(ulong CurrAddress)
            {
                for (int CurrBlockIndex = BlockIndex;
                         CurrBlockIndex < BlockOrdersCount && CurrAddress != 0;
                         CurrBlockIndex++)
                {
                    KMemoryRegionBlock Block = Blocks[CurrBlockIndex];

                    Block.FreeCount++;

                    ulong FreedBlocks = (CurrAddress - Block.StartAligned) >> Block.Order;

                    int Index = (int)FreedBlocks;

                    for (int Level = Block.MaxLevel - 1; Level >= 0; Level--, Index /= 64)
                    {
                        long Mask = Block.Masks[Level][Index / 64];

                        Block.Masks[Level][Index / 64] = Mask | (1L << (Index & 63));

                        if (Mask != 0)
                        {
                            break;
                        }
                    }

                    int BlockSizeDelta = 1 << (Block.NextOrder - Block.Order);

                    int FreedBlocksTruncated = BitUtils.AlignDown((int)FreedBlocks, BlockSizeDelta);

                    if (!Block.TryCoalesce(FreedBlocksTruncated, BlockSizeDelta))
                    {
                        break;
                    }

                    CurrAddress = Block.StartAligned + ((ulong)FreedBlocksTruncated << Block.Order);
                }
            }

            //Free inside aligned region.
            ulong BaseAddress = AddressRounded;

            while (BaseAddress < EndAddrTruncated)
            {
                ulong BlockSize = 1UL << Blocks[BlockIndex].Order;

                FreeRegion(BaseAddress);

                BaseAddress += BlockSize;
            }

            int NextBlockIndex = BlockIndex - 1;

            //Free region between Address and aligned region start.
            BaseAddress = AddressRounded;

            for (BlockIndex = NextBlockIndex; BlockIndex >= 0; BlockIndex--)
            {
                ulong BlockSize = 1UL << Blocks[BlockIndex].Order;

                while (BaseAddress - BlockSize >= Address)
                {
                    BaseAddress -= BlockSize;

                    FreeRegion(BaseAddress);
                }
            }

            //Free region between aligned region end and End Address.
            BaseAddress = EndAddrTruncated;

            for (BlockIndex = NextBlockIndex; BlockIndex >= 0; BlockIndex--)
            {
                ulong BlockSize = 1UL << Blocks[BlockIndex].Order;

                while (BaseAddress + BlockSize <= EndAddr)
                {
                    FreeRegion(BaseAddress);

                    BaseAddress += BlockSize;
                }
            }
        }

        public ulong GetFreePages()
        {
            lock (Blocks)
            {
                return GetFreePagesImpl();
            }
        }

        private ulong GetFreePagesImpl()
        {
            ulong AvailablePages = 0;

            for (int BlockIndex = 0; BlockIndex < BlockOrdersCount; BlockIndex++)
            {
                KMemoryRegionBlock Block = Blocks[BlockIndex];

                ulong BlockPagesCount = (1UL << Block.Order) / KMemoryManager.PageSize;

                AvailablePages += BlockPagesCount * Block.FreeCount;
            }

            return AvailablePages;
        }
    }
}