using ChocolArm64.Memory;
using ChocolArm64.State;
using Ryujinx.Core.OsHle.Handles;

namespace Ryujinx.Core.OsHle.Svc
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

            ThreadState.X0 = (int)SvcResult.Success;
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

            ThreadState.X0 = (int)SvcResult.Success;
        }

        private void SvcMapMemory(AThreadState ThreadState)
        {
            long Dst  = (long)ThreadState.X0;
            long Src  = (long)ThreadState.X1;
            long Size = (long)ThreadState.X2;

            AMemoryMapInfo SrcInfo = Memory.Manager.GetMapInfo(Src);

            Memory.Manager.Map(Dst, Size, (int)MemoryType.MappedMemory, SrcInfo.Perm);

            Memory.Manager.Reprotect(Src, Size, AMemoryPerm.None);

            Memory.Manager.SetAttrBit(Src, Size, 0);

            ThreadState.X0 = (int)SvcResult.Success;
        }

        private void SvcUnmapMemory(AThreadState ThreadState)
        {
            long Dst  = (long)ThreadState.X0;
            long Src  = (long)ThreadState.X1;
            long Size = (long)ThreadState.X2;

            AMemoryMapInfo DstInfo = Memory.Manager.GetMapInfo(Dst);

            Memory.Manager.Unmap(Dst, Size, (int)MemoryType.MappedMemory);

            Memory.Manager.Reprotect(Src, Size, DstInfo.Perm);

            Memory.Manager.ClearAttrBit(Src, Size, 0);

            ThreadState.X0 = (int)SvcResult.Success;
        }

        private void SvcQueryMemory(AThreadState ThreadState)
        {
            long InfoPtr  = (long)ThreadState.X0;
            long Position = (long)ThreadState.X2;

            AMemoryMapInfo MapInfo = Memory.Manager.GetMapInfo(Position);

            if (MapInfo == null)
            {
                //TODO: Correct error code.
                ThreadState.X0 = ulong.MaxValue;

                return;
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

            ThreadState.X0 = (int)SvcResult.Success;
            ThreadState.X1 = 0;
        }

        private void SvcMapSharedMemory(AThreadState ThreadState)
        {
            int  Handle =  (int)ThreadState.X0;
            long Src    = (long)ThreadState.X1;
            long Size   = (long)ThreadState.X2;
            int  Perm   =  (int)ThreadState.X3;

            HSharedMem SharedMem = Ns.Os.Handles.GetData<HSharedMem>(Handle);

            if (SharedMem != null)
            {
                SharedMem.AddVirtualPosition(Src);

                Memory.Manager.Map(Src, Size, (int)MemoryType.SharedMemory, (AMemoryPerm)Perm);

                ThreadState.X0 = (int)SvcResult.Success;
            }

            //TODO: Error codes.
        }

        private void SvcUnmapSharedMemory(AThreadState ThreadState)
        {
            int  Handle   =  (int)ThreadState.X0;
            long Position = (long)ThreadState.X1;
            long Size     = (long)ThreadState.X2;

            HSharedMem HndData = Ns.Os.Handles.GetData<HSharedMem>(Handle);

            if (HndData != null)
            {
                ThreadState.X0 = (int)SvcResult.Success;
            }

            //TODO: Error codes.
        }

        private void SvcCreateTransferMemory(AThreadState ThreadState)
        {
            long Position = (long)ThreadState.X1;
            long Size     = (long)ThreadState.X2;
            int  Perm     =  (int)ThreadState.X3;

            AMemoryMapInfo MapInfo = Memory.Manager.GetMapInfo(Position);

            Memory.Manager.Reprotect(Position, Size, (AMemoryPerm)Perm);

            HTransferMem HndData = new HTransferMem(Memory, MapInfo.Perm, Position, Size);

            int Handle = Ns.Os.Handles.GenerateId(HndData);

            ThreadState.X1 = (ulong)Handle;
            ThreadState.X0 = (int)SvcResult.Success;
        }
    }
}