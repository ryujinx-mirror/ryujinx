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

        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private List<DirectoryEntry> DirectoryEntries;

        private int CurrentItemIndex;

        public event EventHandler<EventArgs> Disposed;

        public string DirectoryPath { get; private set; }

        private IFileSystemProvider Provider;

        public IDirectory(string DirectoryPath, int Flags, IFileSystemProvider Provider)
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, Read          },
                { 1, GetEntryCount }
            };

            this.Provider      = Provider;
            this.DirectoryPath = DirectoryPath;

            DirectoryEntries = new List<DirectoryEntry>();

            if ((Flags & 1) != 0)
            {
                DirectoryEntries.AddRange(Provider.GetDirectories(DirectoryPath));
            }

            if ((Flags & 2) != 0)
            {
                DirectoryEntries.AddRange(Provider.GetFiles(DirectoryPath));
            }

            CurrentItemIndex = 0;
        }

        // Read() -> (u64 count, buffer<nn::fssrv::sf::IDirectoryEntry, 6, 0> entries)
        public long Read(ServiceCtx Context)
        {
            long BufferPosition = Context.Request.ReceiveBuff[0].Position;
            long BufferLen      = Context.Request.ReceiveBuff[0].Size;

            int MaxReadCount = (int)(BufferLen / DirectoryEntrySize);

            int Count = Math.Min(DirectoryEntries.Count - CurrentItemIndex, MaxReadCount);

            for (int Index = 0; Index < Count; Index++)
            {
                long Position = BufferPosition + Index * DirectoryEntrySize;

                WriteDirectoryEntry(Context, Position, DirectoryEntries[CurrentItemIndex++]);
            }

            Context.ResponseData.Write((long)Count);

            return 0;
        }

        private void WriteDirectoryEntry(ServiceCtx Context, long Position, DirectoryEntry Entry)
        {
            for (int Offset = 0; Offset < 0x300; Offset += 8)
            {
                Context.Memory.WriteInt64(Position + Offset, 0);
            }

            byte[] NameBuffer = Encoding.UTF8.GetBytes(Path.GetFileName(Entry.Path));

            Context.Memory.WriteBytes(Position, NameBuffer);

            Context.Memory.WriteInt32(Position + 0x300, 0); //Padding?
            Context.Memory.WriteInt32(Position + 0x304, (byte)Entry.EntryType);
            Context.Memory.WriteInt64(Position + 0x308, Entry.Size);
        }

        // GetEntryCount() -> u64
        public long GetEntryCount(ServiceCtx Context)
        {
            Context.ResponseData.Write((long)DirectoryEntries.Count);

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
