using LibHac;
using LibHac.Fs;
using LibHac.FsSrv;
using LibHac.FsSystem;
using LibHac.FsSystem.NcaUtils;
using LibHac.Ncm;
using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.Cpu;
using Ryujinx.HLE.HOS.Services.Fs.FileSystemProxy;
using System.IO;

using static Ryujinx.HLE.Utilities.StringUtils;
using StorageId = Ryujinx.HLE.FileSystem.StorageId;

namespace Ryujinx.HLE.HOS.Services.Fs
{
    [Service("fsp-srv")]
    class IFileSystemProxy : IpcService
    {
        private LibHac.FsSrv.IFileSystemProxy _baseFileSystemProxy;

        public IFileSystemProxy(ServiceCtx context)
        {
            _baseFileSystemProxy = context.Device.FileSystem.FsServer.CreateFileSystemProxyService();
        }

        [Command(1)]
        // Initialize(u64, pid)
        public ResultCode Initialize(ServiceCtx context)
        {
            return ResultCode.Success;
        }

        [Command(8)]
        // OpenFileSystemWithId(nn::fssrv::sf::FileSystemType filesystem_type, nn::ApplicationId tid, buffer<bytes<0x301>, 0x19, 0x301> path)
        // -> object<nn::fssrv::sf::IFileSystem> contentFs
        public ResultCode OpenFileSystemWithId(ServiceCtx context)
        {
            FileSystemType fileSystemType = (FileSystemType)context.RequestData.ReadInt32();
            long titleId = context.RequestData.ReadInt64();
            string switchPath = ReadUtf8String(context);
            string fullPath = context.Device.FileSystem.SwitchPathToSystemPath(switchPath);

            if (!File.Exists(fullPath))
            {
                if (fullPath.Contains("."))
                {
                    ResultCode result = FileSystemProxyHelper.OpenFileSystemFromInternalFile(context, fullPath, out FileSystemProxy.IFileSystem fileSystem);

                    if (result == ResultCode.Success)
                    {
                        MakeObject(context, fileSystem);
                    }

                    return result;
                }

                return ResultCode.PathDoesNotExist;
            }

            FileStream fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            string extension = Path.GetExtension(fullPath);

            if (extension == ".nca")
            {
                ResultCode result = FileSystemProxyHelper.OpenNcaFs(context, fullPath, fileStream.AsStorage(), out FileSystemProxy.IFileSystem fileSystem);

                if (result == ResultCode.Success)
                {
                    MakeObject(context, fileSystem);
                }

                return result;
            }
            else if (extension == ".nsp")
            {
                ResultCode result = FileSystemProxyHelper.OpenNsp(context, fullPath, out FileSystemProxy.IFileSystem fileSystem);

                if (result == ResultCode.Success)
                {
                    MakeObject(context, fileSystem);
                }

                return result;
            }

            return ResultCode.InvalidInput;
        }

        [Command(11)]
        // OpenBisFileSystem(nn::fssrv::sf::Partition partitionID, buffer<bytes<0x301>, 0x19, 0x301>) -> object<nn::fssrv::sf::IFileSystem> Bis
        public ResultCode OpenBisFileSystem(ServiceCtx context)
        {
            BisPartitionId bisPartitionId = (BisPartitionId)context.RequestData.ReadInt32();

            Result rc = FileSystemProxyHelper.ReadFsPath(out FsPath path, context);
            if (rc.IsFailure()) return (ResultCode)rc.Value;

            rc = _baseFileSystemProxy.OpenBisFileSystem(out LibHac.Fs.Fsa.IFileSystem fileSystem, ref path, bisPartitionId);
            if (rc.IsFailure()) return (ResultCode)rc.Value;

            MakeObject(context, new FileSystemProxy.IFileSystem(fileSystem));

            return ResultCode.Success;
        }

        [Command(18)]
        // OpenSdCardFileSystem() -> object<nn::fssrv::sf::IFileSystem>
        public ResultCode OpenSdCardFileSystem(ServiceCtx context)
        {
            Result rc = _baseFileSystemProxy.OpenSdCardFileSystem(out LibHac.Fs.Fsa.IFileSystem fileSystem);
            if (rc.IsFailure()) return (ResultCode)rc.Value;

            MakeObject(context, new FileSystemProxy.IFileSystem(fileSystem));

            return ResultCode.Success;
        }

        [Command(21)]
        public ResultCode DeleteSaveDataFileSystem(ServiceCtx context)
        {
            ulong saveDataId = context.RequestData.ReadUInt64();

            Result result = _baseFileSystemProxy.DeleteSaveDataFileSystem(saveDataId);

            return (ResultCode)result.Value;
        }

