using LibHac;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Shim;
using LibHac.FsSystem;
using LibHac.Ncm;
using Ryujinx.HLE.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using ApplicationId = LibHac.Ncm.ApplicationId;

namespace Ryujinx.Ui
{
    internal class SaveImporter
    {
        private FileSystemClient FsClient { get; }
        private string ImportPath { get; }

        public SaveImporter(string importPath, FileSystemClient destFsClient)
        {
            ImportPath = importPath;
            FsClient = destFsClient;
        }

        // Returns the number of saves imported
        public int Import()
        {
            return ImportSaves(FsClient, ImportPath);
        }

        private static int ImportSaves(FileSystemClient fsClient, string rootSaveDir)
        {
            if (!Directory.Exists(rootSaveDir))
            {
                return 0;
            }

            SaveFinder finder = new SaveFinder();
            finder.FindSaves(rootSaveDir);

            foreach (SaveToImport save in finder.Saves)
            {
                Result importResult = ImportSave(fsClient, save);

                if (importResult.IsFailure())
                {
                    throw new HorizonResultException(importResult, $"Error importing save {save.Path}");
                }
            }

            return finder.Saves.Count;
        }

        private static Result ImportSave(FileSystemClient fs, SaveToImport save)
        {
            SaveDataAttribute key = save.Attribute;

            Result result = fs.CreateSaveData(new ApplicationId(key.ProgramId.Value), key.UserId, key.ProgramId.Value, 0, 0, 0);
            if (result.IsFailure()) return result;

            bool isOldMounted = false;
            bool isNewMounted = false;

            try
            {
                result = fs.Register("OldSave".ToU8Span(), new LocalFileSystem(save.Path));
                if (result.IsFailure()) return result;

                isOldMounted = true;

                result = fs.MountSaveData("NewSave".ToU8Span(), new ApplicationId(key.ProgramId.Value), key.UserId);
                if (result.IsFailure()) return result;

                isNewMounted = true;

                result = fs.CopyDirectory("OldSave:/", "NewSave:/");
                if (result.IsFailure()) return result;

                result = fs.Commit("NewSave".ToU8Span());
            }
            finally
            {
                if (isOldMounted)
                {
                    fs.Unmount("OldSave".ToU8Span());
                }

                if (isNewMounted)
                {
                    fs.Unmount("NewSave".ToU8Span());
                }
            }

            return result;
        }

        private class SaveFinder
        {
            public List<SaveToImport> Saves { get; } = new List<SaveToImport>();

            public void FindSaves(string rootPath)
            {
                foreach (string subDir in Directory.EnumerateDirectories(rootPath))
                {
                    if (TryGetUInt64(subDir, out ulong saveDataId))
                    {
                        SearchSaveId(subDir, saveDataId);
                    }
                }
            }

            private void SearchSaveId(string path, ulong saveDataId)
            {
                foreach (string subDir in Directory.EnumerateDirectories(path))
                {
                    if (TryGetUserId(subDir, out UserId userId))
                    {
                        SearchUser(subDir, saveDataId, userId);
                    }
                }
            }

            private void SearchUser(string path, ulong saveDataId, UserId userId)
            {
                foreach (string subDir in Directory.EnumerateDirectories(path))
                {
                    if (TryGetUInt64(subDir, out ulong titleId) && TryGetDataPath(subDir, out string dataPath))
                    {
                        SaveDataAttribute attribute = new SaveDataAttribute
                        {
                            Type = SaveDataType.Account,
                            UserId = userId,
                            ProgramId = new ProgramId(titleId)
                        };

                        SaveToImport save = new SaveToImport(dataPath, attribute);

                        Saves.Add(save);
                    }
                }
            }

            private static bool TryGetDataPath(string path, out string dataPath)
            {
                string committedPath = Path.Combine(path, "0");
                string workingPath = Path.Combine(path, "1");

                if (Directory.Exists(committedPath) && Directory.EnumerateFileSystemEntries(committedPath).Any())
                {
                    dataPath = committedPath;
                    return true;
                }

                if (Directory.Exists(workingPath) && Directory.EnumerateFileSystemEntries(workingPath).Any())
                {
                    dataPath = workingPath;
                    return true;
                }

                dataPath = default;
                return false;
            }

            private static bool TryGetUInt64(string path, out ulong converted)
            {
                string name = Path.GetFileName(path);

                if (name.Length == 16)
                {
                    try
                    {
                        converted = Convert.ToUInt64(name, 16);
                        return true;
                    }
                    catch { }
                }

                converted = default;
                return false;
            }

            private static bool TryGetUserId(string path, out UserId userId)
            {
                string name = Path.GetFileName(path);

                if (name.Length == 32)
                {
                    try
                    {
                        UInt128 id = new UInt128(name);

                        userId = Unsafe.As<UInt128, UserId>(ref id);
                        return true;
                    }
                    catch { }
                }

                userId = default;
                return false;
            }
        }

        private class SaveToImport
        {
            public string Path { get; }
            public SaveDataAttribute Attribute { get; }

            public SaveToImport(string path, SaveDataAttribute attribute)
            {
                Path = path;
                Attribute = attribute;
            }
        }
    }
}
