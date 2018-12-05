using Ryujinx.HLE.HOS.Ipc;
using System;
using System.Collections.Generic;
using System.IO;

namespace Ryujinx.HLE.HOS.Services.FspSrv
{
    class IFile : IpcService, IDisposable
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private Stream BaseStream;

        public event EventHandler<EventArgs> Disposed;

        public string HostPath { get; private set; }

        public IFile(Stream BaseStream, string HostPath)
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, Read    },
                { 1, Write   },
                { 2, Flush   },
                { 3, SetSize },
                { 4, GetSize }
            };

            this.BaseStream = BaseStream;
            this.HostPath   = HostPath;
        }

        // Read(u32, u64 offset, u64 size) -> (u64 out_size, buffer<u8, 0x46, 0> out_buf)
        public long Read(ServiceCtx Context)
        {
            long Position = Context.Request.ReceiveBuff[0].Position;

            long Zero   = Context.RequestData.ReadInt64();
            long Offset = Context.RequestData.ReadInt64();
            long Size   = Context.RequestData.ReadInt64();

            byte[] Data = new byte[Size];

            BaseStream.Seek(Offset, SeekOrigin.Begin);

            int ReadSize = BaseStream.Read(Data, 0, (int)Size);

            Context.Memory.WriteBytes(Position, Data);

            Context.ResponseData.Write((long)ReadSize);

            return 0;
        }

        // Write(u32, u64 offset, u64 size, buffer<u8, 0x45, 0>)
        public long Write(ServiceCtx Context)
        {
            long Position = Context.Request.SendBuff[0].Position;

            long Zero   = Context.RequestData.ReadInt64();
            long Offset = Context.RequestData.ReadInt64();
            long Size   = Context.RequestData.ReadInt64();

            byte[] Data = Context.Memory.ReadBytes(Position, Size);

            BaseStream.Seek(Offset, SeekOrigin.Begin);
            BaseStream.Write(Data, 0, (int)Size);

            return 0;
        }

        // Flush()
        public long Flush(ServiceCtx Context)
        {
            BaseStream.Flush();

            return 0;
        }

        // SetSize(u64 size)
        public long SetSize(ServiceCtx Context)
        {
            long Size = Context.RequestData.ReadInt64();

            BaseStream.SetLength(Size);

            return 0;
        }

        // GetSize() -> u64 fileSize
        public long GetSize(ServiceCtx Context)
        {
            Context.ResponseData.Write(BaseStream.Length);

            return 0;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && BaseStream != null)
            {
                BaseStream.Dispose();

                Disposed?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}