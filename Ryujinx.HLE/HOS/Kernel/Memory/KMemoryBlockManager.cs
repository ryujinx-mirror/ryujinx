using Ryujinx.HLE.HOS.Kernel.Common;
using System.Collections.Generic;
using System.Diagnostics;

namespace Ryujinx.HLE.HOS.Kernel.Memory
{
    class KMemoryBlockManager
    {
        private const int PageSize = KPageTableBase.PageSize;

        private readonly LinkedList<KMemoryBlock> _blocks;

        public int BlocksCount => _blocks.Count;

        private KMemoryBlockSlabManager _slabManager;

        private ulong _addrSpaceEnd;

        public KMemoryBlockManager()
        {
            _blocks = new LinkedList<KMemoryBlock>();
        }

        public KernelResult Initialize(ulong addrSpaceStart, ulong addrSpaceEnd, KMemoryBlockSlabManager slabManager)
        {
            _slabManager = slabManager;
            _addrSpaceEnd = addrSpaceEnd;

            // First insertion will always need only a single block,
            // because there's nothing else to split.
            if (!slabManager.CanAllocate(1))
            {
                return KernelResult.OutOfResource;
            }

            ulong addrSpacePagesCount = (addrSpaceEnd - addrSpaceStart) / PageSize;

            _blocks.AddFirst(new KMemoryBlock(
                addrSpaceStart,
                addrSpacePagesCount,
                MemoryState.Unmapped,
                KMemoryPermission.None,
                MemoryAttribute.None));

            return KernelResult.Success;
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
            int oldCount = _blocks.Count;

            oldAttribute |= MemoryAttribute.IpcAndDeviceMapped;

            ulong endAddr = baseAddress + pagesCount * PageSize;

            LinkedListNode<KMemoryBlock> node = _blocks.First;

            while (node != null)
            {
                LinkedListNode<KMemoryBlock> newNode = node;

                KMemoryBlock currBlock = node.Value;

                ulong currBaseAddr = currBlock.BaseAddress;
                ulong currEndAddr = currBlock.PagesCount * PageSize + currBaseAddr;

                if (baseAddress < currEndAddr && currBaseAddr < endAddr)
                {
                    MemoryAttribute currBlockAttr = currBlock.Attribute | MemoryAttribute.IpcAndDeviceMapped;

                    if (currBlock.State != oldState ||
                        currBlock.Permission != oldPermission ||
                        currBlockAttr != oldAttribute)
                    {
                        node = node.Next;

                        continue;
                    }

                    if (baseAddress > currBaseAddr)
                    {
                        _blocks.AddBefore(node, currBlock.SplitRightAtAddress(baseAddress));
                    }

                    if (endAddr < currEndAddr)
                    {
                        newNode = _blocks.AddBefore(node, currBlock.SplitRightAtAddress(endAddr));
                    }

                    newNode.Value.SetState(newPermission, newState, newAttribute);

                    newNode = MergeEqualStateNeighbors(newNode);
                }

                if (currEndAddr - 1 >= endAddr - 1)
                {
                    break;
                }

                node = newNode.Next;
            }

            _slabManager.Count += _blocks.Count - oldCount;

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
            int oldCount = _blocks.Count;

            ulong endAddr = baseAddress + pagesCount * PageSize;

            LinkedListNode<KMemoryBlock> node = _blocks.First;

            while (node != null)
            {
                LinkedListNode<KMemoryBlock> newNode = node;

                KMemoryBlock currBlock = node.Value;

                ulong currBaseAddr = currBlock.BaseAddress;
                ulong currEndAddr = currBlock.PagesCount * PageSize + currBaseAddr;

                if (baseAddress < currEndAddr && currBaseAddr < endAddr)
                {
                    if (baseAddress > currBaseAddr)
                    {
                        _blocks.AddBefore(node, currBlock.SplitRightAtAddress(baseAddress));
                    }

                    if (endAddr < currEndAddr)
                    {
                        newNode = _blocks.AddBefore(node, currBlock.SplitRightAtAddress(endAddr));
                    }

                    newNode.Value.SetState(permission, state, attribute);

                    newNode = MergeEqualStateNeighbors(newNode);
                }

                if (currEndAddr - 1 >= endAddr - 1)
                {
                    break;
                }

                node = newNode.Next;
            }

            _slabManager.Count += _blocks.Count - oldCount;

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
            int oldCount = _blocks.Count;

            ulong endAddr = baseAddress + pagesCount * PageSize;

            LinkedListNode<KMemoryBlock> node = _blocks.First;

            while (node != null)
            {
                LinkedListNode<KMemoryBlock> newNode = node;

                KMemoryBlock currBlock = node.Value;

                ulong currBaseAddr = currBlock.BaseAddress;
                ulong currEndAddr = currBlock.PagesCount * PageSize + currBaseAddr;

                if (baseAddress < currEndAddr && currBaseAddr < endAddr)
                {
                    if (baseAddress > currBaseAddr)
                    {
                        _blocks.AddBefore(node, currBlock.SplitRightAtAddress(baseAddress));
                    }

                    if (endAddr < currEndAddr)
                    {
                        newNode = _blocks.AddBefore(node, currBlock.SplitRightAtAddress(endAddr));
                    }

                    KMemoryBlock newBlock = newNode.Value;

                    blockMutate(newBlock, permission);

                    newNode = MergeEqualStateNeighbors(newNode);
                }

                if (currEndAddr - 1 >= endAddr - 1)
                {
                    break;
                }

                node = newNode.Next;
            }

            _slabManager.Count += _blocks.Count - oldCount;

            ValidateInternalState();
        }

        [Conditional("DEBUG")]
        private void ValidateInternalState()
        {
            ulong expectedAddress = 0;

            LinkedListNode<KMemoryBlock> node = _blocks.First;

            while (node != null)
            {
                LinkedListNode<KMemoryBlock> newNode = node;

                KMemoryBlock currBlock = node.Value;

                Debug.Assert(currBlock.BaseAddress == expectedAddress);

                expectedAddress = currBlock.BaseAddress + currBlock.PagesCount * PageSize;

                node = newNode.Next;
            }

            Debug.Assert(expectedAddress == _addrSpaceEnd);
        }

        private LinkedListNode<KMemoryBlock> MergeEqualStateNeighbors(LinkedListNode<KMemoryBlock> node)
        {
            KMemoryBlock block = node.Value;

            if (node.Previous != null)
            {
                KMemoryBlock previousBlock = node.Previous.Value;

                if (BlockStateEquals(block, previousBlock))
                {
                    LinkedListNode<KMemoryBlock> previousNode = node.Previous;

                    _blocks.Remove(node);

                    previousBlock.AddPages(block.PagesCount);

                    node = previousNode;
                    block = previousBlock;
                }
            }

            if (node.Next != null)
            {
                KMemoryBlock nextBlock = node.Next.Value;

                if (BlockStateEquals(block, nextBlock))
                {
                    _blocks.Remove(node.Next);

                    block.AddPages(nextBlock.PagesCount);
                }
            }

            return node;
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
            return FindBlockNode(address)?.Value;
        }

        public LinkedListNode<KMemoryBlock> FindBlockNode(ulong address)
        {
            lock (_blocks)
            {
                LinkedListNode<KMemoryBlock> node = _blocks.First;

                while (node != null)
                {
                    KMemoryBlock block = node.Value;

                    ulong currEndAddr = block.PagesCount * PageSize + block.BaseAddress;

                    if (block.BaseAddress <= address && currEndAddr - 1 >= address)
                    {
                        return node;
                    }

                    node = node.Next;
                }
            }

            return null;
        }
    }
}
