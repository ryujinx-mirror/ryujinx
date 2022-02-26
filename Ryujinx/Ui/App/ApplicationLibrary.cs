using LibHac;
using LibHac.Common;
using LibHac.Common.Keys;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Ns;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using Ryujinx.Configuration.System;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS;
using Ryujinx.HLE.HOS.SystemState;
using Ryujinx.HLE.Loaders.Npdm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;

using JsonHelper = Ryujinx.Common.Utilities.JsonHelper;
using Path = System.IO.Path;

namespace Ryujinx.Ui.App
{
    public class ApplicationLibrary
    {
        public event EventHandler<ApplicationAddedEventArgs>        ApplicationAdded;
        public event EventHandler<ApplicationCountUpdatedEventArgs> ApplicationCountUpdated;

        private readonly byte[] _nspIcon;
        private readonly byte[] _xciIcon;
        private readonly byte[] _ncaIcon;
        private readonly byte[] _nroIcon;
        private readonly byte[] _nsoIcon;

        private VirtualFileSystem _virtualFileSystem;
        private Language          _desiredTitleLanguage;

        public ApplicationLibrary(VirtualFileSystem virtualFileSystem)
        {
            _virtualFileSystem = virtualFileSystem;

            _nspIcon = GetResourceBytes("Ryujinx.Ui.Resources.Icon_NSP.png");
            _xciIcon = GetResourceBytes("Ryujinx.Ui.Resources.Icon_XCI.png");
            _ncaIcon = GetResourceBytes("Ryujinx.Ui.Resources.Icon_NCA.png");
            _nroIcon = GetResourceBytes("Ryujinx.Ui.Resources.Icon_NRO.png");
            _nsoIcon = GetResourceBytes("Ryujinx.Ui.Resources.Icon_NSO.png");
        }

        private byte[] GetResourceBytes(string resourceName)
        {
            Stream resourceStream    = Assembly.GetCallingAssembly().GetManifestResourceStream(resourceName);
            byte[] resourceByteArray = new byte[resourceStream.Length];

            resourceStream.Read(resourceByteArray);

            return resourceByteArray;
        }

        public IEnumerable<string> GetFilesInDirectory(string directory)
        {
            Stack<string> stack = new Stack<string>();

            stack.Push(directory);

            while (stack.Count > 0)
            {
                string   dir     = stack.Pop();
                string[] content = Array.Empty<string>();

                try
                {
                    content = Directory.GetFiles(dir, "*");
                }
                catch (UnauthorizedAccessException)
                {
                    Logger.Warning?.Print(LogClass.Application, $"Failed to get access to directory: \"{dir}\"");
                }

                if (content.Length > 0)
                {
                    foreach (string file in content)
                    {
                        yield return file;
                    }
                }

                try
                {
                    content = Directory.GetDirectories(dir);
                }
                catch (UnauthorizedAccessException)
                {
                    Logger.Warning?.Print(LogClass.Application, $"Failed to get access to directory: \"{dir}\"");
                }

                if (content.Length > 0)
                {
                    foreach (string subdir in content)
                    {
                        stack.Push(subdir);
                    }
                }
            }
        }

        public void ReadControlData(IFileSystem controlFs, Span<byte> outProperty)
        {
            using var controlFile = new UniqueRef<IFile>();

            controlFs.OpenFile(ref controlFile.Ref(), "/control.nacp".ToU8Span(), OpenMode.Read).ThrowIfFailure();
            controlFile.Get.Read(out _, 0, outProperty, ReadOption.None).ThrowIfFailure();
        }

