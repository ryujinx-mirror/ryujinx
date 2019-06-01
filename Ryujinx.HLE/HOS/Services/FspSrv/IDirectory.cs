using Ryujinx.HLE.HOS.Ipc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.FspSrv
{
    class IDirectory : IpcService, IDisposable
    {
        private const int DirectoryEntrySize = 0x310;

        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        private IEnumerator<LibHac.Fs.DirectoryEntry> _enumerator;

        public event EventHandler<EventArgs> Disposed;

        public string Path { get; }

        private LibHac.Fs.IDirectory _provider;

        public IDirectory(LibHac.Fs.IDirectory directory)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, Read          },
                { 1, GetEntryCount }
            };

            _provider = directory;

            Path = directory.FullPath;

            _enumerator = directory.Read().GetEnumerator();
        }

        // Read() -> (u64 count, buffer<nn::fssrv::sf::IDirectoryEntry, 6, 0> entries)
        public long Read(ServiceCtx context)
        {
            long bufferPosition = context.Request.ReceiveBuff[0].Position;
            long bufferLen      = context.Request.ReceiveBuff[0].Size;

            int maxReadCount = (int)(bufferLen / DirectoryEntrySize);
            int readCount    = 0;

            while (readCount < maxReadCount && _enumerator.MoveNext())
            {
                long position = bufferPosition + readCount * DirectoryEntrySize;

                WriteDirectoryEntry(context, position, _enumerator.Current);

                readCount++;
            }

            context.ResponseData.Write((long)readCount);

            return 0;
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

        // GetEntryCount() -> u64
        public long GetEntryCount(ServiceCtx context)
        {
            context.ResponseData.Write((long)_provider.GetEntryCount());

            return 0;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Disposed?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
