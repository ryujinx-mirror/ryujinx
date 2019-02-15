using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;
using System.IO;

namespace Ryujinx.HLE.HOS.Services.FspSrv
{
    class IStorage : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        private Stream _baseStream;

        public IStorage(Stream baseStream)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, Read    },
                { 4, GetSize }
            };

            _baseStream = baseStream;
        }

        // Read(u64 offset, u64 length) -> buffer<u8, 0x46, 0> buffer
        public long Read(ServiceCtx context)
        {
            long offset = context.RequestData.ReadInt64();
            long size   = context.RequestData.ReadInt64();

            if (context.Request.ReceiveBuff.Count > 0)
            {
                IpcBuffDesc buffDesc = context.Request.ReceiveBuff[0];

                //Use smaller length to avoid overflows.
                if (size > buffDesc.Size)
                {
                    size = buffDesc.Size;
                }

                byte[] data = new byte[size];

                lock (_baseStream)
                {
                    _baseStream.Seek(offset, SeekOrigin.Begin);
                    _baseStream.Read(data, 0, data.Length);
                }

                context.Memory.WriteBytes(buffDesc.Position, data);
            }

            return 0;
        }

        // GetSize() -> u64 size
        public long GetSize(ServiceCtx context)
        {
            context.ResponseData.Write(_baseStream.Length);

            return 0;
        }
    }
}
