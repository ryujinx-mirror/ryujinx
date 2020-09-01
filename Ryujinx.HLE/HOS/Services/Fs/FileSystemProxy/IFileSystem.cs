using LibHac;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using static Ryujinx.HLE.Utilities.StringUtils;

namespace Ryujinx.HLE.HOS.Services.Fs.FileSystemProxy
{
    class IFileSystem : IpcService
    {
        private LibHac.Fs.Fsa.IFileSystem _fileSystem;

        public IFileSystem(LibHac.Fs.Fsa.IFileSystem provider)
        {
            _fileSystem = provider;
        }

        public LibHac.Fs.Fsa.IFileSystem GetBaseFileSystem()
        {
            return _fileSystem;
        }

        [Command(0)]
        // CreateFile(u32 createOption, u64 size, buffer<bytes<0x301>, 0x19, 0x301> path)
        public ResultCode CreateFile(ServiceCtx context)
        {
            U8Span name = ReadUtf8Span(context);

            CreateFileOptions createOption = (CreateFileOptions)context.RequestData.ReadInt32();
            context.RequestData.BaseStream.Position += 4;

            long size = context.RequestData.ReadInt64();

            return (ResultCode)_fileSystem.CreateFile(name, size, createOption).Value;
        }

        [Command(1)]
        // DeleteFile(buffer<bytes<0x301>, 0x19, 0x301> path)
        public ResultCode DeleteFile(ServiceCtx context)
        {
            U8Span name = ReadUtf8Span(context);

            return (ResultCode)_fileSystem.DeleteFile(name).Value;
        }

        [Command(2)]
        // CreateDirectory(buffer<bytes<0x301>, 0x19, 0x301> path)
        public ResultCode CreateDirectory(ServiceCtx context)
        {
            U8Span name = ReadUtf8Span(context);

            return (ResultCode)_fileSystem.CreateDirectory(name).Value;
        }

        [Command(3)]
        // DeleteDirectory(buffer<bytes<0x301>, 0x19, 0x301> path)
        public ResultCode DeleteDirectory(ServiceCtx context)
        {
            U8Span name = ReadUtf8Span(context);

            return (ResultCode)_fileSystem.DeleteDirectory(name).Value;
        }

        [Command(4)]
        // DeleteDirectoryRecursively(buffer<bytes<0x301>, 0x19, 0x301> path)
        public ResultCode DeleteDirectoryRecursively(ServiceCtx context)
        {
            U8Span name = ReadUtf8Span(context);

            return (ResultCode)_fileSystem.DeleteDirectoryRecursively(name).Value;
        }

        [Command(5)]
        // RenameFile(buffer<bytes<0x301>, 0x19, 0x301> oldPath, buffer<bytes<0x301>, 0x19, 0x301> newPath)
        public ResultCode RenameFile(ServiceCtx context)
        {
            U8Span oldName = ReadUtf8Span(context, 0);
            U8Span newName = ReadUtf8Span(context, 1);

            return (ResultCode)_fileSystem.RenameFile(oldName, newName).Value;
        }

        [Command(6)]
        // RenameDirectory(buffer<bytes<0x301>, 0x19, 0x301> oldPath, buffer<bytes<0x301>, 0x19, 0x301> newPath)
        public ResultCode RenameDirectory(ServiceCtx context)
        {
            U8Span oldName = ReadUtf8Span(context, 0);
            U8Span newName = ReadUtf8Span(context, 1);

            return (ResultCode)_fileSystem.RenameDirectory(oldName, newName).Value;
        }

        [Command(7)]
        // GetEntryType(buffer<bytes<0x301>, 0x19, 0x301> path) -> nn::fssrv::sf::DirectoryEntryType
        public ResultCode GetEntryType(ServiceCtx context)
        {
            U8Span name = ReadUtf8Span(context);

            Result result = _fileSystem.GetEntryType(out DirectoryEntryType entryType, name);

            context.ResponseData.Write((int)entryType);

            return (ResultCode)result.Value;
        }

        [Command(8)]
        // OpenFile(u32 mode, buffer<bytes<0x301>, 0x19, 0x301> path) -> object<nn::fssrv::sf::IFile> file
        public ResultCode OpenFile(ServiceCtx context)
        {
            OpenMode mode = (OpenMode)context.RequestData.ReadInt32();

            U8Span name = ReadUtf8Span(context);

            Result result = _fileSystem.OpenFile(out LibHac.Fs.Fsa.IFile file, name, mode);

            if (result.IsSuccess())
            {
                IFile fileInterface = new IFile(file);

                MakeObject(context, fileInterface);
            }

            return (ResultCode)result.Value;
        }

        [Command(9)]
        // OpenDirectory(u32 filter_flags, buffer<bytes<0x301>, 0x19, 0x301> path) -> object<nn::fssrv::sf::IDirectory> directory
        public ResultCode OpenDirectory(ServiceCtx context)
        {
            OpenDirectoryMode mode = (OpenDirectoryMode)context.RequestData.ReadInt32();

            U8Span name = ReadUtf8Span(context);

            Result result = _fileSystem.OpenDirectory(out LibHac.Fs.Fsa.IDirectory dir, name, mode);

            if (result.IsSuccess())
            {
                IDirectory dirInterface = new IDirectory(dir);

                MakeObject(context, dirInterface);
            }

            return (ResultCode)result.Value;
        }

        [Command(10)]
        // Commit()
        public ResultCode Commit(ServiceCtx context)
        {
            return (ResultCode)_fileSystem.Commit().Value;
        }

        [Command(11)]
        // GetFreeSpaceSize(buffer<bytes<0x301>, 0x19, 0x301> path) -> u64 totalFreeSpace
        public ResultCode GetFreeSpaceSize(ServiceCtx context)
        {
            U8Span name = ReadUtf8Span(context);

            Result result = _fileSystem.GetFreeSpaceSize(out long size, name);

            context.ResponseData.Write(size);

            return (ResultCode)result.Value;
        }

        [Command(12)]
        // GetTotalSpaceSize(buffer<bytes<0x301>, 0x19, 0x301> path) -> u64 totalSize
        public ResultCode GetTotalSpaceSize(ServiceCtx context)
        {
            U8Span name = ReadUtf8Span(context);

            Result result = _fileSystem.GetTotalSpaceSize(out long size, name);

            context.ResponseData.Write(size);

            return (ResultCode)result.Value;
        }

        [Command(13)]
        // CleanDirectoryRecursively(buffer<bytes<0x301>, 0x19, 0x301> path)
        public ResultCode CleanDirectoryRecursively(ServiceCtx context)
        {
            U8Span name = ReadUtf8Span(context);

            return (ResultCode)_fileSystem.CleanDirectoryRecursively(name).Value;
        }

        [Command(14)]
        // GetFileTimeStampRaw(buffer<bytes<0x301>, 0x19, 0x301> path) -> bytes<0x20> timestamp
        public ResultCode GetFileTimeStampRaw(ServiceCtx context)
        {
            U8Span name = ReadUtf8Span(context);

            Result result = _fileSystem.GetFileTimeStampRaw(out FileTimeStampRaw timestamp, name);

            context.ResponseData.Write(timestamp.Created);
            context.ResponseData.Write(timestamp.Modified);
            context.ResponseData.Write(timestamp.Accessed);

            byte[] data = new byte[8];

            // is valid?
            data[0] = 1;

            context.ResponseData.Write(data);

            return (ResultCode)result.Value;
        }
    }
}