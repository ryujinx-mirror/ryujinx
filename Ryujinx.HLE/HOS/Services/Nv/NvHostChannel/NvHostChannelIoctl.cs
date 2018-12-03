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
        private static ConcurrentDictionary<KProcess, NvChannel> Channels;

        static NvHostChannelIoctl()
        {
            Channels = new ConcurrentDictionary<KProcess, NvChannel>();
        }

        public static int ProcessIoctl(ServiceCtx Context, int Cmd)
        {
            switch (Cmd & 0xffff)
            {
                case 0x0001: return Submit           (Context);
                case 0x0002: return GetSyncpoint     (Context);
                case 0x0003: return GetWaitBase      (Context);
                case 0x0009: return MapBuffer        (Context);
                case 0x000a: return UnmapBuffer      (Context);
                case 0x4714: return SetUserData      (Context);
                case 0x4801: return SetNvMap         (Context);
                case 0x4803: return SetTimeout       (Context);
                case 0x4808: return SubmitGpfifo     (Context);
                case 0x4809: return AllocObjCtx      (Context);
                case 0x480b: return ZcullBind        (Context);
                case 0x480c: return SetErrorNotifier (Context);
                case 0x480d: return SetPriority      (Context);
                case 0x481a: return AllocGpfifoEx2   (Context);
                case 0x481b: return KickoffPbWithAttr(Context);
            }

            throw new NotImplementedException(Cmd.ToString("x8"));
        }

        private static int Submit(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            NvHostChannelSubmit Args = MemoryHelper.Read<NvHostChannelSubmit>(Context.Memory, InputPosition);

            NvGpuVmm Vmm = NvGpuASIoctl.GetASCtx(Context).Vmm;

            for (int Index = 0; Index < Args.CmdBufsCount; Index++)
            {
                long CmdBufOffset = InputPosition + 0x10 + Index * 0xc;

                NvHostChannelCmdBuf CmdBuf = MemoryHelper.Read<NvHostChannelCmdBuf>(Context.Memory, CmdBufOffset);

                NvMapHandle Map = NvMapIoctl.GetNvMap(Context, CmdBuf.MemoryId);

                int[] CmdBufData = new int[CmdBuf.WordsCount];

                for (int Offset = 0; Offset < CmdBufData.Length; Offset++)
                {
                    CmdBufData[Offset] = Context.Memory.ReadInt32(Map.Address + CmdBuf.Offset + Offset * 4);
                }

                Context.Device.Gpu.PushCommandBuffer(Vmm, CmdBufData);
            }

            //TODO: Relocation, waitchecks, etc.

            return NvResult.Success;
        }

        private static int GetSyncpoint(ServiceCtx Context)
        {
            //TODO
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            NvHostChannelGetParamArg Args = MemoryHelper.Read<NvHostChannelGetParamArg>(Context.Memory, InputPosition);

            Args.Value = 0;

            MemoryHelper.Write(Context.Memory, OutputPosition, Args);

            return NvResult.Success;
        }

        private static int GetWaitBase(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            NvHostChannelGetParamArg Args = MemoryHelper.Read<NvHostChannelGetParamArg>(Context.Memory, InputPosition);

            Args.Value = 0;

            MemoryHelper.Write(Context.Memory, OutputPosition, Args);

            return NvResult.Success;
        }

        private static int MapBuffer(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            NvHostChannelMapBuffer Args = MemoryHelper.Read<NvHostChannelMapBuffer>(Context.Memory, InputPosition);

            NvGpuVmm Vmm = NvGpuASIoctl.GetASCtx(Context).Vmm;

            for (int Index = 0; Index < Args.NumEntries; Index++)
            {
                int Handle = Context.Memory.ReadInt32(InputPosition + 0xc + Index * 8);

                NvMapHandle Map = NvMapIoctl.GetNvMap(Context, Handle);

                if (Map == null)
                {
                    Logger.PrintWarning(LogClass.ServiceNv, $"Invalid handle 0x{Handle:x8}!");

                    return NvResult.InvalidInput;
                }

                lock (Map)
                {
                    if (Map.DmaMapAddress == 0)
                    {
                        Map.DmaMapAddress = Vmm.MapLow(Map.Address, Map.Size);
                    }

                    Context.Memory.WriteInt32(OutputPosition + 0xc + 4 + Index * 8, (int)Map.DmaMapAddress);
                }
            }

            return NvResult.Success;
        }

        private static int UnmapBuffer(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;

            NvHostChannelMapBuffer Args = MemoryHelper.Read<NvHostChannelMapBuffer>(Context.Memory, InputPosition);

            NvGpuVmm Vmm = NvGpuASIoctl.GetASCtx(Context).Vmm;

            for (int Index = 0; Index < Args.NumEntries; Index++)
            {
                int Handle = Context.Memory.ReadInt32(InputPosition + 0xc + Index * 8);

                NvMapHandle Map = NvMapIoctl.GetNvMap(Context, Handle);

                if (Map == null)
                {
                    Logger.PrintWarning(LogClass.ServiceNv, $"Invalid handle 0x{Handle:x8}!");

                    return NvResult.InvalidInput;
                }

                lock (Map)
                {
                    if (Map.DmaMapAddress != 0)
                    {
                        Vmm.Free(Map.DmaMapAddress, Map.Size);

                        Map.DmaMapAddress = 0;
                    }
                }
            }

            return NvResult.Success;
        }

        private static int SetUserData(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            Logger.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int SetNvMap(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            Logger.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int SetTimeout(ServiceCtx Context)
        {
            long InputPosition = Context.Request.GetBufferType0x21().Position;

            GetChannel(Context).Timeout = Context.Memory.ReadInt32(InputPosition);

            return NvResult.Success;
        }

        private static int SubmitGpfifo(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            NvHostChannelSubmitGpfifo Args = MemoryHelper.Read<NvHostChannelSubmitGpfifo>(Context.Memory, InputPosition);

            NvGpuVmm Vmm = NvGpuASIoctl.GetASCtx(Context).Vmm;;

            for (int Index = 0; Index < Args.NumEntries; Index++)
            {
                long Gpfifo = Context.Memory.ReadInt64(InputPosition + 0x18 + Index * 8);

                PushGpfifo(Context, Vmm, Gpfifo);
            }

            Args.SyncptId    = 0;
            Args.SyncptValue = 0;

            MemoryHelper.Write(Context.Memory, OutputPosition, Args);

            return NvResult.Success;
        }

        private static int AllocObjCtx(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            Logger.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int ZcullBind(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            Logger.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int SetErrorNotifier(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            Logger.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int SetPriority(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            Logger.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int AllocGpfifoEx2(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            Logger.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int KickoffPbWithAttr(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            NvHostChannelSubmitGpfifo Args = MemoryHelper.Read<NvHostChannelSubmitGpfifo>(Context.Memory, InputPosition);

            NvGpuVmm Vmm = NvGpuASIoctl.GetASCtx(Context).Vmm;;

            for (int Index = 0; Index < Args.NumEntries; Index++)
            {
                long Gpfifo = Context.Memory.ReadInt64(Args.Address + Index * 8);

                PushGpfifo(Context, Vmm, Gpfifo);
            }

            Args.SyncptId    = 0;
            Args.SyncptValue = 0;

            MemoryHelper.Write(Context.Memory, OutputPosition, Args);

            return NvResult.Success;
        }

        private static void PushGpfifo(ServiceCtx Context, NvGpuVmm Vmm, long Gpfifo)
        {
            Context.Device.Gpu.Pusher.Push(Vmm, Gpfifo);
        }

        public static NvChannel GetChannel(ServiceCtx Context)
        {
            return Channels.GetOrAdd(Context.Process, (Key) => new NvChannel());
        }

        public static void UnloadProcess(KProcess Process)
        {
            Channels.TryRemove(Process, out _);
        }
    }
}