using LibHac;
using Ryujinx.HLE.HOS;
using Ryujinx.HLE.HOS.Services.FspSrv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using static Ryujinx.HLE.HOS.ErrorCode;

namespace Ryujinx.HLE.FileSystem
{
    class PFsProvider : IFileSystemProvider
    {
        private Pfs Pfs;

        public PFsProvider(Pfs Pfs)
        {
            this.Pfs = Pfs;
        }

        public long CreateDirectory(string Name)
        {
            throw new NotSupportedException();
        }

        public long CreateFile(string Name, long Size)
        {
            throw new NotSupportedException();
        }

        public long DeleteDirectory(string Name, bool Recursive)
        {
            throw new NotSupportedException();
        }

        public long DeleteFile(string Name)
        {
            throw new NotSupportedException();
        }

        public DirectoryEntry[] GetDirectories(string Path)
        {
            return new DirectoryEntry[0];
        }

        public DirectoryEntry[] GetEntries(string Path)
        {
            List<DirectoryEntry> Entries = new List<DirectoryEntry>();

            foreach (PfsFileEntry File in Pfs.Files)
            {
                DirectoryEntry DirectoryEntry = new DirectoryEntry(File.Name, DirectoryEntryType.File, File.Size);

                Entries.Add(DirectoryEntry);
            }

            return Entries.ToArray();
        }

        public DirectoryEntry[] GetFiles(string Path)
        {
            List<DirectoryEntry> Entries = new List<DirectoryEntry>();

            foreach (PfsFileEntry File in Pfs.Files)
            {
                DirectoryEntry DirectoryEntry = new DirectoryEntry(File.Name, DirectoryEntryType.File, File.Size);

                Entries.Add(DirectoryEntry);
            }

            return Entries.ToArray();
        }

        public long GetFreeSpace(ServiceCtx Context)
        {
            return 0;
        }

        public string GetFullPath(string Name)
        {
            return Name;
        }

        public long GetTotalSpace(ServiceCtx Context)
        {
            return Pfs.Files.Sum(x => x.Size);
        }

        public bool DirectoryExists(string Name)
        {
            return Name == "/" ? true : false;
        }

        public bool FileExists(string Name)
        {
            Name = Name.TrimStart('/');

            return Pfs.FileExists(Name);
        }

        public long OpenDirectory(string Name, int FilterFlags, out IDirectory DirectoryInterface)
        {
            if (Name == "/")
            {
                DirectoryInterface = new IDirectory(Name, FilterFlags, this);

                return 0;
            }

            throw new NotSupportedException();
        }

        public long OpenFile(string Name, out IFile FileInterface)
        {
            Name = Name.TrimStart('/');

            if (Pfs.FileExists(Name))
            {
                Stream Stream = Pfs.OpenFile(Name);
                FileInterface = new IFile(Stream, Name);

                return 0;
            }

            FileInterface = null;

            return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
        }

        public long RenameDirectory(string OldName, string NewName)
        {
            throw new NotSupportedException();
        }

        public long RenameFile(string OldName, string NewName)
        {
            throw new NotSupportedException();
        }

        public void CheckIfOutsideBasePath(string Path)
        {
            throw new NotSupportedException();
        }
    }
}
