using ChocolArm64.Memory;
using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.Loaders.Executables;
using Ryujinx.HLE.Loaders.Npdm;

namespace Ryujinx.HLE.HOS
{
    class ProgramLoader
    {
        private const bool AslrEnabled = true;

        private const int ArgsHeaderSize = 8;
        private const int ArgsDataSize   = 0x9000;
        private const int ArgsTotalSize  = ArgsHeaderSize + ArgsDataSize;

        public static bool LoadKernelInitalProcess(Horizon System, KernelInitialProcess Kip)
        {
            int EndOffset = Kip.DataOffset + Kip.Data.Length;

            if (Kip.BssSize != 0)
            {
                EndOffset = Kip.BssOffset + Kip.BssSize;
            }

            int CodeSize = BitUtils.AlignUp(Kip.TextOffset + EndOffset, KMemoryManager.PageSize);

            int CodePagesCount = CodeSize / KMemoryManager.PageSize;

            ulong CodeBaseAddress = Kip.Addr39Bits ? 0x8000000UL : 0x200000UL;

            ulong CodeAddress = CodeBaseAddress + (ulong)Kip.TextOffset;

            int MmuFlags = 0;

            if (AslrEnabled)
            {
                //TODO: Randomization.

                MmuFlags |= 0x20;
            }

            if (Kip.Addr39Bits)
            {
                MmuFlags |= (int)AddressSpaceType.Addr39Bits << 1;
            }

            if (Kip.Is64Bits)
            {
                MmuFlags |= 1;
            }

            ProcessCreationInfo CreationInfo = new ProcessCreationInfo(
                Kip.Name,
                Kip.ProcessCategory,
                Kip.TitleId,
                CodeAddress,
                CodePagesCount,
                MmuFlags,
                0,
                0);

            MemoryRegion MemRegion = Kip.IsService
                ? MemoryRegion.Service
                : MemoryRegion.Application;

            KMemoryRegionManager Region = System.MemoryRegions[(int)MemRegion];

            KernelResult Result = Region.AllocatePages((ulong)CodePagesCount, false, out KPageList PageList);

            if (Result != KernelResult.Success)
            {
                Logger.PrintError(LogClass.Loader, $"Process initialization returned error \"{Result}\".");

                return false;
            }

            KProcess Process = new KProcess(System);

            Result = Process.InitializeKip(
                CreationInfo,
                Kip.Capabilities,
                PageList,
                System.ResourceLimit,
                MemRegion);

            if (Result != KernelResult.Success)
            {
                Logger.PrintError(LogClass.Loader, $"Process initialization returned error \"{Result}\".");

                return false;
            }

            Result = LoadIntoMemory(Process, Kip, CodeBaseAddress);

            if (Result != KernelResult.Success)
            {
                Logger.PrintError(LogClass.Loader, $"Process initialization returned error \"{Result}\".");

                return false;
            }

            Result = Process.Start(Kip.MainThreadPriority, (ulong)Kip.MainThreadStackSize);

            if (Result != KernelResult.Success)
            {
                Logger.PrintError(LogClass.Loader, $"Process start returned error \"{Result}\".");

                return false;
            }

            System.Processes.Add(Process.Pid, Process);

            return true;
        }

