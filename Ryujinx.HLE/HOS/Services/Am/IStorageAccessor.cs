using Ryujinx.HLE.HOS.Ipc;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Am
{
    class IStorageAccessor : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        private IStorage _storage;

        public IStorageAccessor(IStorage storage)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0,  GetSize },
                { 10, Write   },
                { 11, Read    }
            };

            _storage = storage;
        }

        public long GetSize(ServiceCtx context)
        {
            context.ResponseData.Write((long)_storage.Data.Length);

            return 0;
        }

        public long Write(ServiceCtx context)
        {
            // TODO: Error conditions.
            long writePosition = context.RequestData.ReadInt64();

            (long position, long size) = context.Request.GetBufferType0x21();

            if (size > 0)
            {
                long maxSize = _storage.Data.Length - writePosition;

                if (size > maxSize)
                {
                    size = maxSize;
                }

                byte[] data = context.Memory.ReadBytes(position, size);

                Buffer.BlockCopy(data, 0, _storage.Data, (int)writePosition, (int)size);
            }

            return 0;
        }

        public long Read(ServiceCtx context)
        {
            // TODO: Error conditions.
            long readPosition = context.RequestData.ReadInt64();

            (long position, long size) = context.Request.GetBufferType0x22();

            byte[] data;

            if (_storage.Data.Length > size)
            {
                data = new byte[size];

                Buffer.BlockCopy(_storage.Data, 0, data, 0, (int)size);
            }
            else
            {
                data = _storage.Data;
            }

            context.Memory.WriteBytes(position, data);

            return 0;
        }
    }
}