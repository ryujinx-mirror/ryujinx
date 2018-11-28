using ChocolArm64.State;
using Ryujinx.Common.Logging;

using static Ryujinx.HLE.HOS.ErrorCode;

namespace Ryujinx.HLE.HOS.Kernel
{
    partial class SvcHandler
    {
        private void SvcSetHeapSize(CpuThreadState ThreadState)
        {
            ulong Size = ThreadState.X1;

            if ((Size & 0xfffffffe001fffff) != 0)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Heap size 0x{Size:x16} is not aligned!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidSize);

                return;
            }

            KernelResult Result = Process.MemoryManager.SetHeapSize(Size, out ulong Position);

            ThreadState.X0 = (ulong)Result;

            if (Result == KernelResult.Success)
            {
                ThreadState.X1 = Position;
            }
            else
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Operation failed with error \"{Result}\".");
            }
        }

        private void SvcSetMemoryAttribute(CpuThreadState ThreadState)
        {
            ulong Position = ThreadState.X0;
            ulong Size     = ThreadState.X1;

            if (!PageAligned(Position))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Address 0x{Position:x16} is not page aligned!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAddress);

                return;
            }

            if (!PageAligned(Size) || Size == 0)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Size 0x{Size:x16} is not page aligned or is zero!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidSize);

                return;
            }

            MemoryAttribute AttributeMask  = (MemoryAttribute)ThreadState.X2;
            MemoryAttribute AttributeValue = (MemoryAttribute)ThreadState.X3;

            MemoryAttribute Attributes = AttributeMask | AttributeValue;

            if (Attributes != AttributeMask ||
               (Attributes | MemoryAttribute.Uncached) != MemoryAttribute.Uncached)
            {
                Logger.PrintWarning(LogClass.KernelSvc, "Invalid memory attributes!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidMaskValue);

                return;
            }

            KernelResult Result = Process.MemoryManager.SetMemoryAttribute(
                Position,
                Size,
                AttributeMask,
                AttributeValue);

            if (Result != KernelResult.Success)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Operation failed with error \"{Result}\".");
            }
            else
            {
                Memory.StopObservingRegion((long)Position, (long)Size);
            }

            ThreadState.X0 = (ulong)Result;
        }

        private void SvcMapMemory(CpuThreadState ThreadState)
        {
            ulong Dst  = ThreadState.X0;
            ulong Src  = ThreadState.X1;
            ulong Size = ThreadState.X2;

            if (!PageAligned(Src | Dst))
            {
                Logger.PrintWarning(LogClass.KernelSvc, "Addresses are not page aligned!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAddress);

                return;
            }

            if (!PageAligned(Size) || Size == 0)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Size 0x{Size:x16} is not page aligned or is zero!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidSize);

                return;
            }

            if (Src + Size <= Src || Dst + Size <= Dst)
            {
                Logger.PrintWarning(LogClass.KernelSvc, "Addresses outside of range!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            KProcess CurrentProcess = System.Scheduler.GetCurrentProcess();

            if (!CurrentProcess.MemoryManager.InsideAddrSpace(Src, Size))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Src address 0x{Src:x16} out of range!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            if (CurrentProcess.MemoryManager.OutsideStackRegion(Dst, Size) ||
                CurrentProcess.MemoryManager.InsideHeapRegion  (Dst, Size) ||
                CurrentProcess.MemoryManager.InsideAliasRegion (Dst, Size))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Dst address 0x{Dst:x16} out of range!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidMemRange);

                return;
            }

            KernelResult Result = Process.MemoryManager.Map(Dst, Src, Size);

            if (Result != KernelResult.Success)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Operation failed with error 0x{Result:x}!");
            }

            ThreadState.X0 = (ulong)Result;
        }

        private void SvcUnmapMemory(CpuThreadState ThreadState)
        {
            ulong Dst  = ThreadState.X0;
            ulong Src  = ThreadState.X1;
            ulong Size = ThreadState.X2;

            if (!PageAligned(Src | Dst))
            {
                Logger.PrintWarning(LogClass.KernelSvc, "Addresses are not page aligned!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAddress);

                return;
            }

            if (!PageAligned(Size) || Size == 0)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Size 0x{Size:x16} is not page aligned or is zero!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidSize);

                return;
            }

            if (Src + Size <= Src || Dst + Size <= Dst)
            {
                Logger.PrintWarning(LogClass.KernelSvc, "Addresses outside of range!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            KProcess CurrentProcess = System.Scheduler.GetCurrentProcess();

            if (!CurrentProcess.MemoryManager.InsideAddrSpace(Src, Size))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Src address 0x{Src:x16} out of range!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            if (CurrentProcess.MemoryManager.OutsideStackRegion(Dst, Size) ||
                CurrentProcess.MemoryManager.InsideHeapRegion  (Dst, Size) ||
                CurrentProcess.MemoryManager.InsideAliasRegion (Dst, Size))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Dst address 0x{Dst:x16} out of range!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidMemRange);

                return;
            }

            KernelResult Result = Process.MemoryManager.Unmap(Dst, Src, Size);

            if (Result != KernelResult.Success)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Operation failed with error 0x{Result:x}!");
            }

            ThreadState.X0 = (ulong)Result;
        }

        private void SvcQueryMemory(CpuThreadState ThreadState)
        {
            long  InfoPtr  = (long)ThreadState.X0;
            ulong Position =       ThreadState.X2;

            KMemoryInfo BlkInfo = Process.MemoryManager.QueryMemory(Position);

            Memory.WriteUInt64(InfoPtr + 0x00, BlkInfo.Address);
            Memory.WriteUInt64(InfoPtr + 0x08, BlkInfo.Size);
            Memory.WriteInt32 (InfoPtr + 0x10, (int)BlkInfo.State & 0xff);
            Memory.WriteInt32 (InfoPtr + 0x14, (int)BlkInfo.Attribute);
            Memory.WriteInt32 (InfoPtr + 0x18, (int)BlkInfo.Permission);
            Memory.WriteInt32 (InfoPtr + 0x1c, BlkInfo.IpcRefCount);
            Memory.WriteInt32 (InfoPtr + 0x20, BlkInfo.DeviceRefCount);
            Memory.WriteInt32 (InfoPtr + 0x24, 0);

            ThreadState.X0 = 0;
            ThreadState.X1 = 0;
        }

        private void SvcMapSharedMemory(CpuThreadState ThreadState)
        {
            int   Handle  =  (int)ThreadState.X0;
            ulong Address =       ThreadState.X1;
            ulong Size    =       ThreadState.X2;

            if (!PageAligned(Address))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Address 0x{Address:x16} is not page aligned!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAddress);

                return;
            }

            if (!PageAligned(Size) || Size == 0)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Size 0x{Size:x16} is not page aligned or is zero!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidSize);

                return;
            }

            if (Address + Size <= Address)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid region address 0x{Address:x16} / size 0x{Size:x16}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            MemoryPermission Permission = (MemoryPermission)ThreadState.X3;

            if ((Permission | MemoryPermission.Write) != MemoryPermission.ReadAndWrite)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid permission {Permission}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidPermission);

                return;
            }

            KProcess CurrentProcess = System.Scheduler.GetCurrentProcess();

            KSharedMemory SharedMemory = CurrentProcess.HandleTable.GetObject<KSharedMemory>(Handle);

            if (SharedMemory == null)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid shared memory handle 0x{Handle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                return;
            }

            if (CurrentProcess.MemoryManager.IsInvalidRegion  (Address, Size) ||
                CurrentProcess.MemoryManager.InsideHeapRegion (Address, Size) ||
                CurrentProcess.MemoryManager.InsideAliasRegion(Address, Size))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Address 0x{Address:x16} out of range!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            KernelResult Result = SharedMemory.MapIntoProcess(
                CurrentProcess.MemoryManager,
                Address,
                Size,
                CurrentProcess,
                Permission);

            if (Result != KernelResult.Success)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Operation failed with error \"{Result}\".");
            }

            ThreadState.X0 = (ulong)Result;
        }

        private void SvcUnmapSharedMemory(CpuThreadState ThreadState)
        {
            int   Handle  =  (int)ThreadState.X0;
            ulong Address =       ThreadState.X1;
            ulong Size    =       ThreadState.X2;

            if (!PageAligned(Address))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Address 0x{Address:x16} is not page aligned!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAddress);

                return;
            }

            if (!PageAligned(Size) || Size == 0)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Size 0x{Size:x16} is not page aligned or is zero!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidSize);

                return;
            }

            if (Address + Size <= Address)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid region address 0x{Address:x16} / size 0x{Size:x16}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            KProcess CurrentProcess = System.Scheduler.GetCurrentProcess();

            KSharedMemory SharedMemory = CurrentProcess.HandleTable.GetObject<KSharedMemory>(Handle);

            if (SharedMemory == null)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid shared memory handle 0x{Handle:x8}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidHandle);

                return;
            }

            if (CurrentProcess.MemoryManager.IsInvalidRegion  (Address, Size) ||
                CurrentProcess.MemoryManager.InsideHeapRegion (Address, Size) ||
                CurrentProcess.MemoryManager.InsideAliasRegion(Address, Size))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Address 0x{Address:x16} out of range!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            KernelResult Result = SharedMemory.UnmapFromProcess(
                CurrentProcess.MemoryManager,
                Address,
                Size,
                CurrentProcess);

            if (Result != KernelResult.Success)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Operation failed with error \"{Result}\".");
            }

            ThreadState.X0 = (ulong)Result;
        }

        private void SvcCreateTransferMemory(CpuThreadState ThreadState)
        {
            ulong Address = ThreadState.X1;
            ulong Size    = ThreadState.X2;

            if (!PageAligned(Address))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Address 0x{Address:x16} is not page aligned!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAddress);

                return;
            }

            if (!PageAligned(Size) || Size == 0)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Size 0x{Size:x16} is not page aligned or is zero!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAddress);

                return;
            }

            if (Address + Size <= Address)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid region address 0x{Address:x16} / size 0x{Size:x16}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            MemoryPermission Permission = (MemoryPermission)ThreadState.X3;

            if (Permission > MemoryPermission.ReadAndWrite || Permission == MemoryPermission.Write)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid permission {Permission}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidPermission);

                return;
            }

            Process.MemoryManager.ReserveTransferMemory(Address, Size, Permission);

            KTransferMemory TransferMemory = new KTransferMemory(Address, Size);

            KernelResult Result = Process.HandleTable.GenerateHandle(TransferMemory, out int Handle);

            ThreadState.X0 = (uint)Result;
            ThreadState.X1 = (ulong)Handle;
        }

        private void SvcMapPhysicalMemory(CpuThreadState ThreadState)
        {
            ulong Address = ThreadState.X0;
            ulong Size    = ThreadState.X1;

            if (!PageAligned(Address))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Address 0x{Address:x16} is not page aligned!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAddress);

                return;
            }

            if (!PageAligned(Size) || Size == 0)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Size 0x{Size:x16} is not page aligned or is zero!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidSize);

                return;
            }

            if (Address + Size <= Address)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid region address 0x{Address:x16} / size 0x{Size:x16}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            KProcess CurrentProcess = System.Scheduler.GetCurrentProcess();

            if ((CurrentProcess.PersonalMmHeapPagesCount & 0xfffffffffffff) == 0)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"System resource size is zero.");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidState);

                return;
            }

            if (!CurrentProcess.MemoryManager.InsideAddrSpace   (Address, Size) ||
                 CurrentProcess.MemoryManager.OutsideAliasRegion(Address, Size))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid address {Address:x16}.");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            KernelResult Result = Process.MemoryManager.MapPhysicalMemory(Address, Size);

            if (Result != KernelResult.Success)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Operation failed with error 0x{Result:x}!");
            }

            ThreadState.X0 = (ulong)Result;
        }

        private void SvcUnmapPhysicalMemory(CpuThreadState ThreadState)
        {
            ulong Address = ThreadState.X0;
            ulong Size    = ThreadState.X1;

            if (!PageAligned(Address))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Address 0x{Address:x16} is not page aligned!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidAddress);

                return;
            }

            if (!PageAligned(Size) || Size == 0)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Size 0x{Size:x16} is not page aligned or is zero!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidSize);

                return;
            }

            if (Address + Size <= Address)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid region address 0x{Address:x16} / size 0x{Size:x16}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            KProcess CurrentProcess = System.Scheduler.GetCurrentProcess();

            if ((CurrentProcess.PersonalMmHeapPagesCount & 0xfffffffffffff) == 0)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"System resource size is zero.");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidState);

                return;
            }

            if (!CurrentProcess.MemoryManager.InsideAddrSpace   (Address, Size) ||
                 CurrentProcess.MemoryManager.OutsideAliasRegion(Address, Size))
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Invalid address {Address:x16}.");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.NoAccessPerm);

                return;
            }

            KernelResult Result = Process.MemoryManager.UnmapPhysicalMemory(Address, Size);

            if (Result != KernelResult.Success)
            {
                Logger.PrintWarning(LogClass.KernelSvc, $"Operation failed with error 0x{Result:x}!");
            }

            ThreadState.X0 = (ulong)Result;
        }

        private static bool PageAligned(ulong Position)
        {
            return (Position & (KMemoryManager.PageSize - 1)) == 0;
        }
    }
}