        public void LoadApplications(List<string> appDirs, Language desiredTitleLanguage)
        {
            int numApplicationsFound  = 0;
            int numApplicationsLoaded = 0;

            _desiredTitleLanguage = desiredTitleLanguage;

            // Builds the applications list with paths to found applications
            List<string> applications = new List<string>();

            foreach (string appDir in appDirs)
            {

                if (!Directory.Exists(appDir))
                {
                    Logger.Warning?.Print(LogClass.Application, $"The \"game_dirs\" section in \"Config.json\" contains an invalid directory: \"{appDir}\"");

                    continue;
                }

                foreach (string app in GetFilesInDirectory(appDir))
                {
                    if ((Path.GetExtension(app).ToLower() == ".nsp") ||
                        (Path.GetExtension(app).ToLower() == ".pfs0") ||
                        (Path.GetExtension(app).ToLower() == ".xci") ||
                        (Path.GetExtension(app).ToLower() == ".nca") ||
                        (Path.GetExtension(app).ToLower() == ".nro") ||
                        (Path.GetExtension(app).ToLower() == ".nso"))
                    {
                        applications.Add(app);
                        numApplicationsFound++;
                    }
                }
            }

            // Loops through applications list, creating a struct and then firing an event containing the struct for each application
            foreach (string applicationPath in applications)
            {
                double fileSize        = new FileInfo(applicationPath).Length * 0.000000000931;
                string titleName       = "Unknown";
                string titleId         = "0000000000000000";
                string developer       = "Unknown";
                string version         = "0";
                byte[] applicationIcon = null;

                BlitStruct<ApplicationControlProperty> controlHolder = new BlitStruct<ApplicationControlProperty>(1);

                try
                {
                    using (FileStream file = new FileStream(applicationPath, FileMode.Open, FileAccess.Read))
                    {
                        if ((Path.GetExtension(applicationPath).ToLower() == ".nsp")  ||
                            (Path.GetExtension(applicationPath).ToLower() == ".pfs0") ||
                            (Path.GetExtension(applicationPath).ToLower() == ".xci"))
                        {
                            try
                            {
                                PartitionFileSystem pfs;

                                bool isExeFs = false;

                                if (Path.GetExtension(applicationPath).ToLower() == ".xci")
                                {
                                    Xci xci = new Xci(_virtualFileSystem.KeySet, file.AsStorage());

                                    pfs = xci.OpenPartition(XciPartitionType.Secure);
                                }
                                else
                                {
                                    pfs = new PartitionFileSystem(file.AsStorage());

                                    // If the NSP doesn't have a main NCA, decrement the number of applications found and then continue to the next application.
                                    bool hasMainNca = false;

                                    foreach (DirectoryEntryEx fileEntry in pfs.EnumerateEntries("/", "*"))
                                    {
                                        if (Path.GetExtension(fileEntry.FullPath).ToLower() == ".nca")
                                        {
                                            using var ncaFile = new UniqueRef<IFile>();

                                            pfs.OpenFile(ref ncaFile.Ref(), fileEntry.FullPath.ToU8Span(), OpenMode.Read).ThrowIfFailure();

                                            Nca nca       = new Nca(_virtualFileSystem.KeySet, ncaFile.Get.AsStorage());
                                            int dataIndex = Nca.GetSectionIndexFromType(NcaSectionType.Data, NcaContentType.Program);

                                            // Some main NCAs don't have a data partition, so check if the partition exists before opening it
                                            if (nca.Header.ContentType == NcaContentType.Program && !(nca.SectionExists(NcaSectionType.Data) && nca.Header.GetFsHeader(dataIndex).IsPatchSection()))
                                            {
                                                hasMainNca = true;

                                                break;
                                            }
                                        }
                                        else if (Path.GetFileNameWithoutExtension(fileEntry.FullPath) == "main")
                                        {
                                            isExeFs = true;
                                        }
                                    }

                                    if (!hasMainNca && !isExeFs)
                                    {
                                        numApplicationsFound--;

                                        continue;
                                    }
                                }

                                if (isExeFs)
                                {
                                    applicationIcon = _nspIcon;

                                    using var npdmFile = new UniqueRef<IFile>();

                                    Result result = pfs.OpenFile(ref npdmFile.Ref(), "/main.npdm".ToU8Span(), OpenMode.Read);

                                    if (ResultFs.PathNotFound.Includes(result))
                                    {
                                        Npdm npdm = new Npdm(npdmFile.Get.AsStream());

                                        titleName = npdm.TitleName;
                                        titleId   = npdm.Aci0.TitleId.ToString("x16");
                                    }
                                }
                                else
                                {
                                    GetControlFsAndTitleId(pfs, out IFileSystem controlFs, out titleId);

                                    // Check if there is an update available.
                                    if (IsUpdateApplied(titleId, out IFileSystem updatedControlFs))
                                    {
                                        // Replace the original ControlFs by the updated one.
                                        controlFs = updatedControlFs;
                                    }

                                    ReadControlData(controlFs, controlHolder.ByteSpan);

                                    GetGameInformation(ref controlHolder.Value, out titleName, out _, out developer, out version);

                                    // Read the icon from the ControlFS and store it as a byte array
                                    try
                                    {
                                        using var icon = new UniqueRef<IFile>();

                                        controlFs.OpenFile(ref icon.Ref(), $"/icon_{_desiredTitleLanguage}.dat".ToU8Span(), OpenMode.Read).ThrowIfFailure();

                                        using (MemoryStream stream = new MemoryStream())
                                        {
                                            icon.Get.AsStream().CopyTo(stream);
                                            applicationIcon = stream.ToArray();
                                        }
                                    }
                                    catch (HorizonResultException)
                                    {
                                        foreach (DirectoryEntryEx entry in controlFs.EnumerateEntries("/", "*"))
                                        {
                                            if (entry.Name == "control.nacp")
                                            {
                                                continue;
                                            }

                                            using var icon = new UniqueRef<IFile>();

                                            controlFs.OpenFile(ref icon.Ref(), entry.FullPath.ToU8Span(), OpenMode.Read).ThrowIfFailure();

                                            using (MemoryStream stream = new MemoryStream())
                                            {
                                                icon.Get.AsStream().CopyTo(stream);
                                                applicationIcon = stream.ToArray();
                                            }

                                            if (applicationIcon != null)
                                            {
                                                break;
                                            }
                                        }

                                        if (applicationIcon == null)
                                        {
                                            applicationIcon = Path.GetExtension(applicationPath).ToLower() == ".xci" ? _xciIcon : _nspIcon;
                                        }
                                    }
                                }
                            }
                            catch (MissingKeyException exception)
                            {
                                applicationIcon = Path.GetExtension(applicationPath).ToLower() == ".xci" ? _xciIcon : _nspIcon;

                                Logger.Warning?.Print(LogClass.Application, $"Your key set is missing a key with the name: {exception.Name}");
                            }
                            catch (InvalidDataException)
                            {
                                applicationIcon = Path.GetExtension(applicationPath).ToLower() == ".xci" ? _xciIcon : _nspIcon;

                                Logger.Warning?.Print(LogClass.Application, $"The header key is incorrect or missing and therefore the NCA header content type check has failed. Errored File: {applicationPath}");
                            }
                            catch (Exception exception)
                            {
                                Logger.Warning?.Print(LogClass.Application, $"The file encountered was not of a valid type. File: '{applicationPath}' Error: {exception}");

                                numApplicationsFound--;

                                continue;
                            }
                        }
                        else if (Path.GetExtension(applicationPath).ToLower() == ".nro")
                        {
                            BinaryReader reader = new BinaryReader(file);

                            byte[] Read(long position, int size)
                            {
                                file.Seek(position, SeekOrigin.Begin);

                                return reader.ReadBytes(size);
                            }

                            try
                            {
                                file.Seek(24, SeekOrigin.Begin);

                                int assetOffset = reader.ReadInt32();

                                if (Encoding.ASCII.GetString(Read(assetOffset, 4)) == "ASET")
                                {
                                    byte[] iconSectionInfo = Read(assetOffset + 8, 0x10);

                                    long iconOffset = BitConverter.ToInt64(iconSectionInfo, 0);
                                    long iconSize   = BitConverter.ToInt64(iconSectionInfo, 8);

                                    ulong nacpOffset = reader.ReadUInt64();
                                    ulong nacpSize   = reader.ReadUInt64();

                                    // Reads and stores game icon as byte array
                                    applicationIcon = Read(assetOffset + iconOffset, (int) iconSize);

                                    // Read the NACP data
                                    Read(assetOffset + (int)nacpOffset, (int)nacpSize).AsSpan().CopyTo(controlHolder.ByteSpan);

                                    GetGameInformation(ref controlHolder.Value, out titleName, out titleId, out developer, out version);
                                }
                                else
                                {
                                    applicationIcon = _nroIcon;
                                    titleName       = Path.GetFileNameWithoutExtension(applicationPath);
                                }
                            }
                            catch
                            {
                                Logger.Warning?.Print(LogClass.Application, $"The file encountered was not of a valid type. Errored File: {applicationPath}");

                                numApplicationsFound--;

                                continue;
                            }
                        }
                        else if (Path.GetExtension(applicationPath).ToLower() == ".nca")
                        {
                            try
                            {
                                Nca nca       = new Nca(_virtualFileSystem.KeySet, new FileStream(applicationPath, FileMode.Open, FileAccess.Read).AsStorage());
                                int dataIndex = Nca.GetSectionIndexFromType(NcaSectionType.Data, NcaContentType.Program);

                                if (nca.Header.ContentType != NcaContentType.Program || (nca.SectionExists(NcaSectionType.Data) && nca.Header.GetFsHeader(dataIndex).IsPatchSection()))
                                {
                                    numApplicationsFound--;

                                    continue;
                                }
                            }
                            catch (InvalidDataException)
                            {
                                Logger.Warning?.Print(LogClass.Application, $"The NCA header content type check has failed. This is usually because the header key is incorrect or missing. Errored File: {applicationPath}");
                            }
                            catch
                            {
                                Logger.Warning?.Print(LogClass.Application, $"The file encountered was not of a valid type. Errored File: {applicationPath}");

                                numApplicationsFound--;

                                continue;
                            }

                            applicationIcon = _ncaIcon;
                            titleName       = Path.GetFileNameWithoutExtension(applicationPath);
                        }
                        // If its an NSO we just set defaults
                        else if (Path.GetExtension(applicationPath).ToLower() == ".nso")
                        {
                            applicationIcon = _nsoIcon;
                            titleName       = Path.GetFileNameWithoutExtension(applicationPath);
                        }
                    }
                }
                catch (IOException exception)
                {
                    Logger.Warning?.Print(LogClass.Application, exception.Message);

                    numApplicationsFound--;

                    continue;
                }

                ApplicationMetadata appMetadata = LoadAndSaveMetaData(titleId);

                if (appMetadata.LastPlayed != "Never" && !DateTime.TryParse(appMetadata.LastPlayed, out _))
                {
                    Logger.Warning?.Print(LogClass.Application, $"Last played datetime \"{appMetadata.LastPlayed}\" is invalid for current system culture, skipping (did current culture change?)");

                    appMetadata.LastPlayed = "Never";
                }

                ApplicationData data = new ApplicationData
                {
                    Favorite      = appMetadata.Favorite,
                    Icon          = applicationIcon,
                    TitleName     = titleName,
                    TitleId       = titleId,
                    Developer     = developer,
                    Version       = version,
                    TimePlayed    = ConvertSecondsToReadableString(appMetadata.TimePlayed),
                    LastPlayed    = appMetadata.LastPlayed,
                    FileExtension = Path.GetExtension(applicationPath).ToUpper().Remove(0, 1),
                    FileSize      = (fileSize < 1) ? (fileSize * 1024).ToString("0.##") + "MB" : fileSize.ToString("0.##") + "GB",
                    Path          = applicationPath,
                    ControlHolder = controlHolder
                };

                numApplicationsLoaded++;

                OnApplicationAdded(new ApplicationAddedEventArgs()
                {
                    AppData = data
                });

                OnApplicationCountUpdated(new ApplicationCountUpdatedEventArgs()
                {
                    NumAppsFound  = numApplicationsFound,
                    NumAppsLoaded = numApplicationsLoaded
                });
            }

            OnApplicationCountUpdated(new ApplicationCountUpdatedEventArgs()
            {
                NumAppsFound  = numApplicationsFound,
                NumAppsLoaded = numApplicationsLoaded
            });
        }

