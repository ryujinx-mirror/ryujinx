using LibHac;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Shim;
using Ryujinx.HLE.HOS.Services.Mii.Types;
using System.Runtime.CompilerServices;

namespace Ryujinx.HLE.HOS.Services.Mii
{
    class MiiDatabaseManager
    {
        private static bool IsTestModeEnabled = false;
        private static uint MountCounter      = 0;

        private const ulong  DatabaseTestSaveDataId = 0x8000000000000031;
        private const ulong  DatabaseSaveDataId     = 0x8000000000000030;
        private const ulong  NsTitleId              = 0x010000000000001F;
        private const ulong  SdbTitleId             = 0x0100000000000039;

        private static U8String DatabasePath = new U8String("mii:/MiiDatabase.dat");
        private static U8String MountName    = new U8String("mii");

        private NintendoFigurineDatabase _database;
        private bool                     _isDirty;

        private FileSystemClient _filesystemClient;

        protected ulong UpdateCounter { get; private set; }

        public MiiDatabaseManager()
        {
            _database     = new NintendoFigurineDatabase();
            _isDirty      = false;
            UpdateCounter = 0;
        }

        private void ResetDatabase()
        {
            _database = new NintendoFigurineDatabase();
            _database.Format();
        }

        private void MarkDirty(DatabaseSessionMetadata metadata)
        {
            _isDirty = true;

            UpdateCounter++;

            metadata.UpdateCounter = UpdateCounter;
        }

        private bool GetAtVirtualIndex(int index, out int realIndex, out StoreData storeData)
        {
            realIndex = -1;
            storeData = new StoreData();

            int virtualIndex = 0;

            for (int i = 0; i < _database.Length; i++)
            {
                StoreData tmp = _database.Get(i);

                if (!tmp.IsSpecial())
                {
                    if (index == virtualIndex)
                    {
                        realIndex = i;
                        storeData = tmp;

                        return true;
                    }

                    virtualIndex++;
                }
            }

            return false;
        }

        private int ConvertRealIndexToVirtualIndex(int realIndex)
        {
            int virtualIndex = 0;

            for (int i = 0; i < realIndex; i++)
            {
                StoreData tmp = _database.Get(i);

                if (!tmp.IsSpecial())
                {
                    virtualIndex++;
                }
            }

            return virtualIndex;
        }

        public void InitializeDatabase(Switch device)
        {
            _filesystemClient = device.FileSystem.FsClient;

            // Ensure we have valid data in the database
            _database.Format();

            MountSave();
        }

        private Result MountSave()
        {
            Result result = Result.Success;

            if (MountCounter == 0)
            {
                ulong targetSaveDataId;
                ulong targetTitleId;

                if (IsTestModeEnabled)
                {
                    targetSaveDataId = DatabaseTestSaveDataId;
                    targetTitleId    = SdbTitleId;
                }
                else
                {
                    targetSaveDataId = DatabaseSaveDataId;

                    // Nintendo use NS TitleID when creating the production save even on sdb, let's follow that behaviour.
                    targetTitleId = NsTitleId;
                }

                U8Span mountName = new U8Span(MountName);

                result = _filesystemClient.MountSystemSaveData(mountName, SaveDataSpaceId.System, targetSaveDataId);

                if (result.IsFailure())
                {
                    if (ResultFs.TargetNotFound.Includes(result))
                    {
                        // TODO: We're currently always specifying the owner ID because FS doesn't have a way of
                        // knowing which process called it
                        result = _filesystemClient.CreateSystemSaveData(targetSaveDataId, targetTitleId, 0x10000,
                            0x10000, SaveDataFlags.KeepAfterResettingSystemSaveDataWithoutUserSaveData);
                        if (result.IsFailure()) return result;

                        result = _filesystemClient.MountSystemSaveData(mountName, SaveDataSpaceId.System, targetSaveDataId);
                        if (result.IsFailure()) return result;
                    }
                }

                if (result == Result.Success)
                {
                    MountCounter++;
                }
            }



            return result;
        }

        public ResultCode DeleteFile()
        {
            ResultCode result = (ResultCode)_filesystemClient.DeleteFile(DatabasePath).Value;

            _filesystemClient.Commit(MountName);

            return result;
        }

