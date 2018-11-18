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
    class RomFsProvider : IFileSystemProvider
    {
        private Romfs RomFs;

        public RomFsProvider(Stream StorageStream)
        {
            RomFs = new Romfs(StorageStream);
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
            List<DirectoryEntry> Directories = new List<DirectoryEntry>();

            foreach(RomfsDir Directory in RomFs.Directories)
            {
                DirectoryEntry DirectoryEntry = new DirectoryEntry(Directory.Name, DirectoryEntryType.Directory);

                Directories.Add(DirectoryEntry);
            }

            return Directories.ToArray();
        }

        public DirectoryEntry[] GetEntries(string Path)
        {
            List<DirectoryEntry> Entries = new List<DirectoryEntry>();

            foreach (RomfsDir Directory in RomFs.Directories)
            {
                DirectoryEntry DirectoryEntry = new DirectoryEntry(Directory.Name, DirectoryEntryType.Directory);

                Entries.Add(DirectoryEntry);
            }

            foreach (RomfsFile File in RomFs.Files)
            {
                DirectoryEntry DirectoryEntry = new DirectoryEntry(File.Name, DirectoryEntryType.File, File.DataLength);

                Entries.Add(DirectoryEntry);
            }

            return Entries.ToArray();
        }

        public DirectoryEntry[] GetFiles(string Path)
        {
            List<DirectoryEntry> Files = new List<DirectoryEntry>();

            foreach (RomfsFile File in RomFs.Files)
            {
                DirectoryEntry DirectoryEntry = new DirectoryEntry(File.Name, DirectoryEntryType.File, File.DataLength);

                Files.Add(DirectoryEntry);
            }

            return Files.ToArray();
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
            return RomFs.Files.Sum(x => x.DataLength);
        }

        public bool DirectoryExists(string Name)
        {
            return RomFs.Directories.Exists(x=>x.Name == Name);
        }

        public bool FileExists(string Name)
        {
            return RomFs.FileExists(Name);
        }

        public long OpenDirectory(string Name, int FilterFlags, out IDirectory DirectoryInterface)
        {
            RomfsDir Directory = RomFs.Directories.Find(x => x.Name == Name);

            if (Directory != null)
            {
                DirectoryInterface = new IDirectory(Name, FilterFlags, this);

                return 0;
            }

            DirectoryInterface = null;

            return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
        }

        public long OpenFile(string Name, out IFile FileInterface)
        {
            if (RomFs.FileExists(Name))
            {
                Stream Stream = RomFs.OpenFile(Name);

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