        [Command(22)]
        public ResultCode CreateSaveDataFileSystem(ServiceCtx context)
        {
            SaveDataAttribute attribute = context.RequestData.ReadStruct<SaveDataAttribute>();
            SaveDataCreationInfo creationInfo = context.RequestData.ReadStruct<SaveDataCreationInfo>();
            SaveMetaCreateInfo metaCreateInfo = context.RequestData.ReadStruct<SaveMetaCreateInfo>();

            // TODO: There's currently no program registry for FS to reference.
            // Workaround that by setting the application ID and owner ID if they're not already set
            if (attribute.ProgramId == ProgramId.InvalidId)
            {
                attribute.ProgramId = new ProgramId(context.Device.Application.TitleId);
            }

            Logger.Info?.Print(LogClass.ServiceFs, $"Creating save with title ID {attribute.ProgramId.Value:x16}");

            Result result = _baseFileSystemProxy.CreateSaveDataFileSystem(ref attribute, ref creationInfo, ref metaCreateInfo);

            return (ResultCode)result.Value;
        }

        [Command(23)]
        public ResultCode CreateSaveDataFileSystemBySystemSaveDataId(ServiceCtx context)
        {
            SaveDataAttribute attribute = context.RequestData.ReadStruct<SaveDataAttribute>();
            SaveDataCreationInfo creationInfo = context.RequestData.ReadStruct<SaveDataCreationInfo>();

            Result result = _baseFileSystemProxy.CreateSaveDataFileSystemBySystemSaveDataId(ref attribute, ref creationInfo);

            return (ResultCode)result.Value;
        }

        [Command(25)]
        public ResultCode DeleteSaveDataFileSystemBySaveDataSpaceId(ServiceCtx context)
        {
            SaveDataSpaceId spaceId = (SaveDataSpaceId)context.RequestData.ReadInt64();
            ulong saveDataId = context.RequestData.ReadUInt64();

            Result result = _baseFileSystemProxy.DeleteSaveDataFileSystemBySaveDataSpaceId(spaceId, saveDataId);

            return (ResultCode)result.Value;
        }

        [Command(28)]
        public ResultCode DeleteSaveDataFileSystemBySaveDataAttribute(ServiceCtx context)
        {
            SaveDataSpaceId spaceId = (SaveDataSpaceId)context.RequestData.ReadInt64();
            SaveDataAttribute attribute = context.RequestData.ReadStruct<SaveDataAttribute>();

            Result result = _baseFileSystemProxy.DeleteSaveDataFileSystemBySaveDataAttribute(spaceId, ref attribute);

            return (ResultCode)result.Value;
        }

        [Command(30)]
        // OpenGameCardStorage(u32, u32) -> object<nn::fssrv::sf::IStorage>
        public ResultCode OpenGameCardStorage(ServiceCtx context)
        {
            GameCardHandle handle = new GameCardHandle(context.RequestData.ReadInt32());
            GameCardPartitionRaw partitionId = (GameCardPartitionRaw)context.RequestData.ReadInt32();

            Result result = _baseFileSystemProxy.OpenGameCardStorage(out LibHac.Fs.IStorage storage, handle, partitionId);

            if (result.IsSuccess())
            {
                MakeObject(context, new FileSystemProxy.IStorage(storage));
            }

            return (ResultCode)result.Value;
        }

        [Command(35)]
        public ResultCode CreateSaveDataFileSystemWithHashSalt(ServiceCtx context)
        {
            SaveDataAttribute attribute = context.RequestData.ReadStruct<SaveDataAttribute>();
            SaveDataCreationInfo creationInfo = context.RequestData.ReadStruct<SaveDataCreationInfo>();
            SaveMetaCreateInfo metaCreateInfo = context.RequestData.ReadStruct<SaveMetaCreateInfo>();
            HashSalt hashSalt = context.RequestData.ReadStruct<HashSalt>();

            // TODO: There's currently no program registry for FS to reference.
            // Workaround that by setting the application ID and owner ID if they're not already set
            if (attribute.ProgramId == ProgramId.InvalidId)
            {
                attribute.ProgramId = new ProgramId(context.Device.Application.TitleId);
            }

            Result result = _baseFileSystemProxy.CreateSaveDataFileSystemWithHashSalt(ref attribute, ref creationInfo, ref metaCreateInfo, ref hashSalt);

            return (ResultCode)result.Value;
        }

