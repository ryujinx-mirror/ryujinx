using Ryujinx.Common;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Memory;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Kernel.Ipc
{
    class KBufferDescriptorTable
    {
        private const int MaxInternalBuffersCount = 8;

        private List<KBufferDescriptor> _sendBufferDescriptors;
        private List<KBufferDescriptor> _receiveBufferDescriptors;
        private List<KBufferDescriptor> _exchangeBufferDescriptors;

        public KBufferDescriptorTable()
        {
            _sendBufferDescriptors     = new List<KBufferDescriptor>(MaxInternalBuffersCount);
            _receiveBufferDescriptors  = new List<KBufferDescriptor>(MaxInternalBuffersCount);
            _exchangeBufferDescriptors = new List<KBufferDescriptor>(MaxInternalBuffersCount);
        }

        public KernelResult AddSendBuffer(ulong src, ulong dst, ulong size, MemoryState state)
        {
            return Add(_sendBufferDescriptors, src, dst, size, state);
        }

        public KernelResult AddReceiveBuffer(ulong src, ulong dst, ulong size, MemoryState state)
        {
            return Add(_receiveBufferDescriptors, src, dst, size, state);
        }

        public KernelResult AddExchangeBuffer(ulong src, ulong dst, ulong size, MemoryState state)
        {
            return Add(_exchangeBufferDescriptors, src, dst, size, state);
        }

        private KernelResult Add(List<KBufferDescriptor> list, ulong src, ulong dst, ulong size, MemoryState state)
        {
            if (list.Count < MaxInternalBuffersCount)
            {
                list.Add(new KBufferDescriptor(src, dst, size, state));

                return KernelResult.Success;
            }

            return KernelResult.OutOfMemory;
        }

        public KernelResult CopyBuffersToClient(KMemoryManager memoryManager)
        {
            KernelResult result = CopyToClient(memoryManager, _receiveBufferDescriptors);

            if (result != KernelResult.Success)
            {
                return result;
            }

            return CopyToClient(memoryManager, _exchangeBufferDescriptors);
        }

        private KernelResult CopyToClient(KMemoryManager memoryManager, List<KBufferDescriptor> list)
        {
            foreach (KBufferDescriptor desc in list)
            {
                MemoryState stateMask;

                switch (desc.State)
                {
                    case MemoryState.IpcBuffer0: stateMask = MemoryState.IpcSendAllowedType0; break;
                    case MemoryState.IpcBuffer1: stateMask = MemoryState.IpcSendAllowedType1; break;
                    case MemoryState.IpcBuffer3: stateMask = MemoryState.IpcSendAllowedType3; break;

                    default: return KernelResult.InvalidCombination;
                }

                MemoryAttribute attributeMask = MemoryAttribute.Borrowed | MemoryAttribute.Uncached;

                if (desc.State == MemoryState.IpcBuffer0)
                {
                    attributeMask |= MemoryAttribute.DeviceMapped;
                }

                ulong clientAddrTruncated = BitUtils.AlignDown(desc.ClientAddress, KMemoryManager.PageSize);
                ulong clientAddrRounded   = BitUtils.AlignUp  (desc.ClientAddress, KMemoryManager.PageSize);

                // Check if address is not aligned, in this case we need to perform 2 copies.
                if (clientAddrTruncated != clientAddrRounded)
                {
                    ulong copySize = clientAddrRounded - desc.ClientAddress;

                    if (copySize > desc.Size)
                    {
                        copySize = desc.Size;
                    }

                    KernelResult result = memoryManager.CopyDataFromCurrentProcess(
                        desc.ClientAddress,
                        copySize,
                        stateMask,
                        stateMask,
                        KMemoryPermission.ReadAndWrite,
                        attributeMask,
                        MemoryAttribute.None,
                        desc.ServerAddress);

                    if (result != KernelResult.Success)
                    {
                        return result;
                    }
                }

                ulong clientEndAddr = desc.ClientAddress + desc.Size;
                ulong serverEndAddr = desc.ServerAddress + desc.Size;

                ulong clientEndAddrTruncated = BitUtils.AlignDown(clientEndAddr, KMemoryManager.PageSize);
                ulong clientEndAddrRounded   = BitUtils.AlignUp  (clientEndAddr, KMemoryManager.PageSize);
                ulong serverEndAddrTruncated = BitUtils.AlignDown(serverEndAddr, KMemoryManager.PageSize);

                if (clientEndAddrTruncated < clientEndAddrRounded &&
                    (clientAddrTruncated == clientAddrRounded || clientAddrTruncated < clientEndAddrTruncated))
                {
                    KernelResult result = memoryManager.CopyDataFromCurrentProcess(
                        clientEndAddrTruncated,
                        clientEndAddr - clientEndAddrTruncated,
                        stateMask,
                        stateMask,
                        KMemoryPermission.ReadAndWrite,
                        attributeMask,
                        MemoryAttribute.None,
                        serverEndAddrTruncated);

                    if (result != KernelResult.Success)
                    {
                        return result;
                    }
                }
            }

            return KernelResult.Success;
        }

        public KernelResult UnmapServerBuffers(KMemoryManager memoryManager)
        {
            KernelResult result = UnmapServer(memoryManager, _sendBufferDescriptors);

            if (result != KernelResult.Success)
            {
                return result;
            }

            result = UnmapServer(memoryManager, _receiveBufferDescriptors);

            if (result != KernelResult.Success)
            {
                return result;
            }

            return UnmapServer(memoryManager, _exchangeBufferDescriptors);
        }

        private KernelResult UnmapServer(KMemoryManager memoryManager, List<KBufferDescriptor> list)
        {
            foreach (KBufferDescriptor descriptor in list)
            {
                KernelResult result = memoryManager.UnmapNoAttributeIfStateEquals(
                    descriptor.ServerAddress,
                    descriptor.Size,
                    descriptor.State);

                if (result != KernelResult.Success)
                {
                    return result;
                }
            }

            return KernelResult.Success;
        }

        public KernelResult RestoreClientBuffers(KMemoryManager memoryManager)
        {
            KernelResult result = RestoreClient(memoryManager, _sendBufferDescriptors);

            if (result != KernelResult.Success)
            {
                return result;
            }

            result = RestoreClient(memoryManager, _receiveBufferDescriptors);

            if (result != KernelResult.Success)
            {
                return result;
            }

            return RestoreClient(memoryManager, _exchangeBufferDescriptors);
        }

        private KernelResult RestoreClient(KMemoryManager memoryManager, List<KBufferDescriptor> list)
        {
            foreach (KBufferDescriptor descriptor in list)
            {
                KernelResult result = memoryManager.UnmapIpcRestorePermission(
                    descriptor.ClientAddress,
                    descriptor.Size,
                    descriptor.State);

                if (result != KernelResult.Success)
                {
                    return result;
                }
            }

            return KernelResult.Success;
        }
    }
}