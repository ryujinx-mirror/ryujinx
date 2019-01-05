using LibHac;
using LibHac.IO;
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
        private Romfs _romFs;

        public RomFsProvider(LibHac.IO.IStorage storage)
        {
            _romFs = new Romfs(storage);
        }

        public long CreateDirectory(string name)
        {
            throw new NotSupportedException();
        }

        public long CreateFile(string name, long size)
        {
            throw new NotSupportedException();
        }

        public long DeleteDirectory(string name, bool recursive)
        {
            throw new NotSupportedException();
        }

        public long DeleteFile(string name)
        {
            throw new NotSupportedException();
        }

        public DirectoryEntry[] GetDirectories(string path)
        {
            List<DirectoryEntry> directories = new List<DirectoryEntry>();

            foreach(RomfsDir directory in _romFs.Directories)
            {
                DirectoryEntry directoryEntry = new DirectoryEntry(directory.Name, DirectoryEntryType.Directory);

                directories.Add(directoryEntry);
            }

            return directories.ToArray();
        }

        public DirectoryEntry[] GetEntries(string path)
        {
            List<DirectoryEntry> entries = new List<DirectoryEntry>();

            foreach (RomfsDir directory in _romFs.Directories)
            {
                DirectoryEntry directoryEntry = new DirectoryEntry(directory.Name, DirectoryEntryType.Directory);

                entries.Add(directoryEntry);
            }

            foreach (RomfsFile file in _romFs.Files)
            {
                DirectoryEntry directoryEntry = new DirectoryEntry(file.Name, DirectoryEntryType.File, file.DataLength);

                entries.Add(directoryEntry);
            }

            return entries.ToArray();
        }

        public DirectoryEntry[] GetFiles(string path)
        {
            List<DirectoryEntry> files = new List<DirectoryEntry>();

            foreach (RomfsFile file in _romFs.Files)
            {
                DirectoryEntry directoryEntry = new DirectoryEntry(file.Name, DirectoryEntryType.File, file.DataLength);

                files.Add(directoryEntry);
            }

            return files.ToArray();
        }

        public long GetFreeSpace(ServiceCtx context)
        {
            return 0;
        }

        public string GetFullPath(string name)
        {
            return name;
        }

        public long GetTotalSpace(ServiceCtx context)
        {
            return _romFs.Files.Sum(x => x.DataLength);
        }

        public bool DirectoryExists(string name)
        {
            return _romFs.Directories.Exists(x=>x.Name == name);
        }

        public bool FileExists(string name)
        {
            return _romFs.FileExists(name);
        }

        public long OpenDirectory(string name, int filterFlags, out IDirectory directoryInterface)
        {
            RomfsDir directory = _romFs.Directories.Find(x => x.Name == name);

            if (directory != null)
            {
                directoryInterface = new IDirectory(name, filterFlags, this);

                return 0;
            }

            directoryInterface = null;

            return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
        }

        public long OpenFile(string name, out IFile fileInterface)
        {
            if (_romFs.FileExists(name))
            {
                Stream stream = _romFs.OpenFile(name).AsStream();

                fileInterface = new IFile(stream, name);

                return 0;
            }

            fileInterface = null;

            return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
        }

        public long RenameDirectory(string oldName, string newName)
        {
            throw new NotSupportedException();
        }

        public long RenameFile(string oldName, string newName)
        {
            throw new NotSupportedException();
        }

        public void CheckIfOutsideBasePath(string path)
        {
            throw new NotSupportedException();
        }
    }
}
