using ChocolArm64.Memory;
using Ryujinx.HLE.Gpu;
using Ryujinx.HLE.Logging;
using Ryujinx.HLE.OsHle.Services.Nv.NvMap;
using System;
using System.Collections.Concurrent;

namespace Ryujinx.HLE.OsHle.Services.Nv.NvGpuAS
{
    class NvGpuASIoctl
    {
        private const int FlagFixedOffset = 1;

        private static ConcurrentDictionary<Process, NvGpuVmm> Vmms;

        static NvGpuASIoctl()
        {
            Vmms = new ConcurrentDictionary<Process, NvGpuVmm>();
        }

        public static int ProcessIoctl(ServiceCtx Context, int Cmd)
        {
            switch (Cmd & 0xffff)
            {
                case 0x4101: return BindChannel (Context);
                case 0x4102: return AllocSpace  (Context);
                case 0x4103: return FreeSpace   (Context);
                case 0x4105: return UnmapBuffer (Context);
                case 0x4106: return MapBufferEx (Context);
                case 0x4108: return GetVaRegions(Context);
                case 0x4109: return InitializeEx(Context);
                case 0x4114: return Remap       (Context);
            }

            throw new NotImplementedException(Cmd.ToString("x8"));
        }

        private static int BindChannel(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            Context.Ns.Log.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int AllocSpace(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            NvGpuASAllocSpace Args = AMemoryHelper.Read<NvGpuASAllocSpace>(Context.Memory, InputPosition);

            NvGpuVmm Vmm = GetVmm(Context);

            ulong Size = (ulong)Args.Pages *
                         (ulong)Args.PageSize;

            if ((Args.Flags & FlagFixedOffset) != 0)
            {
                Args.Offset = Vmm.Reserve(Args.Offset, (long)Size, 1);
            }
            else
            {
                Args.Offset = Vmm.Reserve((long)Size, 1);
            }

            int Result = NvResult.Success;

            if (Args.Offset < 0)
            {
                Args.Offset = 0;

                Context.Ns.Log.PrintWarning(LogClass.ServiceNv, $"No memory to allocate size {Size:x16}!");

                Result = NvResult.OutOfMemory;
            }

            AMemoryHelper.Write(Context.Memory, OutputPosition, Args);

            return Result;
        }

        private static int FreeSpace(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            NvGpuASAllocSpace Args = AMemoryHelper.Read<NvGpuASAllocSpace>(Context.Memory, InputPosition);

            NvGpuVmm Vmm = GetVmm(Context);

            ulong Size = (ulong)Args.Pages *
                         (ulong)Args.PageSize;

            Vmm.Free(Args.Offset, (long)Size);

            return NvResult.Success;
        }

        private static int UnmapBuffer(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            NvGpuASUnmapBuffer Args = AMemoryHelper.Read<NvGpuASUnmapBuffer>(Context.Memory, InputPosition);

            NvGpuVmm Vmm = GetVmm(Context);

            if (!Vmm.Unmap(Args.Offset))
            {
                Context.Ns.Log.PrintWarning(LogClass.ServiceNv, $"Invalid buffer offset {Args.Offset:x16}!");
            }

            return NvResult.Success;
        }

        private static int MapBufferEx(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            NvGpuASMapBufferEx Args = AMemoryHelper.Read<NvGpuASMapBufferEx>(Context.Memory, InputPosition);

            NvGpuVmm Vmm = GetVmm(Context);

            NvMapHandle Map = NvMapIoctl.GetNvMapWithFb(Context, Args.NvMapHandle);

            if (Map == null)
            {
                Context.Ns.Log.PrintWarning(LogClass.ServiceNv, $"Invalid NvMap handle 0x{Args.NvMapHandle:x8}!");

                return NvResult.InvalidInput;
            }

            long PA = Map.Address + Args.BufferOffset;

            long Size = Args.MappingSize;

            if (Size == 0)
            {
                Size = (uint)Map.Size;
            }

            int Result = NvResult.Success;

            //Note: When the fixed offset flag is not set,
            //the Offset field holds the alignment size instead.
            if ((Args.Flags & FlagFixedOffset) != 0)
            {
                long MapEnd = Args.Offset + Args.MappingSize;

                if ((ulong)MapEnd <= (ulong)Args.Offset)
                {
                    Context.Ns.Log.PrintWarning(LogClass.ServiceNv, $"Offset 0x{Args.Offset:x16} and size 0x{Args.MappingSize:x16} results in a overflow!");

                    return NvResult.InvalidInput;
                }

                if ((Args.Offset & NvGpuVmm.PageMask) != 0)
                {
                    Context.Ns.Log.PrintWarning(LogClass.ServiceNv, $"Offset 0x{Args.Offset:x16} is not page aligned!");

                    return NvResult.InvalidInput;
                }

                Args.Offset = Vmm.Map(PA, Args.Offset, Size);
            }
            else
            {
                Args.Offset = Vmm.Map(PA, Size);

                if (Args.Offset < 0)
                {
                    Args.Offset = 0;

                    Context.Ns.Log.PrintWarning(LogClass.ServiceNv, $"No memory to map size {Args.MappingSize:x16}!");

                    Result = NvResult.InvalidInput;
                }
            }

            AMemoryHelper.Write(Context.Memory, OutputPosition, Args);

            return Result;
        }

        private static int GetVaRegions(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            Context.Ns.Log.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int InitializeEx(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            Context.Ns.Log.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int Remap(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;

            NvGpuASRemap Args = AMemoryHelper.Read<NvGpuASRemap>(Context.Memory, InputPosition);

            NvGpuVmm Vmm = GetVmm(Context);

            NvMapHandle Map = NvMapIoctl.GetNvMapWithFb(Context, Args.NvMapHandle);

            if (Map == null)
            {
                Context.Ns.Log.PrintWarning(LogClass.ServiceNv, $"Invalid NvMap handle 0x{Args.NvMapHandle:x8}!");

                return NvResult.InvalidInput;
            }

            //FIXME: This is most likely wrong...
            Vmm.Map(Map.Address, (long)(uint)Args.Offset << 16,
                                 (long)(uint)Args.Pages  << 16);

            return NvResult.Success;
        }

        public static NvGpuVmm GetVmm(ServiceCtx Context)
        {
            return Vmms.GetOrAdd(Context.Process, (Key) => new NvGpuVmm(Context.Memory));
        }

        public static void UnloadProcess(Process Process)
        {
            Vmms.TryRemove(Process, out _);
        }
    }
}