using System;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE
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
            if (_storage.IsReadOnly)
            {
                return ResultCode.ObjectInvalid;
            }

            long writePosition = context.RequestData.ReadInt64();

            if (writePosition > _storage.Data.Length)
            {
                return ResultCode.OutOfBounds;
            }

            (long position, long size) = context.Request.GetBufferType0x21();

            size = Math.Min(size, _storage.Data.Length - writePosition);

            if (size > 0)
            {
                long maxSize = _storage.Data.Length - writePosition;

                if (size > maxSize)
                {
                    size = maxSize;
                }

                byte[] data = new byte[size];

                context.Memory.Read((ulong)position, data);

                Buffer.BlockCopy(data, 0, _storage.Data, (int)writePosition, (int)size);
            }

            return ResultCode.Success;
        }

        [Command(11)]
        // Read(u64) -> buffer<bytes, 0x22>
        public ResultCode Read(ServiceCtx context)
        {
            long readPosition = context.RequestData.ReadInt64();

            if (readPosition > _storage.Data.Length)
            {
                return ResultCode.OutOfBounds;
            }

            (long position, long size) = context.Request.GetBufferType0x22();

            size = Math.Min(size, _storage.Data.Length - readPosition);

            byte[] data = new byte[size];

            Buffer.BlockCopy(_storage.Data, (int)readPosition, data, 0, (int)size);

            context.Memory.Write((ulong)position, data);

            return ResultCode.Success;
        }
    }
}