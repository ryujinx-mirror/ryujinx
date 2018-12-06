using ChocolArm64.State;
using Ryujinx.Common.Logging;

using static Ryujinx.HLE.HOS.ErrorCode;

namespace Ryujinx.HLE.HOS.Kernel
{
    partial class SvcHandler
    {
        private void SvcSetHeapSize(CpuThreadState threadState)
        {
            ulong size = threadState.X1;

            if ((size & 0xfffffffe001fffff) != 0)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Heap size 0x{size:x16} is not aligned!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidSize);

                return;
            }

            KernelResult result = _process.MemoryManager.SetHeapSize(size, out ulong position);

            threadState.X0 = (ulong)result;

            if (result == KernelResult.Success)
            {
                threadState.X1 = position;
            }
            else
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Operation failed with error \"{result}\".");
            }
        }

        private void SvcSetMemoryAttribute(CpuThreadState threadState)
        {
            ulong position = threadState.X0;
            ulong size     = threadState.X1;

            if (!PageAligned(position))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Address 0x{position:x16} is not page aligned!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAddress);

                return;
            }

            if (!PageAligned(size) || size == 0)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Size 0x{size:x16} is not page aligned or is zero!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidSize);

                return;
            }

            MemoryAttribute attributeMask  = (MemoryAttribute)threadState.X2;
            MemoryAttribute attributeValue = (MemoryAttribute)threadState.X3;

            MemoryAttribute attributes = attributeMask | attributeValue;

            if (attributes != attributeMask ||
               (attributes | MemoryAttribute.Uncached) != MemoryAttribute.Uncached)
            {
                Logger.PrintWarning(LogClass.KernelSvc, "Invalid memory attributes!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidMaskValue);

                return;
            }

            KernelResult result = _process.MemoryManager.SetMemoryAttribute(
                position,
                size,
                attributeMask,
                attributeValue);

            if (result != KernelResult.Success)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Operation failed with error \"{result}\".");
            }
            else
            {
                _memory.StopObservingRegion((long)position, (long)size);
            }

            threadState.X0 = (ulong)result;
        }

        private void SvcMapMemory(CpuThreadState threadState)
        {
            ulong dst  = threadState.X0;
            ulong src  = threadState.X1;
            ulong size = threadState.X2;

            if (!PageAligned(src | dst))
            {
                Logger.PrintWarning(LogClass.KernelSvc, "Addresses are not page aligned!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAddress);

                return;
            }

            if (!PageAligned(size) || size == 0)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Size 0x{size:x16} is not page aligned or is zero!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidSize);

                return;
            }

            if (src + size <= src || dst + size <= dst)
            {
                Logger.PrintWarning(LogClass.KernelSvc, "Addresses outside of range!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            if (!currentProcess.MemoryManager.InsideAddrSpace(src, size))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Src address 0x{src:x16} out of range!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            if (currentProcess.MemoryManager.OutsideStackRegion(dst, size) ||
                currentProcess.MemoryManager.InsideHeapRegion  (dst, size) ||
                currentProcess.MemoryManager.InsideAliasRegion (dst, size))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Dst address 0x{dst:x16} out of range!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidMemRange);

                return;
            }

            KernelResult result = _process.MemoryManager.Map(dst, src, size);

            if (result != KernelResult.Success)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Operation failed with error 0x{result:x}!");
            }

            threadState.X0 = (ulong)result;
        }

        private void SvcUnmapMemory(CpuThreadState threadState)
        {
            ulong dst  = threadState.X0;
            ulong src  = threadState.X1;
            ulong size = threadState.X2;

            if (!PageAligned(src | dst))
            {
                Logger.PrintWarning(LogClass.KernelSvc, "Addresses are not page aligned!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAddress);

                return;
            }

            if (!PageAligned(size) || size == 0)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Size 0x{size:x16} is not page aligned or is zero!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidSize);

                return;
            }

            if (src + size <= src || dst + size <= dst)
            {
                Logger.PrintWarning(LogClass.KernelSvc, "Addresses outside of range!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            if (!currentProcess.MemoryManager.InsideAddrSpace(src, size))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Src address 0x{src:x16} out of range!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            if (currentProcess.MemoryManager.OutsideStackRegion(dst, size) ||
                currentProcess.MemoryManager.InsideHeapRegion  (dst, size) ||
                currentProcess.MemoryManager.InsideAliasRegion (dst, size))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Dst address 0x{dst:x16} out of range!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidMemRange);

                return;
            }

            KernelResult result = _process.MemoryManager.Unmap(dst, src, size);

            if (result != KernelResult.Success)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Operation failed with error 0x{result:x}!");
            }

            threadState.X0 = (ulong)result;
        }

        private void SvcQueryMemory(CpuThreadState threadState)
        {
            long  infoPtr  = (long)threadState.X0;
            ulong position =       threadState.X2;

            KMemoryInfo blkInfo = _process.MemoryManager.QueryMemory(position);

            _memory.WriteUInt64(infoPtr + 0x00, blkInfo.Address);
            _memory.WriteUInt64(infoPtr + 0x08, blkInfo.Size);
            _memory.WriteInt32 (infoPtr + 0x10, (int)blkInfo.State & 0xff);
            _memory.WriteInt32 (infoPtr + 0x14, (int)blkInfo.Attribute);
            _memory.WriteInt32 (infoPtr + 0x18, (int)blkInfo.Permission);
            _memory.WriteInt32 (infoPtr + 0x1c, blkInfo.IpcRefCount);
            _memory.WriteInt32 (infoPtr + 0x20, blkInfo.DeviceRefCount);
            _memory.WriteInt32 (infoPtr + 0x24, 0);

            threadState.X0 = 0;
            threadState.X1 = 0;
        }

        private void SvcMapSharedMemory(CpuThreadState threadState)
        {
            int   handle  =  (int)threadState.X0;
            ulong address =       threadState.X1;
            ulong size    =       threadState.X2;

            if (!PageAligned(address))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Address 0x{address:x16} is not page aligned!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAddress);

                return;
            }

            if (!PageAligned(size) || size == 0)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Size 0x{size:x16} is not page aligned or is zero!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidSize);

                return;
            }

            if (address + size <= address)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid region address 0x{address:x16} / size 0x{size:x16}!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            MemoryPermission permission = (MemoryPermission)threadState.X3;

            if ((permission | MemoryPermission.Write) != MemoryPermission.ReadAndWrite)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid permission {permission}!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidPermission);

                return;
            }

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            KSharedMemory sharedMemory = currentProcess.HandleTable.GetObject<KSharedMemory>(handle);

            if (sharedMemory == null)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid shared memory handle 0x{handle:x8}!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                return;
            }

            if (currentProcess.MemoryManager.IsInvalidRegion  (address, size) ||
                currentProcess.MemoryManager.InsideHeapRegion (address, size) ||
                currentProcess.MemoryManager.InsideAliasRegion(address, size))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Address 0x{address:x16} out of range!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            KernelResult result = sharedMemory.MapIntoProcess(
                currentProcess.MemoryManager,
                address,
                size,
                currentProcess,
                permission);

            if (result != KernelResult.Success)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Operation failed with error \"{result}\".");
            }

            threadState.X0 = (ulong)result;
        }

        private void SvcUnmapSharedMemory(CpuThreadState threadState)
        {
            int   handle  =  (int)threadState.X0;
            ulong address =       threadState.X1;
            ulong size    =       threadState.X2;

            if (!PageAligned(address))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Address 0x{address:x16} is not page aligned!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAddress);

                return;
            }

            if (!PageAligned(size) || size == 0)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Size 0x{size:x16} is not page aligned or is zero!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidSize);

                return;
            }

            if (address + size <= address)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid region address 0x{address:x16} / size 0x{size:x16}!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            KSharedMemory sharedMemory = currentProcess.HandleTable.GetObject<KSharedMemory>(handle);

            if (sharedMemory == null)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid shared memory handle 0x{handle:x8}!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                return;
            }

            if (currentProcess.MemoryManager.IsInvalidRegion  (address, size) ||
                currentProcess.MemoryManager.InsideHeapRegion (address, size) ||
                currentProcess.MemoryManager.InsideAliasRegion(address, size))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Address 0x{address:x16} out of range!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            KernelResult result = sharedMemory.UnmapFromProcess(
                currentProcess.MemoryManager,
                address,
                size,
                currentProcess);

            if (result != KernelResult.Success)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Operation failed with error \"{result}\".");
            }

            threadState.X0 = (ulong)result;
        }

        private void SvcCreateTransferMemory(CpuThreadState threadState)
        {
            ulong address = threadState.X1;
            ulong size    = threadState.X2;

            if (!PageAligned(address))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Address 0x{address:x16} is not page aligned!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAddress);

                return;
            }

            if (!PageAligned(size) || size == 0)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Size 0x{size:x16} is not page aligned or is zero!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAddress);

                return;
            }

            if (address + size <= address)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid region address 0x{address:x16} / size 0x{size:x16}!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            MemoryPermission permission = (MemoryPermission)threadState.X3;

            if (permission > MemoryPermission.ReadAndWrite || permission == MemoryPermission.Write)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid permission {permission}!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidPermission);

                return;
            }

            _process.MemoryManager.ReserveTransferMemory(address, size, permission);

            KTransferMemory transferMemory = new KTransferMemory(address, size);

            KernelResult result = _process.HandleTable.GenerateHandle(transferMemory, out int handle);

            threadState.X0 = (uint)result;
            threadState.X1 = (ulong)handle;
        }

        private void SvcMapPhysicalMemory(CpuThreadState threadState)
        {
            ulong address = threadState.X0;
            ulong size    = threadState.X1;

            if (!PageAligned(address))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Address 0x{address:x16} is not page aligned!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAddress);

                return;
            }

            if (!PageAligned(size) || size == 0)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Size 0x{size:x16} is not page aligned or is zero!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidSize);

                return;
            }

            if (address + size <= address)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid region address 0x{address:x16} / size 0x{size:x16}!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            if ((currentProcess.PersonalMmHeapPagesCount & 0xfffffffffffff) == 0)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"System resource size is zero.");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidState);

                return;
            }

            if (!currentProcess.MemoryManager.InsideAddrSpace   (address, size) ||
                 currentProcess.MemoryManager.OutsideAliasRegion(address, size))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid address {address:x16}.");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            KernelResult result = _process.MemoryManager.MapPhysicalMemory(address, size);

            if (result != KernelResult.Success)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Operation failed with error 0x{result:x}!");
            }

            threadState.X0 = (ulong)result;
        }

        private void SvcUnmapPhysicalMemory(CpuThreadState threadState)
        {
            ulong address = threadState.X0;
            ulong size    = threadState.X1;

            if (!PageAligned(address))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Address 0x{address:x16} is not page aligned!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAddress);

                return;
            }

            if (!PageAligned(size) || size == 0)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Size 0x{size:x16} is not page aligned or is zero!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidSize);

                return;
            }

            if (address + size <= address)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid region address 0x{address:x16} / size 0x{size:x16}!");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            if ((currentProcess.PersonalMmHeapPagesCount & 0xfffffffffffff) == 0)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"System resource size is zero.");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidState);

                return;
            }

            if (!currentProcess.MemoryManager.InsideAddrSpace   (address, size) ||
                 currentProcess.MemoryManager.OutsideAliasRegion(address, size))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid address {address:x16}.");

                threadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            KernelResult result = _process.MemoryManager.UnmapPhysicalMemory(address, size);

            if (result != KernelResult.Success)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Operation failed with error 0x{result:x}!");
            }

            threadState.X0 = (ulong)result;
        }

        private static bool PageAligned(ulong position)
        {
            return (position & (KMemoryManager.PageSize - 1)) == 0;
        }
    }
}