using ChocolArm64.Memory;
using Ryujinx.Core.OsHle.Ipc;
using Ryujinx.Core.OsHle.IpcServices.Android;
using System;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.IpcServices.Vi
{
    class IHOSBinderDriver : IIpcService, IDisposable
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private NvFlinger Flinger;

        public IHOSBinderDriver()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, TransactParcel  },
                { 1, AdjustRefcount  },
                { 2, GetNativeHandle }
            };

            Flinger = new NvFlinger();
        }

        public long TransactParcel(ServiceCtx Context)
        {
            int Id   = Context.RequestData.ReadInt32();
            int Code = Context.RequestData.ReadInt32();

            long DataPos  = Context.Request.SendBuff[0].Position;
            long DataSize = Context.Request.SendBuff[0].Size;

            byte[] Data = AMemoryHelper.ReadBytes(Context.Memory, DataPos, (int)DataSize);

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

            Context.Response.HandleDesc = IpcHandleDesc.MakeMove(0xbadcafe);

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
                Flinger.Dispose();
            }
        }
    }
}