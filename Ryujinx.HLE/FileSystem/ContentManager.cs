using LibHac.Common;
using LibHac.Common.Keys;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Ncm;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using LibHac.Tools.Ncm;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.Exceptions;
using Ryujinx.HLE.HOS.Services.Ssl;
using Ryujinx.HLE.HOS.Services.Time;
using Ryujinx.HLE.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Path = System.IO.Path;

namespace Ryujinx.HLE.FileSystem
{
    public class ContentManager
    {
        private const ulong SystemVersionTitleId = 0x0100000000000809;
        private const ulong SystemUpdateTitleId  = 0x0100000000000816;

        private Dictionary<StorageId, LinkedList<LocationEntry>> _locationEntries;

        private Dictionary<string, ulong>  _sharedFontTitleDictionary;
        private Dictionary<ulong, string>  _systemTitlesNameDictionary;
        private Dictionary<string, string> _sharedFontFilenameDictionary;

        private SortedDictionary<(ulong titleId, NcaContentType type), string> _contentDictionary;

        private struct AocItem
        {
            public readonly string ContainerPath;
            public readonly string NcaPath;
            public bool Enabled;

            public AocItem(string containerPath, string ncaPath, bool enabled)
            {
                ContainerPath = containerPath;
                NcaPath = ncaPath;
                Enabled = enabled;
            }
        }

        private SortedList<ulong, AocItem> _aocData { get; }

        private VirtualFileSystem _virtualFileSystem;

        private readonly object _lock = new object();

        public ContentManager(VirtualFileSystem virtualFileSystem)
        {
            _contentDictionary = new SortedDictionary<(ulong, NcaContentType), string>();
            _locationEntries   = new Dictionary<StorageId, LinkedList<LocationEntry>>();

            _sharedFontTitleDictionary = new Dictionary<string, ulong>
            {
                { "FontStandard",                  0x0100000000000811 },
                { "FontChineseSimplified",         0x0100000000000814 },
                { "FontExtendedChineseSimplified", 0x0100000000000814 },
                { "FontKorean",                    0x0100000000000812 },
                { "FontChineseTraditional",        0x0100000000000813 },
                { "FontNintendoExtended",          0x0100000000000810 }
            };

            _systemTitlesNameDictionary = new Dictionary<ulong, string>()
            {
                { 0x010000000000080E, "TimeZoneBinary"         },
                { 0x0100000000000810, "FontNintendoExtension"  },
                { 0x0100000000000811, "FontStandard"           },
                { 0x0100000000000812, "FontKorean"             },
                { 0x0100000000000813, "FontChineseTraditional" },
                { 0x0100000000000814, "FontChineseSimple"      },
            };

            _sharedFontFilenameDictionary = new Dictionary<string, string>
            {
                { "FontStandard",                  "nintendo_udsg-r_std_003.bfttf" },
                { "FontChineseSimplified",         "nintendo_udsg-r_org_zh-cn_003.bfttf" },
                { "FontExtendedChineseSimplified", "nintendo_udsg-r_ext_zh-cn_003.bfttf" },
                { "FontKorean",                    "nintendo_udsg-r_ko_003.bfttf" },
                { "FontChineseTraditional",        "nintendo_udjxh-db_zh-tw_003.bfttf" },
                { "FontNintendoExtended",          "nintendo_ext_003.bfttf" }
            };

            _virtualFileSystem = virtualFileSystem;

            _aocData = new SortedList<ulong, AocItem>();
        }

