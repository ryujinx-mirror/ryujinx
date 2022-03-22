using LibHac;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Shim;
using LibHac.FsSrv.Impl;
using LibHac.FsSystem;
using LibHac.Ncm;
using LibHac.Sf;
using LibHac.Spl;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Fs.FileSystemProxy;
using System.IO;

using static Ryujinx.HLE.Utilities.StringUtils;
using IFileSystem = LibHac.FsSrv.Sf.IFileSystem;
using IStorage = LibHac.FsSrv.Sf.IStorage;
using RightsId = LibHac.Fs.RightsId;

namespace Ryujinx.HLE.HOS.Services.Fs
{
    [Service("fsp-srv")]
    class IFileSystemProxy : DisposableIpcService
    {
        private SharedRef<LibHac.FsSrv.Sf.IFileSystemProxy> _baseFileSystemProxy;

        public IFileSystemProxy(ServiceCtx context) : base(context.Device.System.FsServer)
        {
            var applicationClient = context.Device.System.LibHacHorizonManager.ApplicationClient;
            _baseFileSystemProxy = applicationClient.Fs.Impl.GetFileSystemProxyServiceObject();
        }

        [CommandHipc(1)]
        // SetCurrentProcess(u64, pid)
        public ResultCode SetCurrentProcess(ServiceCtx context)
        {
            return ResultCode.Success;
        }

