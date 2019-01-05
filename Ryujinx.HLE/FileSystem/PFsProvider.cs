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
    class PFsProvider : IFileSystemProvider
    {
        private Pfs _pfs;

        public PFsProvider(Pfs pfs)
        {
            _pfs = pfs;
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
            return new DirectoryEntry[0];
        }

        public DirectoryEntry[] GetEntries(string path)
        {
            List<DirectoryEntry> entries = new List<DirectoryEntry>();

            foreach (PfsFileEntry file in _pfs.Files)
            {
                DirectoryEntry directoryEntry = new DirectoryEntry(file.Name, DirectoryEntryType.File, file.Size);

                entries.Add(directoryEntry);
            }

            return entries.ToArray();
        }

        public DirectoryEntry[] GetFiles(string path)
        {
            List<DirectoryEntry> entries = new List<DirectoryEntry>();

            foreach (PfsFileEntry file in _pfs.Files)
            {
                DirectoryEntry directoryEntry = new DirectoryEntry(file.Name, DirectoryEntryType.File, file.Size);

                entries.Add(directoryEntry);
            }

            return entries.ToArray();
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
            return _pfs.Files.Sum(x => x.Size);
        }

        public bool DirectoryExists(string name)
        {
            return name == "/";
        }

        public bool FileExists(string name)
        {
            name = name.TrimStart('/');

            return _pfs.FileExists(name);
        }

        public long OpenDirectory(string name, int filterFlags, out IDirectory directoryInterface)
        {
            if (name == "/")
            {
                directoryInterface = new IDirectory(name, filterFlags, this);

                return 0;
            }

            throw new NotSupportedException();
        }

        public long OpenFile(string name, out IFile fileInterface)
        {
            name = name.TrimStart('/');

            if (_pfs.FileExists(name))
            {
                Stream stream = _pfs.OpenFile(name).AsStream();
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