        [Command(51)]
        // OpenSaveDataFileSystem(u8 save_data_space_id, nn::fssrv::sf::SaveStruct saveStruct) -> object<nn::fssrv::sf::IFileSystem> saveDataFs
        public ResultCode OpenSaveDataFileSystem(ServiceCtx context)
        {
            SaveDataSpaceId spaceId = (SaveDataSpaceId)context.RequestData.ReadInt64();
            SaveDataAttribute attribute = context.RequestData.ReadStruct<SaveDataAttribute>();

            // TODO: There's currently no program registry for FS to reference.
            // Workaround that by setting the application ID if it's not already set
            if (attribute.ProgramId == ProgramId.InvalidId)
            {
                attribute.ProgramId = new ProgramId(context.Device.Application.TitleId);
            }

            Result result = _baseFileSystemProxy.OpenSaveDataFileSystem(out LibHac.Fs.Fsa.IFileSystem fileSystem, spaceId, ref attribute);

            if (result.IsSuccess())
            {
                MakeObject(context, new FileSystemProxy.IFileSystem(fileSystem));
            }

            return (ResultCode)result.Value;
        }

        [Command(52)]
        // OpenSaveDataFileSystemBySystemSaveDataId(u8 save_data_space_id, nn::fssrv::sf::SaveStruct saveStruct) -> object<nn::fssrv::sf::IFileSystem> systemSaveDataFs
        public ResultCode OpenSaveDataFileSystemBySystemSaveDataId(ServiceCtx context)
        {
            SaveDataSpaceId spaceId = (SaveDataSpaceId)context.RequestData.ReadInt64();
            SaveDataAttribute attribute = context.RequestData.ReadStruct<SaveDataAttribute>();

            Result result = _baseFileSystemProxy.OpenSaveDataFileSystemBySystemSaveDataId(out LibHac.Fs.Fsa.IFileSystem fileSystem, spaceId, ref attribute);

            if (result.IsSuccess())
            {
                MakeObject(context, new FileSystemProxy.IFileSystem(fileSystem));
            }

            return (ResultCode)result.Value;
        }

        [Command(53)]
        // OpenReadOnlySaveDataFileSystem(u8 save_data_space_id, nn::fssrv::sf::SaveStruct save_struct) -> object<nn::fssrv::sf::IFileSystem>
        public ResultCode OpenReadOnlySaveDataFileSystem(ServiceCtx context)
        {
            SaveDataSpaceId spaceId = (SaveDataSpaceId)context.RequestData.ReadInt64();
            SaveDataAttribute attribute = context.RequestData.ReadStruct<SaveDataAttribute>();

            // TODO: There's currently no program registry for FS to reference.
            // Workaround that by setting the application ID if it's not already set
            if (attribute.ProgramId == ProgramId.InvalidId)
            {
                attribute.ProgramId = new ProgramId(context.Device.Application.TitleId);
            }

            Result result = _baseFileSystemProxy.OpenReadOnlySaveDataFileSystem(out LibHac.Fs.Fsa.IFileSystem fileSystem, spaceId, ref attribute);

            if (result.IsSuccess())
            {
                MakeObject(context, new FileSystemProxy.IFileSystem(fileSystem));
            }

            return (ResultCode)result.Value;
        }

        [Command(60)]
        public ResultCode OpenSaveDataInfoReader(ServiceCtx context)
        {
            Result result = _baseFileSystemProxy.OpenSaveDataInfoReader(out ReferenceCountedDisposable<LibHac.FsSrv.ISaveDataInfoReader> infoReader);

            if (result.IsSuccess())
            {
                MakeObject(context, new ISaveDataInfoReader(infoReader));
            }

            return (ResultCode)result.Value;
        }

        [Command(61)]
        public ResultCode OpenSaveDataInfoReaderBySaveDataSpaceId(ServiceCtx context)
        {
            SaveDataSpaceId spaceId = (SaveDataSpaceId)context.RequestData.ReadByte();

            Result result = _baseFileSystemProxy.OpenSaveDataInfoReaderBySaveDataSpaceId(out ReferenceCountedDisposable<LibHac.FsSrv.ISaveDataInfoReader> infoReader, spaceId);

            if (result.IsSuccess())
            {
                MakeObject(context, new ISaveDataInfoReader(infoReader));
            }

            return (ResultCode)result.Value;
        }

