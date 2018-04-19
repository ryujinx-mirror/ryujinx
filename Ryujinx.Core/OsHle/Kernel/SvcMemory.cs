using ChocolArm64.Memory;
using ChocolArm64.State;
using Ryujinx.Core.OsHle.Handles;

using static Ryujinx.Core.OsHle.ErrorCode;

namespace Ryujinx.Core.OsHle.Kernel
{
    partial class SvcHandler
    {
        private void SvcSetHeapSize(AThreadState ThreadState)
        {
            uint Size = (uint)ThreadState.X1;

            long Position = MemoryRegions.HeapRegionAddress;

            if (Size > CurrentHeapSize)
            {
                Memory.Manager.Map(Position, Size, (int)MemoryType.Heap, AMemoryPerm.RW);
            }
            else
            {
                Memory.Manager.Unmap(Position + Size, (long)CurrentHeapSize - Size);
            }

            CurrentHeapSize = Size;

            ThreadState.X0 = 0;
            ThreadState.X1 = (ulong)Position;
        }

        private void SvcSetMemoryAttribute(AThreadState ThreadState)
        {
            long Position = (long)ThreadState.X0;
            long Size     = (long)ThreadState.X1;
            int  State0   =  (int)ThreadState.X2;
            int  State1   =  (int)ThreadState.X3;

            if ((State0 == 0 && State1 == 0) ||
                (State0 == 8 && State1 == 0))
            {
                Memory.Manager.ClearAttrBit(Position, Size, 3);
            }
            else if (State0 == 8 && State1 == 8)
            {
                Memory.Manager.SetAttrBit(Position, Size, 3);
            }

            ThreadState.X0 = 0;
        }

