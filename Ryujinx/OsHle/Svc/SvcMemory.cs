using ChocolArm64.Memory;
using ChocolArm64.State;
using Ryujinx.OsHle.Handles;

namespace Ryujinx.OsHle.Svc
{
    partial class SvcHandler
    {
        private static void SvcSetHeapSize(Switch Ns, ARegisters Registers, AMemory Memory)
        {
            int Size = (int)Registers.X1;

            Memory.Manager.SetHeapSize(Size, (int)MemoryType.Heap);

            Registers.X0 = (int)SvcResult.Success;
            Registers.X1 = (ulong)Memory.Manager.HeapAddr;
        }

        private static void SvcSetMemoryAttribute(Switch Ns, ARegisters Registers, AMemory Memory)
        {
            long Position = (long)Registers.X0;
            long Size     = (long)Registers.X1;
            int  State0   =  (int)Registers.X2;
            int  State1   =  (int)Registers.X3;

            //TODO

            Registers.X0 = (int)SvcResult.Success;
        }

        private static void SvcMapMemory(Switch Ns, ARegisters Registers, AMemory Memory)
        {
            long Src  = (long)Registers.X0;
            long Dst  = (long)Registers.X1;
            long Size = (long)Registers.X2;

            Memory.Manager.MapMirror(Src, Dst, Size, (int)MemoryType.MappedMemory);

            Registers.X0 = (int)SvcResult.Success;
        }

        private static void SvcQueryMemory(Switch Ns, ARegisters Registers, AMemory Memory)
        {
            long InfoPtr  = (long)Registers.X0;
            long Position = (long)Registers.X2;

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

            Registers.X0 = (int)SvcResult.Success;
            Registers.X1 = 0;
        }

        private static void SvcMapSharedMemory(Switch Ns, ARegisters Registers, AMemory Memory)
        {
            int  Handle   =  (int)Registers.X0;
            long Position = (long)Registers.X1;
            long Size     = (long)Registers.X2;
            int  Perm     =  (int)Registers.X3;

            HSharedMem HndData = Ns.Os.Handles.GetData<HSharedMem>(Handle);

            if (HndData != null)
            {
                long Src = Position;
                long Dst = HndData.PhysPos;

                if (Memory.Manager.MapPhys(Src, Dst, Size,
                    (int)MemoryType.SharedMemory, (AMemoryPerm)Perm))
                {
                    Registers.X0 = (int)SvcResult.Success;
                }
            }

            //TODO: Error codes.
        }

        private static void SvcUnmapSharedMemory(Switch Ns, ARegisters Registers, AMemory Memory)
        {
            int  Handle   =  (int)Registers.X0;
            long Position = (long)Registers.X1;
            long Size     = (long)Registers.X2;

            HSharedMem HndData = Ns.Os.Handles.GetData<HSharedMem>(Handle);

            if (HndData != null)
            {
                Registers.X0 = (int)SvcResult.Success;
            }

            //TODO: Error codes.
        }

        private static void SvcCreateTransferMemory(Switch Ns, ARegisters Registers, AMemory Memory)
        {
            long Position = (long)Registers.X1;
            long Size     = (long)Registers.X2;
            int  Perm     =  (int)Registers.X3;

            AMemoryMapInfo MapInfo = Memory.Manager.GetMapInfo(Position);

            Memory.Manager.Reprotect(Position, Size, (AMemoryPerm)Perm);

            long PhysPos = Memory.Manager.GetPhys(Position, AMemoryPerm.None);

            HTransferMem HndData = new HTransferMem(Memory, MapInfo.Perm, Position, Size, PhysPos);

            int Handle = Ns.Os.Handles.GenerateId(HndData);

            Registers.X1 = (ulong)Handle;
            Registers.X0 = (int)SvcResult.Success;
        }
    }
}