using ChocolArm64.Memory;
using Ryujinx.Common.Logging;
using Ryujinx.Graphics.Memory;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.HOS.Services.Nv.NvGpuAS;
using Ryujinx.HLE.HOS.Services.Nv.NvMap;
using System;
using System.Collections.Concurrent;

namespace Ryujinx.HLE.HOS.Services.Nv.NvHostChannel
{
    class NvHostChannelIoctl
    {
        private static ConcurrentDictionary<KProcess, NvChannel> _channels;

        static NvHostChannelIoctl()
        {
            _channels = new ConcurrentDictionary<KProcess, NvChannel>();
        }

        public static int ProcessIoctl(ServiceCtx context, int cmd)
        {
            switch (cmd & 0xffff)
            {
                case 0x0001: return Submit           (context);
                case 0x0002: return GetSyncpoint     (context);
                case 0x0003: return GetWaitBase      (context);
                case 0x0009: return MapBuffer        (context);
                case 0x000a: return UnmapBuffer      (context);
                case 0x4714: return SetUserData      (context);
                case 0x4801: return SetNvMap         (context);
                case 0x4803: return SetTimeout       (context);
                case 0x4808: return SubmitGpfifo     (context);
                case 0x4809: return AllocObjCtx      (context);
                case 0x480b: return ZcullBind        (context);
                case 0x480c: return SetErrorNotifier (context);
                case 0x480d: return SetPriority      (context);
                case 0x481a: return AllocGpfifoEx2   (context);
                case 0x481b: return KickoffPbWithAttr(context);
            }

            throw new NotImplementedException(cmd.ToString("x8"));
        }

        private static int Submit(ServiceCtx context)
        {
            long inputPosition  = context.Request.GetBufferType0x21().Position;
            long outputPosition = context.Request.GetBufferType0x22().Position;

            NvHostChannelSubmit args = MemoryHelper.Read<NvHostChannelSubmit>(context.Memory, inputPosition);

            NvGpuVmm vmm = NvGpuASIoctl.GetASCtx(context).Vmm;

            for (int index = 0; index < args.CmdBufsCount; index++)
            {
                long cmdBufOffset = inputPosition + 0x10 + index * 0xc;

                NvHostChannelCmdBuf cmdBuf = MemoryHelper.Read<NvHostChannelCmdBuf>(context.Memory, cmdBufOffset);

                NvMapHandle map = NvMapIoctl.GetNvMap(context, cmdBuf.MemoryId);

                int[] cmdBufData = new int[cmdBuf.WordsCount];

                for (int offset = 0; offset < cmdBufData.Length; offset++)
                {
                    cmdBufData[offset] = context.Memory.ReadInt32(map.Address + cmdBuf.Offset + offset * 4);
                }

                context.Device.Gpu.PushCommandBuffer(vmm, cmdBufData);
            }

            //TODO: Relocation, waitchecks, etc.

            return NvResult.Success;
        }

        private static int GetSyncpoint(ServiceCtx context)
        {
            //TODO
            long inputPosition  = context.Request.GetBufferType0x21().Position;
            long outputPosition = context.Request.GetBufferType0x22().Position;

            NvHostChannelGetParamArg args = MemoryHelper.Read<NvHostChannelGetParamArg>(context.Memory, inputPosition);

            args.Value = 0;

            MemoryHelper.Write(context.Memory, outputPosition, args);

            return NvResult.Success;
        }

        private static int GetWaitBase(ServiceCtx context)
        {
            long inputPosition  = context.Request.GetBufferType0x21().Position;
            long outputPosition = context.Request.GetBufferType0x22().Position;

            NvHostChannelGetParamArg args = MemoryHelper.Read<NvHostChannelGetParamArg>(context.Memory, inputPosition);

            args.Value = 0;

            MemoryHelper.Write(context.Memory, outputPosition, args);

            return NvResult.Success;
        }

        private static int MapBuffer(ServiceCtx context)
        {
            long inputPosition  = context.Request.GetBufferType0x21().Position;
            long outputPosition = context.Request.GetBufferType0x22().Position;

            NvHostChannelMapBuffer args = MemoryHelper.Read<NvHostChannelMapBuffer>(context.Memory, inputPosition);

            NvGpuVmm vmm = NvGpuASIoctl.GetASCtx(context).Vmm;

            for (int index = 0; index < args.NumEntries; index++)
            {
                int handle = context.Memory.ReadInt32(inputPosition + 0xc + index * 8);

                NvMapHandle map = NvMapIoctl.GetNvMap(context, handle);

                if (map == null)
                {
                    Logger.PrintWarning(LogClass.ServiceNv, $"Invalid handle 0x{handle:x8}!");

                    return NvResult.InvalidInput;
                }

                lock (map)
                {
                    if (map.DmaMapAddress == 0)
                    {
                        map.DmaMapAddress = vmm.MapLow(map.Address, map.Size);
                    }

                    context.Memory.WriteInt32(outputPosition + 0xc + 4 + index * 8, (int)map.DmaMapAddress);
                }
            }

            return NvResult.Success;
        }

