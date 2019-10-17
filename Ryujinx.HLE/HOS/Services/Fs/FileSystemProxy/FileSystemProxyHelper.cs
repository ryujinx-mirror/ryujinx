using LibHac;
using LibHac.Fs;
using LibHac.FsSystem;
using LibHac.FsSystem.NcaUtils;
using LibHac.Spl;
using Ryujinx.Common;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.Utilities;
using System.IO;

namespace Ryujinx.HLE.HOS.Services.Fs.FileSystemProxy
{
    static class FileSystemProxyHelper
    {
        public static ResultCode LoadSaveDataFileSystem(ServiceCtx context, bool readOnly, out IFileSystem loadedFileSystem)
        {
            loadedFileSystem = null;

            SaveSpaceId  saveSpaceId  = (SaveSpaceId)context.RequestData.ReadInt64();
            ulong        titleId      = context.RequestData.ReadUInt64();
            UInt128      userId       = context.RequestData.ReadStruct<UInt128>();
            long         saveId       = context.RequestData.ReadInt64();
            SaveDataType saveDataType = (SaveDataType)context.RequestData.ReadByte();
            SaveInfo     saveInfo     = new SaveInfo(titleId, saveId, saveDataType, saveSpaceId, userId);
            string       savePath     = context.Device.FileSystem.GetSavePath(context, saveInfo);

            try
            {
                LocalFileSystem       fileSystem     = new LocalFileSystem(savePath);

                Result result = DirectorySaveDataFileSystem.CreateNew(out DirectorySaveDataFileSystem dirFileSystem, fileSystem);
                if (result.IsFailure())
                {
                    return (ResultCode)result.Value;
                }

                LibHac.Fs.IFileSystem saveFileSystem = dirFileSystem;

                if (readOnly)
                {
                    saveFileSystem = new ReadOnlyFileSystem(saveFileSystem);
                }

                loadedFileSystem = new IFileSystem(saveFileSystem);
            }
            catch (HorizonResultException ex)
            {
                return (ResultCode)ex.ResultValue.Value;
            }

            return ResultCode.Success;
        }

        public static ResultCode OpenNsp(ServiceCtx context, string pfsPath, out IFileSystem openedFileSystem)
        {
            openedFileSystem = null;

            try
            {
                LocalStorage        storage = new LocalStorage(pfsPath, FileAccess.Read, FileMode.Open);
                PartitionFileSystem nsp     = new PartitionFileSystem(storage);

                ImportTitleKeysFromNsp(nsp, context.Device.System.KeySet);

                openedFileSystem = new IFileSystem(nsp);
            }
            catch (HorizonResultException ex)
            {
                return (ResultCode)ex.ResultValue.Value;
            }

            return ResultCode.Success;
        }

        public static ResultCode OpenNcaFs(ServiceCtx context, string ncaPath, LibHac.Fs.IStorage ncaStorage, out IFileSystem openedFileSystem)
        {
            openedFileSystem = null;

            try
            {
                Nca nca = new Nca(context.Device.System.KeySet, ncaStorage);

                if (!nca.SectionExists(NcaSectionType.Data))
                {
                    return ResultCode.PartitionNotFound;
                }

                LibHac.Fs.IFileSystem fileSystem = nca.OpenFileSystem(NcaSectionType.Data, context.Device.System.FsIntegrityCheckLevel);

                openedFileSystem = new IFileSystem(fileSystem);
            }
            catch (HorizonResultException ex)
            {
                return (ResultCode)ex.ResultValue.Value;
            }

            return ResultCode.Success;
        }

        public static ResultCode OpenFileSystemFromInternalFile(ServiceCtx context, string fullPath, out IFileSystem openedFileSystem)
        {
            openedFileSystem = null;

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

                    Result result = nsp.OpenFile(out LibHac.Fs.IFile ncaFile, filename, OpenMode.Read);
                    if (result.IsFailure())
                    {
                        return (ResultCode)result.Value;
                    }

                    return OpenNcaFs(context, fullPath, ncaFile.AsStorage(), out openedFileSystem);
                }
                catch (HorizonResultException ex)
                {
                    return (ResultCode)ex.ResultValue.Value;
                }
            }

            return ResultCode.PathDoesNotExist;
        }

        public static void ImportTitleKeysFromNsp(LibHac.Fs.IFileSystem nsp, Keyset keySet)
        {
            foreach (DirectoryEntryEx ticketEntry in nsp.EnumerateEntries("/", "*.tik"))
            {
                Result result = nsp.OpenFile(out LibHac.Fs.IFile ticketFile, ticketEntry.FullPath, OpenMode.Read);

                if (result.IsSuccess())
                {
                    Ticket ticket = new Ticket(ticketFile.AsStream());

                    keySet.ExternalKeySet.Add(new RightsId(ticket.RightsId), new AccessKey(ticket.GetTitleKey(keySet)));
                }
            }
        }
    }
}