        protected void OnApplicationAdded(ApplicationAddedEventArgs e)
        {
            ApplicationAdded?.Invoke(null, e);
        }

        protected void OnApplicationCountUpdated(ApplicationCountUpdatedEventArgs e)
        {
            ApplicationCountUpdated?.Invoke(null, e);
        }

        private void GetControlFsAndTitleId(PartitionFileSystem pfs, out IFileSystem controlFs, out string titleId)
        {
            (_, _, Nca controlNca) = ApplicationLoader.GetGameData(_virtualFileSystem, pfs, 0);

            // Return the ControlFS
            controlFs = controlNca?.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.None);
            titleId   = controlNca?.Header.TitleId.ToString("x16");
        }

        internal ApplicationMetadata LoadAndSaveMetaData(string titleId, Action<ApplicationMetadata> modifyFunction = null)
        {
            string metadataFolder = Path.Combine(AppDataManager.GamesDirPath, titleId, "gui");
            string metadataFile   = Path.Combine(metadataFolder, "metadata.json");

            ApplicationMetadata appMetadata;

            if (!File.Exists(metadataFile))
            {
                Directory.CreateDirectory(metadataFolder);

                appMetadata = new ApplicationMetadata();

                using (FileStream stream = File.Create(metadataFile, 4096, FileOptions.WriteThrough))
                {
                    JsonHelper.Serialize(stream, appMetadata, true);
                }
            }

            try
            {
                appMetadata = JsonHelper.DeserializeFromFile<ApplicationMetadata>(metadataFile);
            }
            catch (JsonException)
            {
                Logger.Warning?.Print(LogClass.Application, $"Failed to parse metadata json for {titleId}. Loading defaults.");

                appMetadata = new ApplicationMetadata();
            }

            if (modifyFunction != null)
            {
                modifyFunction(appMetadata);

                using (FileStream stream = File.Create(metadataFile, 4096, FileOptions.WriteThrough))
                {
                    JsonHelper.Serialize(stream, appMetadata, true);
                }
            }

            return appMetadata;
        }