        public ResultCode LoadFromFile(out bool isBroken)
        {
            isBroken = false;

            if (MountCounter == 0)
            {
                return ResultCode.InvalidArgument;
            }

            UpdateCounter++;

            ResetDatabase();

            Result result = _filesystemClient.OpenFile(out FileHandle handle, DatabasePath, OpenMode.Read);

            if (result.IsSuccess())
            {
                result = _filesystemClient.GetFileSize(out long fileSize, handle);
                if (result.IsSuccess())
                {
                    if (fileSize == Unsafe.SizeOf<NintendoFigurineDatabase>())
                    {
                        result = _filesystemClient.ReadFile(handle, 0, _database.AsSpan());

                        if (result.IsSuccess())
                        {
                            if (_database.Verify() != ResultCode.Success)
                            {
                                ResetDatabase();

                                isBroken = true;
                            }
                            else
                            {
                                isBroken = _database.FixDatabase();
                            }
                        }
                    }
                    else
                    {
                        isBroken = true;
                    }
                }

                _filesystemClient.CloseFile(handle);

                return (ResultCode)result.Value;
            }
            else if (ResultFs.PathNotFound.Includes(result))
            {
                return (ResultCode)ForceSaveDatabase().Value;
            }

            return ResultCode.Success;
        }

        private Result ForceSaveDatabase()
        {
            Result result = _filesystemClient.CreateFile(DatabasePath, Unsafe.SizeOf<NintendoFigurineDatabase>());

            if (result.IsSuccess() || ResultFs.PathAlreadyExists.Includes(result))
            {
                result = _filesystemClient.OpenFile(out FileHandle handle, DatabasePath, OpenMode.Write);

                if (result.IsSuccess())
                {
                    result = _filesystemClient.GetFileSize(out long fileSize, handle);

                    if (result.IsSuccess())
                    {
                        // If the size doesn't match, recreate the file
                        if (fileSize != Unsafe.SizeOf<NintendoFigurineDatabase>())
                        {
                            _filesystemClient.CloseFile(handle);

                            result = _filesystemClient.DeleteFile(DatabasePath);

                            if (result.IsSuccess())
                            {
                                result = _filesystemClient.CreateFile(DatabasePath, Unsafe.SizeOf<NintendoFigurineDatabase>());

                                if (result.IsSuccess())
                                {
                                    result = _filesystemClient.OpenFile(out handle, DatabasePath, OpenMode.Write);
                                }
                            }

                            if (result.IsFailure())
                            {
                                return result;
                            }
                        }

                        result = _filesystemClient.WriteFile(handle, 0, _database.AsReadOnlySpan(), WriteOption.Flush);
                    }

                    _filesystemClient.CloseFile(handle);
                }
            }

            if (result.IsSuccess())
            {
                _isDirty = false;

                result = _filesystemClient.Commit(MountName);
            }

            return result;
        }

        public DatabaseSessionMetadata CreateSessionMetadata(SpecialMiiKeyCode miiKeyCode)
        {
            return new DatabaseSessionMetadata(UpdateCounter, miiKeyCode);
        }

        public void SetInterfaceVersion(DatabaseSessionMetadata metadata, uint interfaceVersion)
        {
            metadata.InterfaceVersion = interfaceVersion;
        }

        public bool IsUpdated(DatabaseSessionMetadata metadata)
        {
            bool result = metadata.UpdateCounter != UpdateCounter;

            metadata.UpdateCounter = UpdateCounter;

            return result;
        }

        public int GetCount(DatabaseSessionMetadata metadata)
        {
            if (!metadata.MiiKeyCode.IsEnabledSpecialMii())
            {
                int count = 0;

                for (int i = 0; i < _database.Length; i++)
                {
                    StoreData tmp = _database.Get(i);

                    if (!tmp.IsSpecial())
                    {
                        count++;
                    }
                }

                return count;
            }
            else
            {
                return _database.Length;
            }
        }

        public void Get(DatabaseSessionMetadata metadata, int index, out StoreData storeData)
        {
            if (!metadata.MiiKeyCode.IsEnabledSpecialMii())
            {
                if (GetAtVirtualIndex(index, out int realIndex, out _))
                {
                    index = realIndex;
                }
                else
                {
                    index = 0;
                }
            }

            storeData = _database.Get(index);
        }

