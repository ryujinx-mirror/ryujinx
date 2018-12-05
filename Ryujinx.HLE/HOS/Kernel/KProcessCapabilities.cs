using Ryujinx.Common;

namespace Ryujinx.HLE.HOS.Kernel
{
    class KProcessCapabilities
    {
        public byte[] SvcAccessMask { get; private set; }
        public byte[] IrqAccessMask { get; private set; }

        public long AllowedCpuCoresMask    { get; private set; }
        public long AllowedThreadPriosMask { get; private set; }

        public int DebuggingFlags       { get; private set; }
        public int HandleTableSize      { get; private set; }
        public int KernelReleaseVersion { get; private set; }
        public int ApplicationType      { get; private set; }

        public KProcessCapabilities()
        {
            SvcAccessMask = new byte[0x10];
            IrqAccessMask = new byte[0x80];
        }

        public KernelResult InitializeForKernel(int[] Caps, KMemoryManager MemoryManager)
        {
            AllowedCpuCoresMask    = 0xf;
            AllowedThreadPriosMask = -1;
            DebuggingFlags        &= ~3;
            KernelReleaseVersion   = KProcess.KernelVersionPacked;

            return Parse(Caps, MemoryManager);
        }

        public KernelResult InitializeForUser(int[] Caps, KMemoryManager MemoryManager)
        {
            return Parse(Caps, MemoryManager);
        }

        private KernelResult Parse(int[] Caps, KMemoryManager MemoryManager)
        {
            int Mask0 = 0;
            int Mask1 = 0;

            for (int Index = 0; Index < Caps.Length; Index++)
            {
                int Cap = Caps[Index];

                if (((Cap + 1) & ~Cap) != 0x40)
                {
                    KernelResult Result = ParseCapability(Cap, ref Mask0, ref Mask1, MemoryManager);

                    if (Result != KernelResult.Success)
                    {
                        return Result;
                    }
                }
                else
                {
                    if ((uint)Index + 1 >= Caps.Length)
                    {
                        return KernelResult.InvalidCombination;
                    }

                    int PrevCap = Cap;

                    Cap = Caps[++Index];

                    if (((Cap + 1) & ~Cap) != 0x40)
                    {
                        return KernelResult.InvalidCombination;
                    }

                    if ((Cap & 0x78000000) != 0)
                    {
                        return KernelResult.MaximumExceeded;
                    }

                    if ((Cap & 0x7ffff80) == 0)
                    {
                        return KernelResult.InvalidSize;
                    }

                    long Address = ((long)(uint)PrevCap << 5) & 0xffffff000;
                    long Size    = ((long)(uint)Cap     << 5) & 0xfffff000;

                    if (((ulong)(Address + Size - 1) >> 36) != 0)
                    {
                        return KernelResult.InvalidAddress;
                    }

                    MemoryPermission Perm = (PrevCap >> 31) != 0
                        ? MemoryPermission.Read
                        : MemoryPermission.ReadAndWrite;

                    KernelResult Result;

                    if ((Cap >> 31) != 0)
                    {
                        Result = MemoryManager.MapNormalMemory(Address, Size, Perm);
                    }
                    else
                    {
                        Result = MemoryManager.MapIoMemory(Address, Size, Perm);
                    }

                    if (Result != KernelResult.Success)
                    {
                        return Result;
                    }
                }
            }

            return KernelResult.Success;
        }

