using LibHac;
using Ryujinx.Cpu;
using Ryujinx.HLE.HOS.Services.Mii.Types;
using System;

namespace Ryujinx.HLE.HOS.Services.Mii
{
    class DatabaseImpl
    {
        private static DatabaseImpl _instance;

        public static DatabaseImpl Instance
        {
            get
            {
                _instance ??= new DatabaseImpl();

                return _instance;
            }
        }

        private UtilityImpl _utilityImpl;
        private readonly MiiDatabaseManager _miiDatabase;
        private bool _isBroken;

        public DatabaseImpl()
        {
            _miiDatabase = new MiiDatabaseManager();
        }

        public bool IsUpdated(DatabaseSessionMetadata metadata, SourceFlag flag)
        {
            if (flag.HasFlag(SourceFlag.Database))
            {
                return _miiDatabase.IsUpdated(metadata);
            }

            return false;
        }

        public bool IsBrokenDatabaseWithClearFlag()
        {
            bool result = _isBroken;

            if (_isBroken)
            {
                _isBroken = false;

                Format(new DatabaseSessionMetadata(0, new SpecialMiiKeyCode()));
            }

            return result;
        }

        public bool IsFullDatabase()
        {
            return _miiDatabase.IsFullDatabase();
        }

        private ResultCode GetDefault<T>(SourceFlag flag, ref int count, Span<T> elements) where T : struct, IElement
        {
            if (!flag.HasFlag(SourceFlag.Default))
            {
                return ResultCode.Success;
            }

            for (uint i = 0; i < DefaultMii.TableLength; i++)
            {
                if (count >= elements.Length)
                {
                    return ResultCode.BufferTooSmall;
                }

                elements[count] = default;
                elements[count].SetFromStoreData(StoreData.BuildDefault(_utilityImpl, i));
                elements[count].SetSource(Source.Default);

                count++;
            }

            return ResultCode.Success;
        }

        public ResultCode UpdateLatest<T>(DatabaseSessionMetadata metadata, IStoredData<T> oldMiiData, SourceFlag flag, IStoredData<T> newMiiData) where T : unmanaged
        {
            if (!flag.HasFlag(SourceFlag.Database))
            {
                return ResultCode.NotFound;
            }

            if (metadata.IsInterfaceVersionSupported(1) && !oldMiiData.IsValid())
            {
                return oldMiiData.InvalidData;
            }

            ResultCode result = _miiDatabase.FindIndex(metadata, out int index, oldMiiData.CreateId);

            if (result == ResultCode.Success)
            {
                _miiDatabase.Get(metadata, index, out StoreData storeData);

                if (storeData.Type != oldMiiData.Type)
                {
                    return ResultCode.NotFound;
                }

                newMiiData.SetFromStoreData(storeData);

                if (oldMiiData == newMiiData)
                {
                    return ResultCode.NotUpdated;
                }
            }

            return result;
        }

        public ResultCode Get<T>(DatabaseSessionMetadata metadata, SourceFlag flag, out int count, Span<T> elements) where T : struct, IElement
        {
            count = 0;

            if (!flag.HasFlag(SourceFlag.Database))
            {
                return GetDefault(flag, ref count, elements);
            }

            int databaseCount = _miiDatabase.GetCount(metadata);

            for (int i = 0; i < databaseCount; i++)
            {
                if (count >= elements.Length)
                {
                    return ResultCode.BufferTooSmall;
                }

                _miiDatabase.Get(metadata, i, out StoreData storeData);

                elements[count] = default;
                elements[count].SetFromStoreData(storeData);
                elements[count].SetSource(Source.Database);

                count++;
            }

            return GetDefault(flag, ref count, elements);
        }

        public ResultCode InitializeDatabase(ITickSource tickSource, HorizonClient horizonClient)
        {
            _utilityImpl = new UtilityImpl(tickSource);
            _miiDatabase.InitializeDatabase(horizonClient);
            _miiDatabase.LoadFromFile(out _isBroken);

            // Nintendo ignores any error code from before.
            return ResultCode.Success;
        }

        public DatabaseSessionMetadata CreateSessionMetadata(SpecialMiiKeyCode miiKeyCode)
        {
            return _miiDatabase.CreateSessionMetadata(miiKeyCode);
        }

