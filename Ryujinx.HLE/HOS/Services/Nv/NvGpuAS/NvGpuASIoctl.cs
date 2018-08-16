using ChocolArm64.Memory;
using Ryujinx.HLE.Gpu.Memory;
using Ryujinx.HLE.HOS.Services.Nv.NvMap;
using Ryujinx.HLE.Logging;
using System;
using System.Collections.Concurrent;

namespace Ryujinx.HLE.HOS.Services.Nv.NvGpuAS
{
    class NvGpuASIoctl
    {
        private const int FlagFixedOffset = 1;

        private const int FlagRemapSubRange = 0x100;

        private static ConcurrentDictionary<Process, NvGpuASCtx> ASCtxs;

        static NvGpuASIoctl()
        {
            ASCtxs = new ConcurrentDictionary<Process, NvGpuASCtx>();
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
                case 0x4114: return Remap       (Context, Cmd);
            }

            throw new NotImplementedException(Cmd.ToString("x8"));
        }

        private static int BindChannel(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            Context.Device.Log.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int AllocSpace(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            NvGpuASAllocSpace Args = AMemoryHelper.Read<NvGpuASAllocSpace>(Context.Memory, InputPosition);

            NvGpuASCtx ASCtx = GetASCtx(Context);

            ulong Size = (ulong)Args.Pages *
                         (ulong)Args.PageSize;

            int Result = NvResult.Success;

            lock (ASCtx)
            {
                //Note: When the fixed offset flag is not set,
                //the Offset field holds the alignment size instead.
                if ((Args.Flags & FlagFixedOffset) != 0)
                {
                    Args.Offset = ASCtx.Vmm.ReserveFixed(Args.Offset, (long)Size);
                }
                else
                {
                    Args.Offset = ASCtx.Vmm.Reserve((long)Size, Args.Offset);
                }

                if (Args.Offset < 0)
                {
                    Args.Offset = 0;

                    Context.Device.Log.PrintWarning(LogClass.ServiceNv, $"Failed to allocate size {Size:x16}!");

                    Result = NvResult.OutOfMemory;
                }
                else
                {
                    ASCtx.AddReservation(Args.Offset, (long)Size);
                }
            }

            AMemoryHelper.Write(Context.Memory, OutputPosition, Args);

            return Result;
        }

        private static int FreeSpace(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            NvGpuASAllocSpace Args = AMemoryHelper.Read<NvGpuASAllocSpace>(Context.Memory, InputPosition);

            NvGpuASCtx ASCtx = GetASCtx(Context);

            int Result = NvResult.Success;

            lock (ASCtx)
            {
                ulong Size = (ulong)Args.Pages *
                             (ulong)Args.PageSize;

                if (ASCtx.RemoveReservation(Args.Offset))
                {
                    ASCtx.Vmm.Free(Args.Offset, (long)Size);
                }
                else
                {
                    Context.Device.Log.PrintWarning(LogClass.ServiceNv,
                        $"Failed to free offset 0x{Args.Offset:x16} size 0x{Size:x16}!");

                    Result = NvResult.InvalidInput;
                }
            }

            return Result;
        }

        private static int UnmapBuffer(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            NvGpuASUnmapBuffer Args = AMemoryHelper.Read<NvGpuASUnmapBuffer>(Context.Memory, InputPosition);

            NvGpuASCtx ASCtx = GetASCtx(Context);

            lock (ASCtx)
            {
                if (ASCtx.RemoveMap(Args.Offset, out long Size))
                {
                    if (Size != 0)
                    {
                        ASCtx.Vmm.Free(Args.Offset, Size);
                    }
                }
                else
                {
                    Context.Device.Log.PrintWarning(LogClass.ServiceNv, $"Invalid buffer offset {Args.Offset:x16}!");
                }
            }

            return NvResult.Success;
        }

        private static int MapBufferEx(ServiceCtx Context)
        {
            const string MapErrorMsg = "Failed to map fixed buffer with offset 0x{0:x16} and size 0x{1:x16}!";

            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            NvGpuASMapBufferEx Args = AMemoryHelper.Read<NvGpuASMapBufferEx>(Context.Memory, InputPosition);

            NvGpuASCtx ASCtx = GetASCtx(Context);

            NvMapHandle Map = NvMapIoctl.GetNvMapWithFb(Context, Args.NvMapHandle);

            if (Map == null)
            {
                Context.Device.Log.PrintWarning(LogClass.ServiceNv, $"Invalid NvMap handle 0x{Args.NvMapHandle:x8}!");

                return NvResult.InvalidInput;
            }

            long PA;

            if ((Args.Flags & FlagRemapSubRange) != 0)
            {
                lock (ASCtx)
                {
                    if (ASCtx.TryGetMapPhysicalAddress(Args.Offset, out PA))
                    {
                        long VA = Args.Offset + Args.BufferOffset;

                        PA += Args.BufferOffset;

                        if (ASCtx.Vmm.Map(PA, VA, Args.MappingSize) < 0)
                        {
                            string Msg = string.Format(MapErrorMsg, VA, Args.MappingSize);

                            Context.Device.Log.PrintWarning(LogClass.ServiceNv, Msg);

                            return NvResult.InvalidInput;
                        }

                        return NvResult.Success;
                    }
                    else
                    {
                        Context.Device.Log.PrintWarning(LogClass.ServiceNv, $"Address 0x{Args.Offset:x16} not mapped!");

                        return NvResult.InvalidInput;
                    }
                }
            }

            PA = Map.Address + Args.BufferOffset;

            long Size = Args.MappingSize;

            if (Size == 0)
            {
                Size = (uint)Map.Size;
            }

            int Result = NvResult.Success;

            lock (ASCtx)
            {
                //Note: When the fixed offset flag is not set,
                //the Offset field holds the alignment size instead.
                bool VaAllocated = (Args.Flags & FlagFixedOffset) == 0;

                if (!VaAllocated)
                {
                    if (ASCtx.ValidateFixedBuffer(Args.Offset, Size))
                    {
                        Args.Offset = ASCtx.Vmm.Map(PA, Args.Offset, Size);
                    }
                    else
                    {
                        string Msg = string.Format(MapErrorMsg, Args.Offset, Size);

                        Context.Device.Log.PrintWarning(LogClass.ServiceNv, Msg);

                        Result = NvResult.InvalidInput;
                    }
                }
                else
                {
                    Args.Offset = ASCtx.Vmm.Map(PA, Size);
                }

                if (Args.Offset < 0)
                {
                    Args.Offset = 0;

                    Context.Device.Log.PrintWarning(LogClass.ServiceNv, $"Failed to map size 0x{Size:x16}!");

                    Result = NvResult.InvalidInput;
                }
                else
                {
                    ASCtx.AddMap(Args.Offset, Size, PA, VaAllocated);
                }
            }

            AMemoryHelper.Write(Context.Memory, OutputPosition, Args);

            return Result;
        }

        private static int GetVaRegions(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            Context.Device.Log.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int InitializeEx(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            Context.Device.Log.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int Remap(ServiceCtx Context, int Cmd)
        {
            int Count = ((Cmd >> 16) & 0xff) / 0x14;

            long InputPosition  = Context.Request.GetBufferType0x21().Position;

            for (int Index = 0; Index < Count; Index++, InputPosition += 0x14)
            {
                NvGpuASRemap Args = AMemoryHelper.Read<NvGpuASRemap>(Context.Memory, InputPosition);

                NvGpuVmm Vmm = GetASCtx(Context).Vmm;

                NvMapHandle Map = NvMapIoctl.GetNvMapWithFb(Context, Args.NvMapHandle);

                if (Map == null)
                {
                    Context.Device.Log.PrintWarning(LogClass.ServiceNv, $"Invalid NvMap handle 0x{Args.NvMapHandle:x8}!");

                    return NvResult.InvalidInput;
                }

                long Result = Vmm.Map(Map.Address, (long)(uint)Args.Offset << 16,
                                                   (long)(uint)Args.Pages  << 16);

                if (Result < 0)
                {
                    Context.Device.Log.PrintWarning(LogClass.ServiceNv,
                        $"Page 0x{Args.Offset:x16} size 0x{Args.Pages:x16} not allocated!");

                    return NvResult.InvalidInput;
                }
            }

            return NvResult.Success;
        }

        public static NvGpuASCtx GetASCtx(ServiceCtx Context)
        {
            return ASCtxs.GetOrAdd(Context.Process, (Key) => new NvGpuASCtx(Context));
        }

        public static void UnloadProcess(Process Process)
        {
            ASCtxs.TryRemove(Process, out _);
        }
    }
}