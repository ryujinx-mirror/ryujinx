using ChocolArm64.Memory;
using Ryujinx.Core.OsHle.Ipc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ryujinx.Core.OsHle.Objects.FspSrv
{
    class IDirectory : IIpcInterface, IDisposable
    {
        private const int DirectoryEntrySize = 0x310;

        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private List<string> DirectoryEntries;

        private int CurrentItemIndex;

        public event EventHandler<EventArgs> Disposed;

        public string HostPath { get; private set; }

        public IDirectory(string HostPath, int Flags)
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, Read          },
                { 1, GetEntryCount }
            };

            this.HostPath = HostPath;

            DirectoryEntries = new List<string>();

            if ((Flags & 1) != 0)
            {
                DirectoryEntries.AddRange(Directory.GetDirectories(HostPath));
            }

            if ((Flags & 2) != 0)
            {
                DirectoryEntries.AddRange(Directory.GetFiles(HostPath));
            }

            CurrentItemIndex = 0;
        }

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

        private void WriteDirectoryEntry(ServiceCtx Context, long Position, string FullPath)
        {
            for (int Offset = 0; Offset < 0x300; Offset += 8)
            {
                Context.Memory.WriteInt64(Position + Offset, 0);
            }

            byte[] NameBuffer = Encoding.UTF8.GetBytes(Path.GetFileName(FullPath));

            AMemoryHelper.WriteBytes(Context.Memory, Position, NameBuffer);

            int  Type = 0;
            long Size = 0;

            if (File.Exists(FullPath))
            {
                Type = 1;
                Size = new FileInfo(FullPath).Length;
            }

            Context.Memory.WriteInt32(Position + 0x300, 0); //Padding?
            Context.Memory.WriteInt32(Position + 0x304, Type);
            Context.Memory.WriteInt64(Position + 0x308, Size);
        }

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
