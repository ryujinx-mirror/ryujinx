using LibHac;
using LibHac.Fs;
using LibHac.Sf;
using Ryujinx.Common;

namespace Ryujinx.HLE.HOS.Services.Fs.FileSystemProxy
{
    class IFile : DisposableIpcService
    {
        private ReferenceCountedDisposable<LibHac.FsSrv.Sf.IFile> _baseFile;

        public IFile(ReferenceCountedDisposable<LibHac.FsSrv.Sf.IFile> baseFile)
        {
            _baseFile = baseFile;
        }

        [CommandHipc(0)]
        // Read(u32 readOption, u64 offset, u64 size) -> (u64 out_size, buffer<u8, 0x46, 0> out_buf)
        public ResultCode Read(ServiceCtx context)
        {
            ulong position = context.Request.ReceiveBuff[0].Position;

            ReadOption readOption = context.RequestData.ReadStruct<ReadOption>();
            context.RequestData.BaseStream.Position += 4;

            long offset = context.RequestData.ReadInt64();
            long size   = context.RequestData.ReadInt64();

            byte[] data = new byte[context.Request.ReceiveBuff[0].Size];

            Result result = _baseFile.Target.Read(out long bytesRead, offset, new OutBuffer(data), size, readOption);

            context.Memory.Write(position, data);

            context.ResponseData.Write(bytesRead);

            return (ResultCode)result.Value;
        }

        [CommandHipc(1)]
        // Write(u32 writeOption, u64 offset, u64 size, buffer<u8, 0x45, 0>)
        public ResultCode Write(ServiceCtx context)
        {
            ulong position = context.Request.SendBuff[0].Position;

            WriteOption writeOption = context.RequestData.ReadStruct<WriteOption>();
            context.RequestData.BaseStream.Position += 4;

            long offset = context.RequestData.ReadInt64();
            long size   = context.RequestData.ReadInt64();

            byte[] data = new byte[context.Request.SendBuff[0].Size];

            context.Memory.Read(position, data);

            return (ResultCode)_baseFile.Target.Write(offset, new InBuffer(data), size, writeOption).Value;
        }

        [CommandHipc(2)]
        // Flush()
        public ResultCode Flush(ServiceCtx context)
        {
            return (ResultCode)_baseFile.Target.Flush().Value;
        }

        [CommandHipc(3)]
        // SetSize(u64 size)
        public ResultCode SetSize(ServiceCtx context)
        {
            long size = context.RequestData.ReadInt64();

            return (ResultCode)_baseFile.Target.SetSize(size).Value;
        }

        [CommandHipc(4)]
        // GetSize() -> u64 fileSize
        public ResultCode GetSize(ServiceCtx context)
        {
            Result result = _baseFile.Target.GetSize(out long size);

            context.ResponseData.Write(size);

            return (ResultCode)result.Value;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                _baseFile?.Dispose();
            }
        }
    }
}