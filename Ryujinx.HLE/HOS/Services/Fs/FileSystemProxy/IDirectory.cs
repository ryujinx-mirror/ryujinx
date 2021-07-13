using LibHac;
using LibHac.Fs;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Fs.FileSystemProxy
{
    class IDirectory : IpcService
    {
        private LibHac.Fs.Fsa.IDirectory _baseDirectory;

        public IDirectory(LibHac.Fs.Fsa.IDirectory directory)
        {
            _baseDirectory = directory;
        }

        [CommandHipc(0)]
        // Read() -> (u64 count, buffer<nn::fssrv::sf::IDirectoryEntry, 6, 0> entries)
        public ResultCode Read(ServiceCtx context)
        {
            ulong bufferPosition = context.Request.ReceiveBuff[0].Position;
            ulong bufferLen      = context.Request.ReceiveBuff[0].Size;

            byte[]               entriesBytes = new byte[bufferLen];
            Span<DirectoryEntry> entries      = MemoryMarshal.Cast<byte, DirectoryEntry>(entriesBytes);

            Result result = _baseDirectory.Read(out long entriesRead, entries);

            context.Memory.Write(bufferPosition, entriesBytes);
            context.ResponseData.Write(entriesRead);

            return (ResultCode)result.Value;
        }

        [CommandHipc(1)]
        // GetEntryCount() -> u64
        public ResultCode GetEntryCount(ServiceCtx context)
        {
            Result result = _baseDirectory.GetEntryCount(out long entryCount);

            context.ResponseData.Write(entryCount);

            return (ResultCode)result.Value;
        }
    }
}
