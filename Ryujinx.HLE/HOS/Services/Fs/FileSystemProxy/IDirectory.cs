using LibHac;
using System.Collections.Generic;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.Fs.FileSystemProxy
{
    class IDirectory : IpcService
    {
        private const int DirectoryEntrySize = 0x310;

        private IEnumerator<LibHac.Fs.DirectoryEntry> _enumerator;

        private LibHac.Fs.IDirectory _baseDirectory;

        public IDirectory(LibHac.Fs.IDirectory directory)
        {
            _baseDirectory = directory;
            _enumerator    = directory.Read().GetEnumerator();
        }

        [Command(0)]
        // Read() -> (u64 count, buffer<nn::fssrv::sf::IDirectoryEntry, 6, 0> entries)
        public ResultCode Read(ServiceCtx context)
        {
            long bufferPosition = context.Request.ReceiveBuff[0].Position;
            long bufferLen      = context.Request.ReceiveBuff[0].Size;

            int maxReadCount = (int)(bufferLen / DirectoryEntrySize);
            int readCount    = 0;

            try
            {
                while (readCount < maxReadCount && _enumerator.MoveNext())
                {
                    long position = bufferPosition + readCount * DirectoryEntrySize;

                    WriteDirectoryEntry(context, position, _enumerator.Current);

                    readCount++;
                }
            }
            catch (HorizonResultException ex)
            {
                return (ResultCode)ex.ResultValue.Value;
            }

            context.ResponseData.Write((long)readCount);

            return ResultCode.Success;
        }

        private void WriteDirectoryEntry(ServiceCtx context, long position, LibHac.Fs.DirectoryEntry entry)
        {
            for (int offset = 0; offset < 0x300; offset += 8)
            {
                context.Memory.WriteInt64(position + offset, 0);
            }

            byte[] nameBuffer = Encoding.UTF8.GetBytes(entry.Name);

            context.Memory.WriteBytes(position, nameBuffer);

            context.Memory.WriteInt32(position + 0x300, (int)entry.Attributes);
            context.Memory.WriteInt32(position + 0x304, (byte)entry.Type);
            context.Memory.WriteInt64(position + 0x308, entry.Size);
        }

        [Command(1)]
        // GetEntryCount() -> u64
        public ResultCode GetEntryCount(ServiceCtx context)
        {
            try
            {
                context.ResponseData.Write((long)_baseDirectory.GetEntryCount());
            }
            catch (HorizonResultException ex)
            {
                return (ResultCode)ex.ResultValue.Value;
            }

            return ResultCode.Success;
        }
    }
}
