using System;

namespace Ryujinx.HLE.HOS.Services.Am
{
    class IStorageAccessor : IpcService
    {
        private IStorage _storage;

        public IStorageAccessor(IStorage storage)
        {
            _storage = storage;
        }

        [Command(0)]
        // GetSize() -> u64
        public ResultCode GetSize(ServiceCtx context)
        {
            context.ResponseData.Write((long)_storage.Data.Length);

            return ResultCode.Success;
        }

        [Command(10)]
        // Write(u64, buffer<bytes, 0x21>)
        public ResultCode Write(ServiceCtx context)
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

            return ResultCode.Success;
        }

        [Command(11)]
        // Read(u64) -> buffer<bytes, 0x22>
        public ResultCode Read(ServiceCtx context)
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

            return ResultCode.Success;
        }
    }
}