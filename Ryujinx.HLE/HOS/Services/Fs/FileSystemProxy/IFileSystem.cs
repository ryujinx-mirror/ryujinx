using LibHac;
using LibHac.Fs;

using static Ryujinx.HLE.Utilities.StringUtils;

namespace Ryujinx.HLE.HOS.Services.Fs.FileSystemProxy
{
    class IFileSystem : IpcService
    {
        private LibHac.Fs.IFileSystem _fileSystem;

        public IFileSystem(LibHac.Fs.IFileSystem provider)
        {
            _fileSystem = provider;
        }

        [Command(0)]
        // CreateFile(u32 createOption, u64 size, buffer<bytes<0x301>, 0x19, 0x301> path)
        public ResultCode CreateFile(ServiceCtx context)
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
                return (ResultCode)ex.ResultValue.Value;
            }

            return ResultCode.Success;
        }

        [Command(1)]
        // DeleteFile(buffer<bytes<0x301>, 0x19, 0x301> path)
        public ResultCode DeleteFile(ServiceCtx context)
        {
            string name = ReadUtf8String(context);

            try
            {
                _fileSystem.DeleteFile(name);
            }
            catch (HorizonResultException ex)
            {
                return (ResultCode)ex.ResultValue.Value;
            }

            return ResultCode.Success;
        }

        [Command(2)]
        // CreateDirectory(buffer<bytes<0x301>, 0x19, 0x301> path)
        public ResultCode CreateDirectory(ServiceCtx context)
        {
            string name = ReadUtf8String(context);

            try
            {
                _fileSystem.CreateDirectory(name);
            }
            catch (HorizonResultException ex)
            {
                return (ResultCode)ex.ResultValue.Value;
            }

            return ResultCode.Success;
        }

        [Command(3)]
        // DeleteDirectory(buffer<bytes<0x301>, 0x19, 0x301> path)
        public ResultCode DeleteDirectory(ServiceCtx context)
        {
            string name = ReadUtf8String(context);

            try
            {
                _fileSystem.DeleteDirectory(name);
            }
            catch (HorizonResultException ex)
            {
                return (ResultCode)ex.ResultValue.Value;
            }

            return ResultCode.Success;
        }

        [Command(4)]
        // DeleteDirectoryRecursively(buffer<bytes<0x301>, 0x19, 0x301> path)
        public ResultCode DeleteDirectoryRecursively(ServiceCtx context)
        {
            string name = ReadUtf8String(context);

            try
            {
                _fileSystem.DeleteDirectoryRecursively(name);
            }
            catch (HorizonResultException ex)
            {
                return (ResultCode)ex.ResultValue.Value;
            }

            return ResultCode.Success;
        }

        [Command(5)]
        // RenameFile(buffer<bytes<0x301>, 0x19, 0x301> oldPath, buffer<bytes<0x301>, 0x19, 0x301> newPath)
        public ResultCode RenameFile(ServiceCtx context)
        {
            string oldName = ReadUtf8String(context, 0);
            string newName = ReadUtf8String(context, 1);

            try
            {
                _fileSystem.RenameFile(oldName, newName);
            }
            catch (HorizonResultException ex)
            {
                return (ResultCode)ex.ResultValue.Value;
            }

            return ResultCode.Success;
        }

        [Command(6)]
        // RenameDirectory(buffer<bytes<0x301>, 0x19, 0x301> oldPath, buffer<bytes<0x301>, 0x19, 0x301> newPath)
        public ResultCode RenameDirectory(ServiceCtx context)
        {
            string oldName = ReadUtf8String(context, 0);
            string newName = ReadUtf8String(context, 1);

            try
            {
                _fileSystem.RenameDirectory(oldName, newName);
            }
            catch (HorizonResultException ex)
            {
                return (ResultCode)ex.ResultValue.Value;
            }

            return ResultCode.Success;
        }

        [Command(7)]
        // GetEntryType(buffer<bytes<0x301>, 0x19, 0x301> path) -> nn::fssrv::sf::DirectoryEntryType
        public ResultCode GetEntryType(ServiceCtx context)
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
                    return ResultCode.PathDoesNotExist;
                }
            }
            catch (HorizonResultException ex)
            {
                return (ResultCode)ex.ResultValue.Value;
            }

            return ResultCode.Success;
        }

        [Command(8)]
        // OpenFile(u32 mode, buffer<bytes<0x301>, 0x19, 0x301> path) -> object<nn::fssrv::sf::IFile> file
        public ResultCode OpenFile(ServiceCtx context)
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
                return (ResultCode)ex.ResultValue.Value;
            }

            return ResultCode.Success;
        }

        [Command(9)]
        // OpenDirectory(u32 filter_flags, buffer<bytes<0x301>, 0x19, 0x301> path) -> object<nn::fssrv::sf::IDirectory> directory
        public ResultCode OpenDirectory(ServiceCtx context)
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
                return (ResultCode)ex.ResultValue.Value;
            }

            return ResultCode.Success;
        }

        [Command(10)]
        // Commit()
        public ResultCode Commit(ServiceCtx context)
        {
            try
            {
                _fileSystem.Commit();
            }
            catch (HorizonResultException ex)
            {
                return (ResultCode)ex.ResultValue.Value;
            }

            return ResultCode.Success;
        }

        [Command(11)]
        // GetFreeSpaceSize(buffer<bytes<0x301>, 0x19, 0x301> path) -> u64 totalFreeSpace
        public ResultCode GetFreeSpaceSize(ServiceCtx context)
        {
            string name = ReadUtf8String(context);

            try
            {
                context.ResponseData.Write(_fileSystem.GetFreeSpaceSize(name));
            }
            catch (HorizonResultException ex)
            {
                return (ResultCode)ex.ResultValue.Value;
            }

            return ResultCode.Success;
        }

        [Command(12)]
        // GetTotalSpaceSize(buffer<bytes<0x301>, 0x19, 0x301> path) -> u64 totalSize
        public ResultCode GetTotalSpaceSize(ServiceCtx context)
        {
            string name = ReadUtf8String(context);

            try
            {
                context.ResponseData.Write(_fileSystem.GetTotalSpaceSize(name));
            }
            catch (HorizonResultException ex)
            {
                return (ResultCode)ex.ResultValue.Value;
            }

            return ResultCode.Success;
        }

        [Command(13)]
        // CleanDirectoryRecursively(buffer<bytes<0x301>, 0x19, 0x301> path)
        public ResultCode CleanDirectoryRecursively(ServiceCtx context)
        {
            string name = ReadUtf8String(context);

            try
            {
                _fileSystem.CleanDirectoryRecursively(name);
            }
            catch (HorizonResultException ex)
            {
                return (ResultCode)ex.ResultValue.Value;
            }

            return ResultCode.Success;
        }

        [Command(14)]
        // GetFileTimeStampRaw(buffer<bytes<0x301>, 0x19, 0x301> path) -> bytes<0x20> timestamp
        public ResultCode GetFileTimeStampRaw(ServiceCtx context)
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
                return (ResultCode)ex.ResultValue.Value;
            }

            return ResultCode.Success;
        }
    }
}