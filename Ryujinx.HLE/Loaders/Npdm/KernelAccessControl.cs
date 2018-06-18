using System.Collections.Generic;
using System.IO;

namespace Ryujinx.HLE.Loaders.Npdm
{
    public class KernelAccessControlIRQ
    {
        public uint IRQ0;
        public uint IRQ1;
    }

    public class KernelAccessControlMMIO
    {
        public ulong Address;
        public ulong Size;
        public bool  IsRO;
        public bool  IsNormal;
    }

    public class KernelAccessControlItems
    {
        public bool  HasKernelFlags;
        public uint  LowestThreadPriority;
        public uint  HighestThreadPriority;
        public uint  LowestCpuId;
        public uint  HighestCpuId;

        public bool  HasSVCFlags;
        public int[] SVCsAllowed;

        public List<KernelAccessControlMMIO> NormalMMIO;
        public List<KernelAccessControlMMIO> PageMMIO;
        public List<KernelAccessControlIRQ>  IRQ;

        public bool HasApplicationType;
        public int  ApplicationType;

        public bool HasKernelVersion;
        public int  KernelVersionRelease;

        public bool HasHandleTableSize;
        public int  HandleTableSize;

        public bool HasDebugFlags;
        public bool AllowDebug;
        public bool ForceDebug;
    }

    public class KernelAccessControl
    {
        public KernelAccessControlItems[] Items;

        public KernelAccessControl(Stream FSAccessControlsStream, int Offset, int Size)
        {
            FSAccessControlsStream.Seek(Offset, SeekOrigin.Begin);

            BinaryReader Reader = new BinaryReader(FSAccessControlsStream);

            Items = new KernelAccessControlItems[Size / 4];

            for (int i = 0; i < Size / 4; i++)
            {
                uint Descriptor = Reader.ReadUInt32();

                if (Descriptor == 0xFFFFFFFF) //Ignore the descriptor
                {
                    continue;
                }

                Items[i] = new KernelAccessControlItems();

                int LowBits = 0;

                while ((Descriptor & 1) != 0)
                {
                    Descriptor >>= 1;
                    LowBits++;
                }

                Descriptor >>= 1;

                switch (LowBits)
                {
                    case 3: // Kernel flags
                    {
                        Items[i].HasKernelFlags        = true;

                        Items[i].HighestThreadPriority = Descriptor & 0x3F;
                        Items[i].LowestThreadPriority  = (Descriptor >> 6) & 0x3F;
                        Items[i].LowestCpuId           = (Descriptor >> 12) & 0xFF;
                        Items[i].HighestCpuId          = (Descriptor >> 20) & 0xFF;

                        break;
                    }

                    case 4: // Syscall mask
                    {
                        Items[i].HasSVCFlags = true;

                        Items[i].SVCsAllowed = new int[0x80];

                        int SysCallBase = (int)(Descriptor >> 24) * 0x18;

                        for (int SysCall = 0; SysCall < 0x18 && SysCallBase + SysCall < 0x80; SysCall++)
                        {
                            Items[i].SVCsAllowed[SysCallBase + SysCall] = (int)Descriptor & 1;
                            Descriptor >>= 1;
                        }

                        break;
                    }

                    case 6: // Map IO/Normal - Never tested.
                    {
                        KernelAccessControlMMIO TempNormalMMIO = new KernelAccessControlMMIO
                        {
                            Address = (Descriptor & 0xFFFFFF) << 12,
                            IsRO    = (Descriptor >> 24) != 0
                        };

                        if (i == Size / 4 - 1)
                        {
                            throw new InvalidNpdmException("Invalid Kernel Access Control Descriptors!");
                        }

                        Descriptor = Reader.ReadUInt32();

                        if ((Descriptor & 0x7F) != 0x3F)
                        {
                            throw new InvalidNpdmException("Invalid Kernel Access Control Descriptors!");
                        }

                        Descriptor >>= 7;
                        TempNormalMMIO.Size     = (Descriptor & 0xFFFFFF) << 12;
                        TempNormalMMIO.IsNormal = (Descriptor >> 24) != 0;

                        Items[i].NormalMMIO.Add(TempNormalMMIO);
                        i++;

                        break;
                    }

                    case 7: // Map Normal Page - Never tested.
                    {
                        KernelAccessControlMMIO TempPageMMIO = new KernelAccessControlMMIO
                        {
                            Address  = Descriptor << 12,
                            Size     = 0x1000,
                            IsRO     = false,
                            IsNormal = false
                        };

                        Items[i].PageMMIO.Add(TempPageMMIO);

                        break;
                    }

                    case 11: // IRQ Pair - Never tested.
                    {
                        KernelAccessControlIRQ TempIRQ = new KernelAccessControlIRQ
                        {
                            IRQ0 = Descriptor & 0x3FF,
                            IRQ1 = (Descriptor >> 10) & 0x3FF
                        };

                        break;
                    }

                    case 13: // App Type
                    {
                        Items[i].HasApplicationType = true;
                        Items[i].ApplicationType    = (int)Descriptor & 7;

                        break;
                    }

                    case 14: // Kernel Release Version
                    {
                        Items[i].HasKernelVersion     = true;

                        Items[i].KernelVersionRelease = (int)Descriptor;

                        break;
                    }

                    case 15: // Handle Table Size
                    {
                        Items[i].HasHandleTableSize = true;

                        Items[i].HandleTableSize    = (int)Descriptor;

                        break;
                    }

                    case 16: // Debug Flags
                    {
                        Items[i].HasDebugFlags = true;

                        Items[i].AllowDebug    = (Descriptor & 1) != 0;
                        Items[i].ForceDebug    = ((Descriptor >> 1) & 1) != 0;

                        break;
                    }
                }
            }
        }
    }
}