        [Command(62)]
        public ResultCode OpenSaveDataInfoReaderOnlyCacheStorage(ServiceCtx context)
        {
            SaveDataFilter filter = new SaveDataFilter();
            filter.SetSaveDataType(SaveDataType.Cache);
            filter.SetProgramId(new ProgramId(context.Process.TitleId));

            // FS would query the User and SdCache space IDs to find where the existing cache is (if any).
            // We always have the SD card inserted, so we can always use SdCache for now.
            Result result = _baseFileSystemProxy.OpenSaveDataInfoReaderBySaveDataSpaceId(
                out ReferenceCountedDisposable<LibHac.FsSrv.ISaveDataInfoReader> infoReader, SaveDataSpaceId.SdCache);

            if (result.IsSuccess())
            {
                MakeObject(context, new ISaveDataInfoReader(infoReader));
            }

            return (ResultCode)result.Value;
        }

        [Command(67)]
        public ResultCode FindSaveDataWithFilter(ServiceCtx context)
        {
            SaveDataSpaceId spaceId = (SaveDataSpaceId)context.RequestData.ReadInt64();
            SaveDataFilter filter = context.RequestData.ReadStruct<SaveDataFilter>();

            long bufferPosition = context.Request.ReceiveBuff[0].Position;
            long bufferLen = context.Request.ReceiveBuff[0].Size;

            byte[] infoBuffer = new byte[bufferLen];

            Result result = _baseFileSystemProxy.FindSaveDataWithFilter(out long count, infoBuffer, spaceId, ref filter);

            context.Memory.Write((ulong)bufferPosition, infoBuffer);
            context.ResponseData.Write(count);

            return (ResultCode)result.Value;
        }

        [Command(68)]
        public ResultCode OpenSaveDataInfoReaderWithFilter(ServiceCtx context)
        {
            SaveDataSpaceId spaceId = (SaveDataSpaceId)context.RequestData.ReadInt64();
            SaveDataFilter filter = context.RequestData.ReadStruct<SaveDataFilter>();

            Result result = _baseFileSystemProxy.OpenSaveDataInfoReaderWithFilter(
                out ReferenceCountedDisposable<LibHac.FsSrv.ISaveDataInfoReader> infoReader, spaceId, ref filter);

            if (result.IsSuccess())
            {
                MakeObject(context, new ISaveDataInfoReader(infoReader));
            }

            return (ResultCode)result.Value;
        }

        [Command(71)]
        public ResultCode ReadSaveDataFileSystemExtraDataWithMaskBySaveDataAttribute(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceFs);

            MemoryHelper.FillWithZeros(context.Memory, context.Request.ReceiveBuff[0].Position, (int)context.Request.ReceiveBuff[0].Size);

            return ResultCode.Success;
        }

        [Command(200)]
        // OpenDataStorageByCurrentProcess() -> object<nn::fssrv::sf::IStorage> dataStorage
        public ResultCode OpenDataStorageByCurrentProcess(ServiceCtx context)
        {
            MakeObject(context, new FileSystemProxy.IStorage(context.Device.FileSystem.RomFs.AsStorage()));

            return 0;
        }

        [Command(202)]
        // OpenDataStorageByDataId(u8 storageId, nn::ApplicationId tid) -> object<nn::fssrv::sf::IStorage> dataStorage
        public ResultCode OpenDataStorageByDataId(ServiceCtx context)
        {
            StorageId storageId = (StorageId)context.RequestData.ReadByte();
            byte[] padding = context.RequestData.ReadBytes(7);
            long titleId = context.RequestData.ReadInt64();

            // We do a mitm here to find if the request is for an AOC.
            // This is because AOC can be distributed over multiple containers in the emulator.
            if (context.Device.System.ContentManager.GetAocDataStorage((ulong)titleId, out LibHac.Fs.IStorage aocStorage))
            {
                Logger.Info?.Print(LogClass.Loader, $"Opened AddOnContent Data TitleID={titleId:X16}");

                MakeObject(context, new FileSystemProxy.IStorage(context.Device.FileSystem.ModLoader.ApplyRomFsMods((ulong)titleId, aocStorage)));

                return ResultCode.Success;
            }

            NcaContentType contentType = NcaContentType.Data;

            StorageId installedStorage = context.Device.System.ContentManager.GetInstalledStorage(titleId, contentType, storageId);

            if (installedStorage == StorageId.None)
            {
                contentType = NcaContentType.PublicData;

                installedStorage = context.Device.System.ContentManager.GetInstalledStorage(titleId, contentType, storageId);
            }

            if (installedStorage != StorageId.None)
            {
                string contentPath = context.Device.System.ContentManager.GetInstalledContentPath(titleId, storageId, contentType);
                string installPath = context.Device.FileSystem.SwitchPathToSystemPath(contentPath);

                if (!string.IsNullOrWhiteSpace(installPath))
                {
                    string ncaPath = installPath;

                    if (File.Exists(ncaPath))
                    {
                        try
                        {
                            LibHac.Fs.IStorage ncaStorage = new LocalStorage(ncaPath, FileAccess.Read, FileMode.Open);
                            Nca nca = new Nca(context.Device.System.KeySet, ncaStorage);
                            LibHac.Fs.IStorage romfsStorage = nca.OpenStorage(NcaSectionType.Data, context.Device.System.FsIntegrityCheckLevel);

                            MakeObject(context, new FileSystemProxy.IStorage(romfsStorage));
                        }
                        catch (HorizonResultException ex)
                        {
                            return (ResultCode)ex.ResultValue.Value;
                        }

                        return ResultCode.Success;
                    }
                    else
                    {
                        throw new FileNotFoundException($"No Nca found in Path `{ncaPath}`.");
                    }
                }
                else
                {
                    throw new DirectoryNotFoundException($"Path for title id {titleId:x16} on Storage {storageId} was not found in Path {installPath}.");
                }
            }

            throw new FileNotFoundException($"System archive with titleid {titleId:x16} was not found on Storage {storageId}. Found in {installedStorage}.");
        }

