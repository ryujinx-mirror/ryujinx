using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Memory;
using Ryujinx.HLE.HOS.Kernel.Process;

namespace Ryujinx.HLE.HOS.Kernel.SupervisorCall
{
    partial class SvcHandler
    {
        public KernelResult SetHeapSize64(ulong size, out ulong position)
        {
            return SetHeapSize(size, out position);
        }

        private KernelResult SetHeapSize(ulong size, out ulong position)
        {
            if ((size & 0xfffffffe001fffff) != 0)
            {
                position = 0;

                return KernelResult.InvalidSize;
            }

            return _process.MemoryManager.SetHeapSize(size, out position);
        }

        public KernelResult SetMemoryAttribute64(
            ulong           position,
            ulong           size,
            MemoryAttribute attributeMask,
            MemoryAttribute attributeValue)
        {
            return SetMemoryAttribute(position, size, attributeMask, attributeValue);
        }

        private KernelResult SetMemoryAttribute(
            ulong           position,
            ulong           size,
            MemoryAttribute attributeMask,
            MemoryAttribute attributeValue)
        {
            if (!PageAligned(position))
            {
                return KernelResult.InvalidAddress;
            }

            if (!PageAligned(size) || size == 0)
            {
                return KernelResult.InvalidSize;
            }

            MemoryAttribute attributes = attributeMask | attributeValue;

            if (attributes != attributeMask ||
               (attributes | MemoryAttribute.Uncached) != MemoryAttribute.Uncached)
            {
                return KernelResult.InvalidCombination;
            }

            KernelResult result = _process.MemoryManager.SetMemoryAttribute(
                position,
                size,
                attributeMask,
                attributeValue);

            if (result == KernelResult.Success)
            {
                _memory.StopObservingRegion((long)position, (long)size);
            }

            return result;
        }

        public KernelResult MapMemory64(ulong dst, ulong src, ulong size)
        {
            return MapMemory(dst, src, size);
        }

        private KernelResult MapMemory(ulong dst, ulong src, ulong size)
        {
            if (!PageAligned(src | dst))
            {
                return KernelResult.InvalidAddress;
            }

            if (!PageAligned(size) || size == 0)
            {
                return KernelResult.InvalidSize;
            }

            if (src + size <= src || dst + size <= dst)
            {
                return KernelResult.InvalidMemState;
            }

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            if (!currentProcess.MemoryManager.InsideAddrSpace(src, size))
            {
                return KernelResult.InvalidMemState;
            }

            if (currentProcess.MemoryManager.OutsideStackRegion(dst, size) ||
                currentProcess.MemoryManager.InsideHeapRegion  (dst, size) ||
                currentProcess.MemoryManager.InsideAliasRegion (dst, size))
            {
                return KernelResult.InvalidMemRange;
            }

            return _process.MemoryManager.Map(dst, src, size);
        }

        public KernelResult UnmapMemory64(ulong dst, ulong src, ulong size)
        {
            return UnmapMemory(dst, src, size);
        }

        private KernelResult UnmapMemory(ulong dst, ulong src, ulong size)
        {
            if (!PageAligned(src | dst))
            {
                return KernelResult.InvalidAddress;
            }

            if (!PageAligned(size) || size == 0)
            {
                return KernelResult.InvalidSize;
            }

            if (src + size <= src || dst + size <= dst)
            {
                return KernelResult.InvalidMemState;
            }

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            if (!currentProcess.MemoryManager.InsideAddrSpace(src, size))
            {
                return KernelResult.InvalidMemState;
            }

            if (currentProcess.MemoryManager.OutsideStackRegion(dst, size) ||
                currentProcess.MemoryManager.InsideHeapRegion  (dst, size) ||
                currentProcess.MemoryManager.InsideAliasRegion (dst, size))
            {
                return KernelResult.InvalidMemRange;
            }

            return _process.MemoryManager.Unmap(dst, src, size);
        }

        public KernelResult QueryMemory64(ulong infoPtr, ulong x1, ulong position)
        {
            return QueryMemory(infoPtr, position);
        }

        private KernelResult QueryMemory(ulong infoPtr, ulong position)
        {
            KMemoryInfo blkInfo = _process.MemoryManager.QueryMemory(position);

            _memory.WriteUInt64((long)infoPtr + 0x00, blkInfo.Address);
            _memory.WriteUInt64((long)infoPtr + 0x08, blkInfo.Size);
            _memory.WriteInt32 ((long)infoPtr + 0x10, (int)blkInfo.State & 0xff);
            _memory.WriteInt32 ((long)infoPtr + 0x14, (int)blkInfo.Attribute);
            _memory.WriteInt32 ((long)infoPtr + 0x18, (int)blkInfo.Permission);
            _memory.WriteInt32 ((long)infoPtr + 0x1c, blkInfo.IpcRefCount);
            _memory.WriteInt32 ((long)infoPtr + 0x20, blkInfo.DeviceRefCount);
            _memory.WriteInt32 ((long)infoPtr + 0x24, 0);

            return KernelResult.Success;
        }

        public KernelResult MapSharedMemory64(int handle, ulong address, ulong size, MemoryPermission permission)
        {
            return MapSharedMemory(handle, address, size, permission);
        }

