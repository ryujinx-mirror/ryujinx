using ChocolArm64.Memory;
using Ryujinx.Core.OsHle.Ipc;
using System;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Services.Am
{
    class IStorageAccessor : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private IStorage Storage;

        public IStorageAccessor(IStorage Storage)
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0,  GetSize },
                { 11, Read    }
            };

            this.Storage = Storage;
        }

        public long GetSize(ServiceCtx Context)
        {
            Context.ResponseData.Write((long)Storage.Data.Length);

            return 0;
        }

        public long Read(ServiceCtx Context)
        {
            long ReadPosition = Context.RequestData.ReadInt64();

            if (Context.Request.RecvListBuff.Count > 0)
            {
                long  Position = Context.Request.RecvListBuff[0].Position;
                short Size     = Context.Request.RecvListBuff[0].Size;

                byte[] Data;

                if (Storage.Data.Length > Size)
                {
                    Data = new byte[Size];

                    Buffer.BlockCopy(Storage.Data, 0, Data, 0, Size);
                }
                else
                {
                    Data = Storage.Data;
                }

                AMemoryHelper.WriteBytes(Context.Memory, Position, Data);
            }

            return 0;
        }
    }
}