        [CommandHipc(8)]
        // OpenFileSystemWithId(nn::fssrv::sf::FileSystemType filesystem_type, nn::ApplicationId tid, buffer<bytes<0x301>, 0x19, 0x301> path)
        // -> object<nn::fssrv::sf::IFileSystem> contentFs
        public ResultCode OpenFileSystemWithId(ServiceCtx context)
        {
            FileSystemType fileSystemType = (FileSystemType)context.RequestData.ReadInt32();
            ulong titleId = context.RequestData.ReadUInt64();
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
            string extension = System.IO.Path.GetExtension(fullPath);

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

        [CommandHipc(11)]
        // OpenBisFileSystem(nn::fssrv::sf::Partition partitionID, buffer<bytes<0x301>, 0x19, 0x301>) -> object<nn::fssrv::sf::IFileSystem> Bis
        public ResultCode OpenBisFileSystem(ServiceCtx context)
        {
            BisPartitionId bisPartitionId = (BisPartitionId)context.RequestData.ReadInt32();

            ref readonly var path = ref FileSystemProxyHelper.GetFspPath(context);
            using var fileSystem = new SharedRef<IFileSystem>();

            Result result = _baseFileSystemProxy.Get.OpenBisFileSystem(ref fileSystem.Ref(), in path, bisPartitionId);
            if (result.IsFailure()) return (ResultCode)result.Value;

            MakeObject(context, new FileSystemProxy.IFileSystem(ref fileSystem.Ref()));

            return ResultCode.Success;
        }

        [CommandHipc(12)]
        // OpenBisStorage(u32 partitionId) -> object<nn::fssrv::sf::IStorage> bisStorage
        public ResultCode OpenBisStorage(ServiceCtx context)
        {
            BisPartitionId bisPartitionId = (BisPartitionId)context.RequestData.ReadInt32();
            using var storage = new SharedRef<IStorage>();

            Result result = _baseFileSystemProxy.Get.OpenBisStorage(ref storage.Ref(), bisPartitionId);
            if (result.IsFailure()) return (ResultCode)result.Value;

            MakeObject(context, new FileSystemProxy.IStorage(ref storage.Ref()));

            return ResultCode.Success;
        }

        [CommandHipc(13)]
        // InvalidateBisCache() -> ()
        public ResultCode InvalidateBisCache(ServiceCtx context)
        {
            return (ResultCode)_baseFileSystemProxy.Get.InvalidateBisCache().Value;
        }

        [CommandHipc(18)]
        // OpenSdCardFileSystem() -> object<nn::fssrv::sf::IFileSystem>
        public ResultCode OpenSdCardFileSystem(ServiceCtx context)
        {
            using var fileSystem = new SharedRef<IFileSystem>();

            Result result = _baseFileSystemProxy.Get.OpenSdCardFileSystem(ref fileSystem.Ref());
            if (result.IsFailure()) return (ResultCode)result.Value;

            MakeObject(context, new FileSystemProxy.IFileSystem(ref fileSystem.Ref()));

            return ResultCode.Success;
        }

        [CommandHipc(19)]
        // FormatSdCardFileSystem() -> ()
        public ResultCode FormatSdCardFileSystem(ServiceCtx context)
        {
            return (ResultCode)_baseFileSystemProxy.Get.FormatSdCardFileSystem().Value;
        }

        [CommandHipc(21)]
        // DeleteSaveDataFileSystem(u64 saveDataId) -> ()
        public ResultCode DeleteSaveDataFileSystem(ServiceCtx context)
        {
            ulong saveDataId = context.RequestData.ReadUInt64();

            return (ResultCode)_baseFileSystemProxy.Get.DeleteSaveDataFileSystem(saveDataId).Value;
        }

        [CommandHipc(22)]
        // CreateSaveDataFileSystem(nn::fs::SaveDataAttribute attribute, nn::fs::SaveDataCreationInfo creationInfo, nn::fs::SaveDataMetaInfo metaInfo) -> ()
        public ResultCode CreateSaveDataFileSystem(ServiceCtx context)
        {
            SaveDataAttribute attribute = context.RequestData.ReadStruct<SaveDataAttribute>();
            SaveDataCreationInfo creationInfo = context.RequestData.ReadStruct<SaveDataCreationInfo>();
            SaveDataMetaInfo metaInfo = context.RequestData.ReadStruct<SaveDataMetaInfo>();

            return (ResultCode)_baseFileSystemProxy.Get.CreateSaveDataFileSystem(in attribute, in creationInfo, in metaInfo).Value;
        }

        [CommandHipc(23)]
        // CreateSaveDataFileSystemBySystemSaveDataId(nn::fs::SaveDataAttribute attribute, nn::fs::SaveDataCreationInfo creationInfo) -> ()
        public ResultCode CreateSaveDataFileSystemBySystemSaveDataId(ServiceCtx context)
        {
            SaveDataAttribute attribute = context.RequestData.ReadStruct<SaveDataAttribute>();
            SaveDataCreationInfo creationInfo = context.RequestData.ReadStruct<SaveDataCreationInfo>();

            return (ResultCode)_baseFileSystemProxy.Get.CreateSaveDataFileSystemBySystemSaveDataId(in attribute, in creationInfo).Value;
        }

        [CommandHipc(24)]
        // RegisterSaveDataFileSystemAtomicDeletion(buffer<u64, 5> saveDataIds) -> ()
        public ResultCode RegisterSaveDataFileSystemAtomicDeletion(ServiceCtx context)
        {
            byte[] saveIdBuffer = new byte[context.Request.SendBuff[0].Size];
            context.Memory.Read(context.Request.SendBuff[0].Position, saveIdBuffer);

            return (ResultCode)_baseFileSystemProxy.Get.RegisterSaveDataFileSystemAtomicDeletion(new InBuffer(saveIdBuffer)).Value;
        }

        [CommandHipc(25)]
        // DeleteSaveDataFileSystemBySaveDataSpaceId(u8 spaceId, u64 saveDataId) -> ()
        public ResultCode DeleteSaveDataFileSystemBySaveDataSpaceId(ServiceCtx context)
        {
            SaveDataSpaceId spaceId = (SaveDataSpaceId)context.RequestData.ReadInt64();
            ulong saveDataId = context.RequestData.ReadUInt64();

            return (ResultCode)_baseFileSystemProxy.Get.DeleteSaveDataFileSystemBySaveDataSpaceId(spaceId, saveDataId).Value;
        }

        [CommandHipc(26)]
        // FormatSdCardDryRun() -> ()
        public ResultCode FormatSdCardDryRun(ServiceCtx context)
        {
            return (ResultCode)_baseFileSystemProxy.Get.FormatSdCardDryRun().Value;
        }

        [CommandHipc(27)]
        // IsExFatSupported() -> (u8 isSupported)
        public ResultCode IsExFatSupported(ServiceCtx context)
        {
            Result result = _baseFileSystemProxy.Get.IsExFatSupported(out bool isSupported);
            if (result.IsFailure()) return (ResultCode)result.Value;

            context.ResponseData.Write(isSupported);

            return ResultCode.Success;
        }

        [CommandHipc(28)]
        // DeleteSaveDataFileSystemBySaveDataAttribute(u8 spaceId, nn::fs::SaveDataAttribute attribute) -> ()
        public ResultCode DeleteSaveDataFileSystemBySaveDataAttribute(ServiceCtx context)
        {
            SaveDataSpaceId spaceId = (SaveDataSpaceId)context.RequestData.ReadInt64();
            SaveDataAttribute attribute = context.RequestData.ReadStruct<SaveDataAttribute>();

            return (ResultCode)_baseFileSystemProxy.Get.DeleteSaveDataFileSystemBySaveDataAttribute(spaceId, in attribute).Value;
        }

        [CommandHipc(30)]
        // OpenGameCardStorage(u32 handle, u32 partitionId) -> object<nn::fssrv::sf::IStorage>
        public ResultCode OpenGameCardStorage(ServiceCtx context)
        {
            GameCardHandle handle = new GameCardHandle(context.RequestData.ReadInt32());
            GameCardPartitionRaw partitionId = (GameCardPartitionRaw)context.RequestData.ReadInt32();
            using var storage = new SharedRef<IStorage>();

            Result result = _baseFileSystemProxy.Get.OpenGameCardStorage(ref storage.Ref(), handle, partitionId);
            if (result.IsFailure()) return (ResultCode)result.Value;

            MakeObject(context, new FileSystemProxy.IStorage(ref storage.Ref()));

            return ResultCode.Success;
        }

        [CommandHipc(31)]
        // OpenGameCardFileSystem(u32 handle, u32 partitionId) -> object<nn::fssrv::sf::IFileSystem>
        public ResultCode OpenGameCardFileSystem(ServiceCtx context)
        {
            GameCardHandle handle = new GameCardHandle(context.RequestData.ReadInt32());
            GameCardPartition partitionId = (GameCardPartition)context.RequestData.ReadInt32();
            using var fileSystem = new SharedRef<IFileSystem>();

            Result result = _baseFileSystemProxy.Get.OpenGameCardFileSystem(ref fileSystem.Ref(), handle, partitionId);
            if (result.IsFailure()) return (ResultCode)result.Value;

            MakeObject(context, new FileSystemProxy.IFileSystem(ref fileSystem.Ref()));

            return ResultCode.Success;
        }

        [CommandHipc(32)]
        // ExtendSaveDataFileSystem(u8 spaceId, u64 saveDataId, s64 dataSize, s64 journalSize) -> ()
        public ResultCode ExtendSaveDataFileSystem(ServiceCtx context)
        {
            SaveDataSpaceId spaceId = (SaveDataSpaceId)context.RequestData.ReadInt64();
            ulong saveDataId = context.RequestData.ReadUInt64();
            long dataSize = context.RequestData.ReadInt64();
            long journalSize = context.RequestData.ReadInt64();

            return (ResultCode)_baseFileSystemProxy.Get.ExtendSaveDataFileSystem(spaceId, saveDataId, dataSize, journalSize).Value;
        }

        [CommandHipc(33)]
        // DeleteCacheStorage(u16 index) -> ()
        public ResultCode DeleteCacheStorage(ServiceCtx context)
        {
            ushort index = context.RequestData.ReadUInt16();

            return (ResultCode)_baseFileSystemProxy.Get.DeleteCacheStorage(index).Value;
        }

        [CommandHipc(34)]
        // GetCacheStorageSize(u16 index) -> (s64 dataSize, s64 journalSize)
        public ResultCode GetCacheStorageSize(ServiceCtx context)
        {
            ushort index = context.RequestData.ReadUInt16();

            Result result = _baseFileSystemProxy.Get.GetCacheStorageSize(out long dataSize, out long journalSize, index);
            if (result.IsFailure()) return (ResultCode)result.Value;

            context.ResponseData.Write(dataSize);
            context.ResponseData.Write(journalSize);

            return ResultCode.Success;
        }

        [CommandHipc(35)]
        // CreateSaveDataFileSystemWithHashSalt(nn::fs::SaveDataAttribute attribute, nn::fs::SaveDataCreationInfo creationInfo, nn::fs::SaveDataMetaInfo metaInfo nn::fs::HashSalt hashSalt) -> ()
        public ResultCode CreateSaveDataFileSystemWithHashSalt(ServiceCtx context)
        {
            SaveDataAttribute attribute = context.RequestData.ReadStruct<SaveDataAttribute>();
            SaveDataCreationInfo creationInfo = context.RequestData.ReadStruct<SaveDataCreationInfo>();
            SaveDataMetaInfo metaCreateInfo = context.RequestData.ReadStruct<SaveDataMetaInfo>();
            HashSalt hashSalt = context.RequestData.ReadStruct<HashSalt>();

            return (ResultCode)_baseFileSystemProxy.Get.CreateSaveDataFileSystemWithHashSalt(in attribute, in creationInfo, in metaCreateInfo, in hashSalt).Value;
        }

        [CommandHipc(51)]
        // OpenSaveDataFileSystem(u8 spaceId, nn::fs::SaveDataAttribute attribute) -> object<nn::fssrv::sf::IFileSystem> saveDataFs
        public ResultCode OpenSaveDataFileSystem(ServiceCtx context)
        {
            SaveDataSpaceId spaceId = (SaveDataSpaceId)context.RequestData.ReadInt64();
            SaveDataAttribute attribute = context.RequestData.ReadStruct<SaveDataAttribute>();
            using var fileSystem = new SharedRef<IFileSystem>();

            Result result = _baseFileSystemProxy.Get.OpenSaveDataFileSystem(ref fileSystem.Ref(), spaceId, in attribute);
            if (result.IsFailure()) return (ResultCode)result.Value;

            MakeObject(context, new FileSystemProxy.IFileSystem(ref fileSystem.Ref()));

            return ResultCode.Success;
        }

        [CommandHipc(52)]
        // OpenSaveDataFileSystemBySystemSaveDataId(u8 spaceId, nn::fs::SaveDataAttribute attribute) -> object<nn::fssrv::sf::IFileSystem> systemSaveDataFs
        public ResultCode OpenSaveDataFileSystemBySystemSaveDataId(ServiceCtx context)
        {
            SaveDataSpaceId spaceId = (SaveDataSpaceId)context.RequestData.ReadInt64();
            SaveDataAttribute attribute = context.RequestData.ReadStruct<SaveDataAttribute>();
            using var fileSystem = new SharedRef<IFileSystem>();

            Result result = _baseFileSystemProxy.Get.OpenSaveDataFileSystemBySystemSaveDataId(ref fileSystem.Ref(), spaceId, in attribute);
            if (result.IsFailure()) return (ResultCode)result.Value;

            MakeObject(context, new FileSystemProxy.IFileSystem(ref fileSystem.Ref()));

            return ResultCode.Success;
        }

        [CommandHipc(53)]
        // OpenReadOnlySaveDataFileSystem(u8 spaceId, nn::fs::SaveDataAttribute attribute) -> object<nn::fssrv::sf::IFileSystem>
        public ResultCode OpenReadOnlySaveDataFileSystem(ServiceCtx context)
        {
            SaveDataSpaceId spaceId = (SaveDataSpaceId)context.RequestData.ReadInt64();
            SaveDataAttribute attribute = context.RequestData.ReadStruct<SaveDataAttribute>();
            using var fileSystem = new SharedRef<IFileSystem>();

            Result result = _baseFileSystemProxy.Get.OpenReadOnlySaveDataFileSystem(ref fileSystem.Ref(), spaceId, in attribute);
            if (result.IsFailure()) return (ResultCode)result.Value;

            MakeObject(context, new FileSystemProxy.IFileSystem(ref fileSystem.Ref()));

            return ResultCode.Success;
        }

        [CommandHipc(57)]
        // ReadSaveDataFileSystemExtraDataBySaveDataSpaceId(u8 spaceId, u64 saveDataId) -> (buffer<nn::fs::SaveDataExtraData, 6> extraData)
        public ResultCode ReadSaveDataFileSystemExtraDataBySaveDataSpaceId(ServiceCtx context)
        {
            SaveDataSpaceId spaceId = (SaveDataSpaceId)context.RequestData.ReadInt64();
            ulong saveDataId = context.RequestData.ReadUInt64();

            byte[] extraDataBuffer = new byte[context.Request.ReceiveBuff[0].Size];
            context.Memory.Read(context.Request.ReceiveBuff[0].Position, extraDataBuffer);

            Result result = _baseFileSystemProxy.Get.ReadSaveDataFileSystemExtraDataBySaveDataSpaceId(new OutBuffer(extraDataBuffer), spaceId, saveDataId);
            if (result.IsFailure()) return (ResultCode)result.Value;

            context.Memory.Write(context.Request.ReceiveBuff[0].Position, extraDataBuffer);

            return ResultCode.Success;
        }

        [CommandHipc(58)]
        // ReadSaveDataFileSystemExtraData(u64 saveDataId) -> (buffer<nn::fs::SaveDataExtraData, 6> extraData)
        public ResultCode ReadSaveDataFileSystemExtraData(ServiceCtx context)
        {
            ulong saveDataId = context.RequestData.ReadUInt64();

            byte[] extraDataBuffer = new byte[context.Request.ReceiveBuff[0].Size];
            context.Memory.Read(context.Request.ReceiveBuff[0].Position, extraDataBuffer);

            Result result = _baseFileSystemProxy.Get.ReadSaveDataFileSystemExtraData(new OutBuffer(extraDataBuffer), saveDataId);
            if (result.IsFailure()) return (ResultCode)result.Value;

            context.Memory.Write(context.Request.ReceiveBuff[0].Position, extraDataBuffer);

            return ResultCode.Success;
        }

        [CommandHipc(59)]
        // WriteSaveDataFileSystemExtraData(u8 spaceId, u64 saveDataId, buffer<nn::fs::SaveDataExtraData, 5> extraData) -> ()
        public ResultCode WriteSaveDataFileSystemExtraData(ServiceCtx context)
        {
            SaveDataSpaceId spaceId = (SaveDataSpaceId)context.RequestData.ReadInt64();
            ulong saveDataId = context.RequestData.ReadUInt64();

            byte[] extraDataBuffer = new byte[context.Request.SendBuff[0].Size];
            context.Memory.Read(context.Request.SendBuff[0].Position, extraDataBuffer);

            return (ResultCode)_baseFileSystemProxy.Get.WriteSaveDataFileSystemExtraData(saveDataId, spaceId, new InBuffer(extraDataBuffer)).Value;
        }

        [CommandHipc(60)]
        // OpenSaveDataInfoReader() -> object<nn::fssrv::sf::ISaveDataInfoReader>
        public ResultCode OpenSaveDataInfoReader(ServiceCtx context)
        {
            using var infoReader = new SharedRef<LibHac.FsSrv.Sf.ISaveDataInfoReader>();

            Result result = _baseFileSystemProxy.Get.OpenSaveDataInfoReader(ref infoReader.Ref());
            if (result.IsFailure()) return (ResultCode)result.Value;

            MakeObject(context, new ISaveDataInfoReader(ref infoReader.Ref()));

            return ResultCode.Success;
        }

        [CommandHipc(61)]
        // OpenSaveDataInfoReaderBySaveDataSpaceId(u8 spaceId) -> object<nn::fssrv::sf::ISaveDataInfoReader>
        public ResultCode OpenSaveDataInfoReaderBySaveDataSpaceId(ServiceCtx context)
        {
            SaveDataSpaceId spaceId = (SaveDataSpaceId)context.RequestData.ReadByte();
            using var infoReader = new SharedRef<LibHac.FsSrv.Sf.ISaveDataInfoReader>();

            Result result = _baseFileSystemProxy.Get.OpenSaveDataInfoReaderBySaveDataSpaceId(ref infoReader.Ref(), spaceId);
            if (result.IsFailure()) return (ResultCode)result.Value;

            MakeObject(context, new ISaveDataInfoReader(ref infoReader.Ref()));

            return ResultCode.Success;
        }

        [CommandHipc(62)]
        // OpenSaveDataInfoReaderOnlyCacheStorage() -> object<nn::fssrv::sf::ISaveDataInfoReader>
        public ResultCode OpenSaveDataInfoReaderOnlyCacheStorage(ServiceCtx context)
        {
            using var infoReader = new SharedRef<LibHac.FsSrv.Sf.ISaveDataInfoReader>();

            Result result = _baseFileSystemProxy.Get.OpenSaveDataInfoReaderOnlyCacheStorage(ref infoReader.Ref());
            if (result.IsFailure()) return (ResultCode)result.Value;

            MakeObject(context, new ISaveDataInfoReader(ref infoReader.Ref()));

            return ResultCode.Success;
        }

        [CommandHipc(64)]
        // OpenSaveDataInternalStorageFileSystem(u8 spaceId, u64 saveDataId) -> object<nn::fssrv::sf::ISaveDataInfoReader>
        public ResultCode OpenSaveDataInternalStorageFileSystem(ServiceCtx context)
        {
            SaveDataSpaceId spaceId = (SaveDataSpaceId)context.RequestData.ReadInt64();
            ulong saveDataId = context.RequestData.ReadUInt64();
            using var fileSystem = new SharedRef<IFileSystem>();

            Result result = _baseFileSystemProxy.Get.OpenSaveDataInternalStorageFileSystem(ref fileSystem.Ref(), spaceId, saveDataId);
            if (result.IsFailure()) return (ResultCode)result.Value;

            MakeObject(context, new FileSystemProxy.IFileSystem(ref fileSystem.Ref()));

            return ResultCode.Success;
        }

        [CommandHipc(65)]
        // UpdateSaveDataMacForDebug(u8 spaceId, u64 saveDataId) -> ()
        public ResultCode UpdateSaveDataMacForDebug(ServiceCtx context)
        {
            SaveDataSpaceId spaceId = (SaveDataSpaceId)context.RequestData.ReadInt64();
            ulong saveDataId = context.RequestData.ReadUInt64();

            return (ResultCode)_baseFileSystemProxy.Get.UpdateSaveDataMacForDebug(spaceId, saveDataId).Value;
        }

        [CommandHipc(66)]
        public ResultCode WriteSaveDataFileSystemExtraDataWithMask(ServiceCtx context)
        {
            SaveDataSpaceId spaceId = (SaveDataSpaceId)context.RequestData.ReadInt64();
            ulong saveDataId = context.RequestData.ReadUInt64();

            byte[] extraDataBuffer = new byte[context.Request.SendBuff[0].Size];
            context.Memory.Read(context.Request.SendBuff[0].Position, extraDataBuffer);

            byte[] maskBuffer = new byte[context.Request.SendBuff[1].Size];
            context.Memory.Read(context.Request.SendBuff[1].Position, maskBuffer);

            return (ResultCode)_baseFileSystemProxy.Get.WriteSaveDataFileSystemExtraDataWithMask(saveDataId, spaceId, new InBuffer(extraDataBuffer), new InBuffer(maskBuffer)).Value;
        }

        [CommandHipc(67)]
        public ResultCode FindSaveDataWithFilter(ServiceCtx context)
        {
            SaveDataSpaceId spaceId = (SaveDataSpaceId)context.RequestData.ReadInt64();
            SaveDataFilter filter = context.RequestData.ReadStruct<SaveDataFilter>();

            ulong bufferPosition = context.Request.ReceiveBuff[0].Position;
            ulong bufferLen = context.Request.ReceiveBuff[0].Size;

            byte[] infoBuffer = new byte[bufferLen];

            Result result = _baseFileSystemProxy.Get.FindSaveDataWithFilter(out long count, new OutBuffer(infoBuffer), spaceId, in filter);
            if (result.IsFailure()) return (ResultCode)result.Value;

            context.Memory.Write(bufferPosition, infoBuffer);
            context.ResponseData.Write(count);

            return ResultCode.Success;
        }

        [CommandHipc(68)]
        public ResultCode OpenSaveDataInfoReaderWithFilter(ServiceCtx context)
        {
            SaveDataSpaceId spaceId = (SaveDataSpaceId)context.RequestData.ReadInt64();
            SaveDataFilter filter = context.RequestData.ReadStruct<SaveDataFilter>();
            using var infoReader = new SharedRef<LibHac.FsSrv.Sf.ISaveDataInfoReader>();

            Result result = _baseFileSystemProxy.Get.OpenSaveDataInfoReaderWithFilter(ref infoReader.Ref(), spaceId, in filter);
            if (result.IsFailure()) return (ResultCode)result.Value;

            MakeObject(context, new ISaveDataInfoReader(ref infoReader.Ref()));

            return ResultCode.Success;
        }

        [CommandHipc(69)]
        public ResultCode ReadSaveDataFileSystemExtraDataBySaveDataAttribute(ServiceCtx context)
        {
            SaveDataSpaceId spaceId = (SaveDataSpaceId)context.RequestData.ReadInt64();
            SaveDataAttribute attribute = context.RequestData.ReadStruct<SaveDataAttribute>();

            byte[] outputBuffer = new byte[context.Request.ReceiveBuff[0].Size];
            context.Memory.Read(context.Request.ReceiveBuff[0].Position, outputBuffer);

            Result result = _baseFileSystemProxy.Get.ReadSaveDataFileSystemExtraDataBySaveDataAttribute(new OutBuffer(outputBuffer), spaceId, in attribute);
            if (result.IsFailure()) return (ResultCode)result.Value;

            context.Memory.Write(context.Request.ReceiveBuff[0].Position, outputBuffer);

            return ResultCode.Success;
        }

        [CommandHipc(70)]
        public ResultCode WriteSaveDataFileSystemExtraDataWithMaskBySaveDataAttribute(ServiceCtx context)
        {
            SaveDataSpaceId spaceId = (SaveDataSpaceId)context.RequestData.ReadInt64();
            SaveDataAttribute attribute = context.RequestData.ReadStruct<SaveDataAttribute>();

            byte[] extraDataBuffer = new byte[context.Request.SendBuff[0].Size];
            context.Memory.Read(context.Request.SendBuff[0].Position, extraDataBuffer);

            byte[] maskBuffer = new byte[context.Request.SendBuff[1].Size];
            context.Memory.Read(context.Request.SendBuff[1].Position, maskBuffer);

            return (ResultCode)_baseFileSystemProxy.Get.WriteSaveDataFileSystemExtraDataWithMaskBySaveDataAttribute(in attribute, spaceId, new InBuffer(extraDataBuffer), new InBuffer(maskBuffer)).Value;
        }

        [CommandHipc(71)]
        public ResultCode ReadSaveDataFileSystemExtraDataWithMaskBySaveDataAttribute(ServiceCtx context)
        {
            SaveDataSpaceId spaceId = (SaveDataSpaceId)context.RequestData.ReadInt64();
            SaveDataAttribute attribute = context.RequestData.ReadStruct<SaveDataAttribute>();

            byte[] maskBuffer = new byte[context.Request.SendBuff[0].Size];
            context.Memory.Read(context.Request.SendBuff[0].Position, maskBuffer);

            byte[] outputBuffer = new byte[context.Request.ReceiveBuff[0].Size];
            context.Memory.Read(context.Request.ReceiveBuff[0].Position, outputBuffer);

            Result result = _baseFileSystemProxy.Get.ReadSaveDataFileSystemExtraDataWithMaskBySaveDataAttribute(new OutBuffer(outputBuffer), spaceId, in attribute, new InBuffer(maskBuffer));
            if (result.IsFailure()) return (ResultCode)result.Value;

            context.Memory.Write(context.Request.ReceiveBuff[0].Position, outputBuffer);

            return ResultCode.Success;
        }

        [CommandHipc(80)]
        public ResultCode OpenSaveDataMetaFile(ServiceCtx context)
        {
            SaveDataSpaceId spaceId = (SaveDataSpaceId)context.RequestData.ReadInt32();
            SaveDataMetaType metaType = (SaveDataMetaType)context.RequestData.ReadInt32();
            SaveDataAttribute attribute = context.RequestData.ReadStruct<SaveDataAttribute>();
            using var file = new SharedRef<LibHac.FsSrv.Sf.IFile>();

            Result result = _baseFileSystemProxy.Get.OpenSaveDataMetaFile(ref file.Ref(), spaceId, in attribute, metaType);
            if (result.IsFailure()) return (ResultCode)result.Value;

            MakeObject(context, new IFile(ref file.Ref()));

            return ResultCode.Success;
        }

        [CommandHipc(84)]
        public ResultCode ListAccessibleSaveDataOwnerId(ServiceCtx context)
        {
            int startIndex = context.RequestData.ReadInt32();
            int bufferCount = context.RequestData.ReadInt32();
            ProgramId programId = context.RequestData.ReadStruct<ProgramId>();

            byte[] outputBuffer = new byte[context.Request.ReceiveBuff[0].Size];
            context.Memory.Read(context.Request.ReceiveBuff[0].Position, outputBuffer);

            Result result = _baseFileSystemProxy.Get.ListAccessibleSaveDataOwnerId(out int readCount, new OutBuffer(outputBuffer), programId, startIndex, bufferCount);
            if (result.IsFailure()) return (ResultCode)result.Value;

            context.ResponseData.Write(readCount);

            return ResultCode.Success;
        }

        [CommandHipc(100)]
        public ResultCode OpenImageDirectoryFileSystem(ServiceCtx context)
        {
            ImageDirectoryId directoryId = (ImageDirectoryId)context.RequestData.ReadInt32();
            using var fileSystem = new SharedRef<IFileSystem>();

            Result result = _baseFileSystemProxy.Get.OpenImageDirectoryFileSystem(ref fileSystem.Ref(), directoryId);
            if (result.IsFailure()) return (ResultCode)result.Value;

            MakeObject(context, new FileSystemProxy.IFileSystem(ref fileSystem.Ref()));

            return ResultCode.Success;
        }

        [CommandHipc(101)]
        public ResultCode OpenBaseFileSystem(ServiceCtx context)
        {
            BaseFileSystemId fileSystemId = (BaseFileSystemId)context.RequestData.ReadInt32();
            using var fileSystem = new SharedRef<IFileSystem>();

            Result result = _baseFileSystemProxy.Get.OpenBaseFileSystem(ref fileSystem.Ref(), fileSystemId);
            if (result.IsFailure()) return (ResultCode)result.Value;

            MakeObject(context, new FileSystemProxy.IFileSystem(ref fileSystem.Ref()));

            return ResultCode.Success;
        }

        [CommandHipc(110)]
        public ResultCode OpenContentStorageFileSystem(ServiceCtx context)
        {
            ContentStorageId contentStorageId = (ContentStorageId)context.RequestData.ReadInt32();
            using var fileSystem = new SharedRef<IFileSystem>();

            Result result = _baseFileSystemProxy.Get.OpenContentStorageFileSystem(ref fileSystem.Ref(), contentStorageId);
            if (result.IsFailure()) return (ResultCode)result.Value;

            MakeObject(context, new FileSystemProxy.IFileSystem(ref fileSystem.Ref()));

            return ResultCode.Success;
        }

        [CommandHipc(120)]
        public ResultCode OpenCloudBackupWorkStorageFileSystem(ServiceCtx context)
        {
            CloudBackupWorkStorageId storageId = (CloudBackupWorkStorageId)context.RequestData.ReadInt32();
            using var fileSystem = new SharedRef<IFileSystem>();

            Result result = _baseFileSystemProxy.Get.OpenCloudBackupWorkStorageFileSystem(ref fileSystem.Ref(), storageId);
            if (result.IsFailure()) return (ResultCode)result.Value;

            MakeObject(context, new FileSystemProxy.IFileSystem(ref fileSystem.Ref()));

            return ResultCode.Success;
        }

        [CommandHipc(130)]
        public ResultCode OpenCustomStorageFileSystem(ServiceCtx context)
        {
            CustomStorageId customStorageId = (CustomStorageId)context.RequestData.ReadInt32();
            using var fileSystem = new SharedRef<IFileSystem>();

            Result result = _baseFileSystemProxy.Get.OpenCustomStorageFileSystem(ref fileSystem.Ref(), customStorageId);
            if (result.IsFailure()) return (ResultCode)result.Value;

            MakeObject(context, new FileSystemProxy.IFileSystem(ref fileSystem.Ref()));

            return ResultCode.Success;
        }

        [CommandHipc(200)]
        // OpenDataStorageByCurrentProcess() -> object<nn::fssrv::sf::IStorage> dataStorage
        public ResultCode OpenDataStorageByCurrentProcess(ServiceCtx context)
        {
            var storage = context.Device.FileSystem.RomFs.AsStorage(true);
            using var sharedStorage = new SharedRef<LibHac.Fs.IStorage>(storage);
            using var sfStorage = new SharedRef<IStorage>(new StorageInterfaceAdapter(ref sharedStorage.Ref()));

            MakeObject(context, new FileSystemProxy.IStorage(ref sfStorage.Ref()));

            return ResultCode.Success;
        }

        [CommandHipc(202)]
        // OpenDataStorageByDataId(u8 storageId, nn::ncm::DataId dataId) -> object<nn::fssrv::sf::IStorage> dataStorage
        public ResultCode OpenDataStorageByDataId(ServiceCtx context)
        {
            StorageId storageId = (StorageId)context.RequestData.ReadByte();
            byte[] padding = context.RequestData.ReadBytes(7);
            ulong titleId = context.RequestData.ReadUInt64();

            // We do a mitm here to find if the request is for an AOC.
            // This is because AOC can be distributed over multiple containers in the emulator.
            if (context.Device.System.ContentManager.GetAocDataStorage(titleId, out LibHac.Fs.IStorage aocStorage, context.Device.Configuration.FsIntegrityCheckLevel))
            {
                Logger.Info?.Print(LogClass.Loader, $"Opened AddOnContent Data TitleID={titleId:X16}");

                var storage = context.Device.FileSystem.ModLoader.ApplyRomFsMods(titleId, aocStorage);
                using var sharedStorage = new SharedRef<LibHac.Fs.IStorage>(storage);
                using var sfStorage = new SharedRef<IStorage>(new StorageInterfaceAdapter(ref sharedStorage.Ref()));

                MakeObject(context, new FileSystemProxy.IStorage(ref sfStorage.Ref()));

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
                            using var sharedStorage = new SharedRef<LibHac.Fs.IStorage>(romfsStorage);
                            using var sfStorage = new SharedRef<IStorage>(new StorageInterfaceAdapter(ref sharedStorage.Ref()));

                            MakeObject(context, new FileSystemProxy.IStorage(ref sfStorage.Ref()));
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

        [CommandHipc(203)]
        // OpenPatchDataStorageByCurrentProcess() -> object<nn::fssrv::sf::IStorage>
        public ResultCode OpenPatchDataStorageByCurrentProcess(ServiceCtx context)
        {
            var storage = context.Device.FileSystem.RomFs.AsStorage(true);
            using var sharedStorage = new SharedRef<LibHac.Fs.IStorage>(storage);
            using var sfStorage = new SharedRef<IStorage>(new StorageInterfaceAdapter(ref sharedStorage.Ref()));

            MakeObject(context, new FileSystemProxy.IStorage(ref sfStorage.Ref()));

            return ResultCode.Success;
        }

        [CommandHipc(400)]
        // OpenDataStorageByCurrentProcess() -> object<nn::fssrv::sf::IStorage> dataStorage
        public ResultCode OpenDeviceOperator(ServiceCtx context)
        {
            using var deviceOperator = new SharedRef<LibHac.FsSrv.Sf.IDeviceOperator>();

            Result result = _baseFileSystemProxy.Get.OpenDeviceOperator(ref deviceOperator.Ref());
            if (result.IsFailure()) return (ResultCode)result.Value;

            MakeObject(context, new IDeviceOperator(ref deviceOperator.Ref()));

            return ResultCode.Success;
        }

        [CommandHipc(601)]
        public ResultCode QuerySaveDataTotalSize(ServiceCtx context)
        {
            long dataSize = context.RequestData.ReadInt64();
            long journalSize = context.RequestData.ReadInt64();

            Result result = _baseFileSystemProxy.Get.QuerySaveDataTotalSize(out long totalSize, dataSize, journalSize);
            if (result.IsFailure()) return (ResultCode)result.Value;

            context.ResponseData.Write(totalSize);

            return ResultCode.Success;
        }

        [CommandHipc(511)]
        public ResultCode NotifySystemDataUpdateEvent(ServiceCtx context)
        {
            return (ResultCode)_baseFileSystemProxy.Get.NotifySystemDataUpdateEvent().Value;
        }

        [CommandHipc(523)]
        public ResultCode SimulateDeviceDetectionEvent(ServiceCtx context)
        {
            bool signalEvent = context.RequestData.ReadBoolean();
            context.RequestData.BaseStream.Seek(3, SeekOrigin.Current);
            SdmmcPort port = context.RequestData.ReadStruct<SdmmcPort>();
            SimulatingDeviceDetectionMode mode = context.RequestData.ReadStruct<SimulatingDeviceDetectionMode>();

            return (ResultCode)_baseFileSystemProxy.Get.SimulateDeviceDetectionEvent(port, mode, signalEvent).Value;
        }

        [CommandHipc(602)]
        public ResultCode VerifySaveDataFileSystem(ServiceCtx context)
        {
            ulong saveDataId = context.RequestData.ReadUInt64();

            byte[] readBuffer = new byte[context.Request.ReceiveBuff[0].Size];
            context.Memory.Read(context.Request.ReceiveBuff[0].Position, readBuffer);

            return (ResultCode)_baseFileSystemProxy.Get.VerifySaveDataFileSystem(saveDataId, new OutBuffer(readBuffer)).Value;
        }

        [CommandHipc(603)]
        public ResultCode CorruptSaveDataFileSystem(ServiceCtx context)
        {
            ulong saveDataId = context.RequestData.ReadUInt64();

            return (ResultCode)_baseFileSystemProxy.Get.CorruptSaveDataFileSystem(saveDataId).Value;
        }

        [CommandHipc(604)]
        public ResultCode CreatePaddingFile(ServiceCtx context)
        {
            long size = context.RequestData.ReadInt64();

            return (ResultCode)_baseFileSystemProxy.Get.CreatePaddingFile(size).Value;
        }

        [CommandHipc(605)]
        public ResultCode DeleteAllPaddingFiles(ServiceCtx context)
        {
            return (ResultCode)_baseFileSystemProxy.Get.DeleteAllPaddingFiles().Value;
        }

        [CommandHipc(606)]
        public ResultCode GetRightsId(ServiceCtx context)
        {
            LibHac.Ncm.StorageId storageId = (LibHac.Ncm.StorageId)context.RequestData.ReadInt64();
            ProgramId programId = context.RequestData.ReadStruct<ProgramId>();

            Result result = _baseFileSystemProxy.Get.GetRightsId(out RightsId rightsId, programId, storageId);
            if (result.IsFailure()) return (ResultCode)result.Value;

            context.ResponseData.WriteStruct(rightsId);

            return ResultCode.Success;
        }

        [CommandHipc(607)]
        public ResultCode RegisterExternalKey(ServiceCtx context)
        {
            RightsId rightsId = context.RequestData.ReadStruct<RightsId>();
            AccessKey accessKey = context.RequestData.ReadStruct<AccessKey>();

            return (ResultCode)_baseFileSystemProxy.Get.RegisterExternalKey(in rightsId, in accessKey).Value;
        }

        [CommandHipc(608)]
        public ResultCode UnregisterAllExternalKey(ServiceCtx context)
        {
            return (ResultCode)_baseFileSystemProxy.Get.UnregisterAllExternalKey().Value;
        }

        [CommandHipc(609)]
        public ResultCode GetRightsIdByPath(ServiceCtx context)
        {
            ref readonly var path = ref FileSystemProxyHelper.GetFspPath(context);

            Result result = _baseFileSystemProxy.Get.GetRightsIdByPath(out RightsId rightsId, in path);
            if (result.IsFailure()) return (ResultCode)result.Value;

            context.ResponseData.WriteStruct(rightsId);

            return ResultCode.Success;
        }

        [CommandHipc(610)]
        public ResultCode GetRightsIdAndKeyGenerationByPath(ServiceCtx context)
        {
            ref readonly var path = ref FileSystemProxyHelper.GetFspPath(context);

            Result result = _baseFileSystemProxy.Get.GetRightsIdAndKeyGenerationByPath(out RightsId rightsId, out byte keyGeneration, in path);
            if (result.IsFailure()) return (ResultCode)result.Value;

            context.ResponseData.Write(keyGeneration);
            context.ResponseData.BaseStream.Seek(7, SeekOrigin.Current);
            context.ResponseData.WriteStruct(rightsId);

            return ResultCode.Success;
        }

        [CommandHipc(611)]
        public ResultCode SetCurrentPosixTimeWithTimeDifference(ServiceCtx context)
        {
            int timeDifference = context.RequestData.ReadInt32();
            context.RequestData.BaseStream.Seek(4, SeekOrigin.Current);
            long time = context.RequestData.ReadInt64();

            return (ResultCode)_baseFileSystemProxy.Get.SetCurrentPosixTimeWithTimeDifference(time, timeDifference).Value;
        }

        [CommandHipc(612)]
        public ResultCode GetFreeSpaceSizeForSaveData(ServiceCtx context)
        {
            SaveDataSpaceId spaceId = context.RequestData.ReadStruct<SaveDataSpaceId>();

            Result result = _baseFileSystemProxy.Get.GetFreeSpaceSizeForSaveData(out long freeSpaceSize, spaceId);
            if (result.IsFailure()) return (ResultCode)result.Value;

            context.ResponseData.Write(freeSpaceSize);

            return ResultCode.Success;
        }

        [CommandHipc(613)]
        public ResultCode VerifySaveDataFileSystemBySaveDataSpaceId(ServiceCtx context)
        {
            SaveDataSpaceId spaceId = (SaveDataSpaceId)context.RequestData.ReadInt64();
            ulong saveDataId = context.RequestData.ReadUInt64();

            byte[] readBuffer = new byte[context.Request.ReceiveBuff[0].Size];
            context.Memory.Read(context.Request.ReceiveBuff[0].Position, readBuffer);

            return (ResultCode)_baseFileSystemProxy.Get.VerifySaveDataFileSystemBySaveDataSpaceId(spaceId, saveDataId, new OutBuffer(readBuffer)).Value;
        }

        [CommandHipc(614)]
        public ResultCode CorruptSaveDataFileSystemBySaveDataSpaceId(ServiceCtx context)
        {
            SaveDataSpaceId spaceId = (SaveDataSpaceId)context.RequestData.ReadInt64();
            ulong saveDataId = context.RequestData.ReadUInt64();

            return (ResultCode)_baseFileSystemProxy.Get.CorruptSaveDataFileSystemBySaveDataSpaceId(spaceId, saveDataId).Value;
        }

        [CommandHipc(615)]
        public ResultCode QuerySaveDataInternalStorageTotalSize(ServiceCtx context)
        {
            SaveDataSpaceId spaceId = (SaveDataSpaceId)context.RequestData.ReadInt64();
            ulong saveDataId = context.RequestData.ReadUInt64();

            Result result = _baseFileSystemProxy.Get.QuerySaveDataInternalStorageTotalSize(out long size, spaceId, saveDataId);
            if (result.IsFailure()) return (ResultCode)result.Value;

            context.ResponseData.Write(size);

            return ResultCode.Success;
        }

        [CommandHipc(616)]
        public ResultCode GetSaveDataCommitId(ServiceCtx context)
        {
            SaveDataSpaceId spaceId = (SaveDataSpaceId)context.RequestData.ReadInt64();
            ulong saveDataId = context.RequestData.ReadUInt64();

            Result result = _baseFileSystemProxy.Get.GetSaveDataCommitId(out long commitId, spaceId, saveDataId);
            if (result.IsFailure()) return (ResultCode)result.Value;

            context.ResponseData.Write(commitId);

            return ResultCode.Success;
        }

        [CommandHipc(617)]
        public ResultCode UnregisterExternalKey(ServiceCtx context)
        {
            RightsId rightsId = context.RequestData.ReadStruct<RightsId>();

            return (ResultCode)_baseFileSystemProxy.Get.UnregisterExternalKey(in rightsId).Value;
        }

        [CommandHipc(620)]
        public ResultCode SetSdCardEncryptionSeed(ServiceCtx context)
        {
            EncryptionSeed encryptionSeed = context.RequestData.ReadStruct<EncryptionSeed>();

            return (ResultCode)_baseFileSystemProxy.Get.SetSdCardEncryptionSeed(in encryptionSeed).Value;
        }

        [CommandHipc(630)]
        // SetSdCardAccessibility(u8 isAccessible)
        public ResultCode SetSdCardAccessibility(ServiceCtx context)
        {
            bool isAccessible = context.RequestData.ReadBoolean();

            return (ResultCode)_baseFileSystemProxy.Get.SetSdCardAccessibility(isAccessible).Value;
        }

        [CommandHipc(631)]
        // IsSdCardAccessible() -> u8 isAccessible
        public ResultCode IsSdCardAccessible(ServiceCtx context)
        {
            Result result = _baseFileSystemProxy.Get.IsSdCardAccessible(out bool isAccessible);
            if (result.IsFailure()) return (ResultCode)result.Value;

            context.ResponseData.Write(isAccessible);

            return ResultCode.Success;
        }

        [CommandHipc(702)]
        public ResultCode IsAccessFailureDetected(ServiceCtx context)
        {
            ulong processId = context.RequestData.ReadUInt64();

            Result result = _baseFileSystemProxy.Get.IsAccessFailureDetected(out bool isDetected, processId);
            if (result.IsFailure()) return (ResultCode)result.Value;

            context.ResponseData.Write(isDetected);

            return ResultCode.Success;
        }

        [CommandHipc(710)]
        public ResultCode ResolveAccessFailure(ServiceCtx context)
        {
            ulong processId = context.RequestData.ReadUInt64();

            return (ResultCode)_baseFileSystemProxy.Get.ResolveAccessFailure(processId).Value;
        }

        [CommandHipc(720)]
        public ResultCode AbandonAccessFailure(ServiceCtx context)
        {
            ulong processId = context.RequestData.ReadUInt64();

            return (ResultCode)_baseFileSystemProxy.Get.AbandonAccessFailure(processId).Value;
        }

        [CommandHipc(800)]
        public ResultCode GetAndClearErrorInfo(ServiceCtx context)
        {
            Result result = _baseFileSystemProxy.Get.GetAndClearErrorInfo(out FileSystemProxyErrorInfo errorInfo);
            if (result.IsFailure()) return (ResultCode)result.Value;

            context.ResponseData.WriteStruct(errorInfo);

            return ResultCode.Success;
        }

        [CommandHipc(810)]
        public ResultCode RegisterProgramIndexMapInfo(ServiceCtx context)
        {
            int programCount = context.RequestData.ReadInt32();

            byte[] mapInfoBuffer = new byte[context.Request.SendBuff[0].Size];
            context.Memory.Read(context.Request.SendBuff[0].Position, mapInfoBuffer);

            return (ResultCode)_baseFileSystemProxy.Get.RegisterProgramIndexMapInfo(new InBuffer(mapInfoBuffer), programCount).Value;
        }

        [CommandHipc(1000)]
        public ResultCode SetBisRootForHost(ServiceCtx context)
        {
            BisPartitionId partitionId = (BisPartitionId)context.RequestData.ReadInt32();
            ref readonly var path = ref FileSystemProxyHelper.GetFspPath(context);

            return (ResultCode)_baseFileSystemProxy.Get.SetBisRootForHost(partitionId, in path).Value;
        }

        [CommandHipc(1001)]
        public ResultCode SetSaveDataSize(ServiceCtx context)
        {
            long dataSize = context.RequestData.ReadInt64();
            long journalSize = context.RequestData.ReadInt64();

            return (ResultCode)_baseFileSystemProxy.Get.SetSaveDataSize(dataSize, journalSize).Value;
        }

        [CommandHipc(1002)]
        public ResultCode SetSaveDataRootPath(ServiceCtx context)
        {
            ref readonly var path = ref FileSystemProxyHelper.GetFspPath(context);

            return (ResultCode)_baseFileSystemProxy.Get.SetSaveDataRootPath(in path).Value;
        }

        [CommandHipc(1003)]
        public ResultCode DisableAutoSaveDataCreation(ServiceCtx context)
        {
            return (ResultCode)_baseFileSystemProxy.Get.DisableAutoSaveDataCreation().Value;
        }

        [CommandHipc(1004)]
        // SetGlobalAccessLogMode(u32 mode)
        public ResultCode SetGlobalAccessLogMode(ServiceCtx context)
        {
            int mode = context.RequestData.ReadInt32();

            context.Device.System.GlobalAccessLogMode = mode;

            return ResultCode.Success;
        }

        [CommandHipc(1005)]
        // GetGlobalAccessLogMode() -> u32 logMode
        public ResultCode GetGlobalAccessLogMode(ServiceCtx context)
        {
            int mode = context.Device.System.GlobalAccessLogMode;

            context.ResponseData.Write(mode);

            return ResultCode.Success;
        }

        [CommandHipc(1006)]
        // OutputAccessLogToSdCard(buffer<bytes, 5> log_text)
        public ResultCode OutputAccessLogToSdCard(ServiceCtx context)
        {
            string message = ReadUtf8StringSend(context);

            // FS ends each line with a newline. Remove it because Ryujinx logging adds its own newline
            Logger.AccessLog?.PrintMsg(LogClass.ServiceFs, message.TrimEnd('\n'));

            return ResultCode.Success;
        }

        [CommandHipc(1007)]
        public ResultCode RegisterUpdatePartition(ServiceCtx context)
        {
            return (ResultCode)_baseFileSystemProxy.Get.RegisterUpdatePartition().Value;
        }

        [CommandHipc(1008)]
        public ResultCode OpenRegisteredUpdatePartition(ServiceCtx context)
        {
            using var fileSystem = new SharedRef<IFileSystem>();

            Result result = _baseFileSystemProxy.Get.OpenRegisteredUpdatePartition(ref fileSystem.Ref());
            if (result.IsFailure()) return (ResultCode)result.Value;

            MakeObject(context, new FileSystemProxy.IFileSystem(ref fileSystem.Ref()));

            return ResultCode.Success;
        }

        [CommandHipc(1009)]
        public ResultCode GetAndClearMemoryReportInfo(ServiceCtx context)
        {
            Result result = _baseFileSystemProxy.Get.GetAndClearMemoryReportInfo(out MemoryReportInfo reportInfo);
            if (result.IsFailure()) return (ResultCode)result.Value;

            context.ResponseData.WriteStruct(reportInfo);

            return ResultCode.Success;
        }

        [CommandHipc(1011)]
        public ResultCode GetProgramIndexForAccessLog(ServiceCtx context)
        {
            Result result = _baseFileSystemProxy.Get.GetProgramIndexForAccessLog(out int programIndex, out int programCount);
            if (result.IsFailure()) return (ResultCode)result.Value;

            context.ResponseData.Write(programIndex);
            context.ResponseData.Write(programCount);

            return ResultCode.Success;
        }

        [CommandHipc(1012)]
        public ResultCode GetFsStackUsage(ServiceCtx context)
        {
            FsStackUsageThreadType threadType = context.RequestData.ReadStruct<FsStackUsageThreadType>();

            Result result = _baseFileSystemProxy.Get.GetFsStackUsage(out uint usage, threadType);
            if (result.IsFailure()) return (ResultCode)result.Value;

            context.ResponseData.Write(usage);

            return ResultCode.Success;
        }

        [CommandHipc(1013)]
        public ResultCode UnsetSaveDataRootPath(ServiceCtx context)
        {
            return (ResultCode)_baseFileSystemProxy.Get.UnsetSaveDataRootPath().Value;
        }

        [CommandHipc(1014)]
        public ResultCode OutputMultiProgramTagAccessLog(ServiceCtx context)
        {
            return (ResultCode)_baseFileSystemProxy.Get.OutputMultiProgramTagAccessLog().Value;
        }

        [CommandHipc(1016)]
        public ResultCode FlushAccessLogOnSdCard(ServiceCtx context)
        {
            return (ResultCode)_baseFileSystemProxy.Get.FlushAccessLogOnSdCard().Value;
        }

        [CommandHipc(1017)]
        public ResultCode OutputApplicationInfoAccessLog(ServiceCtx context)
        {
            ApplicationInfo info = context.RequestData.ReadStruct<ApplicationInfo>();

            return (ResultCode)_baseFileSystemProxy.Get.OutputApplicationInfoAccessLog(in info).Value;
        }

        [CommandHipc(1100)]
        public ResultCode OverrideSaveDataTransferTokenSignVerificationKey(ServiceCtx context)
        {
            byte[] keyBuffer = new byte[context.Request.SendBuff[0].Size];
            context.Memory.Read(context.Request.SendBuff[0].Position, keyBuffer);

            return (ResultCode)_baseFileSystemProxy.Get.OverrideSaveDataTransferTokenSignVerificationKey(new InBuffer(keyBuffer)).Value;
        }

        [CommandHipc(1110)]
        public ResultCode CorruptSaveDataFileSystemByOffset(ServiceCtx context)
        {
            SaveDataSpaceId spaceId = (SaveDataSpaceId)context.RequestData.ReadInt64();
            ulong saveDataId = context.RequestData.ReadUInt64();
            long offset = context.RequestData.ReadInt64();

            return (ResultCode)_baseFileSystemProxy.Get.CorruptSaveDataFileSystemByOffset(spaceId, saveDataId, offset).Value;
        }

        [CommandHipc(1200)] // 6.0.0+
        // OpenMultiCommitManager() -> object<nn::fssrv::sf::IMultiCommitManager>
        public ResultCode OpenMultiCommitManager(ServiceCtx context)
        {
            using var commitManager = new SharedRef<LibHac.FsSrv.Sf.IMultiCommitManager>();

            Result result = _baseFileSystemProxy.Get.OpenMultiCommitManager(ref commitManager.Ref());
            if (result.IsFailure()) return (ResultCode)result.Value;

            MakeObject(context, new IMultiCommitManager(ref commitManager.Ref()));

            return ResultCode.Success;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                _baseFileSystemProxy.Destroy();
            }
        }
    }
}