        private static int UnmapBuffer(ServiceCtx context)
        {
            long inputPosition  = context.Request.GetBufferType0x21().Position;

            NvHostChannelMapBuffer args = MemoryHelper.Read<NvHostChannelMapBuffer>(context.Memory, inputPosition);

            NvGpuVmm vmm = NvGpuASIoctl.GetASCtx(context).Vmm;

            for (int index = 0; index < args.NumEntries; index++)
            {
                int handle = context.Memory.ReadInt32(inputPosition + 0xc + index * 8);

                NvMapHandle map = NvMapIoctl.GetNvMap(context, handle);

                if (map == null)
                {
                    Logger.PrintWarning(LogClass.ServiceNv, $"Invalid handle 0x{handle:x8}!");

                    return NvResult.InvalidInput;
                }

                lock (map)
                {
                    if (map.DmaMapAddress != 0)
                    {
                        vmm.Free(map.DmaMapAddress, map.Size);

                        map.DmaMapAddress = 0;
                    }
                }
            }

            return NvResult.Success;
        }

        private static int SetUserData(ServiceCtx context)
        {
            long inputPosition  = context.Request.GetBufferType0x21().Position;
            long outputPosition = context.Request.GetBufferType0x22().Position;

            Logger.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int SetNvMap(ServiceCtx context)
        {
            long inputPosition  = context.Request.GetBufferType0x21().Position;
            long outputPosition = context.Request.GetBufferType0x22().Position;

            Logger.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int SetTimeout(ServiceCtx context)
        {
            long inputPosition = context.Request.GetBufferType0x21().Position;

            GetChannel(context).Timeout = context.Memory.ReadInt32(inputPosition);

            return NvResult.Success;
        }

        private static int SubmitGpfifo(ServiceCtx context)
        {
            long inputPosition  = context.Request.GetBufferType0x21().Position;
            long outputPosition = context.Request.GetBufferType0x22().Position;

            NvHostChannelSubmitGpfifo args = MemoryHelper.Read<NvHostChannelSubmitGpfifo>(context.Memory, inputPosition);

            NvGpuVmm vmm = NvGpuASIoctl.GetASCtx(context).Vmm;

            for (int index = 0; index < args.NumEntries; index++)
            {
                long gpfifo = context.Memory.ReadInt64(inputPosition + 0x18 + index * 8);

                PushGpfifo(context, vmm, gpfifo);
            }

            args.SyncptId    = 0;
            args.SyncptValue = 0;

            MemoryHelper.Write(context.Memory, outputPosition, args);

            return NvResult.Success;
        }

        private static int AllocObjCtx(ServiceCtx context)
        {
            long inputPosition  = context.Request.GetBufferType0x21().Position;
            long outputPosition = context.Request.GetBufferType0x22().Position;

            Logger.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int ZcullBind(ServiceCtx context)
        {
            long inputPosition  = context.Request.GetBufferType0x21().Position;
            long outputPosition = context.Request.GetBufferType0x22().Position;

            Logger.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int SetErrorNotifier(ServiceCtx context)
        {
            long inputPosition  = context.Request.GetBufferType0x21().Position;
            long outputPosition = context.Request.GetBufferType0x22().Position;

            Logger.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int SetPriority(ServiceCtx context)
        {
            long inputPosition  = context.Request.GetBufferType0x21().Position;
            long outputPosition = context.Request.GetBufferType0x22().Position;

            Logger.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int AllocGpfifoEx2(ServiceCtx context)
        {
            long inputPosition  = context.Request.GetBufferType0x21().Position;
            long outputPosition = context.Request.GetBufferType0x22().Position;

            Logger.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int KickoffPbWithAttr(ServiceCtx context)
        {
            long inputPosition  = context.Request.GetBufferType0x21().Position;
            long outputPosition = context.Request.GetBufferType0x22().Position;

            NvHostChannelSubmitGpfifo args = MemoryHelper.Read<NvHostChannelSubmitGpfifo>(context.Memory, inputPosition);

            NvGpuVmm vmm = NvGpuASIoctl.GetASCtx(context).Vmm;

            for (int index = 0; index < args.NumEntries; index++)
            {
                long gpfifo = context.Memory.ReadInt64(args.Address + index * 8);

                PushGpfifo(context, vmm, gpfifo);
            }

            args.SyncptId    = 0;
            args.SyncptValue = 0;

            MemoryHelper.Write(context.Memory, outputPosition, args);

            return NvResult.Success;
        }

        private static void PushGpfifo(ServiceCtx context, NvGpuVmm vmm, long gpfifo)
        {
            context.Device.Gpu.Pusher.Push(vmm, gpfifo);
        }

        public static NvChannel GetChannel(ServiceCtx context)
        {
            return _channels.GetOrAdd(context.Process, (key) => new NvChannel());
        }

        public static void UnloadProcess(KProcess process)
        {
            _channels.TryRemove(process, out _);
        }
    }
}