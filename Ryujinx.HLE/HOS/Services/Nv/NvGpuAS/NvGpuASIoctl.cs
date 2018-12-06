using ChocolArm64.Memory;
using Ryujinx.Common.Logging;
using Ryujinx.Graphics.Memory;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.HOS.Services.Nv.NvMap;
using System;
using System.Collections.Concurrent;

namespace Ryujinx.HLE.HOS.Services.Nv.NvGpuAS
{
    class NvGpuASIoctl
    {
        private const int FlagFixedOffset = 1;

        private const int FlagRemapSubRange = 0x100;

        private static ConcurrentDictionary<KProcess, NvGpuASCtx> _asCtxs;

        static NvGpuASIoctl()
        {
            _asCtxs = new ConcurrentDictionary<KProcess, NvGpuASCtx>();
        }

        public static int ProcessIoctl(ServiceCtx context, int cmd)
        {
            switch (cmd & 0xffff)
            {
                case 0x4101: return BindChannel (context);
                case 0x4102: return AllocSpace  (context);
                case 0x4103: return FreeSpace   (context);
                case 0x4105: return UnmapBuffer (context);
                case 0x4106: return MapBufferEx (context);
                case 0x4108: return GetVaRegions(context);
                case 0x4109: return InitializeEx(context);
                case 0x4114: return Remap       (context, cmd);
            }

            throw new NotImplementedException(cmd.ToString("x8"));
        }

