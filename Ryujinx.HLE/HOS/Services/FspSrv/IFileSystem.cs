using LibHac;
using LibHac.Fs;
using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

using static Ryujinx.HLE.HOS.ErrorCode;
using static Ryujinx.HLE.Utilities.StringUtils;

namespace Ryujinx.HLE.HOS.Services.FspSrv
{
    class IFileSystem : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        private LibHac.Fs.IFileSystem _fileSystem;

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

            _fileSystem = provider;
        }

        // CreateFile(u32 createOption, u64 size, buffer<bytes<0x301>, 0x19, 0x301> path)
        public long CreateFile(ServiceCtx context)
        {
            string name = ReadUtf8String(context);

            CreateFileOptions createOption = (CreateFileOptions)context.RequestData.ReadInt32();
            context.RequestData.BaseStream.Position += 4;

            long size = context.RequestData.ReadInt64();

            try
            {
                _fileSystem.CreateFile(name, size, createOption);
            }
            catch (HorizonResultException ex)
            {
                return ex.ResultValue.Value;
            }

            return 0;
        }

        // DeleteFile(buffer<bytes<0x301>, 0x19, 0x301> path)
        public long DeleteFile(ServiceCtx context)
        {
            string name = ReadUtf8String(context);

            try
            {
                _fileSystem.DeleteFile(name);
            }
            catch (HorizonResultException ex)
            {
                return ex.ResultValue.Value;
            }

            return 0;
        }

        // CreateDirectory(buffer<bytes<0x301>, 0x19, 0x301> path)
        public long CreateDirectory(ServiceCtx context)
        {
            string name = ReadUtf8String(context);

            try
            {
                _fileSystem.CreateDirectory(name);
            }
            catch (HorizonResultException ex)
            {
                return ex.ResultValue.Value;
            }

            return 0;
        }

        // DeleteDirectory(buffer<bytes<0x301>, 0x19, 0x301> path)
        public long DeleteDirectory(ServiceCtx context)
        {
            string name = ReadUtf8String(context);

            try
            {
                _fileSystem.DeleteDirectory(name);
            }
            catch (HorizonResultException ex)
            {
                return ex.ResultValue.Value;
            }

            return 0;
        }

        // DeleteDirectoryRecursively(buffer<bytes<0x301>, 0x19, 0x301> path)
        public long DeleteDirectoryRecursively(ServiceCtx context)
        {
            string name = ReadUtf8String(context);

            try
            {
                _fileSystem.DeleteDirectoryRecursively(name);
            }
            catch (HorizonResultException ex)
            {
                return ex.ResultValue.Value;
            }

            return 0;
        }

        // RenameFile(buffer<bytes<0x301>, 0x19, 0x301> oldPath, buffer<bytes<0x301>, 0x19, 0x301> newPath)
        public long RenameFile(ServiceCtx context)
        {
            string oldName = ReadUtf8String(context, 0);
            string newName = ReadUtf8String(context, 1);

            try
            {
                _fileSystem.RenameFile(oldName, newName);
            }
            catch (HorizonResultException ex)
            {
                return ex.ResultValue.Value;
            }

            return 0;
        }

        // RenameDirectory(buffer<bytes<0x301>, 0x19, 0x301> oldPath, buffer<bytes<0x301>, 0x19, 0x301> newPath)
        public long RenameDirectory(ServiceCtx context)
        {
            string oldName = ReadUtf8String(context, 0);
            string newName = ReadUtf8String(context, 1);

            try
            {
                _fileSystem.RenameDirectory(oldName, newName);
            }
            catch (HorizonResultException ex)
            {
                return ex.ResultValue.Value;
            }

            return 0;
        }

        // GetEntryType(buffer<bytes<0x301>, 0x19, 0x301> path) -> nn::fssrv::sf::DirectoryEntryType
        public long GetEntryType(ServiceCtx context)
        {
            string name = ReadUtf8String(context);

            try
            {
                DirectoryEntryType entryType = _fileSystem.GetEntryType(name);

                if (entryType == DirectoryEntryType.Directory || entryType == DirectoryEntryType.File)
                {
                    context.ResponseData.Write((int)entryType);
                }
                else
                {
                    return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
                }
            }
            catch (HorizonResultException ex)
            {
                return ex.ResultValue.Value;
            }

            return 0;
        }

        // OpenFile(u32 mode, buffer<bytes<0x301>, 0x19, 0x301> path) -> object<nn::fssrv::sf::IFile> file
        public long OpenFile(ServiceCtx context)
        {
            OpenMode mode = (OpenMode)context.RequestData.ReadInt32();

            string name = ReadUtf8String(context);

            try
            {
                LibHac.Fs.IFile file = _fileSystem.OpenFile(name, mode);

                IFile fileInterface = new IFile(file);

                MakeObject(context, fileInterface);
            }
            catch (HorizonResultException ex)
            {
                return ex.ResultValue.Value;
            }

            return 0;
        }

        // OpenDirectory(u32 filter_flags, buffer<bytes<0x301>, 0x19, 0x301> path) -> object<nn::fssrv::sf::IDirectory> directory
        public long OpenDirectory(ServiceCtx context)
        {
            OpenDirectoryMode mode = (OpenDirectoryMode)context.RequestData.ReadInt32();

            string name = ReadUtf8String(context);

            try
            {
                LibHac.Fs.IDirectory dir = _fileSystem.OpenDirectory(name, mode);

                IDirectory dirInterface = new IDirectory(dir);

                MakeObject(context, dirInterface);
            }
            catch (HorizonResultException ex)
            {
                return ex.ResultValue.Value;
            }

            return 0;
        }

        // Commit()
        public long Commit(ServiceCtx context)
        {
            try
            {
                _fileSystem.Commit();
            }
            catch (HorizonResultException ex)
            {
                return ex.ResultValue.Value;
            }

            return 0;
        }

        // GetFreeSpaceSize(buffer<bytes<0x301>, 0x19, 0x301> path) -> u64 totalFreeSpace
        public long GetFreeSpaceSize(ServiceCtx context)
        {
            string name = ReadUtf8String(context);

            try
            {
                context.ResponseData.Write(_fileSystem.GetFreeSpaceSize(name));
            }
            catch (HorizonResultException ex)
            {
                return ex.ResultValue.Value;
            }

            return 0;
        }

        // GetTotalSpaceSize(buffer<bytes<0x301>, 0x19, 0x301> path) -> u64 totalSize
        public long GetTotalSpaceSize(ServiceCtx context)
        {
            string name = ReadUtf8String(context);

            try
            {
                context.ResponseData.Write(_fileSystem.GetTotalSpaceSize(name));
            }
            catch (HorizonResultException ex)
            {
                return ex.ResultValue.Value;
            }

            return 0;
        }

        // CleanDirectoryRecursively(buffer<bytes<0x301>, 0x19, 0x301> path)
        public long CleanDirectoryRecursively(ServiceCtx context)
        {
            string name = ReadUtf8String(context);

            try
            {
                _fileSystem.CleanDirectoryRecursively(name);
            }
            catch (HorizonResultException ex)
            {
                return ex.ResultValue.Value;
            }

            return 0;
        }

        // GetFileTimeStampRaw(buffer<bytes<0x301>, 0x19, 0x301> path) -> bytes<0x20> timestamp
        public long GetFileTimeStampRaw(ServiceCtx context)
        {
            string name = ReadUtf8String(context);

            try
            {
                FileTimeStampRaw timestamp = _fileSystem.GetFileTimeStampRaw(name);

                context.ResponseData.Write(timestamp.Created);
                context.ResponseData.Write(timestamp.Modified);
                context.ResponseData.Write(timestamp.Accessed);

                byte[] data = new byte[8];

                // is valid?
                data[0] = 1;

                context.ResponseData.Write(data);
            }
            catch (HorizonResultException ex)
            {
                return ex.ResultValue.Value;
            }

            return 0;
        }
    }
}