        private KernelResult ParseCapability(int Cap, ref int Mask0, ref int Mask1, KMemoryManager MemoryManager)
        {
            int Code = (Cap + 1) & ~Cap;

            if (Code == 1)
            {
                return KernelResult.InvalidCapability;
            }
            else if (Code == 0)
            {
                return KernelResult.Success;
            }

            int CodeMask = 1 << (32 - BitUtils.CountLeadingZeros32(Code + 1));

            //Check if the property was already set.
            if (((Mask0 & CodeMask) & 0x1e008) != 0)
            {
                return KernelResult.InvalidCombination;
            }

            Mask0 |= CodeMask;

            switch (Code)
            {
                case 8:
                {
                    if (AllowedCpuCoresMask != 0 || AllowedThreadPriosMask != 0)
                    {
                        return KernelResult.InvalidCapability;
                    }

                    int LowestCpuCore  = (Cap >> 16) & 0xff;
                    int HighestCpuCore = (Cap >> 24) & 0xff;

                    if (LowestCpuCore > HighestCpuCore)
                    {
                        return KernelResult.InvalidCombination;
                    }

                    int HighestThreadPrio = (Cap >>  4) & 0x3f;
                    int LowestThreadPrio  = (Cap >> 10) & 0x3f;

                    if (LowestThreadPrio > HighestThreadPrio)
                    {
                        return KernelResult.InvalidCombination;
                    }

                    if (HighestCpuCore >= KScheduler.CpuCoresCount)
                    {
                        return KernelResult.InvalidCpuCore;
                    }

                    AllowedCpuCoresMask    = GetMaskFromMinMax(LowestCpuCore,    HighestCpuCore);
                    AllowedThreadPriosMask = GetMaskFromMinMax(LowestThreadPrio, HighestThreadPrio);

                    break;
                }

                case 0x10:
                {
                    int Slot = (Cap >> 29) & 7;

                    int SvcSlotMask = 1 << Slot;

                    if ((Mask1 & SvcSlotMask) != 0)
                    {
                        return KernelResult.InvalidCombination;
                    }

                    Mask1 |= SvcSlotMask;

                    int SvcMask = (Cap >> 5) & 0xffffff;

                    int BaseSvc = Slot * 24;

                    for (int Index = 0; Index < 24; Index++)
                    {
                        if (((SvcMask >> Index) & 1) == 0)
                        {
                            continue;
                        }

                        int SvcId = BaseSvc + Index;

                        if (SvcId > 0x7f)
                        {
                            return KernelResult.MaximumExceeded;
                        }

                        SvcAccessMask[SvcId / 8] |= (byte)(1 << (SvcId & 7));
                    }

                    break;
                }

                case 0x80:
                {
                    long Address = ((long)(uint)Cap << 4) & 0xffffff000;

                    MemoryManager.MapIoMemory(Address, KMemoryManager.PageSize, MemoryPermission.ReadAndWrite);

                    break;
                }

                case 0x800:
                {
                    //TODO: GIC distributor check.
                    int Irq0 = (Cap >> 12) & 0x3ff;
                    int Irq1 = (Cap >> 22) & 0x3ff;

                    if (Irq0 != 0x3ff)
                    {
                        IrqAccessMask[Irq0 / 8] |= (byte)(1 << (Irq0 & 7));
                    }

                    if (Irq1 != 0x3ff)
                    {
                        IrqAccessMask[Irq1 / 8] |= (byte)(1 << (Irq1 & 7));
                    }

                    break;
                }

                case 0x2000:
                {
                    int ApplicationType = Cap >> 14;

                    if ((uint)ApplicationType > 7)
                    {
                        return KernelResult.ReservedValue;
                    }

                    this.ApplicationType = ApplicationType;

                    break;
                }

                case 0x4000:
                {
                    //Note: This check is bugged on kernel too, we are just replicating the bug here.
                    if ((KernelReleaseVersion >> 17) != 0 || Cap < 0x80000)
                    {
                        return KernelResult.ReservedValue;
                    }

                    KernelReleaseVersion = Cap;

                    break;
                }

                case 0x8000:
                {
                    int HandleTableSize = Cap >> 26;

                    if ((uint)HandleTableSize > 0x3ff)
                    {
                        return KernelResult.ReservedValue;
                    }

                    this.HandleTableSize = HandleTableSize;

                    break;
                }

                case 0x10000:
                {
                    int DebuggingFlags = Cap >> 19;

                    if ((uint)DebuggingFlags > 3)
                    {
                        return KernelResult.ReservedValue;
                    }

                    this.DebuggingFlags &= ~3;
                    this.DebuggingFlags |= DebuggingFlags;

                    break;
                }

                default: return KernelResult.InvalidCapability;
            }

            return KernelResult.Success;
        }

        private static long GetMaskFromMinMax(int Min, int Max)
        {
            int Range = Max - Min + 1;

            long Mask = (1L << Range) - 1;

            return Mask << Min;
        }
    }
}