using LibHac;
using LibHac.Sf;

namespace Ryujinx.HLE.HOS.Services.Fs.FileSystemProxy
{
    class IDirectory : DisposableIpcService
    {
        private ReferenceCountedDisposable<LibHac.FsSrv.Sf.IDirectory> _baseDirectory;

        public IDirectory(ReferenceCountedDisposable<LibHac.FsSrv.Sf.IDirectory> directory)
        {
            _baseDirectory = directory;
        }

        [CommandHipc(0)]
        // Read() -> (u64 count, buffer<nn::fssrv::sf::IDirectoryEntry, 6, 0> entries)
        public ResultCode Read(ServiceCtx context)
        {
            ulong bufferPosition = context.Request.ReceiveBuff[0].Position;
            ulong bufferLen = context.Request.ReceiveBuff[0].Size;

            byte[] entryBuffer = new byte[bufferLen];

            Result result = _baseDirectory.Target.Read(out long entriesRead, new OutBuffer(entryBuffer));

            context.Memory.Write(bufferPosition, entryBuffer);
            context.ResponseData.Write(entriesRead);

            return (ResultCode)result.Value;
        }

        [CommandHipc(1)]
        // GetEntryCount() -> u64
        public ResultCode GetEntryCount(ServiceCtx context)
        {
            Result result = _baseDirectory.Target.GetEntryCount(out long entryCount);

            context.ResponseData.Write(entryCount);

            return (ResultCode)result.Value;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                _baseDirectory?.Dispose();
            }
        }
    }
}