        private KernelResult MapSharedMemory(int handle, ulong address, ulong size, MemoryPermission permission)
        {
            if (!PageAligned(address))
            {
                return KernelResult.InvalidAddress;
            }

            if (!PageAligned(size) || size == 0)
            {
                return KernelResult.InvalidSize;
            }

            if (address + size <= address)
            {
                return KernelResult.InvalidMemState;
            }

            if ((permission | MemoryPermission.Write) != MemoryPermission.ReadAndWrite)
            {
                return KernelResult.InvalidPermission;
            }

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            KSharedMemory sharedMemory = currentProcess.HandleTable.GetObject<KSharedMemory>(handle);

            if (sharedMemory == null)
            {
                return KernelResult.InvalidHandle;
            }

            if (currentProcess.MemoryManager.IsInvalidRegion  (address, size) ||
                currentProcess.MemoryManager.InsideHeapRegion (address, size) ||
                currentProcess.MemoryManager.InsideAliasRegion(address, size))
            {
                return KernelResult.InvalidMemRange;
            }

            return sharedMemory.MapIntoProcess(
                currentProcess.MemoryManager,
                address,
                size,
                currentProcess,
                permission);
        }

        public KernelResult UnmapSharedMemory64(int handle, ulong address, ulong size)
        {
            return UnmapSharedMemory(handle, address, size);
        }

        private KernelResult UnmapSharedMemory(int handle, ulong address, ulong size)
        {
            if (!PageAligned(address))
            {
                return KernelResult.InvalidAddress;
            }

            if (!PageAligned(size) || size == 0)
            {
                return KernelResult.InvalidSize;
            }

            if (address + size <= address)
            {
                return KernelResult.InvalidMemState;
            }

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            KSharedMemory sharedMemory = currentProcess.HandleTable.GetObject<KSharedMemory>(handle);

            if (sharedMemory == null)
            {
                return KernelResult.InvalidHandle;
            }

            if (currentProcess.MemoryManager.IsInvalidRegion  (address, size) ||
                currentProcess.MemoryManager.InsideHeapRegion (address, size) ||
                currentProcess.MemoryManager.InsideAliasRegion(address, size))
            {
                return KernelResult.InvalidMemRange;
            }

            return sharedMemory.UnmapFromProcess(
                currentProcess.MemoryManager,
                address,
                size,
                currentProcess);
        }

        public KernelResult CreateTransferMemory64(
            ulong            address,
            ulong            size,
            MemoryPermission permission,
            out int          handle)
        {
            return CreateTransferMemory(address, size, permission, out handle);
        }

        private KernelResult CreateTransferMemory(ulong address, ulong size, MemoryPermission permission, out int handle)
        {
            handle = 0;

            if (!PageAligned(address))
            {
                return KernelResult.InvalidAddress;
            }

            if (!PageAligned(size) || size == 0)
            {
                return KernelResult.InvalidSize;
            }

            if (address + size <= address)
            {
                return KernelResult.InvalidMemState;
            }

            if (permission > MemoryPermission.ReadAndWrite || permission == MemoryPermission.Write)
            {
                return KernelResult.InvalidPermission;
            }

            KernelResult result = _process.MemoryManager.ReserveTransferMemory(address, size, permission);

            if (result != KernelResult.Success)
            {
                return result;
            }

            KTransferMemory transferMemory = new KTransferMemory(_system, address, size);

            return _process.HandleTable.GenerateHandle(transferMemory, out handle);
        }

        public KernelResult MapPhysicalMemory64(ulong address, ulong size)
        {
            return MapPhysicalMemory(address, size);
        }

        private KernelResult MapPhysicalMemory(ulong address, ulong size)
        {
            if (!PageAligned(address))
            {
                return KernelResult.InvalidAddress;
            }

            if (!PageAligned(size) || size == 0)
            {
                return KernelResult.InvalidSize;
            }

            if (address + size <= address)
            {
                return KernelResult.InvalidMemRange;
            }

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            if ((currentProcess.PersonalMmHeapPagesCount & 0xfffffffffffff) == 0)
            {
                return KernelResult.InvalidState;
            }

            if (!currentProcess.MemoryManager.InsideAddrSpace   (address, size) ||
                 currentProcess.MemoryManager.OutsideAliasRegion(address, size))
            {
                return KernelResult.InvalidMemRange;
            }

            return _process.MemoryManager.MapPhysicalMemory(address, size);
        }

        public KernelResult UnmapPhysicalMemory64(ulong address, ulong size)
        {
            return UnmapPhysicalMemory(address, size);
        }

        private KernelResult UnmapPhysicalMemory(ulong address, ulong size)
        {
            if (!PageAligned(address))
            {
                return KernelResult.InvalidAddress;
            }

            if (!PageAligned(size) || size == 0)
            {
                return KernelResult.InvalidSize;
            }

            if (address + size <= address)
            {
                return KernelResult.InvalidMemRange;
            }

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            if ((currentProcess.PersonalMmHeapPagesCount & 0xfffffffffffff) == 0)
            {
                return KernelResult.InvalidState;
            }

            if (!currentProcess.MemoryManager.InsideAddrSpace   (address, size) ||
                 currentProcess.MemoryManager.OutsideAliasRegion(address, size))
            {
                return KernelResult.InvalidMemRange;
            }

            return _process.MemoryManager.UnmapPhysicalMemory(address, size);
        }

        private static bool PageAligned(ulong position)
        {
            return (position & (KMemoryManager.PageSize - 1)) == 0;
        }
    }
}