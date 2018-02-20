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

            Memory.Manager.SetHeapSize(Size, (int)MemoryType.Heap);

            ThreadState.X0 = (int)SvcResult.Success;
            ThreadState.X1 = (ulong)Memory.Manager.HeapAddr;
        }

        private void SvcSetMemoryAttribute(AThreadState ThreadState)
        {
            long Position = (long)ThreadState.X0;
            long Size     = (long)ThreadState.X1;
            int  State0   =  (int)ThreadState.X2;
            int  State1   =  (int)ThreadState.X3;

            //TODO

            ThreadState.X0 = (int)SvcResult.Success;
        }

        private void SvcMapMemory(AThreadState ThreadState)
        {
            long Dst  = (long)ThreadState.X0;
            long Src  = (long)ThreadState.X1;
            long Size = (long)ThreadState.X2;

            Memory.Manager.MapMirror(Src, Dst, Size, (int)MemoryType.MappedMemory);

            ThreadState.X0 = (int)SvcResult.Success;
        }

        private void SvcQueryMemory(AThreadState ThreadState)
        {
            long InfoPtr  = (long)ThreadState.X0;
            long Position = (long)ThreadState.X2;

            AMemoryMapInfo MapInfo = Memory.Manager.GetMapInfo(Position);

            MemoryInfo Info = new MemoryInfo(MapInfo);

            Memory.WriteInt64(InfoPtr + 0x00, Info.BaseAddress);
            Memory.WriteInt64(InfoPtr + 0x08, Info.Size);
            Memory.WriteInt32(InfoPtr + 0x10, Info.MemType);
            Memory.WriteInt32(InfoPtr + 0x14, Info.MemAttr);
            Memory.WriteInt32(InfoPtr + 0x18, Info.MemPerm);
            Memory.WriteInt32(InfoPtr + 0x1c, Info.IpcRefCount);
            Memory.WriteInt32(InfoPtr + 0x20, Info.DeviceRefCount);
            Memory.WriteInt32(InfoPtr + 0x24, Info.Padding);

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

                Memory.Manager.MapPhys(Src, Size, (int)MemoryType.SharedMemory, (AMemoryPerm)Perm);

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