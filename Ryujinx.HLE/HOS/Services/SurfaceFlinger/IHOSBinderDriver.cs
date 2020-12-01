using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System;

namespace Ryujinx.HLE.HOS.Services.SurfaceFlinger
{
    abstract class IHOSBinderDriver : IpcService
    {
        public IHOSBinderDriver() { }

        [Command(0)]
        // TransactParcel(s32, u32, u32, buffer<unknown, 5, 0>) -> buffer<unknown, 6, 0>
        public ResultCode TransactParcel(ServiceCtx context)
        {
            int binderId = context.RequestData.ReadInt32();

            uint code  = context.RequestData.ReadUInt32();
            uint flags = context.RequestData.ReadUInt32();

            ulong dataPos  = (ulong)context.Request.SendBuff[0].Position;
            ulong dataSize = (ulong)context.Request.SendBuff[0].Size;

            long replyPos  = context.Request.ReceiveBuff[0].Position;
            long replySize = context.Request.ReceiveBuff[0].Size;

            ReadOnlySpan<byte> inputParcel = context.Memory.GetSpan(dataPos, (int)dataSize);

            Span<byte> outputParcel = new Span<byte>(new byte[replySize]);

            ResultCode result = OnTransact(binderId, code, flags, inputParcel, outputParcel);

            if (result == ResultCode.Success)
            {
                context.Memory.Write((ulong)replyPos, outputParcel);
            }

            return result;
        }

        [Command(1)]
        // AdjustRefcount(s32, s32, s32)
        public ResultCode AdjustRefcount(ServiceCtx context)
        {
            int binderId = context.RequestData.ReadInt32();
            int addVal   = context.RequestData.ReadInt32();
            int type     = context.RequestData.ReadInt32();

            return AdjustRefcount(binderId, addVal, type);
        }

        [Command(2)]
        // GetNativeHandle(s32, s32) -> handle<copy>
        public ResultCode GetNativeHandle(ServiceCtx context)
        {
            int binderId = context.RequestData.ReadInt32();

            uint typeId = context.RequestData.ReadUInt32();

            GetNativeHandle(binderId, typeId, out KReadableEvent readableEvent);

            if (context.Process.HandleTable.GenerateHandle(readableEvent, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeMove(handle);

            return ResultCode.Success;
        }

        [Command(3)] // 3.0.0+
        // TransactParcelAuto(s32, u32, u32, buffer<unknown, 21, 0>) -> buffer<unknown, 22, 0>
        public ResultCode TransactParcelAuto(ServiceCtx context)
        {
            int binderId = context.RequestData.ReadInt32();

            uint code  = context.RequestData.ReadUInt32();
            uint flags = context.RequestData.ReadUInt32();

            (long dataPos, long dataSize)   = context.Request.GetBufferType0x21();
            (long replyPos, long replySize) = context.Request.GetBufferType0x22();

            ReadOnlySpan<byte> inputParcel = context.Memory.GetSpan((ulong)dataPos, (int)dataSize);

            Span<byte> outputParcel = new Span<byte>(new byte[replySize]);

            ResultCode result = OnTransact(binderId, code, flags, inputParcel, outputParcel);

            if (result == ResultCode.Success)
            {
                context.Memory.Write((ulong)replyPos, outputParcel);
            }

            return result;
        }

        protected abstract ResultCode AdjustRefcount(int binderId, int addVal, int type);

        protected abstract void GetNativeHandle(int binderId, uint typeId, out KReadableEvent readableEvent);

        protected abstract ResultCode OnTransact(int binderId, uint code, uint flags, ReadOnlySpan<byte> inputParcel, Span<byte> outputParcel);
    }
}