        public static bool LoadStaticObjects(
            Horizon       System,
            Npdm          MetaData,
            IExecutable[] StaticObjects,
            byte[]        Arguments = null)
        {
            ulong ArgsStart = 0;
            int   ArgsSize  = 0;
            ulong CodeStart = 0x8000000;
            int   CodeSize  = 0;

            ulong[] NsoBase = new ulong[StaticObjects.Length];

            for (int Index = 0; Index < StaticObjects.Length; Index++)
            {
                IExecutable StaticObject = StaticObjects[Index];

                int TextEnd = StaticObject.TextOffset + StaticObject.Text.Length;
                int ROEnd   = StaticObject.ROOffset   + StaticObject.RO.Length;
                int DataEnd = StaticObject.DataOffset + StaticObject.Data.Length + StaticObject.BssSize;

                int NsoSize = TextEnd;

                if ((uint)NsoSize < (uint)ROEnd)
                {
                    NsoSize = ROEnd;
                }

                if ((uint)NsoSize < (uint)DataEnd)
                {
                    NsoSize = DataEnd;
                }

                NsoSize = BitUtils.AlignUp(NsoSize, KMemoryManager.PageSize);

                NsoBase[Index] = CodeStart + (ulong)CodeSize;

                CodeSize += NsoSize;

                if (Arguments != null && ArgsSize == 0)
                {
                    ArgsStart = (ulong)CodeSize;

                    ArgsSize = BitUtils.AlignDown(Arguments.Length * 2 + ArgsTotalSize - 1, KMemoryManager.PageSize);

                    CodeSize += ArgsSize;
                }
            }

            int CodePagesCount = CodeSize / KMemoryManager.PageSize;

            int PersonalMmHeapPagesCount = MetaData.PersonalMmHeapSize / KMemoryManager.PageSize;

            ProcessCreationInfo CreationInfo = new ProcessCreationInfo(
                MetaData.TitleName,
                MetaData.ProcessCategory,
                MetaData.ACI0.TitleId,
                CodeStart,
                CodePagesCount,
                MetaData.MmuFlags,
                0,
                PersonalMmHeapPagesCount);

            KernelResult Result;

            KResourceLimit ResourceLimit = new KResourceLimit(System);

            long ApplicationRgSize = (long)System.MemoryRegions[(int)MemoryRegion.Application].Size;

            Result  = ResourceLimit.SetLimitValue(LimitableResource.Memory,         ApplicationRgSize);
            Result |= ResourceLimit.SetLimitValue(LimitableResource.Thread,         608);
            Result |= ResourceLimit.SetLimitValue(LimitableResource.Event,          700);
            Result |= ResourceLimit.SetLimitValue(LimitableResource.TransferMemory, 128);
            Result |= ResourceLimit.SetLimitValue(LimitableResource.Session,        894);

            if (Result != KernelResult.Success)
            {
                Logger.PrintError(LogClass.Loader, $"Process initialization failed setting resource limit values.");

                return false;
            }

            KProcess Process = new KProcess(System);

            Result = Process.Initialize(
                CreationInfo,
                MetaData.ACI0.KernelAccessControl.Capabilities,
                ResourceLimit,
                MemoryRegion.Application);

            if (Result != KernelResult.Success)
            {
                Logger.PrintError(LogClass.Loader, $"Process initialization returned error \"{Result}\".");

                return false;
            }

            for (int Index = 0; Index < StaticObjects.Length; Index++)
            {
                Logger.PrintInfo(LogClass.Loader, $"Loading image {Index} at 0x{NsoBase[Index]:x16}...");

                Result = LoadIntoMemory(Process, StaticObjects[Index], NsoBase[Index]);

                if (Result != KernelResult.Success)
                {
                    Logger.PrintError(LogClass.Loader, $"Process initialization returned error \"{Result}\".");

                    return false;
                }
            }

            Result = Process.Start(MetaData.MainThreadPriority, (ulong)MetaData.MainThreadStackSize);

            if (Result != KernelResult.Success)
            {
                Logger.PrintError(LogClass.Loader, $"Process start returned error \"{Result}\".");

                return false;
            }

            System.Processes.Add(Process.Pid, Process);

            return true;
        }

        private static KernelResult LoadIntoMemory(KProcess Process, IExecutable Image, ulong BaseAddress)
        {
            ulong TextStart = BaseAddress + (ulong)Image.TextOffset;
            ulong ROStart   = BaseAddress + (ulong)Image.ROOffset;
            ulong DataStart = BaseAddress + (ulong)Image.DataOffset;
            ulong BssStart  = BaseAddress + (ulong)Image.BssOffset;

            ulong End = DataStart + (ulong)Image.Data.Length;

            if (Image.BssSize != 0)
            {
                End = BssStart + (ulong)Image.BssSize;
            }

            Process.CpuMemory.WriteBytes((long)TextStart, Image.Text);
            Process.CpuMemory.WriteBytes((long)ROStart,   Image.RO);
            Process.CpuMemory.WriteBytes((long)DataStart, Image.Data);

            MemoryHelper.FillWithZeros(Process.CpuMemory, (long)BssStart, Image.BssSize);

            KernelResult SetProcessMemoryPermission(ulong Address, ulong Size, MemoryPermission Permission)
            {
                if (Size == 0)
                {
                    return KernelResult.Success;
                }

                Size = BitUtils.AlignUp(Size, KMemoryManager.PageSize);

                return Process.MemoryManager.SetProcessMemoryPermission(Address, Size, Permission);
            }

            KernelResult Result = SetProcessMemoryPermission(TextStart, (ulong)Image.Text.Length, MemoryPermission.ReadAndExecute);

            if (Result != KernelResult.Success)
            {
                return Result;
            }

            Result = SetProcessMemoryPermission(ROStart, (ulong)Image.RO.Length, MemoryPermission.Read);

            if (Result != KernelResult.Success)
            {
                return Result;
            }

            return SetProcessMemoryPermission(DataStart, End - DataStart, MemoryPermission.ReadAndWrite);
        }
    }
}