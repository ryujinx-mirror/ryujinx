using Ryujinx.HLE.HOS.Ipc;
using System;
using System.Collections.Generic;
using System.IO;

namespace Ryujinx.HLE.HOS.Services.FspSrv
{
    class IFile : IpcService, IDisposable
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        private Stream _baseStream;

        public event EventHandler<EventArgs> Disposed;

        public string HostPath { get; }

        public IFile(Stream baseStream, string hostPath)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, Read    },
                { 1, Write   },
                { 2, Flush   },
                { 3, SetSize },
                { 4, GetSize }
            };

            _baseStream = baseStream;
            HostPath   = hostPath;
        }

        // Read(u32, u64 offset, u64 size) -> (u64 out_size, buffer<u8, 0x46, 0> out_buf)
        public long Read(ServiceCtx context)
        {
            long position = context.Request.ReceiveBuff[0].Position;

            long zero   = context.RequestData.ReadInt64();
            long offset = context.RequestData.ReadInt64();
            long size   = context.RequestData.ReadInt64();

            byte[] data = new byte[size];

            _baseStream.Seek(offset, SeekOrigin.Begin);

            int readSize = _baseStream.Read(data, 0, (int)size);

            context.Memory.WriteBytes(position, data);

            context.ResponseData.Write((long)readSize);

            return 0;
        }

        // Write(u32, u64 offset, u64 size, buffer<u8, 0x45, 0>)
        public long Write(ServiceCtx context)
        {
            long position = context.Request.SendBuff[0].Position;

            long zero   = context.RequestData.ReadInt64();
            long offset = context.RequestData.ReadInt64();
            long size   = context.RequestData.ReadInt64();

            byte[] data = context.Memory.ReadBytes(position, size);

            _baseStream.Seek(offset, SeekOrigin.Begin);
            _baseStream.Write(data, 0, (int)size);

            return 0;
        }

        // Flush()
        public long Flush(ServiceCtx context)
        {
            _baseStream.Flush();

            return 0;
        }

        // SetSize(u64 size)
        public long SetSize(ServiceCtx context)
        {
            long size = context.RequestData.ReadInt64();

            _baseStream.SetLength(size);

            return 0;
        }

        // GetSize() -> u64 fileSize
        public long GetSize(ServiceCtx context)
        {
            context.ResponseData.Write(_baseStream.Length);

            return 0;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _baseStream != null)
            {
                _baseStream.Dispose();

                Disposed?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}