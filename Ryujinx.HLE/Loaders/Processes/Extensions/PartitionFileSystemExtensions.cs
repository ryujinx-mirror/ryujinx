using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using Ryujinx.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Ryujinx.HLE.Loaders.Processes.Extensions
{
    public static class PartitionFileSystemExtensions
    {
        internal static (bool, ProcessResult) TryLoad(this PartitionFileSystem partitionFileSystem, Switch device, string path, out string errorMessage)
        {
            errorMessage = null;

            // Load required NCAs.
            Nca mainNca    = null;
            Nca patchNca   = null;
            Nca controlNca = null;

            try
            {
                device.Configuration.VirtualFileSystem.ImportTickets(partitionFileSystem);

                // TODO: To support multi-games container, this should use CNMT NCA instead.
                foreach (DirectoryEntryEx fileEntry in partitionFileSystem.EnumerateEntries("/", "*.nca"))
                {
                    Nca nca = partitionFileSystem.GetNca(device, fileEntry.FullPath);

                    if (nca.GetProgramIndex() != device.Configuration.UserChannelPersistence.Index)
                    {
                        continue;
                    }

                    if (nca.IsPatch())
                    {
                        patchNca = nca;
                    }
                    else if (nca.IsProgram())
                    {
                        mainNca = nca;
                    }
                    else if (nca.IsControl())
                    {
                        controlNca = nca;
                    }
                }

                ProcessLoaderHelper.RegisterProgramMapInfo(device, partitionFileSystem).ThrowIfFailure();
            }
            catch (Exception ex)
            {
                errorMessage = $"Unable to load: {ex.Message}";

                return (false, ProcessResult.Failed);
            }

            if (mainNca != null)
            {
                if (mainNca.Header.ContentType != NcaContentType.Program)
                {
                    errorMessage = "Selected NCA file is not a \"Program\" NCA";

                    return (false, ProcessResult.Failed);
                }

                // Load Update NCAs.
                Nca updatePatchNca = null;
                Nca updateControlNca = null;

                if (ulong.TryParse(mainNca.Header.TitleId.ToString("x16"), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong titleIdBase))
                {
                    // Clear the program index part.
                    titleIdBase &= ~0xFUL;

                    // Load update information if exists.
                    string titleUpdateMetadataPath = System.IO.Path.Combine(AppDataManager.GamesDirPath, titleIdBase.ToString("x16"), "updates.json");
                    if (File.Exists(titleUpdateMetadataPath))
                    {
                        string updatePath = JsonHelper.DeserializeFromFile<TitleUpdateMetadata>(titleUpdateMetadataPath).Selected;
                        if (File.Exists(updatePath))
                        {
                            PartitionFileSystem updatePartitionFileSystem = new(new FileStream(updatePath, FileMode.Open, FileAccess.Read).AsStorage());

                            device.Configuration.VirtualFileSystem.ImportTickets(updatePartitionFileSystem);

                            // TODO: This should use CNMT NCA instead.
                            foreach (DirectoryEntryEx fileEntry in updatePartitionFileSystem.EnumerateEntries("/", "*.nca"))
                            {
                                Nca nca = updatePartitionFileSystem.GetNca(device, fileEntry.FullPath);

                                if (nca.GetProgramIndex() != device.Configuration.UserChannelPersistence.Index)
                                {
                                    continue;
                                }

                                if ($"{nca.Header.TitleId.ToString("x16")[..^3]}000" != titleIdBase.ToString("x16"))
                                {
                                    break;
                                }

                                if (nca.IsProgram())
                                {
                                    updatePatchNca = nca;
                                }
                                else if (nca.IsControl())
                                {
                                    updateControlNca = nca;
                                }
                            }
                        }
                    }
                }

                if (updatePatchNca != null)
                {
                    patchNca = updatePatchNca;
                }

                if (updateControlNca != null)
                {
                    controlNca = updateControlNca;
                }

                // Load contained DownloadableContents.
                // TODO: If we want to support multi-processes in future, we shouldn't clear AddOnContent data here.
                device.Configuration.ContentManager.ClearAocData();
                device.Configuration.ContentManager.AddAocData(partitionFileSystem, path, mainNca.Header.TitleId, device.Configuration.FsIntegrityCheckLevel);

                // Load DownloadableContents.
                string addOnContentMetadataPath = System.IO.Path.Combine(AppDataManager.GamesDirPath, mainNca.Header.TitleId.ToString("x16"), "dlc.json");
                if (File.Exists(addOnContentMetadataPath))
                {
                    List<DownloadableContentContainer> dlcContainerList = JsonHelper.DeserializeFromFile<List<DownloadableContentContainer>>(addOnContentMetadataPath);

                    foreach (DownloadableContentContainer downloadableContentContainer in dlcContainerList)
                    {
                        foreach (DownloadableContentNca downloadableContentNca in downloadableContentContainer.DownloadableContentNcaList)
                        {
                            if (File.Exists(downloadableContentContainer.ContainerPath) && downloadableContentNca.Enabled)
                            {
                                device.Configuration.ContentManager.AddAocItem(downloadableContentNca.TitleId, downloadableContentContainer.ContainerPath, downloadableContentNca.FullPath);
                            }
                            else
                            {
                                Logger.Warning?.Print(LogClass.Application, $"Cannot find AddOnContent file {downloadableContentContainer.ContainerPath}. It may have been moved or renamed.");
                            }
                        }
                    }
                }

                return (true, mainNca.Load(device, patchNca, controlNca));
            }

            errorMessage = "Unable to load: Could not find Main NCA";

            return (false, ProcessResult.Failed);
        }

        public static Nca GetNca(this IFileSystem fileSystem, Switch device, string path)
        {
            using var ncaFile = new UniqueRef<IFile>();

            fileSystem.OpenFile(ref ncaFile.Ref, path.ToU8Span(), OpenMode.Read).ThrowIfFailure();

            return new Nca(device.Configuration.VirtualFileSystem.KeySet, ncaFile.Release().AsStorage());
        }
    }
}