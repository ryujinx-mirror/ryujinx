using LibHac.Fs;
using Ryujinx.HLE.HOS.Ipc;
using System;
using System.Collections.Generic;
using System.IO;
using Ryujinx.Common.Logging;
using static Ryujinx.HLE.HOS.ErrorCode;
using static Ryujinx.HLE.Utilities.StringUtils;

namespace Ryujinx.HLE.HOS.Services.FspSrv
{
    class IFileSystem : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        private HashSet<string> _openPaths;

        private LibHac.Fs.IFileSystem _provider;

        public IFileSystem(LibHac.Fs.IFileSystem provider)
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

            _provider = provider;
        }

        // CreateFile(u32 createOption, u64 size, buffer<bytes<0x301>, 0x19, 0x301> path)
        public long CreateFile(ServiceCtx context)
        {
            string name = ReadUtf8String(context);

            int createOption = context.RequestData.ReadInt32();
            context.RequestData.BaseStream.Position += 4;

            long size = context.RequestData.ReadInt64();

            if (name == null)
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            if (_provider.FileExists(name))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyExists);
            }

            if (IsPathAlreadyInUse(name))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyInUse);
            }

            try
            {
                _provider.CreateFile(name, size, (CreateFileOptions)createOption);
            }
            catch (DirectoryNotFoundException)
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }
            catch (UnauthorizedAccessException)
            {
                Logger.PrintError(LogClass.ServiceFs, $"Unable to access {name}");

                throw;
            }

            return 0;
        }

        // DeleteFile(buffer<bytes<0x301>, 0x19, 0x301> path)
        public long DeleteFile(ServiceCtx context)
        {
            string name = ReadUtf8String(context);

            if (!_provider.FileExists(name))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            if (IsPathAlreadyInUse(name))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyInUse);
            }

            try
            {
                _provider.DeleteFile(name);
            }
            catch (FileNotFoundException)
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }
            catch (UnauthorizedAccessException)
            {
                Logger.PrintError(LogClass.ServiceFs, $"Unable to access {name}");

                throw;
            }

            return 0;
        }

        // CreateDirectory(buffer<bytes<0x301>, 0x19, 0x301> path)
        public long CreateDirectory(ServiceCtx context)
        {
            string name = ReadUtf8String(context);

            if (name == null)
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            if (_provider.DirectoryExists(name))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyExists);
            }

            if (IsPathAlreadyInUse(name))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyInUse);
            }

            try
            {
                _provider.CreateDirectory(name);
            }
            catch (DirectoryNotFoundException)
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }
            catch (UnauthorizedAccessException)
            {
                Logger.PrintError(LogClass.ServiceFs, $"Unable to access {name}");

                throw;
            }

            return 0;
        }

        // DeleteDirectory(buffer<bytes<0x301>, 0x19, 0x301> path)
        public long DeleteDirectory(ServiceCtx context)
        {
            string name = ReadUtf8String(context);

            if (!_provider.DirectoryExists(name))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            if (IsPathAlreadyInUse(name))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyInUse);
            }

            try
            {
                _provider.DeleteDirectory(name);
            }
            catch (DirectoryNotFoundException)
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }
            catch (UnauthorizedAccessException)
            {
                Logger.PrintError(LogClass.ServiceFs, $"Unable to access {name}");

                throw;
            }

            return 0;
        }

        // DeleteDirectoryRecursively(buffer<bytes<0x301>, 0x19, 0x301> path)
        public long DeleteDirectoryRecursively(ServiceCtx context)
        {
            string name = ReadUtf8String(context);

            if (!_provider.DirectoryExists(name))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            if (IsPathAlreadyInUse(name))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyInUse);
            }

            try
            {
                _provider.DeleteDirectoryRecursively(name);
            }
            catch (UnauthorizedAccessException)
            {
                Logger.PrintError(LogClass.ServiceFs, $"Unable to access {name}");

                throw;
            }

            return 0;
        }

        // RenameFile(buffer<bytes<0x301>, 0x19, 0x301> oldPath, buffer<bytes<0x301>, 0x19, 0x301> newPath)
        public long RenameFile(ServiceCtx context)
        {
            string oldName = ReadUtf8String(context, 0);
            string newName = ReadUtf8String(context, 1);

            if (_provider.FileExists(oldName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            if (_provider.FileExists(newName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyExists);
            }

            if (IsPathAlreadyInUse(oldName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyInUse);
            }

            try
            {
                _provider.RenameFile(oldName, newName);
            }
            catch (FileNotFoundException)
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }
            catch (UnauthorizedAccessException)
            {
                Logger.PrintError(LogClass.ServiceFs, $"Unable to access {oldName} or {newName}");

                throw;
            }

            return 0;
        }

        // RenameDirectory(buffer<bytes<0x301>, 0x19, 0x301> oldPath, buffer<bytes<0x301>, 0x19, 0x301> newPath)
        public long RenameDirectory(ServiceCtx context)
        {
            string oldName = ReadUtf8String(context, 0);
            string newName = ReadUtf8String(context, 1);

            if (!_provider.DirectoryExists(oldName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            if (!_provider.DirectoryExists(newName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyExists);
            }

            if (IsPathAlreadyInUse(oldName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyInUse);
            }

            try
            {
                _provider.RenameFile(oldName, newName);
            }
            catch (DirectoryNotFoundException)
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }
            catch (UnauthorizedAccessException)
            {
                Logger.PrintError(LogClass.ServiceFs, $"Unable to access {oldName} or {newName}");

                throw;
            }

            return 0;
        }

        // GetEntryType(buffer<bytes<0x301>, 0x19, 0x301> path) -> nn::fssrv::sf::DirectoryEntryType
        public long GetEntryType(ServiceCtx context)
        {
            string name = ReadUtf8String(context);

            try
            {
                LibHac.Fs.DirectoryEntryType entryType = _provider.GetEntryType(name);

                context.ResponseData.Write((int)entryType);
            }
            catch (FileNotFoundException)
            {
                context.ResponseData.Write(0);

                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            return 0;
        }

        // OpenFile(u32 mode, buffer<bytes<0x301>, 0x19, 0x301> path) -> object<nn::fssrv::sf::IFile> file
        public long OpenFile(ServiceCtx context)
        {
            int mode = context.RequestData.ReadInt32();

            string name = ReadUtf8String(context);

            if (!_provider.FileExists(name))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            if (IsPathAlreadyInUse(name))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyInUse);
            }

            IFile fileInterface;

            try
            {
                LibHac.Fs.IFile file = _provider.OpenFile(name, (OpenMode)mode);

                fileInterface = new IFile(file, name);
            }
            catch (UnauthorizedAccessException)
            {
                Logger.PrintError(LogClass.ServiceFs, $"Unable to access {name}");

                throw;
            }

            fileInterface.Disposed += RemoveFileInUse;

            lock (_openPaths)
            {
                _openPaths.Add(fileInterface.Path);
            }

            MakeObject(context, fileInterface);

            return 0;
        }

        // OpenDirectory(u32 filter_flags, buffer<bytes<0x301>, 0x19, 0x301> path) -> object<nn::fssrv::sf::IDirectory> directory
        public long OpenDirectory(ServiceCtx context)
        {
            int mode = context.RequestData.ReadInt32();

            string name = ReadUtf8String(context);

            if (!_provider.DirectoryExists(name))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            if (IsPathAlreadyInUse(name))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyInUse);
            }

            IDirectory dirInterface;

            try
            {
                LibHac.Fs.IDirectory dir = _provider.OpenDirectory(name, (OpenDirectoryMode) mode);

                dirInterface = new IDirectory(dir);
            }
            catch (UnauthorizedAccessException)
            {
                Logger.PrintError(LogClass.ServiceFs, $"Unable to access {name}");

                throw;
            }

            dirInterface.Disposed += RemoveDirectoryInUse;

            lock (_openPaths)
            {
                _openPaths.Add(dirInterface.Path);
            }

            MakeObject(context, dirInterface);

            return 0;
        }

        // Commit()
        public long Commit(ServiceCtx context)
        {
            _provider.Commit();

            return 0;
        }

        // GetFreeSpaceSize(buffer<bytes<0x301>, 0x19, 0x301> path) -> u64 totalFreeSpace
        public long GetFreeSpaceSize(ServiceCtx context)
        {
            string name = ReadUtf8String(context);

            context.ResponseData.Write(_provider.GetFreeSpaceSize(name));

            return 0;
        }

        // GetTotalSpaceSize(buffer<bytes<0x301>, 0x19, 0x301> path) -> u64 totalSize
        public long GetTotalSpaceSize(ServiceCtx context)
        {
            string name = ReadUtf8String(context);

            context.ResponseData.Write(_provider.GetTotalSpaceSize(name));

            return 0;
        }

        // CleanDirectoryRecursively(buffer<bytes<0x301>, 0x19, 0x301> path)
        public long CleanDirectoryRecursively(ServiceCtx context)
        {
            string name = ReadUtf8String(context);

            if (!_provider.DirectoryExists(name))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            if (IsPathAlreadyInUse(name))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyInUse);
            }

            try
            {
                _provider.CleanDirectoryRecursively(name);
            }
            catch (UnauthorizedAccessException)
            {
                Logger.PrintError(LogClass.ServiceFs, $"Unable to access {name}");

                throw;
            }

            return 0;
        }

        // GetFileTimeStampRaw(buffer<bytes<0x301>, 0x19, 0x301> path) -> bytes<0x20> timestamp
        public long GetFileTimeStampRaw(ServiceCtx context)
        {
            string name = ReadUtf8String(context);

            if (_provider.FileExists(name) || _provider.DirectoryExists(name))
            {
                FileTimeStampRaw timestamp = _provider.GetFileTimeStampRaw(name);

                context.ResponseData.Write(timestamp.Created);
                context.ResponseData.Write(timestamp.Modified);
                context.ResponseData.Write(timestamp.Accessed);

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

                _openPaths.Remove(fileInterface.Path);
            }
        }

        private void RemoveDirectoryInUse(object sender, EventArgs e)
        {
            IDirectory dirInterface = (IDirectory)sender;

            lock (_openPaths)
            {
                dirInterface.Disposed -= RemoveDirectoryInUse;

                _openPaths.Remove(dirInterface.Path);
            }
        }
    }
}