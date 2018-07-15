using ChocolArm64.Memory;
using Ryujinx.HLE.Gpu.Memory;
using Ryujinx.HLE.Logging;
using Ryujinx.HLE.OsHle.Services.Nv.NvGpuAS;
using System;
using System.Collections.Concurrent;

namespace Ryujinx.HLE.OsHle.Services.Nv.NvHostChannel
{
    class NvHostChannelIoctl
    {
        private class ChannelsPerProcess
        {
            public ConcurrentDictionary<NvChannelName, NvChannel> Channels { get; private set; }

            public ChannelsPerProcess()
            {
                Channels = new ConcurrentDictionary<NvChannelName, NvChannel>();

                Channels.TryAdd(NvChannelName.Gpu, new NvChannel());
            }
        }

        private static ConcurrentDictionary<Process, ChannelsPerProcess> Channels;

        static NvHostChannelIoctl()
        {
            Channels = new ConcurrentDictionary<Process, ChannelsPerProcess>();
        }

        public static int ProcessIoctlGpu(ServiceCtx Context, int Cmd)
        {
            return ProcessIoctl(Context, NvChannelName.Gpu, Cmd);
        }

        public static int ProcessIoctl(ServiceCtx Context, NvChannelName Channel, int Cmd)
        {
            switch (Cmd & 0xffff)
            {
                case 0x4714: return SetUserData      (Context);
                case 0x4801: return SetNvMap         (Context);
                case 0x4803: return SetTimeout       (Context, Channel);
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

        private static int SetUserData(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            Context.Ns.Log.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int SetNvMap(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            Context.Ns.Log.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int SetTimeout(ServiceCtx Context, NvChannelName Channel)
        {
            long InputPosition = Context.Request.GetBufferType0x21().Position;

            GetChannel(Context, Channel).Timeout = Context.Memory.ReadInt32(InputPosition);

            return NvResult.Success;
        }

        private static int SubmitGpfifo(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            NvHostChannelSubmitGpfifo Args = AMemoryHelper.Read<NvHostChannelSubmitGpfifo>(Context.Memory, InputPosition);

            NvGpuVmm Vmm = NvGpuASIoctl.GetVmm(Context);

            for (int Index = 0; Index < Args.NumEntries; Index++)
            {
                long Gpfifo = Context.Memory.ReadInt64(InputPosition + 0x18 + Index * 8);

                PushGpfifo(Context, Vmm, Gpfifo);
            }

            Args.SyncptId    = 0;
            Args.SyncptValue = 0;

            AMemoryHelper.Write(Context.Memory, OutputPosition, Args);

            return NvResult.Success;
        }

        private static int AllocObjCtx(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            Context.Ns.Log.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int ZcullBind(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            Context.Ns.Log.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int SetErrorNotifier(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            Context.Ns.Log.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int SetPriority(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            Context.Ns.Log.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int AllocGpfifoEx2(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            Context.Ns.Log.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int KickoffPbWithAttr(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            NvHostChannelSubmitGpfifo Args = AMemoryHelper.Read<NvHostChannelSubmitGpfifo>(Context.Memory, InputPosition);

            NvGpuVmm Vmm = NvGpuASIoctl.GetVmm(Context);

            for (int Index = 0; Index < Args.NumEntries; Index++)
            {
                long Gpfifo = Context.Memory.ReadInt64(Args.Address + Index * 8);

                PushGpfifo(Context, Vmm, Gpfifo);
            }

            Args.SyncptId    = 0;
            Args.SyncptValue = 0;

            AMemoryHelper.Write(Context.Memory, OutputPosition, Args);

            return NvResult.Success;
        }

        private static void PushGpfifo(ServiceCtx Context, NvGpuVmm Vmm, long Gpfifo)
        {
            long VA = Gpfifo & 0xff_ffff_ffff;

            int Size = (int)(Gpfifo >> 40) & 0x7ffffc;

            byte[] Data = Vmm.ReadBytes(VA, Size);

            NvGpuPBEntry[] PushBuffer = NvGpuPushBuffer.Decode(Data);

            Context.Ns.Gpu.Fifo.PushBuffer(Vmm, PushBuffer);
        }

        public static NvChannel GetChannel(ServiceCtx Context, NvChannelName Channel)
        {
            ChannelsPerProcess Cpp = Channels.GetOrAdd(Context.Process, (Key) =>
            {
                return new ChannelsPerProcess();
            });

            return Cpp.Channels[Channel];
        }

        public static void UnloadProcess(Process Process)
        {
            Channels.TryRemove(Process, out _);
        }
    }
}