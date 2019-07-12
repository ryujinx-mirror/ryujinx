using LibHac;
using LibHac.Fs;
using LibHac.Fs.NcaUtils;
using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.Utilities;
using System.IO;

using static Ryujinx.HLE.FileSystem.VirtualFileSystem;
using static Ryujinx.HLE.HOS.ErrorCode;
using static Ryujinx.HLE.Utilities.StringUtils;

namespace Ryujinx.HLE.HOS.Services.FspSrv
{
    [Service("fsp-srv")]
    class IFileSystemProxy : IpcService
    {
        public IFileSystemProxy(ServiceCtx context) { }

        [Command(1)]
        // Initialize(u64, pid)
        public long Initialize(ServiceCtx context)
        {
            return 0;
        }

        [Command(8)]
        // OpenFileSystemWithId(nn::fssrv::sf::FileSystemType filesystem_type, nn::ApplicationId tid, buffer<bytes<0x301>, 0x19, 0x301> path) 
        // -> object<nn::fssrv::sf::IFileSystem> contentFs
        public long OpenFileSystemWithId(ServiceCtx context)
        {
            FileSystemType fileSystemType = (FileSystemType)context.RequestData.ReadInt32();
            long           titleId        = context.RequestData.ReadInt64();
            string         switchPath     = ReadUtf8String(context);
            string         fullPath       = context.Device.FileSystem.SwitchPathToSystemPath(switchPath);

            if (!File.Exists(fullPath))
            {
                if (fullPath.Contains("."))
                {
                    return OpenFileSystemFromInternalFile(context, fullPath);
                }

                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            FileStream fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            string     extension  = Path.GetExtension(fullPath);

            if (extension == ".nca")
            {
                return OpenNcaFs(context, fullPath, fileStream.AsStorage());
            }
            else if (extension == ".nsp")
            {
                return OpenNsp(context, fullPath);
            }

            return MakeError(ErrorModule.Fs, FsErr.InvalidInput);
        }

        [Command(11)]
        // OpenBisFileSystem(nn::fssrv::sf::Partition partitionID, buffer<bytes<0x301>, 0x19, 0x301>) -> object<nn::fssrv::sf::IFileSystem> Bis
        public long OpenBisFileSystem(ServiceCtx context)
        {
            int    bisPartitionId   = context.RequestData.ReadInt32();
            string partitionString  = ReadUtf8String(context);
            string bisPartitionPath = string.Empty;

            switch (bisPartitionId)
            {
                case 29:
                    bisPartitionPath = SafeNandPath;
                    break;
                case 30:
                case 31:
                    bisPartitionPath = SystemNandPath;
                    break;
                case 32:
                    bisPartitionPath = UserNandPath;
                    break;
                default:
                    return MakeError(ErrorModule.Fs, FsErr.InvalidInput);
            }

            string fullPath = context.Device.FileSystem.GetFullPartitionPath(bisPartitionPath);

            LocalFileSystem fileSystem = new LocalFileSystem(fullPath);

            MakeObject(context, new IFileSystem(fileSystem));

            return 0;
        }

        [Command(18)]
        // OpenSdCardFileSystem() -> object<nn::fssrv::sf::IFileSystem>
        public long OpenSdCardFileSystem(ServiceCtx context)
        {
            string sdCardPath = context.Device.FileSystem.GetSdCardPath();

            LocalFileSystem fileSystem = new LocalFileSystem(sdCardPath);

            MakeObject(context, new IFileSystem(fileSystem));

            return 0;
        }

        [Command(51)]
        // OpenSaveDataFileSystem(u8 save_data_space_id, nn::fssrv::sf::SaveStruct saveStruct) -> object<nn::fssrv::sf::IFileSystem> saveDataFs
        public long OpenSaveDataFileSystem(ServiceCtx context)
        {
            return LoadSaveDataFileSystem(context);
        }

        [Command(52)]
        // OpenSaveDataFileSystemBySystemSaveDataId(u8 save_data_space_id, nn::fssrv::sf::SaveStruct saveStruct) -> object<nn::fssrv::sf::IFileSystem> systemSaveDataFs
        public long OpenSaveDataFileSystemBySystemSaveDataId(ServiceCtx context)
        {
            return LoadSaveDataFileSystem(context);
        }

        [Command(200)]
        // OpenDataStorageByCurrentProcess() -> object<nn::fssrv::sf::IStorage> dataStorage
        public long OpenDataStorageByCurrentProcess(ServiceCtx context)
        {
            MakeObject(context, new IStorage(context.Device.FileSystem.RomFs.AsStorage()));

            return 0;
        }

        [Command(202)]
        // OpenDataStorageByDataId(u8 storageId, nn::ApplicationId tid) -> object<nn::fssrv::sf::IStorage> dataStorage
        public long OpenDataStorageByDataId(ServiceCtx context)
        {
            StorageId storageId = (StorageId)context.RequestData.ReadByte();
            byte[]    padding   = context.RequestData.ReadBytes(7);
            long      titleId   = context.RequestData.ReadInt64();

            ContentType contentType = ContentType.Data;

            StorageId installedStorage =
                context.Device.System.ContentManager.GetInstalledStorage(titleId, contentType, storageId);

            if (installedStorage == StorageId.None)
            {
                contentType = ContentType.PublicData;

                installedStorage =
                    context.Device.System.ContentManager.GetInstalledStorage(titleId, contentType, storageId);
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
                            LibHac.Fs.IStorage ncaStorage   = new LocalStorage(ncaPath, FileAccess.Read, FileMode.Open);
                            Nca                nca          = new Nca(context.Device.System.KeySet, ncaStorage);
                            LibHac.Fs.IStorage romfsStorage = nca.OpenStorage(NcaSectionType.Data, context.Device.System.FsIntegrityCheckLevel);

                            MakeObject(context, new IStorage(romfsStorage));
                        }
                        catch (HorizonResultException ex)
                        {
                            return ex.ResultValue.Value;
                        }

                        return 0;
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
        public long OpenPatchDataStorageByCurrentProcess(ServiceCtx context)
        {
            MakeObject(context, new IStorage(context.Device.FileSystem.RomFs.AsStorage()));

            return 0;
        }

        [Command(1005)]
        // GetGlobalAccessLogMode() -> u32 logMode
        public long GetGlobalAccessLogMode(ServiceCtx context)
        {
            int mode = context.Device.System.GlobalAccessLogMode;

            context.ResponseData.Write(mode);

            return 0;
        }

        [Command(1006)]
        // OutputAccessLogToSdCard(buffer<bytes, 5> log_text)
        public long OutputAccessLogToSdCard(ServiceCtx context)
        {
            string message = ReadUtf8StringSend(context);

            // FS ends each line with a newline. Remove it because Ryujinx logging adds its own newline
            Logger.PrintAccessLog(LogClass.ServiceFs, message.TrimEnd('\n'));

            return 0;
        }

        public long LoadSaveDataFileSystem(ServiceCtx context)
        {
            SaveSpaceId saveSpaceId = (SaveSpaceId)context.RequestData.ReadInt64();

            long titleId = context.RequestData.ReadInt64();

            UInt128 userId = context.RequestData.ReadStruct<UInt128>();

            long            saveId       = context.RequestData.ReadInt64();
            SaveDataType    saveDataType = (SaveDataType)context.RequestData.ReadByte();
            SaveInfo        saveInfo     = new SaveInfo(titleId, saveId, saveDataType, userId, saveSpaceId);
            string          savePath     = context.Device.FileSystem.GetGameSavePath(saveInfo, context);

            try
            {
                LocalFileSystem             fileSystem     = new LocalFileSystem(savePath);
                DirectorySaveDataFileSystem saveFileSystem = new DirectorySaveDataFileSystem(fileSystem);

                MakeObject(context, new IFileSystem(saveFileSystem));
            }
            catch (HorizonResultException ex)
            {
                return ex.ResultValue.Value;
            }

            return 0;
        }

        private long OpenNsp(ServiceCtx context, string pfsPath)
        {
            try
            {
                LocalStorage        storage = new LocalStorage(pfsPath, FileAccess.Read, FileMode.Open);
                PartitionFileSystem nsp     = new PartitionFileSystem(storage);

                ImportTitleKeysFromNsp(nsp, context.Device.System.KeySet);
                
                IFileSystem nspFileSystem = new IFileSystem(nsp);

                MakeObject(context, nspFileSystem);
            }
            catch (HorizonResultException ex)
            {
                return ex.ResultValue.Value;
            }

            return 0;
        }

        private long OpenNcaFs(ServiceCtx context, string ncaPath, LibHac.Fs.IStorage ncaStorage)
        {
            try
            {
                Nca nca = new Nca(context.Device.System.KeySet, ncaStorage);

                if (!nca.SectionExists(NcaSectionType.Data))
                {
                    return MakeError(ErrorModule.Fs, FsErr.PartitionNotFound);
                }

                LibHac.Fs.IFileSystem fileSystem = nca.OpenFileSystem(NcaSectionType.Data, context.Device.System.FsIntegrityCheckLevel);

                MakeObject(context, new IFileSystem(fileSystem));
            }
            catch (HorizonResultException ex)
            {
                return ex.ResultValue.Value;
            }

            return 0;
        }

        private long OpenFileSystemFromInternalFile(ServiceCtx context, string fullPath)
        {
            DirectoryInfo archivePath = new DirectoryInfo(fullPath).Parent;

            while (string.IsNullOrWhiteSpace(archivePath.Extension))
            {
                archivePath = archivePath.Parent;
            }

            if (archivePath.Extension == ".nsp" && File.Exists(archivePath.FullName))
            {
                FileStream pfsFile = new FileStream(
                    archivePath.FullName.TrimEnd(Path.DirectorySeparatorChar),
                    FileMode.Open,
                    FileAccess.Read);

                try
                {
                    PartitionFileSystem nsp = new PartitionFileSystem(pfsFile.AsStorage());

                    ImportTitleKeysFromNsp(nsp, context.Device.System.KeySet);
                    
                    string filename = fullPath.Replace(archivePath.FullName, string.Empty).TrimStart('\\');

                    if (nsp.FileExists(filename))
                    {
                        return OpenNcaFs(context, fullPath, nsp.OpenFile(filename, OpenMode.Read).AsStorage());
                    }
                }
                catch (HorizonResultException ex)
                {
                    return ex.ResultValue.Value;
                }
            }

            return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
        }

        private void ImportTitleKeysFromNsp(LibHac.Fs.IFileSystem nsp, Keyset keySet)
        {
            foreach (DirectoryEntry ticketEntry in nsp.EnumerateEntries("*.tik"))
            {
                Ticket ticket = new Ticket(nsp.OpenFile(ticketEntry.FullPath, OpenMode.Read).AsStream());

                if (!keySet.TitleKeys.ContainsKey(ticket.RightsId))
                {
                    keySet.TitleKeys.Add(ticket.RightsId, ticket.GetTitleKey(keySet));
                }
            }
        }
    }
}