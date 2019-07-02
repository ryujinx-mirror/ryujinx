using Ryujinx.HLE.HOS.Kernel.Memory;
using System;

namespace Ryujinx.HLE.HOS.Kernel.Common
{
    static class KernelInit
    {
        public static void InitializeResourceLimit(KResourceLimit resourceLimit)
        {
            void EnsureSuccess(KernelResult result)
            {
                if (result != KernelResult.Success)
                {
                    throw new InvalidOperationException($"Unexpected result \"{result}\".");
                }
            }

            int kernelMemoryCfg = 0;

            long ramSize = GetRamSize(kernelMemoryCfg);

            EnsureSuccess(resourceLimit.SetLimitValue(LimitableResource.Memory,         ramSize));
            EnsureSuccess(resourceLimit.SetLimitValue(LimitableResource.Thread,         800));
            EnsureSuccess(resourceLimit.SetLimitValue(LimitableResource.Event,          700));
            EnsureSuccess(resourceLimit.SetLimitValue(LimitableResource.TransferMemory, 200));
            EnsureSuccess(resourceLimit.SetLimitValue(LimitableResource.Session,        900));

            if (!resourceLimit.Reserve(LimitableResource.Memory, 0) ||
                !resourceLimit.Reserve(LimitableResource.Memory, 0x60000))
            {
                throw new InvalidOperationException("Unexpected failure reserving memory on resource limit.");
            }
        }

        public static KMemoryRegionManager[] GetMemoryRegions()
        {
            KMemoryArrange arrange = GetMemoryArrange();

            return new KMemoryRegionManager[]
            {
                GetMemoryRegion(arrange.Application),
                GetMemoryRegion(arrange.Applet),
                GetMemoryRegion(arrange.Service),
                GetMemoryRegion(arrange.NvServices)
            };
        }

        private static KMemoryRegionManager GetMemoryRegion(KMemoryArrangeRegion region)
        {
            return new KMemoryRegionManager(region.Address, region.Size, region.EndAddr);
        }

        private static KMemoryArrange GetMemoryArrange()
        {
            int mcEmemCfg = 0x1000;

            ulong ememApertureSize = (ulong)(mcEmemCfg & 0x3fff) << 20;

            int kernelMemoryCfg = 0;

            ulong ramSize = (ulong)GetRamSize(kernelMemoryCfg);

            ulong ramPart0;
            ulong ramPart1;

            if (ramSize * 2 > ememApertureSize)
            {
                ramPart0 = ememApertureSize / 2;
                ramPart1 = ememApertureSize / 2;
            }
            else
            {
                ramPart0 = ememApertureSize;
                ramPart1 = 0;
            }

            int memoryArrange = 1;

            ulong applicationRgSize;

            switch (memoryArrange)
            {
                case 2:    applicationRgSize = 0x80000000;  break;
                case 0x11:
                case 0x21: applicationRgSize = 0x133400000; break;
                default:   applicationRgSize = 0xcd500000;  break;
            }

            ulong appletRgSize;

            switch (memoryArrange)
            {
                case 2:    appletRgSize = 0x61200000; break;
                case 3:    appletRgSize = 0x1c000000; break;
                case 0x11: appletRgSize = 0x23200000; break;
                case 0x12:
                case 0x21: appletRgSize = 0x89100000; break;
                default:   appletRgSize = 0x1fb00000; break;
            }

            KMemoryArrangeRegion serviceRg;
            KMemoryArrangeRegion nvServicesRg;
            KMemoryArrangeRegion appletRg;
            KMemoryArrangeRegion applicationRg;

            const ulong nvServicesRgSize = 0x29ba000;

            ulong applicationRgEnd = DramMemoryMap.DramEnd; //- RamPart0;

            applicationRg = new KMemoryArrangeRegion(applicationRgEnd - applicationRgSize, applicationRgSize);

            ulong nvServicesRgEnd = applicationRg.Address - appletRgSize;

            nvServicesRg = new KMemoryArrangeRegion(nvServicesRgEnd - nvServicesRgSize, nvServicesRgSize);
            appletRg     = new KMemoryArrangeRegion(nvServicesRgEnd, appletRgSize);

            // Note: There is an extra region used by the kernel, however
            // since we are doing HLE we are not going to use that memory, so give all
            // the remaining memory space to services.
            ulong serviceRgSize = nvServicesRg.Address - DramMemoryMap.SlabHeapEnd;

            serviceRg = new KMemoryArrangeRegion(DramMemoryMap.SlabHeapEnd, serviceRgSize);

            return new KMemoryArrange(serviceRg, nvServicesRg, appletRg, applicationRg);
        }

        private static long GetRamSize(int kernelMemoryCfg)
        {
            switch ((kernelMemoryCfg >> 16) & 3)
            {
                case 1:  return 0x180000000;
                case 2:  return 0x200000000;
                default: return 0x100000000;
            }
        }
    }
}