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
        private readonly string BasePath;
        private readonly string RootPath;

        public FileSystemProvider(string BasePath, string RootPath)
        {
            this.BasePath = BasePath;
            this.RootPath = RootPath;

            CheckIfDecendentOfRootPath(BasePath);
        }

        public long CreateDirectory(string Name)
        {
            CheckIfDecendentOfRootPath(Name);

            if (Directory.Exists(Name))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyExists);
            }

            Directory.CreateDirectory(Name);

            return 0;
        }

        public long CreateFile(string Name, long Size)
        {
            CheckIfDecendentOfRootPath(Name);

            if (File.Exists(Name))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyExists);
            }

            using (FileStream NewFile = File.Create(Name))
            {
                NewFile.SetLength(Size);
            }

            return 0;
        }

        public long DeleteDirectory(string Name, bool Recursive)
        {
            CheckIfDecendentOfRootPath(Name);

            string DirName = Name;

            if (!Directory.Exists(DirName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            Directory.Delete(DirName, Recursive);

            return 0;
        }

        public long DeleteFile(string Name)
        {
            CheckIfDecendentOfRootPath(Name);

            if (!File.Exists(Name))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }
            else
            {
                File.Delete(Name);
            }

            return 0;
        }

        public DirectoryEntry[] GetDirectories(string Path)
        {
            CheckIfDecendentOfRootPath(Path);

            List<DirectoryEntry> Entries = new List<DirectoryEntry>();

            foreach(string Directory in Directory.EnumerateDirectories(Path))
            {
                DirectoryEntry DirectoryEntry = new DirectoryEntry(Directory, DirectoryEntryType.Directory);

                Entries.Add(DirectoryEntry);
            }

            return Entries.ToArray();
        }

        public DirectoryEntry[] GetEntries(string Path)
        {
            CheckIfDecendentOfRootPath(Path);

            if (Directory.Exists(Path))
            {
                List<DirectoryEntry> Entries = new List<DirectoryEntry>();

                foreach (string Directory in Directory.EnumerateDirectories(Path))
                {
                    DirectoryEntry DirectoryEntry = new DirectoryEntry(Directory, DirectoryEntryType.Directory);

                    Entries.Add(DirectoryEntry);
                }

                foreach (string File in Directory.EnumerateFiles(Path))
                {
                    FileInfo       FileInfo       = new FileInfo(File);
                    DirectoryEntry DirectoryEntry = new DirectoryEntry(File, DirectoryEntryType.File, FileInfo.Length);

                    Entries.Add(DirectoryEntry);
                }
            }

            return null;
        }

        public DirectoryEntry[] GetFiles(string Path)
        {
            CheckIfDecendentOfRootPath(Path);

            List<DirectoryEntry> Entries = new List<DirectoryEntry>();

            foreach (string File in Directory.EnumerateFiles(Path))
            {
                FileInfo       FileInfo       = new FileInfo(File);
                DirectoryEntry DirectoryEntry = new DirectoryEntry(File, DirectoryEntryType.File, FileInfo.Length);

                Entries.Add(DirectoryEntry);
            }

            return Entries.ToArray();
        }

        public long GetFreeSpace(ServiceCtx Context)
        {
            return Context.Device.FileSystem.GetDrive().AvailableFreeSpace;
        }

        public string GetFullPath(string Name)
        {
            if (Name.StartsWith("//"))
            {
                Name = Name.Substring(2);
            }
            else if (Name.StartsWith('/'))
            {
                Name = Name.Substring(1);
            }
            else
            {
                return null;
            }

            string FullPath = Path.Combine(BasePath, Name);

            CheckIfDecendentOfRootPath(FullPath);

            return FullPath;
        }

        public long GetTotalSpace(ServiceCtx Context)
        {
            return Context.Device.FileSystem.GetDrive().TotalSize;
        }

        public bool DirectoryExists(string Name)
        {
            CheckIfDecendentOfRootPath(Name);

            return Directory.Exists(Name);
        }

        public bool FileExists(string Name)
        {
            CheckIfDecendentOfRootPath(Name);

            return File.Exists(Name);
        }

        public long OpenDirectory(string Name, int FilterFlags, out IDirectory DirectoryInterface)
        {
            CheckIfDecendentOfRootPath(Name);

            if (Directory.Exists(Name))
            {
                DirectoryInterface = new IDirectory(Name, FilterFlags, this);

                return 0;
            }

            DirectoryInterface = null;

            return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
        }

        public long OpenFile(string Name, out IFile FileInterface)
        {
            CheckIfDecendentOfRootPath(Name);

            if (File.Exists(Name))
            {
                FileStream Stream = new FileStream(Name, FileMode.Open);

                FileInterface = new IFile(Stream, Name);

                return 0;
            }

            FileInterface = null;

            return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
        }

        public long RenameDirectory(string OldName, string NewName)
        {
            CheckIfDecendentOfRootPath(OldName);
            CheckIfDecendentOfRootPath(NewName);

            if (Directory.Exists(OldName))
            {
                Directory.Move(OldName, NewName);
            }
            else
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            return 0;
        }

        public long RenameFile(string OldName, string NewName)
        {
            CheckIfDecendentOfRootPath(OldName);
            CheckIfDecendentOfRootPath(NewName);

            if (File.Exists(OldName))
            {
                File.Move(OldName, NewName);
            }
            else
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            return 0;
        }

        public void CheckIfDecendentOfRootPath(string Path)
        {
            DirectoryInfo PathInfo = new DirectoryInfo(Path);
            DirectoryInfo RootInfo = new DirectoryInfo(RootPath);

            while (PathInfo.Parent != null)
            {
                if (PathInfo.Parent.FullName == RootInfo.FullName)
                {
                    return;
                }
                else
                {
                    PathInfo = PathInfo.Parent;
                }
            }

            throw new InvalidOperationException($"Path {Path} is not a child directory of {RootPath}");
        }
    }
}
