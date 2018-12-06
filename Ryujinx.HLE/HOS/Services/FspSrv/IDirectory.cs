using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS.Ipc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.FspSrv
{
    class IDirectory : IpcService, IDisposable
    {
        private const int DirectoryEntrySize = 0x310;

        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        private List<DirectoryEntry> _directoryEntries;

        private int _currentItemIndex;

        public event EventHandler<EventArgs> Disposed;

        public string DirectoryPath { get; private set; }

        private IFileSystemProvider _provider;

        public IDirectory(string directoryPath, int flags, IFileSystemProvider provider)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, Read          },
                { 1, GetEntryCount }
            };

            _provider     = provider;
            DirectoryPath = directoryPath;

            _directoryEntries = new List<DirectoryEntry>();

            if ((flags & 1) != 0)
            {
                _directoryEntries.AddRange(provider.GetDirectories(directoryPath));
            }

            if ((flags & 2) != 0)
            {
                _directoryEntries.AddRange(provider.GetFiles(directoryPath));
            }

            _currentItemIndex = 0;
        }

        // Read() -> (u64 count, buffer<nn::fssrv::sf::IDirectoryEntry, 6, 0> entries)
        public long Read(ServiceCtx context)
        {
            long bufferPosition = context.Request.ReceiveBuff[0].Position;
            long bufferLen      = context.Request.ReceiveBuff[0].Size;

            int maxReadCount = (int)(bufferLen / DirectoryEntrySize);

            int count = Math.Min(_directoryEntries.Count - _currentItemIndex, maxReadCount);

            for (int index = 0; index < count; index++)
            {
                long position = bufferPosition + index * DirectoryEntrySize;

                WriteDirectoryEntry(context, position, _directoryEntries[_currentItemIndex++]);
            }

            context.ResponseData.Write((long)count);

            return 0;
        }

        private void WriteDirectoryEntry(ServiceCtx context, long position, DirectoryEntry entry)
        {
            for (int offset = 0; offset < 0x300; offset += 8)
            {
                context.Memory.WriteInt64(position + offset, 0);
            }

            byte[] nameBuffer = Encoding.UTF8.GetBytes(Path.GetFileName(entry.Path));

            context.Memory.WriteBytes(position, nameBuffer);

            context.Memory.WriteInt32(position + 0x300, 0); //Padding?
            context.Memory.WriteInt32(position + 0x304, (byte)entry.EntryType);
            context.Memory.WriteInt64(position + 0x308, entry.Size);
        }

        // GetEntryCount() -> u64
        public long GetEntryCount(ServiceCtx context)
        {
            context.ResponseData.Write((long)_directoryEntries.Count);

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
