using Ryujinx.Common.Collections;
using Ryujinx.Horizon.Common;
using System.Diagnostics;

namespace Ryujinx.HLE.HOS.Kernel.Memory
{
    class KMemoryBlockManager
    {
        private const int PageSize = KPageTableBase.PageSize;

        private readonly IntrusiveRedBlackTree<KMemoryBlock> _blockTree;

        public int BlocksCount => _blockTree.Count;

        private KMemoryBlockSlabManager _slabManager;

        private ulong _addrSpaceStart;
        private ulong _addrSpaceEnd;

        public KMemoryBlockManager()
        {
            _blockTree = new IntrusiveRedBlackTree<KMemoryBlock>();
        }

        public Result Initialize(ulong addrSpaceStart, ulong addrSpaceEnd, KMemoryBlockSlabManager slabManager)
        {
            _slabManager = slabManager;
            _addrSpaceStart = addrSpaceStart;
            _addrSpaceEnd = addrSpaceEnd;

            // First insertion will always need only a single block, because there's nothing to split.
            if (!slabManager.CanAllocate(1))
            {
                return KernelResult.OutOfResource;
            }

            ulong addrSpacePagesCount = (addrSpaceEnd - addrSpaceStart) / PageSize;

            _blockTree.Add(new KMemoryBlock(
                addrSpaceStart,
                addrSpacePagesCount,
                MemoryState.Unmapped,
                KMemoryPermission.None,
                MemoryAttribute.None));

            return Result.Success;
        }

        public void InsertBlock(
            ulong baseAddress,
            ulong pagesCount,
            MemoryState oldState,
            KMemoryPermission oldPermission,
            MemoryAttribute oldAttribute,
            MemoryState newState,
            KMemoryPermission newPermission,
            MemoryAttribute newAttribute)
        {
            // Insert new block on the list only on areas where the state
            // of the block matches the state specified on the old* state
            // arguments, otherwise leave it as is.

            int oldCount = _blockTree.Count;

            oldAttribute |= MemoryAttribute.IpcAndDeviceMapped;

            ulong endAddr = baseAddress + pagesCount * PageSize;

            KMemoryBlock currBlock = FindBlock(baseAddress);

            while (currBlock != null)
            {
                ulong currBaseAddr = currBlock.BaseAddress;
                ulong currEndAddr = currBlock.PagesCount * PageSize + currBaseAddr;

                if (baseAddress < currEndAddr && currBaseAddr < endAddr)
                {
                    MemoryAttribute currBlockAttr = currBlock.Attribute | MemoryAttribute.IpcAndDeviceMapped;

                    if (currBlock.State != oldState ||
                        currBlock.Permission != oldPermission ||
                        currBlockAttr != oldAttribute)
                    {
                        currBlock = currBlock.Successor;

                        continue;
                    }

                    if (baseAddress > currBaseAddr)
                    {
                        KMemoryBlock newBlock = currBlock.SplitRightAtAddress(baseAddress);
                        _blockTree.Add(newBlock);
                    }

                    if (endAddr < currEndAddr)
                    {
                        KMemoryBlock newBlock = currBlock.SplitRightAtAddress(endAddr);
                        _blockTree.Add(newBlock);
                        currBlock = newBlock;
                    }

                    currBlock.SetState(newPermission, newState, newAttribute);

                    currBlock = MergeEqualStateNeighbors(currBlock);
                }

                if (currEndAddr - 1 >= endAddr - 1)
                {
                    break;
                }

                currBlock = currBlock.Successor;
            }

            _slabManager.Count += _blockTree.Count - oldCount;

            ValidateInternalState();
        }

