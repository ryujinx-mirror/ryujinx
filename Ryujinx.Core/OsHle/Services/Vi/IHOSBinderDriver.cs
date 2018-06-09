using ChocolArm64.Memory;
using Ryujinx.Core.OsHle.Handles;
using Ryujinx.Core.OsHle.Ipc;
using Ryujinx.Core.OsHle.Services.Android;
using Ryujinx.Graphics.Gal;
using System;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Services.Vi
{
    class IHOSBinderDriver : IpcService, IDisposable
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private KEvent ReleaseEvent;

        private NvFlinger Flinger;

        public IHOSBinderDriver(IGalRenderer Renderer)
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, TransactParcel     },
                { 1, AdjustRefcount     },
                { 2, GetNativeHandle    },
                { 3, TransactParcelAuto }
            };

            ReleaseEvent = new KEvent();

            Flinger = new NvFlinger(Renderer, ReleaseEvent);
        }

        public long TransactParcel(ServiceCtx Context)
        {
            int Id   = Context.RequestData.ReadInt32();
            int Code = Context.RequestData.ReadInt32();

            long DataPos  = Context.Request.SendBuff[0].Position;
            long DataSize = Context.Request.SendBuff[0].Size;

            byte[] Data = Context.Memory.ReadBytes(DataPos, DataSize);

            Data = Parcel.GetParcelData(Data);

            return Flinger.ProcessParcelRequest(Context, Data, Code);
        }

        public long TransactParcelAuto(ServiceCtx Context)
        {
            int Id   = Context.RequestData.ReadInt32();
            int Code = Context.RequestData.ReadInt32();

            (long DataPos, long DataSize) = Context.Request.GetBufferType0x21();

            byte[] Data = Context.Memory.ReadBytes(DataPos, DataSize);

            Data = Parcel.GetParcelData(Data);

            return Flinger.ProcessParcelRequest(Context, Data, Code);
        }

        public long AdjustRefcount(ServiceCtx Context)
        {
            int Id     = Context.RequestData.ReadInt32();
            int AddVal = Context.RequestData.ReadInt32();
            int Type   = Context.RequestData.ReadInt32();

            return 0;
        }

        public long GetNativeHandle(ServiceCtx Context)
        {
            int  Id  = Context.RequestData.ReadInt32();
            uint Unk = Context.RequestData.ReadUInt32();

            int Handle = Context.Process.HandleTable.OpenHandle(ReleaseEvent);

            Context.Response.HandleDesc = IpcHandleDesc.MakeMove(Handle);

            return 0;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (Disposing)
            {
                ReleaseEvent.Dispose();

                Flinger.Dispose();
            }
        }
    }
}