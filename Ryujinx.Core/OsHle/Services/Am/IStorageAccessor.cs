using ChocolArm64.Memory;
using Ryujinx.Core.Logging;
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
                { 10, Write   },
                { 11, Read    }
            };

            this.Storage = Storage;
        }

        public long GetSize(ServiceCtx Context)
        {
            Context.ResponseData.Write((long)Storage.Data.Length);

            return 0;
        }

        public long Write(ServiceCtx Context)
        {
            //TODO: Error conditions.
            long WritePosition = Context.RequestData.ReadInt64();

            (long Position, long Size) = Context.Request.GetBufferType0x21();

            if (Size > 0)
            {
                long MaxSize = Storage.Data.Length - WritePosition;

                if (Size > MaxSize)
                {
                    Size = MaxSize;
                }

                byte[] Data = AMemoryHelper.ReadBytes(Context.Memory, Position, Size);

                Buffer.BlockCopy(Data, 0, Storage.Data, (int)WritePosition, (int)Size);
            }

            return 0;
        }

        public long Read(ServiceCtx Context)
        {
            //TODO: Error conditions.
            long ReadPosition = Context.RequestData.ReadInt64();

            (long Position, long Size) = Context.Request.GetBufferType0x22();

            byte[] Data;

            if (Storage.Data.Length > Size)
            {
                Data = new byte[Size];

                Buffer.BlockCopy(Storage.Data, 0, Data, 0, (int)Size);
            }
            else
            {
                Data = Storage.Data;
            }

            AMemoryHelper.WriteBytes(Context.Memory, Position, Data);

            return 0;
        }
    }
}