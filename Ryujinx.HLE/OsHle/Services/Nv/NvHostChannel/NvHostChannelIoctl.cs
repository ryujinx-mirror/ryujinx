using ChocolArm64.Memory;
using Ryujinx.HLE.Gpu.Memory;
using Ryujinx.HLE.Logging;
using Ryujinx.HLE.OsHle.Services.Nv.NvGpuAS;
using System;

namespace Ryujinx.HLE.OsHle.Services.Nv.NvHostChannel
{
    class NvHostChannelIoctl
    {
        public static int ProcessIoctl(ServiceCtx Context, int Cmd)
        {
            switch (Cmd & 0xffff)
            {
                case 0x4714: return SetUserData     (Context);
                case 0x4801: return SetNvMap        (Context);
                case 0x4808: return SubmitGpfifo    (Context);
                case 0x4809: return AllocObjCtx     (Context);
                case 0x480b: return ZcullBind       (Context);
                case 0x480c: return SetErrorNotifier(Context);
                case 0x480d: return SetPriority     (Context);
                case 0x481a: return AllocGpfifoEx2  (Context);
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

        private static int SubmitGpfifo(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            NvHostChannelSubmitGpfifo Args = AMemoryHelper.Read<NvHostChannelSubmitGpfifo>(Context.Memory, InputPosition);

            NvGpuVmm Vmm = NvGpuASIoctl.GetVmm(Context);

            for (int Index = 0; Index < Args.NumEntries; Index++)
            {
                long Gpfifo = Context.Memory.ReadInt64(InputPosition + 0x18 + Index * 8);

                long VA = Gpfifo & 0xff_ffff_ffff;

                int Size = (int)(Gpfifo >> 40) & 0x7ffffc;

                byte[] Data = Vmm.ReadBytes(VA, Size);

                NvGpuPBEntry[] PushBuffer = NvGpuPushBuffer.Decode(Data);

                Context.Ns.Gpu.Fifo.PushBuffer(Vmm, PushBuffer);
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
    }
}