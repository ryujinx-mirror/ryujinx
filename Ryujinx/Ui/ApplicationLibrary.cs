using JsonPrettyPrinterPlus;
using LibHac;
using LibHac.Fs;
using LibHac.FsSystem;
using LibHac.FsSystem.NcaUtils;
using LibHac.Spl;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.Loaders.Npdm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Utf8Json;
using Utf8Json.Resolvers;

using TitleLanguage = Ryujinx.HLE.HOS.SystemState.TitleLanguage;

namespace Ryujinx.Ui
{
    public class ApplicationLibrary
    {
        public static event EventHandler<ApplicationAddedEventArgs> ApplicationAdded;

        private static readonly byte[] _nspIcon = GetResourceBytes("Ryujinx.Ui.assets.NSPIcon.png");
        private static readonly byte[] _xciIcon = GetResourceBytes("Ryujinx.Ui.assets.XCIIcon.png");
        private static readonly byte[] _ncaIcon = GetResourceBytes("Ryujinx.Ui.assets.NCAIcon.png");
        private static readonly byte[] _nroIcon = GetResourceBytes("Ryujinx.Ui.assets.NROIcon.png");
        private static readonly byte[] _nsoIcon = GetResourceBytes("Ryujinx.Ui.assets.NSOIcon.png");

        private static Keyset              _keySet;
        private static TitleLanguage       _desiredTitleLanguage;
        private static ApplicationMetadata _appMetadata;