        public ResultCode FindIndex(DatabaseSessionMetadata metadata, out int index, CreateId createId)
        {
            return FindIndex(out index, createId, metadata.MiiKeyCode.IsEnabledSpecialMii());
        }

        public ResultCode FindIndex(out int index, CreateId createId, bool isSpecial)
        {
            if (_database.GetIndexByCreatorId(out int realIndex, createId))
            {
                if (isSpecial)
                {
                    index = realIndex;

                    return ResultCode.Success;
                }

                StoreData storeData = _database.Get(realIndex);

                if (!storeData.IsSpecial())
                {
                    if (realIndex < 1)
                    {
                        index = 0;
                    }
                    else
                    {
                        index = ConvertRealIndexToVirtualIndex(realIndex);
                    }

                    return ResultCode.Success;
                }
            }

            index = -1;

            return ResultCode.NotFound;
        }

        public ResultCode Move(DatabaseSessionMetadata metadata, int newIndex, CreateId createId)
        {
            if (!metadata.MiiKeyCode.IsEnabledSpecialMii())
            {
                if (GetAtVirtualIndex(newIndex, out int realIndex, out _))
                {
                    newIndex = realIndex;
                }
                else
                {
                    newIndex = 0;
                }
            }

            if (_database.GetIndexByCreatorId(out int oldIndex, createId))
            {
                StoreData realStoreData = _database.Get(oldIndex);

                if (!metadata.MiiKeyCode.IsEnabledSpecialMii() && realStoreData.IsSpecial())
                {
                    return ResultCode.InvalidOperationOnSpecialMii;
                }

                ResultCode result = _database.Move(newIndex, oldIndex);

                if (result == ResultCode.Success)
                {
                    MarkDirty(metadata);
                }

                return result;
            }

            return ResultCode.NotFound;
        }

        public ResultCode AddOrReplace(DatabaseSessionMetadata metadata, StoreData storeData)
        {
            if (!storeData.IsValid())
            {
                return ResultCode.InvalidStoreData;
            }

            if (!metadata.MiiKeyCode.IsEnabledSpecialMii() && !storeData.IsSpecial())
            {
                if (_database.GetIndexByCreatorId(out int index, storeData.CreateId))
                {
                    StoreData oldStoreData = _database.Get(index);

                    if (oldStoreData.IsSpecial())
                    {
                        return ResultCode.InvalidOperationOnSpecialMii;
                    }

                    _database.Replace(index, storeData);
                }
                else
                {
                    if (_database.IsFull())
                    {
                        return ResultCode.DatabaseFull;
                    }

                    _database.Add(storeData);
                }

                MarkDirty(metadata);

                return ResultCode.Success;
            }

            return ResultCode.InvalidOperationOnSpecialMii;
        }

        public ResultCode Delete(DatabaseSessionMetadata metadata, CreateId createId)
        {
            if (!_database.GetIndexByCreatorId(out int index, createId))
            {
                return ResultCode.NotFound;
            }

            if (!metadata.MiiKeyCode.IsEnabledSpecialMii())
            {
                StoreData storeData = _database.Get(index);

                if (storeData.IsSpecial())
                {
                    return ResultCode.InvalidOperationOnSpecialMii;
                }
            }

            _database.Delete(index);

            MarkDirty(metadata);

            return ResultCode.Success;
        }

        public ResultCode DestroyFile(DatabaseSessionMetadata metadata)
        {
            _database.CorruptDatabase();

            MarkDirty(metadata);

            ResultCode result = SaveDatabase();

            ResetDatabase();

            return result;
        }

        public ResultCode SaveDatabase()
        {
            if (_isDirty)
            {
                return (ResultCode)ForceSaveDatabase().Value;
            }
            else
            {
                return ResultCode.NotUpdated;
            }
        }

        public void FormatDatabase(DatabaseSessionMetadata metadata)
        {
            _database.Format();

            MarkDirty(metadata);
        }

        public bool IsFullDatabase()
        {
            return _database.IsFull();
        }
    }
}
