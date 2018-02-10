using ChocolArm64.Memory;
using Ryujinx.OsHle.Ipc;
using System;
using System.Collections.Generic;
using System.IO;

namespace Ryujinx.OsHle.Objects.FspSrv
{
    class IFile : IIpcInterface, IDisposable
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private Stream BaseStream;

        public IFile(Stream BaseStream)
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, Read  },
                { 1, Write }
            };

            this.BaseStream = BaseStream;
        }

        public long Read(ServiceCtx Context)
        {
            long Position = Context.Request.ReceiveBuff[0].Position;

            long Zero   = Context.RequestData.ReadInt64();
            long Offset = Context.RequestData.ReadInt64();
            long Size   = Context.RequestData.ReadInt64();

            byte[] Data = new byte[Size];

            int ReadSize = BaseStream.Read(Data, 0, (int)Size);

            AMemoryHelper.WriteBytes(Context.Memory, Position, Data);

            //TODO: Use ReadSize, we need to return the size that was REALLY read from the file.
            //This is a workaround because we are doing something wrong and the game expects to read
            //data from a file that doesn't yet exists -- and breaks if it can't read anything.
            Context.ResponseData.Write((long)Size);

            return 0;
        }

        public long Write(ServiceCtx Context)
        {
            long Position = Context.Request.SendBuff[0].Position;

            long Zero   = Context.RequestData.ReadInt64();
            long Offset = Context.RequestData.ReadInt64();
            long Size   = Context.RequestData.ReadInt64();

            byte[] Data = AMemoryHelper.ReadBytes(Context.Memory, Position, (int)Size);

            BaseStream.Seek(Offset, SeekOrigin.Begin);
            BaseStream.Write(Data, 0, (int)Size);

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
            }
        }
    }
}