        private static int BindChannel(ServiceCtx context)
        {
            long inputPosition  = context.Request.GetBufferType0x21().Position;
            long outputPosition = context.Request.GetBufferType0x22().Position;

            Logger.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int AllocSpace(ServiceCtx context)
        {
            long inputPosition  = context.Request.GetBufferType0x21().Position;
            long outputPosition = context.Request.GetBufferType0x22().Position;

            NvGpuASAllocSpace args = MemoryHelper.Read<NvGpuASAllocSpace>(context.Memory, inputPosition);

            NvGpuASCtx asCtx = GetASCtx(context);

            ulong size = (ulong)args.Pages *
                         (ulong)args.PageSize;

            int result = NvResult.Success;

            lock (asCtx)
            {
                //Note: When the fixed offset flag is not set,
                //the Offset field holds the alignment size instead.
                if ((args.Flags & FlagFixedOffset) != 0)
                {
                    args.Offset = asCtx.Vmm.ReserveFixed(args.Offset, (long)size);
                }
                else
                {
                    args.Offset = asCtx.Vmm.Reserve((long)size, args.Offset);
                }

                if (args.Offset < 0)
                {
                    args.Offset = 0;

                    Logger.PrintWarning(LogClass.ServiceNv, $"Failed to allocate size {size:x16}!");

                    result = NvResult.OutOfMemory;
                }
                else
                {
                    asCtx.AddReservation(args.Offset, (long)size);
                }
            }

            MemoryHelper.Write(context.Memory, outputPosition, args);

            return result;
        }

        private static int FreeSpace(ServiceCtx context)
        {
            long inputPosition  = context.Request.GetBufferType0x21().Position;
            long outputPosition = context.Request.GetBufferType0x22().Position;

            NvGpuASAllocSpace args = MemoryHelper.Read<NvGpuASAllocSpace>(context.Memory, inputPosition);

            NvGpuASCtx asCtx = GetASCtx(context);

            int result = NvResult.Success;

            lock (asCtx)
            {
                ulong size = (ulong)args.Pages *
                             (ulong)args.PageSize;

                if (asCtx.RemoveReservation(args.Offset))
                {
                    asCtx.Vmm.Free(args.Offset, (long)size);
                }
                else
                {
                    Logger.PrintWarning(LogClass.ServiceNv,
                        $"Failed to free offset 0x{args.Offset:x16} size 0x{size:x16}!");

                    result = NvResult.InvalidInput;
                }
            }

            return result;
        }

        private static int UnmapBuffer(ServiceCtx context)
        {
            long inputPosition  = context.Request.GetBufferType0x21().Position;
            long outputPosition = context.Request.GetBufferType0x22().Position;

            NvGpuASUnmapBuffer args = MemoryHelper.Read<NvGpuASUnmapBuffer>(context.Memory, inputPosition);

            NvGpuASCtx asCtx = GetASCtx(context);

            lock (asCtx)
            {
                if (asCtx.RemoveMap(args.Offset, out long size))
                {
                    if (size != 0)
                    {
                        asCtx.Vmm.Free(args.Offset, size);
                    }
                }
                else
                {
                    Logger.PrintWarning(LogClass.ServiceNv, $"Invalid buffer offset {args.Offset:x16}!");
                }
            }

            return NvResult.Success;
        }

        private static int MapBufferEx(ServiceCtx context)
        {
            const string mapErrorMsg = "Failed to map fixed buffer with offset 0x{0:x16} and size 0x{1:x16}!";

            long inputPosition  = context.Request.GetBufferType0x21().Position;
            long outputPosition = context.Request.GetBufferType0x22().Position;

            NvGpuASMapBufferEx args = MemoryHelper.Read<NvGpuASMapBufferEx>(context.Memory, inputPosition);

            NvGpuASCtx asCtx = GetASCtx(context);

            NvMapHandle map = NvMapIoctl.GetNvMapWithFb(context, args.NvMapHandle);

            if (map == null)
            {
                Logger.PrintWarning(LogClass.ServiceNv, $"Invalid NvMap handle 0x{args.NvMapHandle:x8}!");

                return NvResult.InvalidInput;
            }

            long pa;

            if ((args.Flags & FlagRemapSubRange) != 0)
            {
                lock (asCtx)
                {
                    if (asCtx.TryGetMapPhysicalAddress(args.Offset, out pa))
                    {
                        long va = args.Offset + args.BufferOffset;

                        pa += args.BufferOffset;

                        if (asCtx.Vmm.Map(pa, va, args.MappingSize) < 0)
                        {
                            string msg = string.Format(mapErrorMsg, va, args.MappingSize);

                            Logger.PrintWarning(LogClass.ServiceNv, msg);

                            return NvResult.InvalidInput;
                        }

                        return NvResult.Success;
                    }
                    else
                    {
                        Logger.PrintWarning(LogClass.ServiceNv, $"Address 0x{args.Offset:x16} not mapped!");

                        return NvResult.InvalidInput;
                    }
                }
            }

            pa = map.Address + args.BufferOffset;

            long size = args.MappingSize;

            if (size == 0)
            {
                size = (uint)map.Size;
            }

            int result = NvResult.Success;

            lock (asCtx)
            {
                //Note: When the fixed offset flag is not set,
                //the Offset field holds the alignment size instead.
                bool vaAllocated = (args.Flags & FlagFixedOffset) == 0;

                if (!vaAllocated)
                {
                    if (asCtx.ValidateFixedBuffer(args.Offset, size))
                    {
                        args.Offset = asCtx.Vmm.Map(pa, args.Offset, size);
                    }
                    else
                    {
                        string msg = string.Format(mapErrorMsg, args.Offset, size);

                        Logger.PrintWarning(LogClass.ServiceNv, msg);

                        result = NvResult.InvalidInput;
                    }
                }
                else
                {
                    args.Offset = asCtx.Vmm.Map(pa, size);
                }

                if (args.Offset < 0)
                {
                    args.Offset = 0;

                    Logger.PrintWarning(LogClass.ServiceNv, $"Failed to map size 0x{size:x16}!");

                    result = NvResult.InvalidInput;
                }
                else
                {
                    asCtx.AddMap(args.Offset, size, pa, vaAllocated);
                }
            }

            MemoryHelper.Write(context.Memory, outputPosition, args);

            return result;
        }

        private static int GetVaRegions(ServiceCtx context)
        {
            long inputPosition  = context.Request.GetBufferType0x21().Position;
            long outputPosition = context.Request.GetBufferType0x22().Position;

            Logger.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int InitializeEx(ServiceCtx context)
        {
            long inputPosition  = context.Request.GetBufferType0x21().Position;
            long outputPosition = context.Request.GetBufferType0x22().Position;

            Logger.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int Remap(ServiceCtx context, int cmd)
        {
            int count = ((cmd >> 16) & 0xff) / 0x14;

            long inputPosition  = context.Request.GetBufferType0x21().Position;

            for (int index = 0; index < count; index++, inputPosition += 0x14)
            {
                NvGpuASRemap args = MemoryHelper.Read<NvGpuASRemap>(context.Memory, inputPosition);

                NvGpuVmm vmm = GetASCtx(context).Vmm;

                NvMapHandle map = NvMapIoctl.GetNvMapWithFb(context, args.NvMapHandle);

                if (map == null)
                {
                    Logger.PrintWarning(LogClass.ServiceNv, $"Invalid NvMap handle 0x{args.NvMapHandle:x8}!");

                    return NvResult.InvalidInput;
                }

                long result = vmm.Map(map.Address, (long)(uint)args.Offset << 16,
                                                   (long)(uint)args.Pages  << 16);

                if (result < 0)
                {
                    Logger.PrintWarning(LogClass.ServiceNv,
                        $"Page 0x{args.Offset:x16} size 0x{args.Pages:x16} not allocated!");

                    return NvResult.InvalidInput;
                }
            }

            return NvResult.Success;
        }

        public static NvGpuASCtx GetASCtx(ServiceCtx context)
        {
            return _asCtxs.GetOrAdd(context.Process, (key) => new NvGpuASCtx(context));
        }

        public static void UnloadProcess(KProcess process)
        {
            _asCtxs.TryRemove(process, out _);
        }
    }
}