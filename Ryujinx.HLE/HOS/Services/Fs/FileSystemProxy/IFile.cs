using LibHac;
using LibHac.Fs;
using System;

namespace Ryujinx.HLE.HOS.Services.Fs.FileSystemProxy
{
    class IFile : IpcService, IDisposable
    {
        private LibHac.Fs.Fsa.IFile _baseFile;

        public IFile(LibHac.Fs.Fsa.IFile baseFile)
        {
            _baseFile = baseFile;
        }

        [CommandHipc(0)]
        // Read(u32 readOption, u64 offset, u64 size) -> (u64 out_size, buffer<u8, 0x46, 0> out_buf)
        public ResultCode Read(ServiceCtx context)
        {
            ulong position = context.Request.ReceiveBuff[0].Position;

            ReadOption readOption = new ReadOption(context.RequestData.ReadInt32());
            context.RequestData.BaseStream.Position += 4;

            long offset = context.RequestData.ReadInt64();
            long size   = context.RequestData.ReadInt64();

            byte[] data = new byte[size];

            Result result = _baseFile.Read(out long bytesRead, offset, data, readOption);

            context.Memory.Write(position, data);

            context.ResponseData.Write(bytesRead);

            return (ResultCode)result.Value;
        }

        [CommandHipc(1)]
        // Write(u32 writeOption, u64 offset, u64 size, buffer<u8, 0x45, 0>)
        public ResultCode Write(ServiceCtx context)
        {
            ulong position = context.Request.SendBuff[0].Position;

            WriteOption writeOption = new WriteOption(context.RequestData.ReadInt32());
            context.RequestData.BaseStream.Position += 4;

            long offset = context.RequestData.ReadInt64();
            long size   = context.RequestData.ReadInt64();

            byte[] data = new byte[size];

            context.Memory.Read(position, data);

            return (ResultCode)_baseFile.Write(offset, data, writeOption).Value;
        }

        [CommandHipc(2)]
        // Flush()
        public ResultCode Flush(ServiceCtx context)
        {
            return (ResultCode)_baseFile.Flush().Value;
        }

        [CommandHipc(3)]
        // SetSize(u64 size)
        public ResultCode SetSize(ServiceCtx context)
        {
            long size = context.RequestData.ReadInt64();

            return (ResultCode)_baseFile.SetSize(size).Value;
        }

        [CommandHipc(4)]
        // GetSize() -> u64 fileSize
        public ResultCode GetSize(ServiceCtx context)
        {
            Result result = _baseFile.GetSize(out long size);

            context.ResponseData.Write(size);

            return (ResultCode)result.Value;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _baseFile?.Dispose();
            }
        }
    }
}