        public void LoadEntries(Switch device = null)
        {
            lock (_lock)
            {
                _contentDictionary = new SortedDictionary<(ulong, NcaContentType), string>();
                _locationEntries   = new Dictionary<StorageId, LinkedList<LocationEntry>>();

                foreach (StorageId storageId in Enum.GetValues<StorageId>())
                {
                    string contentDirectory    = null;
                    string contentPathString   = null;
                    string registeredDirectory = null;

                    try
                    {
                        contentPathString   = ContentPath.GetContentPath(storageId);
                        contentDirectory    = ContentPath.GetRealPath(_virtualFileSystem, contentPathString);
                        registeredDirectory = Path.Combine(contentDirectory, "registered");
                    }
                    catch (NotSupportedException)
                    {
                        continue;
                    }

                    Directory.CreateDirectory(registeredDirectory);

                    LinkedList<LocationEntry> locationList = new LinkedList<LocationEntry>();

                    void AddEntry(LocationEntry entry)
                    {
                        locationList.AddLast(entry);
                    }

                    foreach (string directoryPath in Directory.EnumerateDirectories(registeredDirectory))
                    {
                        if (Directory.GetFiles(directoryPath).Length > 0)
                        {
                            string ncaName = new DirectoryInfo(directoryPath).Name.Replace(".nca", string.Empty);

                            using (FileStream ncaFile = File.OpenRead(Directory.GetFiles(directoryPath)[0]))
                            {
                                Nca nca = new Nca(_virtualFileSystem.KeySet, ncaFile.AsStorage());

                                string switchPath = contentPathString + ":/" + ncaFile.Name.Replace(contentDirectory, string.Empty).TrimStart(Path.DirectorySeparatorChar);

                                // Change path format to switch's
                                switchPath = switchPath.Replace('\\', '/');

                                LocationEntry entry = new LocationEntry(switchPath,
                                                                        0,
                                                                        nca.Header.TitleId,
                                                                        nca.Header.ContentType);

                                AddEntry(entry);

                                _contentDictionary.Add((nca.Header.TitleId, nca.Header.ContentType), ncaName);
                            }
                        }
                    }

                    foreach (string filePath in Directory.EnumerateFiles(contentDirectory))
                    {
                        if (Path.GetExtension(filePath) == ".nca")
                        {
                            string ncaName = Path.GetFileNameWithoutExtension(filePath);

                            using (FileStream ncaFile = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                            {
                                Nca nca = new Nca(_virtualFileSystem.KeySet, ncaFile.AsStorage());

                                string switchPath = contentPathString + ":/" + filePath.Replace(contentDirectory, string.Empty).TrimStart(Path.DirectorySeparatorChar);

                                // Change path format to switch's
                                switchPath = switchPath.Replace('\\', '/');

                                LocationEntry entry = new LocationEntry(switchPath,
                                                                        0,
                                                                        nca.Header.TitleId,
                                                                        nca.Header.ContentType);

                                AddEntry(entry);

                                _contentDictionary.Add((nca.Header.TitleId, nca.Header.ContentType), ncaName);
                            }
                        }
                    }

                    if (_locationEntries.ContainsKey(storageId) && _locationEntries[storageId]?.Count == 0)
                    {
                        _locationEntries.Remove(storageId);
                    }

                    if (!_locationEntries.ContainsKey(storageId))
                    {
                        _locationEntries.Add(storageId, locationList);
                    }
                }

                if (device != null)
                {
                    TimeManager.Instance.InitializeTimeZone(device);
                    BuiltInCertificateManager.Instance.Initialize(device);
                    device.System.SharedFontManager.Initialize();
                }
            }
        }

        // fs must contain AOC nca files in its root
        public void AddAocData(IFileSystem fs, string containerPath, ulong aocBaseId, IntegrityCheckLevel integrityCheckLevel)
        {
            _virtualFileSystem.ImportTickets(fs);

            foreach (var ncaPath in fs.EnumerateEntries("*.cnmt.nca", SearchOptions.Default))
            {
                using var ncaFile = new UniqueRef<IFile>();

                fs.OpenFile(ref ncaFile.Ref(), ncaPath.FullPath.ToU8Span(), OpenMode.Read);
                var nca = new Nca(_virtualFileSystem.KeySet, ncaFile.Get.AsStorage());
                if (nca.Header.ContentType != NcaContentType.Meta)
                {
                    Logger.Warning?.Print(LogClass.Application, $"{ncaPath} is not a valid metadata file");

                    continue;
                }

                using var pfs0 = nca.OpenFileSystem(0, integrityCheckLevel);
                using var cnmtFile = new UniqueRef<IFile>();

                pfs0.OpenFile(ref cnmtFile.Ref(), pfs0.EnumerateEntries().Single().FullPath.ToU8Span(), OpenMode.Read);

                var cnmt = new Cnmt(cnmtFile.Get.AsStream());

                if (cnmt.Type != ContentMetaType.AddOnContent || (cnmt.TitleId & 0xFFFFFFFFFFFFE000) != aocBaseId)
                {
                    continue;
                }

                string ncaId = BitConverter.ToString(cnmt.ContentEntries[0].NcaId).Replace("-", "").ToLower();
                if (!_aocData.TryAdd(cnmt.TitleId, new AocItem(containerPath, $"{ncaId}.nca", true)))
                {
                    Logger.Warning?.Print(LogClass.Application, $"Duplicate AddOnContent detected. TitleId {cnmt.TitleId:X16}");
                }
                else
                {
                    Logger.Info?.Print(LogClass.Application, $"Found AddOnContent with TitleId {cnmt.TitleId:X16}");
                }
            }
        }

        public void AddAocItem(ulong titleId, string containerPath, string ncaPath, bool enabled)
        {
            if (!_aocData.TryAdd(titleId, new AocItem(containerPath, ncaPath, enabled)))
            {
                Logger.Warning?.Print(LogClass.Application, $"Duplicate AddOnContent detected. TitleId {titleId:X16}");
            }
            else
            {
                Logger.Info?.Print(LogClass.Application, $"Found AddOnContent with TitleId {titleId:X16}");

                using (FileStream fileStream = File.OpenRead(containerPath))
                using (PartitionFileSystem pfs = new PartitionFileSystem(fileStream.AsStorage()))
                {
                    _virtualFileSystem.ImportTickets(pfs);
                }
            }
        }

        public void ClearAocData() => _aocData.Clear();

        public int GetAocCount() => _aocData.Where(e => e.Value.Enabled).Count();

        public IList<ulong> GetAocTitleIds() => _aocData.Where(e => e.Value.Enabled).Select(e => e.Key).ToList();

        public bool GetAocDataStorage(ulong aocTitleId, out IStorage aocStorage, IntegrityCheckLevel integrityCheckLevel)
        {
            aocStorage = null;

            if (_aocData.TryGetValue(aocTitleId, out AocItem aoc) && aoc.Enabled)
            {
                var file = new FileStream(aoc.ContainerPath, FileMode.Open, FileAccess.Read);
                using var ncaFile = new UniqueRef<IFile>();
                PartitionFileSystem pfs;

                switch (Path.GetExtension(aoc.ContainerPath))
                {
                    case ".xci":
                        pfs = new Xci(_virtualFileSystem.KeySet, file.AsStorage()).OpenPartition(XciPartitionType.Secure);
                        pfs.OpenFile(ref ncaFile.Ref(), aoc.NcaPath.ToU8Span(), OpenMode.Read);
                        break;
                    case ".nsp":
                        pfs = new PartitionFileSystem(file.AsStorage());
                        pfs.OpenFile(ref ncaFile.Ref(), aoc.NcaPath.ToU8Span(), OpenMode.Read);
                        break;
                    default:
                        return false; // Print error?
                }

                aocStorage = new Nca(_virtualFileSystem.KeySet, ncaFile.Get.AsStorage()).OpenStorage(NcaSectionType.Data, integrityCheckLevel);
                
                return true;
            }

            return false;
        }

        public void ClearEntry(ulong titleId, NcaContentType contentType, StorageId storageId)
        {
            lock (_lock)
            {
                RemoveLocationEntry(titleId, contentType, storageId);
            }
        }

        public void RefreshEntries(StorageId storageId, int flag)
        {
            lock (_lock)
            {
                LinkedList<LocationEntry> locationList      = _locationEntries[storageId];
                LinkedListNode<LocationEntry> locationEntry = locationList.First;

                while (locationEntry != null)
                {
                    LinkedListNode<LocationEntry> nextLocationEntry = locationEntry.Next;

                    if (locationEntry.Value.Flag == flag)
                    {
                        locationList.Remove(locationEntry.Value);
                    }

                    locationEntry = nextLocationEntry;
                }
            }
        }

        public bool HasNca(string ncaId, StorageId storageId)
        {
            lock (_lock)
            {
                if (_contentDictionary.ContainsValue(ncaId))
                {
                    var content = _contentDictionary.FirstOrDefault(x => x.Value == ncaId);
                    ulong titleId = content.Key.Item1;

                    NcaContentType contentType = content.Key.type;
                    StorageId storage = GetInstalledStorage(titleId, contentType, storageId);

                    return storage == storageId;
                }
            }

            return false;
        }

        public UInt128 GetInstalledNcaId(ulong titleId, NcaContentType contentType)
        {
            lock (_lock)
            {
                if (_contentDictionary.ContainsKey((titleId, contentType)))
                {
                    return new UInt128(_contentDictionary[(titleId, contentType)]);
                }
            }

            return new UInt128();
        }

        public StorageId GetInstalledStorage(ulong titleId, NcaContentType contentType, StorageId storageId)
        {
            lock (_lock)
            {
                LocationEntry locationEntry = GetLocation(titleId, contentType, storageId);

                return locationEntry.ContentPath != null ? ContentPath.GetStorageId(locationEntry.ContentPath) : StorageId.None;
            }
        }

        public string GetInstalledContentPath(ulong titleId, StorageId storageId, NcaContentType contentType)
        {
            lock (_lock)
            {
                LocationEntry locationEntry = GetLocation(titleId, contentType, storageId);

                if (VerifyContentType(locationEntry, contentType))
                {
                    return locationEntry.ContentPath;
                }
            }

            return string.Empty;
        }

        public void RedirectLocation(LocationEntry newEntry, StorageId storageId)
        {
            lock (_lock)
            {
                LocationEntry locationEntry = GetLocation(newEntry.TitleId, newEntry.ContentType, storageId);

                if (locationEntry.ContentPath != null)
                {
                    RemoveLocationEntry(newEntry.TitleId, newEntry.ContentType, storageId);
                }

                AddLocationEntry(newEntry, storageId);
            }
        }

        private bool VerifyContentType(LocationEntry locationEntry, NcaContentType contentType)
        {
            if (locationEntry.ContentPath == null)
            {
                return false;
            }
            
            string installedPath = _virtualFileSystem.SwitchPathToSystemPath(locationEntry.ContentPath);

            if (!string.IsNullOrWhiteSpace(installedPath))
            {
                if (File.Exists(installedPath))
                {
                    using (FileStream file = new FileStream(installedPath, FileMode.Open, FileAccess.Read))
                    {
                        Nca nca = new Nca(_virtualFileSystem.KeySet, file.AsStorage());
                        bool contentCheck = nca.Header.ContentType == contentType;

                        return contentCheck;
                    }
                }
            }

            return false;
        }

        private void AddLocationEntry(LocationEntry entry, StorageId storageId)
        {
            LinkedList<LocationEntry> locationList = null;

            if (_locationEntries.ContainsKey(storageId))
            {
                locationList = _locationEntries[storageId];
            }

            if (locationList != null)
            {
                if (locationList.Contains(entry))
                {
                    locationList.Remove(entry);
                }

                locationList.AddLast(entry);
            }
        }

        private void RemoveLocationEntry(ulong titleId, NcaContentType contentType, StorageId storageId)
        {
            LinkedList<LocationEntry> locationList = null;

            if (_locationEntries.ContainsKey(storageId))
            {
                locationList = _locationEntries[storageId];
            }

            if (locationList != null)
            {
                LocationEntry entry =
                    locationList.ToList().Find(x => x.TitleId == titleId && x.ContentType == contentType);

                if (entry.ContentPath != null)
                {
                    locationList.Remove(entry);
                }
            }
        }

        public bool TryGetFontTitle(string fontName, out ulong titleId)
        {
            return _sharedFontTitleDictionary.TryGetValue(fontName, out titleId);
        }

        public bool TryGetFontFilename(string fontName, out string filename)
        {
            return _sharedFontFilenameDictionary.TryGetValue(fontName, out filename);
        }

        public bool TryGetSystemTitlesName(ulong titleId, out string name)
        {
            return _systemTitlesNameDictionary.TryGetValue(titleId, out name);
        }

        private LocationEntry GetLocation(ulong titleId, NcaContentType contentType, StorageId storageId)
        {
            LinkedList<LocationEntry> locationList = _locationEntries[storageId];

            return locationList.ToList().Find(x => x.TitleId == titleId && x.ContentType == contentType);
        }

        public void InstallFirmware(string firmwareSource)
        {
            string contentPathString   = ContentPath.GetContentPath(StorageId.BuiltInSystem);
            string contentDirectory    = ContentPath.GetRealPath(_virtualFileSystem, contentPathString);
            string registeredDirectory = Path.Combine(contentDirectory, "registered");
            string temporaryDirectory  = Path.Combine(contentDirectory, "temp");

            if (Directory.Exists(temporaryDirectory))
            {
                Directory.Delete(temporaryDirectory, true);
            }

            if (Directory.Exists(firmwareSource))
            {
                InstallFromDirectory(firmwareSource, temporaryDirectory);
                FinishInstallation(temporaryDirectory, registeredDirectory);

                return;
            }

            if (!File.Exists(firmwareSource))
            {
                throw new FileNotFoundException("Firmware file does not exist.");
            }

            FileInfo info = new FileInfo(firmwareSource);

            using (FileStream file = File.OpenRead(firmwareSource))
            {
                switch (info.Extension)
                {
                    case ".zip":
                        using (ZipArchive archive = ZipFile.OpenRead(firmwareSource))
                        {
                            InstallFromZip(archive, temporaryDirectory);
                        }
                        break;
                    case ".xci":
                        Xci xci = new Xci(_virtualFileSystem.KeySet, file.AsStorage());
                        InstallFromCart(xci, temporaryDirectory);
                        break;
                    default:
                        throw new InvalidFirmwarePackageException("Input file is not a valid firmware package");
                }

                FinishInstallation(temporaryDirectory, registeredDirectory);
            }
        }

        private void FinishInstallation(string temporaryDirectory, string registeredDirectory)
        {
            if (Directory.Exists(registeredDirectory))
            {
                new DirectoryInfo(registeredDirectory).Delete(true);
            }

            Directory.Move(temporaryDirectory, registeredDirectory);

            LoadEntries();
        }

        private void InstallFromDirectory(string firmwareDirectory, string temporaryDirectory)
        {
            InstallFromPartition(new LocalFileSystem(firmwareDirectory), temporaryDirectory);
        }

        private void InstallFromPartition(IFileSystem filesystem, string temporaryDirectory)
        {
            foreach (var entry in filesystem.EnumerateEntries("/", "*.nca"))
            {
                Nca nca = new Nca(_virtualFileSystem.KeySet, OpenPossibleFragmentedFile(filesystem, entry.FullPath, OpenMode.Read).AsStorage());

                SaveNca(nca, entry.Name.Remove(entry.Name.IndexOf('.')), temporaryDirectory);
            }
        }

        private void InstallFromCart(Xci gameCard, string temporaryDirectory)
        {
            if (gameCard.HasPartition(XciPartitionType.Update))
            {
                XciPartition partition = gameCard.OpenPartition(XciPartitionType.Update);

                InstallFromPartition(partition, temporaryDirectory);
            }
            else
            {
                throw new Exception("Update not found in xci file.");
            }
        }

        private void InstallFromZip(ZipArchive archive, string temporaryDirectory)
        {
            using (archive)
            {
                foreach (var entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith(".nca") || entry.FullName.EndsWith(".nca/00"))
                    {
                        // Clean up the name and get the NcaId

                        string[] pathComponents = entry.FullName.Replace(".cnmt", "").Split('/');

                        string ncaId = pathComponents[pathComponents.Length - 1];

                        // If this is a fragmented nca, we need to get the previous element.GetZip
                        if (ncaId.Equals("00"))
                        {
                            ncaId = pathComponents[pathComponents.Length - 2];
                        }

                        if (ncaId.Contains(".nca"))
                        {
                            string newPath = Path.Combine(temporaryDirectory, ncaId);

                            Directory.CreateDirectory(newPath);

                            entry.ExtractToFile(Path.Combine(newPath, "00"));
                        }
                    }
                }
            }
        }

