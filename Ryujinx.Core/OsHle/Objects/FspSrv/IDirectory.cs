using ChocolArm64.Memory;
using Ryujinx.Core.OsHle.Ipc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Ryujinx.Core.OsHle.Objects.FspSrv
{
    [StructLayout(LayoutKind.Sequential, Size = 0x310)]
    struct DirectoryEntry
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x300)]
        public byte[] Name;
        public int Unknown;
        public byte Type;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x3)]
        public byte[] Padding;
        public long Size;
    }

    enum DirectoryEntryType
    {
        Directory,
        File
    }

    class IDirectory : IIpcInterface
    {
        private List<DirectoryEntry> DirectoryEntries = new List<DirectoryEntry>();
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private string HostPath;

        public IDirectory(string HostPath, int flags)
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                {  0, Read          },
                {  1, GetEntryCount }
            };

            this.HostPath = HostPath;

            if ((flags & 1) == 1)
            {
                string[] Directories = Directory.GetDirectories(HostPath, "*", SearchOption.TopDirectoryOnly).
                             Where(x => (new FileInfo(x).Attributes & FileAttributes.Hidden) == 0).ToArray();

                foreach (string Directory in Directories)
                {
                    DirectoryEntry Info = new DirectoryEntry
                    {
                        Name = Encoding.UTF8.GetBytes(Directory),
                        Type = (byte)DirectoryEntryType.Directory,
                        Size = 0
                    };

                    Array.Resize(ref Info.Name, 0x300);
                    DirectoryEntries.Add(Info);
                }
            }

            if ((flags & 2) == 2)
            {
                string[] Files = Directory.GetFiles(HostPath, "*", SearchOption.TopDirectoryOnly).
                       Where(x => (new FileInfo(x).Attributes & FileAttributes.Hidden) == 0).ToArray();

                foreach (string FileName in Files)
                {
                    DirectoryEntry Info = new DirectoryEntry
                    {
                        Name = Encoding.UTF8.GetBytes(Path.GetFileName(FileName)),
                        Type = (byte)DirectoryEntryType.File,
                        Size = new FileInfo(Path.Combine(HostPath, FileName)).Length
                    };

                    Array.Resize(ref Info.Name, 0x300);
                    DirectoryEntries.Add(Info);
                }
            }
        }

        private int LastItem = 0;
        public long Read(ServiceCtx Context)
        {
            long BufferPosition = Context.Request.ReceiveBuff[0].Position;
            long BufferLen = Context.Request.ReceiveBuff[0].Size;
            long MaxDirectories = BufferLen / Marshal.SizeOf(typeof(DirectoryEntry));

            if (MaxDirectories > DirectoryEntries.Count - LastItem)
            {
                MaxDirectories = DirectoryEntries.Count - LastItem;
            }

            int CurrentIndex;
            for (CurrentIndex = 0; CurrentIndex < MaxDirectories; CurrentIndex++)
            {
                int CurrentItem = LastItem + CurrentIndex;

                byte[] DirectoryEntry = new byte[Marshal.SizeOf(typeof(DirectoryEntry))];
                IntPtr Ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(DirectoryEntry)));
                Marshal.StructureToPtr(DirectoryEntries[CurrentItem], Ptr, true);
                Marshal.Copy(Ptr, DirectoryEntry, 0, Marshal.SizeOf(typeof(DirectoryEntry)));
                Marshal.FreeHGlobal(Ptr);

                AMemoryHelper.WriteBytes(Context.Memory, BufferPosition + Marshal.SizeOf(typeof(DirectoryEntry)) * CurrentIndex, DirectoryEntry);
            }

            if (LastItem < DirectoryEntries.Count)
            {
                LastItem += CurrentIndex;
                Context.ResponseData.Write((long)CurrentIndex); // index = number of entries written this call.
            }
            else
            {
                Context.ResponseData.Write((long)0);
            }

            return 0;
        }

        public long GetEntryCount(ServiceCtx Context)
        {
            Context.ResponseData.Write((long)DirectoryEntries.Count);
            return 0;
        }
    }
}
