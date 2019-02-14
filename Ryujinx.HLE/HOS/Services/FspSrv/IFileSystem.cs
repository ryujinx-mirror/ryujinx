using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS.Ipc;
using System;
using System.Collections.Generic;
using System.IO;

using static Ryujinx.HLE.HOS.ErrorCode;
using static Ryujinx.HLE.Utilities.StringUtils;

namespace Ryujinx.HLE.HOS.Services.FspSrv
{
    class IFileSystem : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        private HashSet<string> _openPaths;

        private string _path;

        private IFileSystemProvider _provider;

        public IFileSystem(string path, IFileSystemProvider provider)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0,  CreateFile                 },
                { 1,  DeleteFile                 },
                { 2,  CreateDirectory            },
                { 3,  DeleteDirectory            },
                { 4,  DeleteDirectoryRecursively },
                { 5,  RenameFile                 },
                { 6,  RenameDirectory            },
                { 7,  GetEntryType               },
                { 8,  OpenFile                   },
                { 9,  OpenDirectory              },
                { 10, Commit                     },
                { 11, GetFreeSpaceSize           },
                { 12, GetTotalSpaceSize          },
                { 13, CleanDirectoryRecursively  },
                { 14, GetFileTimeStampRaw        }
            };

            _openPaths = new HashSet<string>();

            _path     = path;
            _provider = provider;
        }

        // CreateFile(u32 mode, u64 size, buffer<bytes<0x301>, 0x19, 0x301> path)
        public long CreateFile(ServiceCtx context)
        {
            string name = ReadUtf8String(context);

            long mode = context.RequestData.ReadInt64();
            int  size = context.RequestData.ReadInt32();

            string fileName = _provider.GetFullPath(name);

            if (fileName == null)
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            if (_provider.FileExists(fileName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyExists);
            }

            if (IsPathAlreadyInUse(fileName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyInUse);
            }

            return _provider.CreateFile(fileName, size);
        }

        // DeleteFile(buffer<bytes<0x301>, 0x19, 0x301> path)
        public long DeleteFile(ServiceCtx context)
        {
            string name = ReadUtf8String(context);

            string fileName = _provider.GetFullPath(name);

            if (!_provider.FileExists(fileName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            if (IsPathAlreadyInUse(fileName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyInUse);
            }

            return _provider.DeleteFile(fileName);
        }

        // CreateDirectory(buffer<bytes<0x301>, 0x19, 0x301> path)
        public long CreateDirectory(ServiceCtx context)
        {
            string name = ReadUtf8String(context);

            string dirName = _provider.GetFullPath(name);

            if (dirName == null)
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            if (_provider.DirectoryExists(dirName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyExists);
            }

            if (IsPathAlreadyInUse(dirName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyInUse);
            }

            _provider.CreateDirectory(dirName);

            return 0;
        }

        // DeleteDirectory(buffer<bytes<0x301>, 0x19, 0x301> path)
        public long DeleteDirectory(ServiceCtx context)
        {
            return DeleteDirectory(context, false);
        }

        // DeleteDirectoryRecursively(buffer<bytes<0x301>, 0x19, 0x301> path)
        public long DeleteDirectoryRecursively(ServiceCtx context)
        {
            return DeleteDirectory(context, true);
        }
        
        private long DeleteDirectory(ServiceCtx context, bool recursive)
        {
            string name = ReadUtf8String(context);

            string dirName = _provider.GetFullPath(name);

            if (!Directory.Exists(dirName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            if (IsPathAlreadyInUse(dirName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyInUse);
            }

            _provider.DeleteDirectory(dirName, recursive);

            return 0;
        }

        // RenameFile(buffer<bytes<0x301>, 0x19, 0x301> oldPath, buffer<bytes<0x301>, 0x19, 0x301> newPath)
        public long RenameFile(ServiceCtx context)
        {
            string oldName = ReadUtf8String(context, 0);
            string newName = ReadUtf8String(context, 1);

            string oldFileName = _provider.GetFullPath(oldName);
            string newFileName = _provider.GetFullPath(newName);

            if (_provider.FileExists(oldFileName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            if (_provider.FileExists(newFileName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyExists);
            }

            if (IsPathAlreadyInUse(oldFileName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyInUse);
            }

            return _provider.RenameFile(oldFileName, newFileName);
        }

        // RenameDirectory(buffer<bytes<0x301>, 0x19, 0x301> oldPath, buffer<bytes<0x301>, 0x19, 0x301> newPath)
        public long RenameDirectory(ServiceCtx context)
        {
            string oldName = ReadUtf8String(context, 0);
            string newName = ReadUtf8String(context, 1);

            string oldDirName = _provider.GetFullPath(oldName);
            string newDirName = _provider.GetFullPath(newName);

            if (!_provider.DirectoryExists(oldDirName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            if (!_provider.DirectoryExists(newDirName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyExists);
            }

            if (IsPathAlreadyInUse(oldDirName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyInUse);
            }

            return _provider.RenameDirectory(oldDirName, newDirName);
        }

        // GetEntryType(buffer<bytes<0x301>, 0x19, 0x301> path) -> nn::fssrv::sf::DirectoryEntryType
        public long GetEntryType(ServiceCtx context)
        {
            string name = ReadUtf8String(context);

            string fileName = _provider.GetFullPath(name);

            if (_provider.FileExists(fileName))
            {
                context.ResponseData.Write(1);
            }
            else if (_provider.DirectoryExists(fileName))
            {
                context.ResponseData.Write(0);
            }
            else
            {
                context.ResponseData.Write(0);

                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            return 0;
        }

        // OpenFile(u32 mode, buffer<bytes<0x301>, 0x19, 0x301> path) -> object<nn::fssrv::sf::IFile> file
        public long OpenFile(ServiceCtx context)
        {
            int filterFlags = context.RequestData.ReadInt32();

            string name = ReadUtf8String(context);

            string fileName = _provider.GetFullPath(name);

            if (!_provider.FileExists(fileName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            if (IsPathAlreadyInUse(fileName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyInUse);
            }


            long error = _provider.OpenFile(fileName, out IFile fileInterface);

            if (error == 0)
            {
                fileInterface.Disposed += RemoveFileInUse;

                lock (_openPaths)
                {
                    _openPaths.Add(fileName);
                }

                MakeObject(context, fileInterface);

                return 0;
            }

            return error;
        }

        // OpenDirectory(u32 filter_flags, buffer<bytes<0x301>, 0x19, 0x301> path) -> object<nn::fssrv::sf::IDirectory> directory
        public long OpenDirectory(ServiceCtx context)
        {
            int filterFlags = context.RequestData.ReadInt32();

            string name = ReadUtf8String(context);

            string dirName = _provider.GetFullPath(name);

            if (!_provider.DirectoryExists(dirName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            if (IsPathAlreadyInUse(dirName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyInUse);
            }

            long error = _provider.OpenDirectory(dirName, filterFlags, out IDirectory dirInterface);

            if (error == 0)
            {
                dirInterface.Disposed += RemoveDirectoryInUse;

                lock (_openPaths)
                {
                    _openPaths.Add(dirName);
                }

                MakeObject(context, dirInterface);
            }

            return error;
        }

        // Commit()
        public long Commit(ServiceCtx context)
        {
            return 0;
        }

        // GetFreeSpaceSize(buffer<bytes<0x301>, 0x19, 0x301> path) -> u64 totalFreeSpace
        public long GetFreeSpaceSize(ServiceCtx context)
        {
            string name = ReadUtf8String(context);

            context.ResponseData.Write(_provider.GetFreeSpace(context));

            return 0;
        }

        // GetTotalSpaceSize(buffer<bytes<0x301>, 0x19, 0x301> path) -> u64 totalSize
        public long GetTotalSpaceSize(ServiceCtx context)
        {
            string name = ReadUtf8String(context);

            context.ResponseData.Write(_provider.GetFreeSpace(context));

            return 0;
        }

        // CleanDirectoryRecursively(buffer<bytes<0x301>, 0x19, 0x301> path)
        public long CleanDirectoryRecursively(ServiceCtx context)
        {
            string name = ReadUtf8String(context);

            string dirName = _provider.GetFullPath(name);

            if (!_provider.DirectoryExists(dirName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            if (IsPathAlreadyInUse(dirName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyInUse);
            }

            foreach (DirectoryEntry entry in _provider.GetEntries(dirName))
            {
                if (_provider.DirectoryExists(entry.Path))
                {
                    _provider.DeleteDirectory(entry.Path, true);
                }
                else if (_provider.FileExists(entry.Path))
                {
                   _provider.DeleteFile(entry.Path);
                }
            }

            return 0;
        }

        // GetFileTimeStampRaw(buffer<bytes<0x301>, 0x19, 0x301> path) -> bytes<0x20> timestamp
        public long GetFileTimeStampRaw(ServiceCtx context)
        {
            string name = ReadUtf8String(context);

            string path = _provider.GetFullPath(name);

            if (_provider.FileExists(path) || _provider.DirectoryExists(path))
            {
                FileTimestamp timestamp = _provider.GetFileTimeStampRaw(path);

                context.ResponseData.Write(new DateTimeOffset(timestamp.CreationDateTime).ToUnixTimeSeconds());
                context.ResponseData.Write(new DateTimeOffset(timestamp.ModifiedDateTime).ToUnixTimeSeconds());
                context.ResponseData.Write(new DateTimeOffset(timestamp.LastAccessDateTime).ToUnixTimeSeconds());

                byte[] data = new byte[8];

                // is valid?
                data[0] = 1;

                context.ResponseData.Write(data);

                return 0;
            }

            return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
        }

        private bool IsPathAlreadyInUse(string path)
        {
            lock (_openPaths)
            {
                return _openPaths.Contains(path);
            }
        }

        private void RemoveFileInUse(object sender, EventArgs e)
        {
            IFile fileInterface = (IFile)sender;

            lock (_openPaths)
            {
                fileInterface.Disposed -= RemoveFileInUse;

                _openPaths.Remove(fileInterface.HostPath);
            }
        }

        private void RemoveDirectoryInUse(object sender, EventArgs e)
        {
            IDirectory dirInterface = (IDirectory)sender;

            lock (_openPaths)
            {
                dirInterface.Disposed -= RemoveDirectoryInUse;

                _openPaths.Remove(dirInterface.DirectoryPath);
            }
        }
    }
}