        private void SvcMapMemory(AThreadState ThreadState)
        {
            long Dst  = (long)ThreadState.X0;
            long Src  = (long)ThreadState.X1;
            long Size = (long)ThreadState.X2;

            if (!IsValidPosition(Src))
            {
                Logging.Warn(LogClass.KernelSvc, $"Tried to map Memory at invalid src address {Src:x16}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidMemRange);

                return;
            }

            if (!IsValidMapPosition(Dst))
            {
                Logging.Warn(LogClass.KernelSvc, $"Tried to map Memory at invalid dst address {Dst:x16}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidMemRange);

                return;
            }

            AMemoryMapInfo SrcInfo = Memory.Manager.GetMapInfo(Src);

            Memory.Manager.Map(Dst, Size, (int)MemoryType.MappedMemory, SrcInfo.Perm);

            Memory.Manager.Reprotect(Src, Size, AMemoryPerm.None);

            Memory.Manager.SetAttrBit(Src, Size, 0);

            ThreadState.X0 = 0;
        }

        private void SvcUnmapMemory(AThreadState ThreadState)
        {
            long Dst  = (long)ThreadState.X0;
            long Src  = (long)ThreadState.X1;
            long Size = (long)ThreadState.X2;

            if (!IsValidPosition(Src))
            {
                Logging.Warn(LogClass.KernelSvc, $"Tried to unmap Memory at invalid src address {Src:x16}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidMemRange);

                return;
            }

            if (!IsValidMapPosition(Dst))
            {
                Logging.Warn(LogClass.KernelSvc, $"Tried to unmap Memory at invalid dst address {Dst:x16}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidMemRange);

                return;
            }

            AMemoryMapInfo DstInfo = Memory.Manager.GetMapInfo(Dst);

            Memory.Manager.Unmap(Dst, Size, (int)MemoryType.MappedMemory);

            Memory.Manager.Reprotect(Src, Size, DstInfo.Perm);

            Memory.Manager.ClearAttrBit(Src, Size, 0);

            ThreadState.X0 = 0;
        }

        private void SvcQueryMemory(AThreadState ThreadState)
        {
            long InfoPtr  = (long)ThreadState.X0;
            long Position = (long)ThreadState.X2;

            AMemoryMapInfo MapInfo = Memory.Manager.GetMapInfo(Position);

            if (MapInfo == null)
            {
                long AddrSpaceEnd = MemoryRegions.AddrSpaceStart + MemoryRegions.AddrSpaceSize;

                long ReservedSize = (long)(ulong.MaxValue - (ulong)AddrSpaceEnd) + 1;

                MapInfo = new AMemoryMapInfo(AddrSpaceEnd, ReservedSize, (int)MemoryType.Reserved, 0, AMemoryPerm.None);
            }

            Memory.WriteInt64(InfoPtr + 0x00, MapInfo.Position);
            Memory.WriteInt64(InfoPtr + 0x08, MapInfo.Size);
            Memory.WriteInt32(InfoPtr + 0x10, MapInfo.Type);
            Memory.WriteInt32(InfoPtr + 0x14, MapInfo.Attr);
            Memory.WriteInt32(InfoPtr + 0x18, (int)MapInfo.Perm);
            Memory.WriteInt32(InfoPtr + 0x1c, 0);
            Memory.WriteInt32(InfoPtr + 0x20, 0);
            Memory.WriteInt32(InfoPtr + 0x24, 0);
            //TODO: X1.

            ThreadState.X0 = 0;
            ThreadState.X1 = 0;
        }

        private void SvcMapSharedMemory(AThreadState ThreadState)
        {
            int  Handle =  (int)ThreadState.X0;
            long Src    = (long)ThreadState.X1;
            long Size   = (long)ThreadState.X2;
            int  Perm   =  (int)ThreadState.X3;

            if (!IsValidPosition(Src))
            {
                Logging.Warn(LogClass.KernelSvc, $"Tried to map SharedMemory at invalid address {Src:x16}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidMemRange);

                return;
            }

            HSharedMem SharedMem = Process.HandleTable.GetData<HSharedMem>(Handle);

            if (SharedMem != null)
            {
                Memory.Manager.Map(Src, Size, (int)MemoryType.SharedMemory, AMemoryPerm.Write);

                AMemoryHelper.FillWithZeros(Memory, Src, (int)Size);

                Memory.Manager.Reprotect(Src, Size, (AMemoryPerm)Perm);

                lock (MappedSharedMems)
                {
                    MappedSharedMems.Add((SharedMem, Src));
                }

                SharedMem.AddVirtualPosition(Memory, Src);

                ThreadState.X0 = 0;
            }

            //TODO: Error codes.
        }

        private void SvcUnmapSharedMemory(AThreadState ThreadState)
        {
            int  Handle =  (int)ThreadState.X0;
            long Src    = (long)ThreadState.X1;
            long Size   = (long)ThreadState.X2;

            if (!IsValidPosition(Src))
            {
                Logging.Warn(LogClass.KernelSvc, $"Tried to unmap SharedMemory at invalid address {Src:x16}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidMemRange);

                return;
            }

            HSharedMem SharedMem = Process.HandleTable.GetData<HSharedMem>(Handle);

            if (SharedMem != null)
            {
                Memory.Manager.Unmap(Src, Size, (int)MemoryType.SharedMemory);

                SharedMem.RemoveVirtualPosition(Memory, Src);

                lock (MappedSharedMems)
                {
                    MappedSharedMems.Remove((SharedMem, Src));
                }

                ThreadState.X0 = 0;
            }

            //TODO: Error codes.
        }

        private void SvcCreateTransferMemory(AThreadState ThreadState)
        {
            long Src  = (long)ThreadState.X1;
            long Size = (long)ThreadState.X2;
            int  Perm =  (int)ThreadState.X3;

            if (!IsValidPosition(Src))
            {
                Logging.Warn(LogClass.KernelSvc, $"Tried to create TransferMemory at invalid address {Src:x16}!");

                ThreadState.X0 = MakeError(ErrorModule.Kernel, KernelErr.InvalidMemRange);

                return;
            }

            AMemoryMapInfo MapInfo = Memory.Manager.GetMapInfo(Src);

            Memory.Manager.Reprotect(Src, Size, (AMemoryPerm)Perm);

            HTransferMem TMem = new HTransferMem(Memory, MapInfo.Perm, Src, Size);

            ulong Handle = (ulong)Process.HandleTable.OpenHandle(TMem);

            ThreadState.X0 = 0;
            ThreadState.X1 = Handle;
        }

        private static bool IsValidPosition(long Position)
        {
            return Position >= MemoryRegions.AddrSpaceStart &&
                   Position <  MemoryRegions.AddrSpaceStart + MemoryRegions.AddrSpaceSize;
        }

        private static bool IsValidMapPosition(long Position)
        {
            return Position >= MemoryRegions.MapRegionAddress &&
                   Position <  MemoryRegions.MapRegionAddress + MemoryRegions.MapRegionSize;
        }
    }
}