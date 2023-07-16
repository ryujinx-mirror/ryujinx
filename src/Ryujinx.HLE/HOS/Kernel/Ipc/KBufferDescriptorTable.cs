using Ryujinx.Common;
using Ryujinx.HLE.HOS.Kernel.Memory;
using Ryujinx.Horizon.Common;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Kernel.Ipc
{
    class KBufferDescriptorTable
    {
        private const int MaxInternalBuffersCount = 8;

        private readonly List<KBufferDescriptor> _sendBufferDescriptors;
        private readonly List<KBufferDescriptor> _receiveBufferDescriptors;
        private readonly List<KBufferDescriptor> _exchangeBufferDescriptors;

        public KBufferDescriptorTable()
        {
            _sendBufferDescriptors = new List<KBufferDescriptor>(MaxInternalBuffersCount);
            _receiveBufferDescriptors = new List<KBufferDescriptor>(MaxInternalBuffersCount);
            _exchangeBufferDescriptors = new List<KBufferDescriptor>(MaxInternalBuffersCount);
        }

        public Result AddSendBuffer(ulong src, ulong dst, ulong size, MemoryState state)
        {
            return Add(_sendBufferDescriptors, src, dst, size, state);
        }

        public Result AddReceiveBuffer(ulong src, ulong dst, ulong size, MemoryState state)
        {
            return Add(_receiveBufferDescriptors, src, dst, size, state);
        }

        public Result AddExchangeBuffer(ulong src, ulong dst, ulong size, MemoryState state)
        {
            return Add(_exchangeBufferDescriptors, src, dst, size, state);
        }

        private static Result Add(List<KBufferDescriptor> list, ulong src, ulong dst, ulong size, MemoryState state)
        {
            if (list.Count < MaxInternalBuffersCount)
            {
                list.Add(new KBufferDescriptor(src, dst, size, state));

                return Result.Success;
            }

            return KernelResult.OutOfMemory;
        }

        public Result CopyBuffersToClient(KPageTableBase memoryManager)
        {
            Result result = CopyToClient(memoryManager, _receiveBufferDescriptors);

            if (result != Result.Success)
            {
                return result;
            }

            return CopyToClient(memoryManager, _exchangeBufferDescriptors);
        }

        private static Result CopyToClient(KPageTableBase memoryManager, List<KBufferDescriptor> list)
        {
            foreach (KBufferDescriptor desc in list)
            {
                MemoryState stateMask;

                switch (desc.State)
                {
                    case MemoryState.IpcBuffer0:
                        stateMask = MemoryState.IpcSendAllowedType0;
                        break;
                    case MemoryState.IpcBuffer1:
                        stateMask = MemoryState.IpcSendAllowedType1;
                        break;
                    case MemoryState.IpcBuffer3:
                        stateMask = MemoryState.IpcSendAllowedType3;
                        break;
                    default:
                        return KernelResult.InvalidCombination;
                }

                MemoryAttribute attributeMask = MemoryAttribute.Borrowed | MemoryAttribute.Uncached;

                if (desc.State == MemoryState.IpcBuffer0)
                {
                    attributeMask |= MemoryAttribute.DeviceMapped;
                }

                ulong clientAddrTruncated = BitUtils.AlignDown<ulong>(desc.ClientAddress, KPageTableBase.PageSize);
                ulong clientAddrRounded = BitUtils.AlignUp<ulong>(desc.ClientAddress, KPageTableBase.PageSize);

                // Check if address is not aligned, in this case we need to perform 2 copies.
                if (clientAddrTruncated != clientAddrRounded)
                {
                    ulong copySize = clientAddrRounded - desc.ClientAddress;

                    if (copySize > desc.Size)
                    {
                        copySize = desc.Size;
                    }

                    Result result = memoryManager.CopyDataFromCurrentProcess(
                        desc.ClientAddress,
                        copySize,
                        stateMask,
                        stateMask,
                        KMemoryPermission.ReadAndWrite,
                        attributeMask,
                        MemoryAttribute.None,
                        desc.ServerAddress);

                    if (result != Result.Success)
                    {
                        return result;
                    }
                }

                ulong clientEndAddr = desc.ClientAddress + desc.Size;
                ulong serverEndAddr = desc.ServerAddress + desc.Size;

                ulong clientEndAddrTruncated = BitUtils.AlignDown<ulong>(clientEndAddr, (ulong)KPageTableBase.PageSize);
                ulong clientEndAddrRounded = BitUtils.AlignUp<ulong>(clientEndAddr, KPageTableBase.PageSize);
                ulong serverEndAddrTruncated = BitUtils.AlignDown<ulong>(serverEndAddr, (ulong)KPageTableBase.PageSize);

                if (clientEndAddrTruncated < clientEndAddrRounded &&
                    (clientAddrTruncated == clientAddrRounded || clientAddrTruncated < clientEndAddrTruncated))
                {
                    Result result = memoryManager.CopyDataFromCurrentProcess(
                        clientEndAddrTruncated,
                        clientEndAddr - clientEndAddrTruncated,
                        stateMask,
                        stateMask,
                        KMemoryPermission.ReadAndWrite,
                        attributeMask,
                        MemoryAttribute.None,
                        serverEndAddrTruncated);

                    if (result != Result.Success)
                    {
                        return result;
                    }
                }
            }

            return Result.Success;
        }

        public Result UnmapServerBuffers(KPageTableBase memoryManager)
        {
            Result result = UnmapServer(memoryManager, _sendBufferDescriptors);

            if (result != Result.Success)
            {
                return result;
            }

            result = UnmapServer(memoryManager, _receiveBufferDescriptors);

            if (result != Result.Success)
            {
                return result;
            }

            return UnmapServer(memoryManager, _exchangeBufferDescriptors);
        }

        private static Result UnmapServer(KPageTableBase memoryManager, List<KBufferDescriptor> list)
        {
            foreach (KBufferDescriptor descriptor in list)
            {
                Result result = memoryManager.UnmapNoAttributeIfStateEquals(
                    descriptor.ServerAddress,
                    descriptor.Size,
                    descriptor.State);

                if (result != Result.Success)
                {
                    return result;
                }
            }

            return Result.Success;
        }

        public Result RestoreClientBuffers(KPageTableBase memoryManager)
        {
            Result result = RestoreClient(memoryManager, _sendBufferDescriptors);

            if (result != Result.Success)
            {
                return result;
            }

            result = RestoreClient(memoryManager, _receiveBufferDescriptors);

            if (result != Result.Success)
            {
                return result;
            }

            return RestoreClient(memoryManager, _exchangeBufferDescriptors);
        }

        private static Result RestoreClient(KPageTableBase memoryManager, List<KBufferDescriptor> list)
        {
            foreach (KBufferDescriptor descriptor in list)
            {
                Result result = memoryManager.UnmapIpcRestorePermission(
                    descriptor.ClientAddress,
                    descriptor.Size,
                    descriptor.State);

                if (result != Result.Success)
                {
                    return result;
                }
            }

            return Result.Success;
        }
    }
}