        public void InsertBlock(
            ulong baseAddress,
            ulong pagesCount,
            MemoryState state,
            KMemoryPermission permission = KMemoryPermission.None,
            MemoryAttribute attribute = MemoryAttribute.None)
        {
            // Inserts new block at the list, replacing and splitting
            // existing blocks as needed.

            int oldCount = _blockTree.Count;

            ulong endAddr = baseAddress + pagesCount * PageSize;

            KMemoryBlock currBlock = FindBlock(baseAddress);

            while (currBlock != null)
            {
                ulong currBaseAddr = currBlock.BaseAddress;
                ulong currEndAddr = currBlock.PagesCount * PageSize + currBaseAddr;

                if (baseAddress < currEndAddr && currBaseAddr < endAddr)
                {
                    if (baseAddress > currBaseAddr)
                    {
                        KMemoryBlock newBlock = currBlock.SplitRightAtAddress(baseAddress);
                        _blockTree.Add(newBlock);
                    }

                    if (endAddr < currEndAddr)
                    {
                        KMemoryBlock newBlock = currBlock.SplitRightAtAddress(endAddr);
                        _blockTree.Add(newBlock);
                        currBlock = newBlock;
                    }

                    currBlock.SetState(permission, state, attribute);

                    currBlock = MergeEqualStateNeighbors(currBlock);
                }

                if (currEndAddr - 1 >= endAddr - 1)
                {
                    break;
                }

                currBlock = currBlock.Successor;
            }

            _slabManager.Count += _blockTree.Count - oldCount;

            ValidateInternalState();
        }

        public delegate void BlockMutator(KMemoryBlock block, KMemoryPermission newPerm);

        public void InsertBlock(
            ulong baseAddress,
            ulong pagesCount,
            BlockMutator blockMutate,
            KMemoryPermission permission = KMemoryPermission.None)
        {
            // Inserts new block at the list, replacing and splitting
            // existing blocks as needed, then calling the callback
            // function on the new block.

            int oldCount = _blockTree.Count;

            ulong endAddr = baseAddress + pagesCount * PageSize;

            KMemoryBlock currBlock = FindBlock(baseAddress);

            while (currBlock != null)
            {
                ulong currBaseAddr = currBlock.BaseAddress;
                ulong currEndAddr = currBlock.PagesCount * PageSize + currBaseAddr;

                if (baseAddress < currEndAddr && currBaseAddr < endAddr)
                {
                    if (baseAddress > currBaseAddr)
                    {
                        KMemoryBlock newBlock = currBlock.SplitRightAtAddress(baseAddress);
                        _blockTree.Add(newBlock);
                    }

                    if (endAddr < currEndAddr)
                    {
                        KMemoryBlock newBlock = currBlock.SplitRightAtAddress(endAddr);
                        _blockTree.Add(newBlock);
                        currBlock = newBlock;
                    }

                    blockMutate(currBlock, permission);

                    currBlock = MergeEqualStateNeighbors(currBlock);
                }

                if (currEndAddr - 1 >= endAddr - 1)
                {
                    break;
                }

                currBlock = currBlock.Successor;
            }

            _slabManager.Count += _blockTree.Count - oldCount;

            ValidateInternalState();
        }

        [Conditional("DEBUG")]
        private void ValidateInternalState()
        {
            ulong expectedAddress = 0;

            KMemoryBlock currBlock = FindBlock(_addrSpaceStart);

            while (currBlock != null)
            {
                Debug.Assert(currBlock.BaseAddress == expectedAddress);

                expectedAddress = currBlock.BaseAddress + currBlock.PagesCount * PageSize;

                currBlock = currBlock.Successor;
            }

            Debug.Assert(expectedAddress == _addrSpaceEnd);
        }

        private KMemoryBlock MergeEqualStateNeighbors(KMemoryBlock block)
        {
            KMemoryBlock previousBlock = block.Predecessor;
            KMemoryBlock nextBlock = block.Successor;

            if (previousBlock != null && BlockStateEquals(block, previousBlock))
            {
                _blockTree.Remove(block);

                previousBlock.AddPages(block.PagesCount);

                block = previousBlock;
            }

            if (nextBlock != null && BlockStateEquals(block, nextBlock))
            {
                _blockTree.Remove(nextBlock);

                block.AddPages(nextBlock.PagesCount);
            }

            return block;
        }

        private static bool BlockStateEquals(KMemoryBlock lhs, KMemoryBlock rhs)
        {
            return lhs.State == rhs.State &&
                   lhs.Permission == rhs.Permission &&
                   lhs.Attribute == rhs.Attribute &&
                   lhs.SourcePermission == rhs.SourcePermission &&
                   lhs.DeviceRefCount == rhs.DeviceRefCount &&
                   lhs.IpcRefCount == rhs.IpcRefCount;
        }

        public KMemoryBlock FindBlock(ulong address)
        {
            return _blockTree.GetNodeByKey(address);
        }
    }
}
