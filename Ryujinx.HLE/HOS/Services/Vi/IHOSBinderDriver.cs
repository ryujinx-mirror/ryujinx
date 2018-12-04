using Ryujinx.Graphics.Gal;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.HOS.Services.Android;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Vi
{
    class IhosBinderDriver : IpcService, IDisposable
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        private KEvent _binderEvent;

        private NvFlinger _flinger;

        public IhosBinderDriver(Horizon system, IGalRenderer renderer)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, TransactParcel     },
                { 1, AdjustRefcount     },
                { 2, GetNativeHandle    },
                { 3, TransactParcelAuto }
            };

            _binderEvent = new KEvent(system);

            _binderEvent.ReadableEvent.Signal();

            _flinger = new NvFlinger(renderer, _binderEvent);
        }

        public long TransactParcel(ServiceCtx context)
        {
            int id   = context.RequestData.ReadInt32();
            int code = context.RequestData.ReadInt32();

            long dataPos  = context.Request.SendBuff[0].Position;
            long dataSize = context.Request.SendBuff[0].Size;

            byte[] data = context.Memory.ReadBytes(dataPos, dataSize);

            data = Parcel.GetParcelData(data);

            return _flinger.ProcessParcelRequest(context, data, code);
        }

        public long TransactParcelAuto(ServiceCtx context)
        {
            int id   = context.RequestData.ReadInt32();
            int code = context.RequestData.ReadInt32();

            (long dataPos, long dataSize) = context.Request.GetBufferType0x21();

            byte[] data = context.Memory.ReadBytes(dataPos, dataSize);

            data = Parcel.GetParcelData(data);

            return _flinger.ProcessParcelRequest(context, data, code);
        }

        public long AdjustRefcount(ServiceCtx context)
        {
            int id     = context.RequestData.ReadInt32();
            int addVal = context.RequestData.ReadInt32();
            int type   = context.RequestData.ReadInt32();

            return 0;
        }

        public long GetNativeHandle(ServiceCtx context)
        {
            int  id  = context.RequestData.ReadInt32();
            uint unk = context.RequestData.ReadUInt32();

            if (context.Process.HandleTable.GenerateHandle(_binderEvent.ReadableEvent, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeMove(handle);

            return 0;
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