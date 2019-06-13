using LibHac.Fs;
using LibHac.Fs.NcaUtils;
using Ryujinx.HLE.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ryujinx.HLE.FileSystem.Content
{
    internal class ContentManager
    {
        private Dictionary<StorageId, LinkedList<LocationEntry>> _locationEntries;

        private Dictionary<string, long> _sharedFontTitleDictionary;
        private Dictionary<string, string> _sharedFontFilenameDictionary;

        private SortedDictionary<(ulong, ContentType), string> _contentDictionary;

        private Switch _device;

        public ContentManager(Switch device)
        {
            _contentDictionary = new SortedDictionary<(ulong, ContentType), string>();
            _locationEntries   = new Dictionary<StorageId, LinkedList<LocationEntry>>();

            _sharedFontTitleDictionary = new Dictionary<string, long>
            {
                { "FontStandard",                  0x0100000000000811 },
                { "FontChineseSimplified",         0x0100000000000814 },
                { "FontExtendedChineseSimplified", 0x0100000000000814 },
                { "FontKorean",                    0x0100000000000812 },
                { "FontChineseTraditional",        0x0100000000000813 },
                { "FontNintendoExtended",          0x0100000000000810 }
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

            _device = device;
        }

        public void LoadEntries()
        {
            _contentDictionary = new SortedDictionary<(ulong, ContentType), string>();

            foreach (StorageId storageId in Enum.GetValues(typeof(StorageId)))
            {
                string contentDirectory    = null;
                string contentPathString   = null;
                string registeredDirectory = null;

                try
                {
                    contentPathString   = LocationHelper.GetContentRoot(storageId);
                    contentDirectory    = LocationHelper.GetRealPath(_device.FileSystem, contentPathString);
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

                        using (FileStream ncaFile = new FileStream(Directory.GetFiles(directoryPath)[0], FileMode.Open, FileAccess.Read))
                        {
                            Nca nca = new Nca(_device.System.KeySet, ncaFile.AsStorage());

                            string switchPath = contentPathString + ":/" + ncaFile.Name.Replace(contentDirectory, string.Empty).TrimStart(Path.DirectorySeparatorChar);

                            // Change path format to switch's
                            switchPath = switchPath.Replace('\\', '/');

                            LocationEntry entry = new LocationEntry(switchPath,
                                                                    0,
                                                                    (long)nca.Header.TitleId,
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
                            Nca nca = new Nca(_device.System.KeySet, ncaFile.AsStorage());

                            string switchPath = contentPathString + ":/" + filePath.Replace(contentDirectory, string.Empty).TrimStart(Path.DirectorySeparatorChar);

                            // Change path format to switch's
                            switchPath = switchPath.Replace('\\', '/');

                            LocationEntry entry = new LocationEntry(switchPath,
                                                                    0,
                                                                    (long)nca.Header.TitleId,
                                                                    nca.Header.ContentType);

                            AddEntry(entry);

                            _contentDictionary.Add((nca.Header.TitleId, nca.Header.ContentType), ncaName);
                        }
                    }
                }

                if(_locationEntries.ContainsKey(storageId) && _locationEntries[storageId]?.Count == 0)
                {
                    _locationEntries.Remove(storageId);
                }

                if (!_locationEntries.ContainsKey(storageId))
                {
                    _locationEntries.Add(storageId, locationList);
                }
            }
        }

        public void ClearEntry(long titleId, ContentType contentType, StorageId storageId)
        {
            RemoveLocationEntry(titleId, contentType, storageId);
        }

        public void RefreshEntries(StorageId storageId, int flag)
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

        public bool HasNca(string ncaId, StorageId storageId)
        {
            if (_contentDictionary.ContainsValue(ncaId))
            {
                var         content     = _contentDictionary.FirstOrDefault(x => x.Value == ncaId);
                long        titleId     = (long)content.Key.Item1;
                ContentType contentType = content.Key.Item2;
                StorageId   storage     = GetInstalledStorage(titleId, contentType, storageId);

                return storage == storageId;
            }

            return false;
        }

        public UInt128 GetInstalledNcaId(long titleId, ContentType contentType)
        {
            if (_contentDictionary.ContainsKey(((ulong)titleId,contentType)))
            {
                return new UInt128(_contentDictionary[((ulong)titleId,contentType)]);
            }

            return new UInt128();
        }

        public StorageId GetInstalledStorage(long titleId, ContentType contentType, StorageId storageId)
        {
            LocationEntry locationEntry = GetLocation(titleId, contentType, storageId);

            return locationEntry.ContentPath != null ?
                LocationHelper.GetStorageId(locationEntry.ContentPath) : StorageId.None;
        }

        public string GetInstalledContentPath(long titleId, StorageId storageId, ContentType contentType)
        {
            LocationEntry locationEntry = GetLocation(titleId, contentType, storageId);

            if (VerifyContentType(locationEntry, contentType))
            {
                return locationEntry.ContentPath;
            }

            return string.Empty;
        }

        public void RedirectLocation(LocationEntry newEntry, StorageId storageId)
        {
            LocationEntry locationEntry = GetLocation(newEntry.TitleId, newEntry.ContentType, storageId);

            if (locationEntry.ContentPath != null)
            {
                RemoveLocationEntry(newEntry.TitleId, newEntry.ContentType, storageId);
            }

            AddLocationEntry(newEntry, storageId);
        }

        private bool VerifyContentType(LocationEntry locationEntry, ContentType contentType)
        {
            if (locationEntry.ContentPath == null)
            {
                return false;
            }

            StorageId storageId     = LocationHelper.GetStorageId(locationEntry.ContentPath);
            string    installedPath = _device.FileSystem.SwitchPathToSystemPath(locationEntry.ContentPath);

            if (!string.IsNullOrWhiteSpace(installedPath))
            {
                if (File.Exists(installedPath))
                {
                    using (FileStream file = new FileStream(installedPath, FileMode.Open, FileAccess.Read))
                    {
                        Nca  nca          = new Nca(_device.System.KeySet, file.AsStorage());
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

        private void RemoveLocationEntry(long titleId, ContentType contentType, StorageId storageId)
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

        public bool TryGetFontTitle(string fontName, out long titleId)
        {
            return _sharedFontTitleDictionary.TryGetValue(fontName, out titleId);
        }

        public bool TryGetFontFilename(string fontName, out string filename)
        {
            return _sharedFontFilenameDictionary.TryGetValue(fontName, out filename);
        }

        private LocationEntry GetLocation(long titleId, ContentType contentType, StorageId storageId)
        {
            LinkedList<LocationEntry> locationList = _locationEntries[storageId];

            return locationList.ToList().Find(x => x.TitleId == titleId && x.ContentType == contentType);
        }
    }
}
