using Ryujinx.Graphics.GAL;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.SurfaceFlinger;
using System;

namespace Ryujinx.HLE.HOS.Services.Vi.RootService.ApplicationDisplayService
{
    class IHOSBinderDriver : IpcService, IDisposable
    {
        private KEvent _binderEvent;

        private NvFlinger _flinger;

        public IHOSBinderDriver(Horizon system, IRenderer renderer)
        {
            _binderEvent = new KEvent(system);

            _binderEvent.ReadableEvent.Signal();

            _flinger = new NvFlinger(renderer, _binderEvent);
        }

        [Command(0)]
        // TransactParcel(s32, u32, u32, buffer<unknown, 5, 0>) -> buffer<unknown, 6, 0>
        public ResultCode TransactParcel(ServiceCtx context)
        {
            int id   = context.RequestData.ReadInt32();
            int code = context.RequestData.ReadInt32();

            long dataPos  = context.Request.SendBuff[0].Position;
            long dataSize = context.Request.SendBuff[0].Size;

            byte[] data = context.Memory.ReadBytes(dataPos, dataSize);

            data = Parcel.GetParcelData(data);

            return (ResultCode)_flinger.ProcessParcelRequest(context, data, code);
        }

        [Command(1)]
        // AdjustRefcount(s32, s32, s32)
        public ResultCode AdjustRefcount(ServiceCtx context)
        {
            int id     = context.RequestData.ReadInt32();
            int addVal = context.RequestData.ReadInt32();
            int type   = context.RequestData.ReadInt32();

            return ResultCode.Success;
        }

        [Command(2)]
        // GetNativeHandle(s32, s32) -> handle<copy>
        public ResultCode GetNativeHandle(ServiceCtx context)
        {
            int  id  = context.RequestData.ReadInt32();
            uint unk = context.RequestData.ReadUInt32();

            if (context.Process.HandleTable.GenerateHandle(_binderEvent.ReadableEvent, out int handle) != KernelResult.Success)
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
            int id = context.RequestData.ReadInt32();
            int code = context.RequestData.ReadInt32();

            (long dataPos, long dataSize) = context.Request.GetBufferType0x21();

            byte[] data = context.Memory.ReadBytes(dataPos, dataSize);

            data = Parcel.GetParcelData(data);

            return (ResultCode)_flinger.ProcessParcelRequest(context, data, code);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _flinger.Dispose();
            }
        }
    }
}