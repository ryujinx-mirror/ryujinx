using System;

namespace Ryujinx.HLE.HOS.Kernel
{
    static class KernelInit
    {
        public static void InitializeResourceLimit(KResourceLimit ResourceLimit)
        {
            void EnsureSuccess(KernelResult Result)
            {
                if (Result != KernelResult.Success)
                {
                    throw new InvalidOperationException($"Unexpected result \"{Result}\".");
                }
            }

            int KernelMemoryCfg = 0;

            long RamSize = GetRamSize(KernelMemoryCfg);

            EnsureSuccess(ResourceLimit.SetLimitValue(LimitableResource.Memory,         RamSize));
            EnsureSuccess(ResourceLimit.SetLimitValue(LimitableResource.Thread,         800));
            EnsureSuccess(ResourceLimit.SetLimitValue(LimitableResource.Event,          700));
            EnsureSuccess(ResourceLimit.SetLimitValue(LimitableResource.TransferMemory, 200));
            EnsureSuccess(ResourceLimit.SetLimitValue(LimitableResource.Session,        900));

            if (!ResourceLimit.Reserve(LimitableResource.Memory, 0) ||
                !ResourceLimit.Reserve(LimitableResource.Memory, 0x60000))
            {
                throw new InvalidOperationException("Unexpected failure reserving memory on resource limit.");
            }
        }

        public static KMemoryRegionManager[] GetMemoryRegions()
        {
            KMemoryArrange Arrange = GetMemoryArrange();

            return new KMemoryRegionManager[]
            {
                GetMemoryRegion(Arrange.Application),
                GetMemoryRegion(Arrange.Applet),
                GetMemoryRegion(Arrange.Service),
                GetMemoryRegion(Arrange.NvServices)
            };
        }

        private static KMemoryRegionManager GetMemoryRegion(KMemoryArrangeRegion Region)
        {
            return new KMemoryRegionManager(Region.Address, Region.Size, Region.EndAddr);
        }

        private static KMemoryArrange GetMemoryArrange()
        {
            int McEmemCfg = 0x1000;

            ulong EmemApertureSize = (ulong)(McEmemCfg & 0x3fff) << 20;

            int KernelMemoryCfg = 0;

            ulong RamSize = (ulong)GetRamSize(KernelMemoryCfg);

            ulong RamPart0;
            ulong RamPart1;

            if (RamSize * 2 > EmemApertureSize)
            {
                RamPart0 = EmemApertureSize / 2;
                RamPart1 = EmemApertureSize / 2;
            }
            else
            {
                RamPart0 = EmemApertureSize;
                RamPart1 = 0;
            }

            int MemoryArrange = 1;

            ulong ApplicationRgSize;

            switch (MemoryArrange)
            {
                case 2:    ApplicationRgSize = 0x80000000;  break;
                case 0x11:
                case 0x21: ApplicationRgSize = 0x133400000; break;
                default:   ApplicationRgSize = 0xcd500000;  break;
            }

            ulong AppletRgSize;

            switch (MemoryArrange)
            {
                case 2:    AppletRgSize = 0x61200000; break;
                case 3:    AppletRgSize = 0x1c000000; break;
                case 0x11: AppletRgSize = 0x23200000; break;
                case 0x12:
                case 0x21: AppletRgSize = 0x89100000; break;
                default:   AppletRgSize = 0x1fb00000; break;
            }

            KMemoryArrangeRegion ServiceRg;
            KMemoryArrangeRegion NvServicesRg;
            KMemoryArrangeRegion AppletRg;
            KMemoryArrangeRegion ApplicationRg;

            const ulong NvServicesRgSize = 0x29ba000;

            ulong ApplicationRgEnd = DramMemoryMap.DramEnd; //- RamPart0;

            ApplicationRg = new KMemoryArrangeRegion(ApplicationRgEnd - ApplicationRgSize, ApplicationRgSize);

            ulong NvServicesRgEnd = ApplicationRg.Address - AppletRgSize;

            NvServicesRg = new KMemoryArrangeRegion(NvServicesRgEnd - NvServicesRgSize, NvServicesRgSize);
            AppletRg     = new KMemoryArrangeRegion(NvServicesRgEnd, AppletRgSize);

            //Note: There is an extra region used by the kernel, however
            //since we are doing HLE we are not going to use that memory, so give all
            //the remaining memory space to services.
            ulong ServiceRgSize = NvServicesRg.Address - DramMemoryMap.SlabHeapEnd;

            ServiceRg = new KMemoryArrangeRegion(DramMemoryMap.SlabHeapEnd, ServiceRgSize);

            return new KMemoryArrange(ServiceRg, NvServicesRg, AppletRg, ApplicationRg);
        }

        private static long GetRamSize(int KernelMemoryCfg)
        {
            switch ((KernelMemoryCfg >> 16) & 3)
            {
                case 1:  return 0x180000000;
                case 2:  return 0x200000000;
                default: return 0x100000000;
            }
        }
    }
}