        [Command(203)]
        // OpenPatchDataStorageByCurrentProcess() -> object<nn::fssrv::sf::IStorage>
        public ResultCode OpenPatchDataStorageByCurrentProcess(ServiceCtx context)
        {
            MakeObject(context, new FileSystemProxy.IStorage(context.Device.FileSystem.RomFs.AsStorage()));

            return ResultCode.Success;
        }

        [Command(400)]
        // OpenDataStorageByCurrentProcess() -> object<nn::fssrv::sf::IStorage> dataStorage
        public ResultCode OpenDeviceOperator(ServiceCtx context)
        {
            Result result = _baseFileSystemProxy.OpenDeviceOperator(out LibHac.FsSrv.IDeviceOperator deviceOperator);

            if (result.IsSuccess())
            {
                MakeObject(context, new IDeviceOperator(deviceOperator));
            }

            return (ResultCode)result.Value;
        }

        [Command(630)]
        // SetSdCardAccessibility(u8)
        public ResultCode SetSdCardAccessibility(ServiceCtx context)
        {
            bool isAccessible = context.RequestData.ReadBoolean();

            return (ResultCode)_baseFileSystemProxy.SetSdCardAccessibility(isAccessible).Value;
        }

        [Command(631)]
        // IsSdCardAccessible() -> u8
        public ResultCode IsSdCardAccessible(ServiceCtx context)
        {
            Result result = _baseFileSystemProxy.IsSdCardAccessible(out bool isAccessible);

            context.ResponseData.Write(isAccessible);

            return (ResultCode)result.Value;
        }

        [Command(1004)]
        // SetGlobalAccessLogMode(u32 mode)
        public ResultCode SetGlobalAccessLogMode(ServiceCtx context)
        {
            int mode = context.RequestData.ReadInt32();

            context.Device.System.GlobalAccessLogMode = mode;

            return ResultCode.Success;
        }

        [Command(1005)]
        // GetGlobalAccessLogMode() -> u32 logMode
        public ResultCode GetGlobalAccessLogMode(ServiceCtx context)
        {
            int mode = context.Device.System.GlobalAccessLogMode;

            context.ResponseData.Write(mode);

            return ResultCode.Success;
        }

        [Command(1006)]
        // OutputAccessLogToSdCard(buffer<bytes, 5> log_text)
        public ResultCode OutputAccessLogToSdCard(ServiceCtx context)
        {
            string message = ReadUtf8StringSend(context);

            // FS ends each line with a newline. Remove it because Ryujinx logging adds its own newline
            Logger.AccessLog?.PrintMsg(LogClass.ServiceFs, message.TrimEnd('\n'));

            return ResultCode.Success;
        }

        [Command(1011)]
        public ResultCode GetProgramIndexForAccessLog(ServiceCtx context)
        {
            int programIndex = 0;
            int programCount = 1;

            context.ResponseData.Write(programIndex);
            context.ResponseData.Write(programCount);

            return ResultCode.Success;
        }

        [Command(1200)] // 6.0.0+
        // OpenMultiCommitManager() -> object<nn::fssrv::sf::IMultiCommitManager>
        public ResultCode OpenMultiCommitManager(ServiceCtx context)
        {
            Result result = _baseFileSystemProxy.OpenMultiCommitManager(out LibHac.FsSrv.IMultiCommitManager commitManager);

            if (result.IsSuccess())
            {
                MakeObject(context, new IMultiCommitManager(commitManager));
            }

            return (ResultCode)result.Value;
        }
    }
}