        public void SaveNca(Nca nca, string ncaId, string temporaryDirectory)
        {
            string newPath = Path.Combine(temporaryDirectory, ncaId + ".nca");

            Directory.CreateDirectory(newPath);

            using (FileStream file = File.Create(Path.Combine(newPath, "00")))
            {
                nca.BaseStorage.AsStream().CopyTo(file);
            }
        }

        private IFile OpenPossibleFragmentedFile(IFileSystem filesystem, string path, OpenMode mode)
        {
            using var file = new UniqueRef<IFile>();

            if (filesystem.FileExists($"{path}/00"))
            {
                filesystem.OpenFile(ref file.Ref(), $"{path}/00".ToU8Span(), mode);
            }
            else
            {
                filesystem.OpenFile(ref file.Ref(), path.ToU8Span(), mode);
            }

            return file.Release();
        }

        private Stream GetZipStream(ZipArchiveEntry entry)
        {
            MemoryStream dest = new MemoryStream();

            Stream src = entry.Open();

            src.CopyTo(dest);
            src.Dispose();

            return dest;
        }

        public SystemVersion VerifyFirmwarePackage(string firmwarePackage)
        {
            _virtualFileSystem.ReloadKeySet();

            // LibHac.NcaHeader's DecryptHeader doesn't check if HeaderKey is empty and throws InvalidDataException instead
            // So, we check it early for a better user experience.
            if (_virtualFileSystem.KeySet.HeaderKey.IsZeros())
            {
                throw new MissingKeyException("HeaderKey is empty. Cannot decrypt NCA headers.");
            }

            Dictionary<ulong, List<(NcaContentType type, string path)>> updateNcas = new Dictionary<ulong, List<(NcaContentType, string)>>();

            if (Directory.Exists(firmwarePackage))
            {
                return VerifyAndGetVersionDirectory(firmwarePackage);
            }

            if (!File.Exists(firmwarePackage))
            {
                throw new FileNotFoundException("Firmware file does not exist.");
            }

            FileInfo info = new FileInfo(firmwarePackage);

            using (FileStream file = File.OpenRead(firmwarePackage))
            {
                switch (info.Extension)
                {
                    case ".zip":
                        using (ZipArchive archive = ZipFile.OpenRead(firmwarePackage))
                        {
                            return VerifyAndGetVersionZip(archive);
                        }
                    case ".xci":
                        Xci xci = new Xci(_virtualFileSystem.KeySet, file.AsStorage());

                        if (xci.HasPartition(XciPartitionType.Update))
                        {
                            XciPartition partition = xci.OpenPartition(XciPartitionType.Update);

                            return VerifyAndGetVersion(partition);
                        }
                        else
                        {
                            throw new InvalidFirmwarePackageException("Update not found in xci file.");
                        }
                    default:
                        break;
                }
            }

            SystemVersion VerifyAndGetVersionDirectory(string firmwareDirectory)
            {
                return VerifyAndGetVersion(new LocalFileSystem(firmwareDirectory));
            }

            SystemVersion VerifyAndGetVersionZip(ZipArchive archive)
            {
                SystemVersion systemVersion = null;

                foreach (var entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith(".nca") || entry.FullName.EndsWith(".nca/00"))
                    {
                        using (Stream ncaStream = GetZipStream(entry))
                        {
                            IStorage storage = ncaStream.AsStorage();

                            Nca nca = new Nca(_virtualFileSystem.KeySet, storage);

                            if (updateNcas.ContainsKey(nca.Header.TitleId))
                            {
                                updateNcas[nca.Header.TitleId].Add((nca.Header.ContentType, entry.FullName));
                            }
                            else
                            {
                                updateNcas.Add(nca.Header.TitleId, new List<(NcaContentType, string)>());
                                updateNcas[nca.Header.TitleId].Add((nca.Header.ContentType, entry.FullName));
                            }
                        }
                    }
                }

                if (updateNcas.ContainsKey(SystemUpdateTitleId))
                {
                    var ncaEntry = updateNcas[SystemUpdateTitleId];

                    string metaPath = ncaEntry.Find(x => x.type == NcaContentType.Meta).path;

                    CnmtContentMetaEntry[] metaEntries = null;

                    var fileEntry = archive.GetEntry(metaPath);

                    using (Stream ncaStream = GetZipStream(fileEntry))
                    {
                        Nca metaNca = new Nca(_virtualFileSystem.KeySet, ncaStream.AsStorage());

                        IFileSystem fs = metaNca.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.ErrorOnInvalid);

                        string cnmtPath = fs.EnumerateEntries("/", "*.cnmt").Single().FullPath;

                        using var metaFile = new UniqueRef<IFile>();

                        if (fs.OpenFile(ref metaFile.Ref(), cnmtPath.ToU8Span(), OpenMode.Read).IsSuccess())
                        {
                            var meta = new Cnmt(metaFile.Get.AsStream());

                            if (meta.Type == ContentMetaType.SystemUpdate)
                            {
                                metaEntries = meta.MetaEntries;

                                updateNcas.Remove(SystemUpdateTitleId);
                            };
                        }
                    }

                    if (metaEntries == null)
                    {
                        throw new FileNotFoundException("System update title was not found in the firmware package.");
                    }

                    if (updateNcas.ContainsKey(SystemVersionTitleId))
                    {
                        string versionEntry = updateNcas[SystemVersionTitleId].Find(x => x.type != NcaContentType.Meta).path;

                        using (Stream ncaStream = GetZipStream(archive.GetEntry(versionEntry)))
                        {
                            Nca nca = new Nca(_virtualFileSystem.KeySet, ncaStream.AsStorage());

                            var romfs = nca.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.ErrorOnInvalid);

                            using var systemVersionFile = new UniqueRef<IFile>();

                            if (romfs.OpenFile(ref systemVersionFile.Ref(), "/file".ToU8Span(), OpenMode.Read).IsSuccess())
                            {
                                systemVersion = new SystemVersion(systemVersionFile.Get.AsStream());
                            }
                        }
                    }