        public void SetInterfaceVersion(DatabaseSessionMetadata metadata, uint interfaceVersion)
        {
            _miiDatabase.SetInterfaceVersion(metadata, interfaceVersion);
        }

        public void Format(DatabaseSessionMetadata metadata)
        {
            _miiDatabase.FormatDatabase(metadata);
            _miiDatabase.SaveDatabase();
        }

        public ResultCode DestroyFile(DatabaseSessionMetadata metadata)
        {
            _isBroken = true;

            return _miiDatabase.DestroyFile(metadata);
        }

        public void BuildDefault(uint index, out CharInfo charInfo)
        {
            StoreData storeData = StoreData.BuildDefault(_utilityImpl, index);

            charInfo = default;

            charInfo.SetFromStoreData(storeData);
        }

        public void BuildRandom(Age age, Gender gender, Race race, out CharInfo charInfo)
        {
            StoreData storeData = StoreData.BuildRandom(_utilityImpl, age, gender, race);

            charInfo = default;

            charInfo.SetFromStoreData(storeData);
        }

        public ResultCode DeleteFile()
        {
            return _miiDatabase.DeleteFile();
        }

        public ResultCode ConvertCoreDataToCharInfo(CoreData coreData, out CharInfo charInfo)
        {
            charInfo = new CharInfo();

            if (!coreData.IsValid())
            {
                return ResultCode.InvalidCoreData;
            }

            StoreData storeData = StoreData.BuildFromCoreData(_utilityImpl, coreData);

            if (!storeData.CoreData.Nickname.IsValidForFontRegion(storeData.CoreData.FontRegion))
            {
                storeData.CoreData.Nickname = Nickname.Question;
                storeData.UpdateCrc();
            }

            charInfo.SetFromStoreData(storeData);

            return ResultCode.Success;
        }

        public int FindIndex(CreateId createId, bool isSpecial)
        {
            if (_miiDatabase.FindIndex(out int index, createId, isSpecial) == ResultCode.Success)
            {
                return index;
            }

            return -1;
        }

        public uint GetCount(DatabaseSessionMetadata metadata, SourceFlag flag)
        {
            int count = 0;

            if (flag.HasFlag(SourceFlag.Default))
            {
                count += DefaultMii.TableLength;
            }

            if (flag.HasFlag(SourceFlag.Database))
            {
                count += _miiDatabase.GetCount(metadata);
            }

            return (uint)count;
        }

        public ResultCode Move(DatabaseSessionMetadata metadata, int index, CreateId createId)
        {
            ResultCode result = _miiDatabase.Move(metadata, index, createId);

            if (result == ResultCode.Success)
            {
                result = _miiDatabase.SaveDatabase();
            }

            return result;
        }

        public ResultCode Delete(DatabaseSessionMetadata metadata, CreateId createId)
        {
            ResultCode result = _miiDatabase.Delete(metadata, createId);

            if (result == ResultCode.Success)
            {
                result = _miiDatabase.SaveDatabase();
            }

            return result;
        }

        public ResultCode AddOrReplace(DatabaseSessionMetadata metadata, StoreData storeData)
        {
            ResultCode result = _miiDatabase.AddOrReplace(metadata, storeData);

            if (result == ResultCode.Success)
            {
                result = _miiDatabase.SaveDatabase();
            }

            return result;
        }

        public ResultCode ConvertCharInfoToCoreData(CharInfo charInfo, out CoreData coreData)
        {
            coreData = new CoreData();

            if (!charInfo.IsValid())
            {
                return ResultCode.InvalidCharInfo;
            }

            coreData.SetFromCharInfo(charInfo);

            if (!coreData.Nickname.IsValidForFontRegion(coreData.FontRegion))
            {
                coreData.Nickname = Nickname.Question;
            }

            return ResultCode.Success;
        }

        public ResultCode GetIndex(DatabaseSessionMetadata metadata, CharInfo charInfo, out int index)
        {
            if (!charInfo.IsValid())
            {
                index = -1;

                return ResultCode.InvalidCharInfo;
            }

            if (_miiDatabase.FindIndex(out index, charInfo.CreateId, metadata.MiiKeyCode.IsEnabledSpecialMii()) != ResultCode.Success)
            {
                return ResultCode.NotFound;
            }

            return ResultCode.Success;
        }
    }
}
