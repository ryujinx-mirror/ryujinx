using Ryujinx.Common;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Memory;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System;

namespace Ryujinx.HLE.HOS.Kernel.Process
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

        public KernelResult InitializeForKernel(ReadOnlySpan<int> capabilities, KMemoryManager memoryManager)
        {
            AllowedCpuCoresMask    = 0xf;
            AllowedThreadPriosMask = -1;
            DebuggingFlags        &= ~3;
            KernelReleaseVersion   = KProcess.KernelVersionPacked;

            return Parse(capabilities, memoryManager);
        }

        public KernelResult InitializeForUser(ReadOnlySpan<int> capabilities, KMemoryManager memoryManager)
        {
            return Parse(capabilities, memoryManager);
        }

        private KernelResult Parse(ReadOnlySpan<int> capabilities, KMemoryManager memoryManager)
        {
            int mask0 = 0;
            int mask1 = 0;

            for (int index = 0; index < capabilities.Length; index++)
            {
                int cap = capabilities[index];

                if (((cap + 1) & ~cap) != 0x40)
                {
                    KernelResult result = ParseCapability(cap, ref mask0, ref mask1, memoryManager);

                    if (result != KernelResult.Success)
                    {
                        return result;
                    }
                }
                else
                {
                    if ((uint)index + 1 >= capabilities.Length)
                    {
                        return KernelResult.InvalidCombination;
                    }

                    int prevCap = cap;

                    cap = capabilities[++index];

                    if (((cap + 1) & ~cap) != 0x40)
                    {
                        return KernelResult.InvalidCombination;
                    }

                    if ((cap & 0x78000000) != 0)
                    {
                        return KernelResult.MaximumExceeded;
                    }

                    if ((cap & 0x7ffff80) == 0)
                    {
                        return KernelResult.InvalidSize;
                    }

                    long address = ((long)(uint)prevCap << 5) & 0xffffff000;
                    long size    = ((long)(uint)cap     << 5) & 0xfffff000;

                    if (((ulong)(address + size - 1) >> 36) != 0)
                    {
                        return KernelResult.InvalidAddress;
                    }

                    KMemoryPermission perm = (prevCap >> 31) != 0
                        ? KMemoryPermission.Read
                        : KMemoryPermission.ReadAndWrite;

                    KernelResult result;

                    if ((cap >> 31) != 0)
                    {
                        result = memoryManager.MapNormalMemory(address, size, perm);
                    }
                    else
                    {
                        result = memoryManager.MapIoMemory(address, size, perm);
                    }

                    if (result != KernelResult.Success)
                    {
                        return result;
                    }
                }
            }

            return KernelResult.Success;
        }

        private KernelResult ParseCapability(int cap, ref int mask0, ref int mask1, KMemoryManager memoryManager)
        {
            int code = (cap + 1) & ~cap;

            if (code == 1)
            {
                return KernelResult.InvalidCapability;
            }
            else if (code == 0)
            {
                return KernelResult.Success;
            }

            int codeMask = 1 << (32 - BitUtils.CountLeadingZeros32(code + 1));

            // Check if the property was already set.
            if (((mask0 & codeMask) & 0x1e008) != 0)
            {
                return KernelResult.InvalidCombination;
            }

            mask0 |= codeMask;

            switch (code)
            {
                case 8:
                {
                    if (AllowedCpuCoresMask != 0 || AllowedThreadPriosMask != 0)
                    {
                        return KernelResult.InvalidCapability;
                    }

                    int lowestCpuCore  = (cap >> 16) & 0xff;
                    int highestCpuCore = (cap >> 24) & 0xff;

                    if (lowestCpuCore > highestCpuCore)
                    {
                        return KernelResult.InvalidCombination;
                    }

                    int highestThreadPrio = (cap >>  4) & 0x3f;
                    int lowestThreadPrio  = (cap >> 10) & 0x3f;

                    if (lowestThreadPrio > highestThreadPrio)
                    {
                        return KernelResult.InvalidCombination;
                    }

                    if (highestCpuCore >= KScheduler.CpuCoresCount)
                    {
                        return KernelResult.InvalidCpuCore;
                    }

                    AllowedCpuCoresMask    = GetMaskFromMinMax(lowestCpuCore,    highestCpuCore);
                    AllowedThreadPriosMask = GetMaskFromMinMax(lowestThreadPrio, highestThreadPrio);

                    break;
                }

                case 0x10:
                {
                    int slot = (cap >> 29) & 7;

                    int svcSlotMask = 1 << slot;

                    if ((mask1 & svcSlotMask) != 0)
                    {
                        return KernelResult.InvalidCombination;
                    }

                    mask1 |= svcSlotMask;

                    int svcMask = (cap >> 5) & 0xffffff;

                    int baseSvc = slot * 24;

                    for (int index = 0; index < 24; index++)
                    {
                        if (((svcMask >> index) & 1) == 0)
                        {
                            continue;
                        }

                        int svcId = baseSvc + index;

                        if (svcId > 0x7f)
                        {
                            return KernelResult.MaximumExceeded;
                        }

                        SvcAccessMask[svcId / 8] |= (byte)(1 << (svcId & 7));
                    }

                    break;
                }

                case 0x80:
                {
                    long address = ((long)(uint)cap << 4) & 0xffffff000;

                    memoryManager.MapIoMemory(address, KMemoryManager.PageSize, KMemoryPermission.ReadAndWrite);

                    break;
                }

                case 0x800:
                {
                    // TODO: GIC distributor check.
                    int irq0 = (cap >> 12) & 0x3ff;
                    int irq1 = (cap >> 22) & 0x3ff;

                    if (irq0 != 0x3ff)
                    {
                        IrqAccessMask[irq0 / 8] |= (byte)(1 << (irq0 & 7));
                    }

                    if (irq1 != 0x3ff)
                    {
                        IrqAccessMask[irq1 / 8] |= (byte)(1 << (irq1 & 7));
                    }

                    break;
                }

                case 0x2000:
                {
                    int applicationType = cap >> 14;

                    if ((uint)applicationType > 7)
                    {
                        return KernelResult.ReservedValue;
                    }

                    ApplicationType = applicationType;

                    break;
                }

                case 0x4000:
                {
                    // Note: This check is bugged on kernel too, we are just replicating the bug here.
                    if ((KernelReleaseVersion >> 17) != 0 || cap < 0x80000)
                    {
                        return KernelResult.ReservedValue;
                    }

                    KernelReleaseVersion = cap;

                    break;
                }

                case 0x8000:
                {
                    int handleTableSize = cap >> 26;

                    if ((uint)handleTableSize > 0x3ff)
                    {
                        return KernelResult.ReservedValue;
                    }

                    HandleTableSize = handleTableSize;

                    break;
                }

                case 0x10000:
                {
                    int debuggingFlags = cap >> 19;

                    if ((uint)debuggingFlags > 3)
                    {
                        return KernelResult.ReservedValue;
                    }

                    DebuggingFlags &= ~3;
                    DebuggingFlags |= debuggingFlags;

                    break;
                }

                default: return KernelResult.InvalidCapability;
            }

            return KernelResult.Success;
        }

        private static long GetMaskFromMinMax(int min, int max)
        {
            int range = max - min + 1;

            if (range == 64)
            {
                return -1L;
            }

            long mask = (1L << range) - 1;

            return mask << min;
        }
    }
}