                    foreach (CnmtContentMetaEntry metaEntry in metaEntries)
                    {
                        if (updateNcas.TryGetValue(metaEntry.TitleId, out ncaEntry))
                        {
                            metaPath = ncaEntry.Find(x => x.type == NcaContentType.Meta).path;

                            string contentPath = ncaEntry.Find(x => x.type != NcaContentType.Meta).path;

                            // Nintendo in 9.0.0, removed PPC and only kept the meta nca of it.
                            // This is a perfect valid case, so we should just ignore the missing content nca and continue.
                            if (contentPath == null)
                            {
                                updateNcas.Remove(metaEntry.TitleId);

                                continue;
                            }

                            ZipArchiveEntry metaZipEntry    = archive.GetEntry(metaPath);
                            ZipArchiveEntry contentZipEntry = archive.GetEntry(contentPath);

                            using (Stream metaNcaStream = GetZipStream(metaZipEntry))
                            {
                                using (Stream contentNcaStream = GetZipStream(contentZipEntry))
                                {
                                    Nca metaNca = new Nca(_virtualFileSystem.KeySet, metaNcaStream.AsStorage());

                                    IFileSystem fs = metaNca.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.ErrorOnInvalid);

                                    string cnmtPath = fs.EnumerateEntries("/", "*.cnmt").Single().FullPath;

                                    using var metaFile = new UniqueRef<IFile>();

                                    if (fs.OpenFile(ref metaFile.Ref(), cnmtPath.ToU8Span(), OpenMode.Read).IsSuccess())
                                    {
                                        var meta = new Cnmt(metaFile.Get.AsStream());

                                        IStorage contentStorage = contentNcaStream.AsStorage();
                                        if (contentStorage.GetSize(out long size).IsSuccess())
                                        {
                                            byte[] contentData = new byte[size];

                                            Span<byte> content = new Span<byte>(contentData);

                                            contentStorage.Read(0, content);

                                            Span<byte> hash = new Span<byte>(new byte[32]);

                                            LibHac.Crypto.Sha256.GenerateSha256Hash(content, hash);

                                            if (LibHac.Common.Utilities.ArraysEqual(hash.ToArray(), meta.ContentEntries[0].Hash))
                                            {
                                                updateNcas.Remove(metaEntry.TitleId);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (updateNcas.Count > 0)
                    {
                        string extraNcas = string.Empty;

                        foreach (var entry in updateNcas)
                        {
                            foreach (var nca in entry.Value)
                            {
                                extraNcas += nca.path + Environment.NewLine;
                            }
                        }

                        throw new InvalidFirmwarePackageException($"Firmware package contains unrelated archives. Please remove these paths: {Environment.NewLine}{extraNcas}");
                    }
                }
                else
                {
                    throw new FileNotFoundException("System update title was not found in the firmware package.");
                }

                return systemVersion;
            }

            SystemVersion VerifyAndGetVersion(IFileSystem filesystem)
            {
                SystemVersion systemVersion = null;

                CnmtContentMetaEntry[] metaEntries = null;

                foreach (var entry in filesystem.EnumerateEntries("/", "*.nca"))
                {
                    IStorage ncaStorage = OpenPossibleFragmentedFile(filesystem, entry.FullPath, OpenMode.Read).AsStorage();

                    Nca nca = new Nca(_virtualFileSystem.KeySet, ncaStorage);

                    if (nca.Header.TitleId == SystemUpdateTitleId && nca.Header.ContentType == NcaContentType.Meta)
                    {
                        IFileSystem fs = nca.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.ErrorOnInvalid);

                        string cnmtPath = fs.EnumerateEntries("/", "*.cnmt").Single().FullPath;

                        using var metaFile = new UniqueRef<IFile>();

                        if (fs.OpenFile(ref metaFile.Ref(), cnmtPath.ToU8Span(), OpenMode.Read).IsSuccess())
                        {
                            var meta = new Cnmt(metaFile.Get.AsStream());

                            if (meta.Type == ContentMetaType.SystemUpdate)
                            {
                                metaEntries = meta.MetaEntries;
                            }
                        };

                        continue;
                    }
                    else if (nca.Header.TitleId == SystemVersionTitleId && nca.Header.ContentType == NcaContentType.Data)
                    {
                        var romfs = nca.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.ErrorOnInvalid);

                        using var systemVersionFile = new UniqueRef<IFile>();

                        if (romfs.OpenFile(ref systemVersionFile.Ref(), "/file".ToU8Span(), OpenMode.Read).IsSuccess())
                        {
                            systemVersion = new SystemVersion(systemVersionFile.Get.AsStream());
                        }
                    }

                    if (updateNcas.ContainsKey(nca.Header.TitleId))
                    {
                        updateNcas[nca.Header.TitleId].Add((nca.Header.ContentType, entry.FullPath));
                    }
                    else
                    {
                        updateNcas.Add(nca.Header.TitleId, new List<(NcaContentType, string)>());
                        updateNcas[nca.Header.TitleId].Add((nca.Header.ContentType, entry.FullPath));
                    }

                    ncaStorage.Dispose();
                }

                if (metaEntries == null)
                {
                    throw new FileNotFoundException("System update title was not found in the firmware package.");
                }

                foreach (CnmtContentMetaEntry metaEntry in metaEntries)
                {
                    if (updateNcas.TryGetValue(metaEntry.TitleId, out var ncaEntry))
                    {
                        var    metaNcaEntry = ncaEntry.Find(x => x.type == NcaContentType.Meta);
                        string contentPath  = ncaEntry.Find(x => x.type != NcaContentType.Meta).path;

                        // Nintendo in 9.0.0, removed PPC and only kept the meta nca of it.
                        // This is a perfect valid case, so we should just ignore the missing content nca and continue.
                        if (contentPath == null)
                        {
                            updateNcas.Remove(metaEntry.TitleId);

                            continue;
                        }

                        IStorage metaStorage = OpenPossibleFragmentedFile(filesystem, metaNcaEntry.path, OpenMode.Read).AsStorage();
                        IStorage contentStorage = OpenPossibleFragmentedFile(filesystem, contentPath, OpenMode.Read).AsStorage();

                        Nca metaNca = new Nca(_virtualFileSystem.KeySet, metaStorage);

                        IFileSystem fs = metaNca.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.ErrorOnInvalid);

                        string cnmtPath = fs.EnumerateEntries("/", "*.cnmt").Single().FullPath;

                        using var metaFile = new UniqueRef<IFile>();

                        if (fs.OpenFile(ref metaFile.Ref(), cnmtPath.ToU8Span(), OpenMode.Read).IsSuccess())
                        {
                            var meta = new Cnmt(metaFile.Get.AsStream());

                            if (contentStorage.GetSize(out long size).IsSuccess())
                            {
                                byte[] contentData = new byte[size];

                                Span<byte> content = new Span<byte>(contentData);

                                contentStorage.Read(0, content);

                                Span<byte> hash = new Span<byte>(new byte[32]);

                                LibHac.Crypto.Sha256.GenerateSha256Hash(content, hash);

                                if (LibHac.Common.Utilities.ArraysEqual(hash.ToArray(), meta.ContentEntries[0].Hash))
                                {
                                    updateNcas.Remove(metaEntry.TitleId);
                                }
                            }
                        }
                    }
                }

                if (updateNcas.Count > 0)
                {
                    string extraNcas = string.Empty;

                    foreach (var entry in updateNcas)
                    {
                        foreach (var (type, path) in entry.Value)
                        {
                            extraNcas += path + Environment.NewLine;
                        }
                    }

                    throw new InvalidFirmwarePackageException($"Firmware package contains unrelated archives. Please remove these paths: {Environment.NewLine}{extraNcas}");
                }

                return systemVersion;
            }

            return null;
        }

        public SystemVersion GetCurrentFirmwareVersion()
        {
            LoadEntries();

            lock (_lock)
            {
                var locationEnties = _locationEntries[StorageId.BuiltInSystem];

                foreach (var entry in locationEnties)
                {
                    if (entry.ContentType == NcaContentType.Data)
                    {
                        var path = _virtualFileSystem.SwitchPathToSystemPath(entry.ContentPath);

                        using (FileStream fileStream = File.OpenRead(path))
                        {
                            Nca nca = new Nca(_virtualFileSystem.KeySet, fileStream.AsStorage());

                            if (nca.Header.TitleId == SystemVersionTitleId && nca.Header.ContentType == NcaContentType.Data)
                            {
                                var romfs = nca.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.ErrorOnInvalid);

                                using var systemVersionFile = new UniqueRef<IFile>();

                                if (romfs.OpenFile(ref systemVersionFile.Ref(), "/file".ToU8Span(), OpenMode.Read).IsSuccess())
                                {
                                    return new SystemVersion(systemVersionFile.Get.AsStream());
                                }
                            }

                        }
                    }
                }
            }

            return null;
        }
    }
}
