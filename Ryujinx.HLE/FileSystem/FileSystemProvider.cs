using Ryujinx.HLE.HOS;
using Ryujinx.HLE.HOS.Services.FspSrv;
using System;
using System.Collections.Generic;
using System.IO;

using static Ryujinx.HLE.HOS.ErrorCode;

namespace Ryujinx.HLE.FileSystem
{
    class FileSystemProvider : IFileSystemProvider
    {
        private readonly string _basePath;
        private readonly string _rootPath;

        public FileSystemProvider(string basePath, string rootPath)
        {
            _basePath = basePath;
            _rootPath = rootPath;

            CheckIfDescendentOfRootPath(basePath);
        }

        public long CreateDirectory(string name)
        {
            CheckIfDescendentOfRootPath(name);

            if (Directory.Exists(name))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyExists);
            }

            Directory.CreateDirectory(name);

            return 0;
        }

        public long CreateFile(string name, long size)
        {
            CheckIfDescendentOfRootPath(name);

            if (File.Exists(name))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyExists);
            }

            using (FileStream newFile = File.Create(name))
            {
                newFile.SetLength(size);
            }

            return 0;
        }

        public long DeleteDirectory(string name, bool recursive)
        {
            CheckIfDescendentOfRootPath(name);

            string dirName = name;

            if (!Directory.Exists(dirName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            Directory.Delete(dirName, recursive);

            return 0;
        }

        public long DeleteFile(string name)
        {
            CheckIfDescendentOfRootPath(name);

            if (!File.Exists(name))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }
            else
            {
                File.Delete(name);
            }

            return 0;
        }

        public DirectoryEntry[] GetDirectories(string path)
        {
            CheckIfDescendentOfRootPath(path);

            List<DirectoryEntry> entries = new List<DirectoryEntry>();

            foreach(string directory in Directory.EnumerateDirectories(path))
            {
                DirectoryEntry directoryEntry = new DirectoryEntry(directory, DirectoryEntryType.Directory);

                entries.Add(directoryEntry);
            }

            return entries.ToArray();
        }

        public DirectoryEntry[] GetEntries(string path)
        {
            CheckIfDescendentOfRootPath(path);

            if (Directory.Exists(path))
            {
                List<DirectoryEntry> entries = new List<DirectoryEntry>();

                foreach (string directory in Directory.EnumerateDirectories(path))
                {
                    DirectoryEntry directoryEntry = new DirectoryEntry(directory, DirectoryEntryType.Directory);

                    entries.Add(directoryEntry);
                }

                foreach (string file in Directory.EnumerateFiles(path))
                {
                    FileInfo       fileInfo       = new FileInfo(file);
                    DirectoryEntry directoryEntry = new DirectoryEntry(file, DirectoryEntryType.File, fileInfo.Length);

                    entries.Add(directoryEntry);
                }
            }

            return null;
        }

        public DirectoryEntry[] GetFiles(string path)
        {
            CheckIfDescendentOfRootPath(path);

            List<DirectoryEntry> entries = new List<DirectoryEntry>();

            foreach (string file in Directory.EnumerateFiles(path))
            {
                FileInfo       fileInfo       = new FileInfo(file);
                DirectoryEntry directoryEntry = new DirectoryEntry(file, DirectoryEntryType.File, fileInfo.Length);

                entries.Add(directoryEntry);
            }

            return entries.ToArray();
        }

        public long GetFreeSpace(ServiceCtx context)
        {
            return context.Device.FileSystem.GetDrive().AvailableFreeSpace;
        }

        public string GetFullPath(string name)
        {
            if (name.StartsWith("//"))
            {
                name = name.Substring(2);
            }
            else if (name.StartsWith('/'))
            {
                name = name.Substring(1);
            }
            else
            {
                return null;
            }

            string fullPath = Path.Combine(_basePath, name);

            CheckIfDescendentOfRootPath(fullPath);

            return fullPath;
        }

        public long GetTotalSpace(ServiceCtx context)
        {
            return context.Device.FileSystem.GetDrive().TotalSize;
        }

        public bool DirectoryExists(string name)
        {
            CheckIfDescendentOfRootPath(name);

            return Directory.Exists(name);
        }

        public bool FileExists(string name)
        {
            CheckIfDescendentOfRootPath(name);

            return File.Exists(name);
        }

        public long OpenDirectory(string name, int filterFlags, out IDirectory directoryInterface)
        {
            CheckIfDescendentOfRootPath(name);

            if (Directory.Exists(name))
            {
                directoryInterface = new IDirectory(name, filterFlags, this);

                return 0;
            }

            directoryInterface = null;

            return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
        }

        public long OpenFile(string name, out IFile fileInterface)
        {
            CheckIfDescendentOfRootPath(name);

            if (File.Exists(name))
            {
                FileStream stream = new FileStream(name, FileMode.Open);

                fileInterface = new IFile(stream, name);

                return 0;
            }

            fileInterface = null;

            return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
        }

        public long RenameDirectory(string oldName, string newName)
        {
            CheckIfDescendentOfRootPath(oldName);
            CheckIfDescendentOfRootPath(newName);

            if (Directory.Exists(oldName))
            {
                Directory.Move(oldName, newName);
            }
            else
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            return 0;
        }

        public long RenameFile(string oldName, string newName)
        {
            CheckIfDescendentOfRootPath(oldName);
            CheckIfDescendentOfRootPath(newName);

            if (File.Exists(oldName))
            {
                File.Move(oldName, newName);
            }
            else
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            return 0;
        }

        public void CheckIfDescendentOfRootPath(string path)
        {
            DirectoryInfo pathInfo = new DirectoryInfo(path);
            DirectoryInfo rootInfo = new DirectoryInfo(_rootPath);

            while (pathInfo.Parent != null)
            {
                if (pathInfo.Parent.FullName == rootInfo.FullName)
                {
                    return;
                }
                else
                {
                    pathInfo = pathInfo.Parent;
                }
            }

            throw new InvalidOperationException($"Path {path} is not a child directory of {_rootPath}");
        }
    }
}