        public static void LoadApplications(List<string> appDirs, Keyset keySet, TitleLanguage desiredTitleLanguage)
        {
            int numApplicationsFound  = 0;
            int numApplicationsLoaded = 0;

            _keySet               = keySet;
            _desiredTitleLanguage = desiredTitleLanguage;

            // Builds the applications list with paths to found applications
            List<string> applications = new List<string>();
            foreach (string appDir in appDirs)
            {
                if (Directory.Exists(appDir) == false)
                {
                    Logger.PrintWarning(LogClass.Application, $"The \"game_dirs\" section in \"Config.json\" contains an invalid directory: \"{appDir}\"");

                    continue;
                }

                foreach (string app in Directory.GetFiles(appDir, "*.*", SearchOption.AllDirectories))
                {
                    if ((Path.GetExtension(app) == ".xci") ||
                        (Path.GetExtension(app) == ".nro") ||
                        (Path.GetExtension(app) == ".nso") ||
                        (Path.GetFileName(app)  == "hbl.nsp"))
                    {
                        applications.Add(app);
                        numApplicationsFound++;
                    }
                    else if ((Path.GetExtension(app) == ".nsp") || (Path.GetExtension(app) == ".pfs0"))
                    {
                        try
                        {
                            bool hasMainNca = false;

                            PartitionFileSystem nsp = new PartitionFileSystem(new FileStream(app, FileMode.Open, FileAccess.Read).AsStorage());
                            foreach (DirectoryEntryEx fileEntry in nsp.EnumerateEntries("/", "*.nca"))
                            {
                                nsp.OpenFile(out IFile ncaFile, fileEntry.FullPath, OpenMode.Read).ThrowIfFailure();

                                Nca nca       = new Nca(_keySet, ncaFile.AsStorage());
                                int dataIndex = Nca.GetSectionIndexFromType(NcaSectionType.Data, NcaContentType.Program);

                                if (nca.Header.ContentType == NcaContentType.Program && !nca.Header.GetFsHeader(dataIndex).IsPatchSection())
                                {
                                    hasMainNca = true;
                                }
                            }

                            if (!hasMainNca)
                            {
                                continue;
                            }
                        }
                        catch (InvalidDataException)
                        {
                            Logger.PrintWarning(LogClass.Application, $"{app}: The header key is incorrect or missing and therefore the NCA header content type check has failed.");
                        }

                        applications.Add(app);
                        numApplicationsFound++;
                    }
                    else if (Path.GetExtension(app) == ".nca")
                    {
                        try
                        {
                            Nca nca       = new Nca(_keySet, new FileStream(app, FileMode.Open, FileAccess.Read).AsStorage());
                            int dataIndex = Nca.GetSectionIndexFromType(NcaSectionType.Data, NcaContentType.Program);

                            if (nca.Header.ContentType != NcaContentType.Program || nca.Header.GetFsHeader(dataIndex).IsPatchSection())
                            {
                                continue;
                            }
                        }
                        catch (InvalidDataException)
                        {
                            Logger.PrintWarning(LogClass.Application, $"{app}: The header key is incorrect or missing and therefore the NCA header content type check has failed.");
                        }

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

                using (FileStream file = new FileStream(applicationPath, FileMode.Open, FileAccess.Read))
                {
                    if ((Path.GetExtension(applicationPath) == ".nsp")  ||
                        (Path.GetExtension(applicationPath) == ".pfs0") ||
                        (Path.GetExtension(applicationPath) == ".xci"))
                    {
                        try
                        {
                            PartitionFileSystem pfs;
                             
                            if (Path.GetExtension(applicationPath) == ".xci")
                            {
                                Xci xci = new Xci(_keySet, file.AsStorage());

                                pfs = xci.OpenPartition(XciPartitionType.Secure);
                            }
                            else
                            {
                                pfs = new PartitionFileSystem(file.AsStorage());
                            }

                            // Store the ControlFS in variable called controlFs
                            IFileSystem controlFs = GetControlFs(pfs);

                            // If this is null then this is probably not a normal NSP, it's probably an ExeFS as an NSP
                            if (controlFs == null)
                            {
                                applicationIcon = _nspIcon;

                                Result result = pfs.OpenFile(out IFile npdmFile, "/main.npdm", OpenMode.Read);

                                if (result != ResultFs.PathNotFound)
                                {
                                    Npdm npdm = new Npdm(npdmFile.AsStream());

                                    titleName = npdm.TitleName;
                                    titleId   = npdm.Aci0.TitleId.ToString("x16");
                                }
                            }
                            else
                            {
                                // Creates NACP class from the NACP file
                                controlFs.OpenFile(out IFile controlNacpFile, "/control.nacp", OpenMode.Read).ThrowIfFailure();

                                Nacp controlData = new Nacp(controlNacpFile.AsStream());

                                // Get the title name, title ID, developer name and version number from the NACP
                                version = controlData.DisplayVersion;

                                titleName = controlData.Descriptions[(int)_desiredTitleLanguage].Title;

                                if (string.IsNullOrWhiteSpace(titleName))
                                {
                                    titleName = controlData.Descriptions.ToList().Find(x => !string.IsNullOrWhiteSpace(x.Title)).Title;
                                }

                                titleId = controlData.PresenceGroupId.ToString("x16");

                                if (string.IsNullOrWhiteSpace(titleId))
                                {
                                    titleId = controlData.SaveDataOwnerId.ToString("x16");
                                }

                                if (string.IsNullOrWhiteSpace(titleId))
                                {
                                    titleId = (controlData.AddOnContentBaseId - 0x1000).ToString("x16");
                                }

                                developer = controlData.Descriptions[(int)_desiredTitleLanguage].Developer;

                                if (string.IsNullOrWhiteSpace(developer))
                                {
                                    developer = controlData.Descriptions.ToList().Find(x => !string.IsNullOrWhiteSpace(x.Developer)).Developer;
                                }

                                // Read the icon from the ControlFS and store it as a byte array
                                try
                                {
                                    controlFs.OpenFile(out IFile icon, $"/icon_{_desiredTitleLanguage}.dat", OpenMode.Read).ThrowIfFailure();

                                    using (MemoryStream stream = new MemoryStream())
                                    {
                                        icon.AsStream().CopyTo(stream);
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

                                        controlFs.OpenFile(out IFile icon, entry.FullPath, OpenMode.Read).ThrowIfFailure();

                                        using (MemoryStream stream = new MemoryStream())
                                        {
                                            icon.AsStream().CopyTo(stream);
                                            applicationIcon = stream.ToArray();
                                        }

                                        if (applicationIcon != null)
                                        {
                                            break;
                                        }
                                    }

                                    if (applicationIcon == null)
                                    {
                                        applicationIcon = Path.GetExtension(applicationPath) == ".xci" ? _xciIcon : _nspIcon;
                                    }
                                }
                            }
                        }
                        catch (MissingKeyException exception)
                        {
                            applicationIcon = Path.GetExtension(applicationPath) == ".xci" ? _xciIcon : _nspIcon;

                            Logger.PrintWarning(LogClass.Application, $"Your key set is missing a key with the name: {exception.Name}");
                        }
                        catch (InvalidDataException)
                        {
                            applicationIcon = Path.GetExtension(applicationPath) == ".xci" ? _xciIcon : _nspIcon;

                            Logger.PrintWarning(LogClass.Application, $"The header key is incorrect or missing and therefore the NCA header content type check has failed. Errored File: {applicationPath}");
                        }
                    }
                    else if (Path.GetExtension(applicationPath) == ".nro")
                    {
                        BinaryReader reader = new BinaryReader(file);

                        byte[] Read(long position, int size)
                        {
                            file.Seek(position, SeekOrigin.Begin);

                            return reader.ReadBytes(size);
                        }

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
                            applicationIcon = Read(assetOffset + iconOffset, (int)iconSize);

                            // Creates memory stream out of byte array which is the NACP
                            using (MemoryStream stream = new MemoryStream(Read(assetOffset + (int)nacpOffset, (int)nacpSize)))
                            {
                                // Creates NACP class from the memory stream
                                Nacp controlData = new Nacp(stream);

                                // Get the title name, title ID, developer name and version number from the NACP
                                version = controlData.DisplayVersion;

                                titleName = controlData.Descriptions[(int)_desiredTitleLanguage].Title;

                                if (string.IsNullOrWhiteSpace(titleName))
                                {
                                    titleName = controlData.Descriptions.ToList().Find(x => !string.IsNullOrWhiteSpace(x.Title)).Title;
                                }

                                titleId = controlData.PresenceGroupId.ToString("x16");

                                if (string.IsNullOrWhiteSpace(titleId))
                                {
                                    titleId = controlData.SaveDataOwnerId.ToString("x16");
                                }

                                if (string.IsNullOrWhiteSpace(titleId))
                                {
                                    titleId = (controlData.AddOnContentBaseId - 0x1000).ToString("x16");
                                }

                                developer = controlData.Descriptions[(int)_desiredTitleLanguage].Developer;

                                if (string.IsNullOrWhiteSpace(developer))
                                {
                                    developer = controlData.Descriptions.ToList().Find(x => !string.IsNullOrWhiteSpace(x.Developer)).Developer;
                                }
                            }
                        }
                        else
                        {
                            applicationIcon = _nroIcon;
                        }
                    }
                    // If its an NCA or NSO we just set defaults
                    else if ((Path.GetExtension(applicationPath) == ".nca") || (Path.GetExtension(applicationPath) == ".nso"))
                    {
                        applicationIcon = Path.GetExtension(applicationPath) == ".nca" ? _ncaIcon : _nsoIcon;
                        titleName       = Path.GetFileNameWithoutExtension(applicationPath);
                    }
                }

                (bool favorite, string timePlayed, string lastPlayed) = GetMetadata(titleId);

                ApplicationData data = new ApplicationData()
                {
                    Favorite      = favorite,
                    Icon          = applicationIcon,
                    TitleName     = titleName,
                    TitleId       = titleId,
                    Developer     = developer,
                    Version       = version,
                    TimePlayed    = timePlayed,
                    LastPlayed    = lastPlayed,
                    FileExtension = Path.GetExtension(applicationPath).ToUpper().Remove(0 ,1),
                    FileSize      = (fileSize < 1) ? (fileSize * 1024).ToString("0.##") + "MB" : fileSize.ToString("0.##") + "GB",
                    Path          = applicationPath,
                };

                numApplicationsLoaded++;

                OnApplicationAdded(new ApplicationAddedEventArgs()
                { 
                    AppData       = data,
                    NumAppsFound  = numApplicationsFound,
                    NumAppsLoaded = numApplicationsLoaded
                });
            }
        }

        protected static void OnApplicationAdded(ApplicationAddedEventArgs e)
        {
            ApplicationAdded?.Invoke(null, e);
        }

        private static byte[] GetResourceBytes(string resourceName)
        {
            Stream resourceStream    = Assembly.GetCallingAssembly().GetManifestResourceStream(resourceName);
            byte[] resourceByteArray = new byte[resourceStream.Length];

            resourceStream.Read(resourceByteArray);

            return resourceByteArray;
        }

        private static IFileSystem GetControlFs(PartitionFileSystem pfs)
        {
            Nca controlNca = null;

            // Add keys to key set if needed
            foreach (DirectoryEntryEx ticketEntry in pfs.EnumerateEntries("/", "*.tik"))
            {
                Result result = pfs.OpenFile(out IFile ticketFile, ticketEntry.FullPath, OpenMode.Read);

                if (result.IsSuccess())
                {
                    Ticket ticket = new Ticket(ticketFile.AsStream());

                    _keySet.ExternalKeySet.Add(new RightsId(ticket.RightsId), new AccessKey(ticket.GetTitleKey(_keySet)));
                }
            }

            // Find the Control NCA and store it in variable called controlNca
            foreach (DirectoryEntryEx fileEntry in pfs.EnumerateEntries("/", "*.nca"))
            {
                pfs.OpenFile(out IFile ncaFile, fileEntry.FullPath, OpenMode.Read).ThrowIfFailure();

                Nca nca = new Nca(_keySet, ncaFile.AsStorage());

                if (nca.Header.ContentType == NcaContentType.Control)
                {
                    controlNca = nca;
                }
            }

            // Return the ControlFS
            return controlNca?.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.None);
        }

