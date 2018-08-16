using Ryujinx.HLE.Exceptions;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace Ryujinx.HLE.Loaders.Npdm
{
    class KernelAccessControl
    {
        public ReadOnlyCollection<KernelAccessControlItem> Items;

        public KernelAccessControl(Stream Stream, int Offset, int Size)
        {
            Stream.Seek(Offset, SeekOrigin.Begin);

            BinaryReader Reader = new BinaryReader(Stream);

            KernelAccessControlItem[] Items = new KernelAccessControlItem[Size / 4];

            for (int Index = 0; Index < Size / 4; Index++)
            {
                uint Descriptor = Reader.ReadUInt32();

                //Ignore the descriptor.
                if (Descriptor == 0xffffffff)
                {
                    continue;
                }

                Items[Index] = new KernelAccessControlItem();

                int LowBits = 0;

                while ((Descriptor & 1) != 0)
                {
                    Descriptor >>= 1;

                    LowBits++;
                }

                Descriptor >>= 1;

                switch (LowBits)
                {
                    //Kernel flags.
                    case 3:
                    {
                        Items[Index].HasKernelFlags = true;

                        Items[Index].HighestThreadPriority = (Descriptor >> 0)  & 0x3f;
                        Items[Index].LowestThreadPriority  = (Descriptor >> 6)  & 0x3f;
                        Items[Index].LowestCpuId           = (Descriptor >> 12) & 0xff;
                        Items[Index].HighestCpuId          = (Descriptor >> 20) & 0xff;

                        break;
                    }

                    //Syscall mask.
                    case 4:
                    {
                        Items[Index].HasSvcFlags = true;

                        Items[Index].AllowedSvcs = new bool[0x80];

                        int SysCallBase = (int)(Descriptor >> 24) * 0x18;

                        for (int SysCall = 0; SysCall < 0x18 && SysCallBase + SysCall < 0x80; SysCall++)
                        {
                            Items[Index].AllowedSvcs[SysCallBase + SysCall] = (Descriptor & 1) != 0;

                            Descriptor >>= 1;
                        }

                        break;
                    }

                    //Map IO/Normal.
                    case 6:
                    {
                        ulong Address = (Descriptor & 0xffffff) << 12;
                        bool  IsRo    = (Descriptor >> 24) != 0;

                        if (Index == Size / 4 - 1)
                        {
                            throw new InvalidNpdmException("Invalid Kernel Access Control Descriptors!");
                        }

                        Descriptor = Reader.ReadUInt32();

                        if ((Descriptor & 0x7f) != 0x3f)
                        {
                            throw new InvalidNpdmException("Invalid Kernel Access Control Descriptors!");
                        }

                        Descriptor >>= 7;

                        ulong MmioSize = (Descriptor & 0xffffff) << 12;
                        bool  IsNormal = (Descriptor >> 24) != 0;

                        Items[Index].NormalMmio.Add(new KernelAccessControlMmio(Address, MmioSize, IsRo, IsNormal));

                        Index++;

                        break;
                    }

                    //Map Normal Page.
                    case 7:
                    {
                        ulong Address = Descriptor << 12;

                        Items[Index].PageMmio.Add(new KernelAccessControlMmio(Address, 0x1000, false, false));

                        break;
                    }

                    //IRQ Pair.
                    case 11:
                    {
                        Items[Index].Irq.Add(new KernelAccessControlIrq(
                            (Descriptor >> 0)  & 0x3ff,
                            (Descriptor >> 10) & 0x3ff));

                        break;
                    }

                    //Application Type.
                    case 13:
                    {
                        Items[Index].HasApplicationType = true;

                        Items[Index].ApplicationType = (int)Descriptor & 7;

                        break;
                    }

                    //Kernel Release Version.
                    case 14:
                    {
                        Items[Index].HasKernelVersion = true;

                        Items[Index].KernelVersionRelease = (int)Descriptor;

                        break;
                    }

                    //Handle Table Size.
                    case 15:
                    {
                        Items[Index].HasHandleTableSize = true;

                        Items[Index].HandleTableSize = (int)Descriptor;

                        break;
                    }

                    //Debug Flags.
                    case 16:
                    {
                        Items[Index].HasDebugFlags = true;

                        Items[Index].AllowDebug = ((Descriptor >> 0) & 1) != 0;
                        Items[Index].ForceDebug = ((Descriptor >> 1) & 1) != 0;

                        break;
                    }
                }
            }

            this.Items = Array.AsReadOnly(Items);
        }
    }
}