        private string ConvertSecondsToReadableString(double seconds)
        {
            const int secondsPerMinute = 60;
            const int secondsPerHour   = secondsPerMinute * 60;
            const int secondsPerDay    = secondsPerHour   * 24;

            string readableString;

            if (seconds < secondsPerMinute)
            {
                readableString = $"{seconds}s";
            }
            else if (seconds < secondsPerHour)
            {
                readableString = $"{Math.Round(seconds / secondsPerMinute, 2, MidpointRounding.AwayFromZero)} mins";
            }
            else if (seconds < secondsPerDay)
            {
                readableString = $"{Math.Round(seconds / secondsPerHour, 2, MidpointRounding.AwayFromZero)} hrs";
            }
            else
            {
                readableString = $"{Math.Round(seconds / secondsPerDay, 2, MidpointRounding.AwayFromZero)} days";
            }

            return readableString;
        }

        private void GetGameInformation(ref ApplicationControlProperty controlData, out string titleName, out string titleId, out string publisher, out string version)
        {
            _ = Enum.TryParse(_desiredTitleLanguage.ToString(), out TitleLanguage desiredTitleLanguage);

            if (controlData.Title.ItemsRo.Length > (int)desiredTitleLanguage)
            {
                titleName = controlData.Title[(int)desiredTitleLanguage].NameString.ToString();
                publisher = controlData.Title[(int)desiredTitleLanguage].PublisherString.ToString();
            }
            else
            {
                titleName = null;
                publisher = null;
            }

            if (string.IsNullOrWhiteSpace(titleName))
            {
                foreach (ref readonly var controlTitle in controlData.Title.ItemsRo)
                {
                    if (!controlTitle.NameString.IsEmpty())
                    {
                        titleName = controlTitle.NameString.ToString();

                        break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(publisher))
            {
                foreach (ref readonly var controlTitle in controlData.Title.ItemsRo)
                {
                    if (!controlTitle.PublisherString.IsEmpty())
                    {
                        publisher = controlTitle.PublisherString.ToString();

                        break;
                    }
                }
            }

            if (controlData.PresenceGroupId != 0)
            {
                titleId = controlData.PresenceGroupId.ToString("x16");
            }
            else if (controlData.SaveDataOwnerId != 0)
            {
                titleId = controlData.SaveDataOwnerId.ToString();
            }
            else if (controlData.AddOnContentBaseId != 0)
            {
                titleId = (controlData.AddOnContentBaseId - 0x1000).ToString("x16");
            }
            else
            {
                titleId = "0000000000000000";
            }

            version = controlData.DisplayVersionString.ToString();
        }

        private bool IsUpdateApplied(string titleId, out IFileSystem updatedControlFs)
        {
            updatedControlFs = null;
            
            string updatePath = "(unknown)";

            try
            {
                (Nca patchNca, Nca controlNca) = ApplicationLoader.GetGameUpdateData(_virtualFileSystem, titleId, 0, out updatePath);

                if (patchNca != null && controlNca != null)
                {
                    updatedControlFs = controlNca?.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.None);

                    return true;
                }
            }
            catch (InvalidDataException)
            {
                Logger.Warning?.Print(LogClass.Application,
                    $"The header key is incorrect or missing and therefore the NCA header content type check has failed. Errored File: {updatePath}");
            }
            catch (MissingKeyException exception)
            {
                Logger.Warning?.Print(LogClass.Application, $"Your key set is missing a key with the name: {exception.Name}. Errored File: {updatePath}");
            }

            return false;
        }
    }
}