        private static (bool favorite, string timePlayed, string lastPlayed) GetMetadata(string titleId)
        {
            string metadataFolder = Path.Combine(new VirtualFileSystem().GetBasePath(), "games", titleId, "gui");
            string metadataFile   = Path.Combine(metadataFolder, "metadata.json");

            IJsonFormatterResolver resolver = CompositeResolver.Create(StandardResolver.AllowPrivateSnakeCase);

            if (!File.Exists(metadataFile))
            {
                Directory.CreateDirectory(metadataFolder);

                _appMetadata = new ApplicationMetadata
                {
                    Favorite   = false,
                    TimePlayed = 0,
                    LastPlayed = "Never"
                };

                byte[] saveData = JsonSerializer.Serialize(_appMetadata, resolver);
                File.WriteAllText(metadataFile, Encoding.UTF8.GetString(saveData, 0, saveData.Length).PrettyPrintJson());
            }

            using (Stream stream = File.OpenRead(metadataFile))
            {
                _appMetadata = JsonSerializer.Deserialize<ApplicationMetadata>(stream, resolver);
            }

            return (_appMetadata.Favorite, ConvertSecondsToReadableString(_appMetadata.TimePlayed), _appMetadata.LastPlayed);
        }

        private static string ConvertSecondsToReadableString(double seconds)
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
    }
}
