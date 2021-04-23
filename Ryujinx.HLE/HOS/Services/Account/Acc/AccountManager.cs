using LibHac;
using LibHac.Fs;
using LibHac.Fs.Shim;
using Ryujinx.Common;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.FileSystem.Content;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ryujinx.HLE.HOS.Services.Account.Acc
{
    public class AccountManager
    {
        public static readonly UserId DefaultUserId = new UserId("00000000000000010000000000000000");

        private readonly VirtualFileSystem      _virtualFileSystem;
        private readonly AccountSaveDataManager _accountSaveDataManager;

        private ConcurrentDictionary<string, UserProfile> _profiles;

        public UserProfile LastOpenedUser { get; private set; }

        public AccountManager(VirtualFileSystem virtualFileSystem)
        {
            _virtualFileSystem = virtualFileSystem;

            _profiles = new ConcurrentDictionary<string, UserProfile>();

            _accountSaveDataManager = new AccountSaveDataManager(_profiles);

            if (!_profiles.TryGetValue(DefaultUserId.ToString(), out _))
            {
                byte[] defaultUserImage = EmbeddedResources.Read("Ryujinx.HLE/HOS/Services/Account/Acc/DefaultUserImage.jpg");

                AddUser("RyuPlayer", defaultUserImage, DefaultUserId);

                OpenUser(DefaultUserId);
            }
            else
            {
                OpenUser(_accountSaveDataManager.LastOpened);
            }
        }

        public void AddUser(string name, byte[] image, UserId userId = new UserId())
        {
            if (userId.IsNull)
            {
                userId = new UserId(Guid.NewGuid().ToString().Replace("-", ""));
            }

            UserProfile profile = new UserProfile(userId, name, image);

            _profiles.AddOrUpdate(userId.ToString(), profile, (key, old) => profile);

            _accountSaveDataManager.Save(_profiles);
        }

        public void OpenUser(UserId userId)
        {
            if (_profiles.TryGetValue(userId.ToString(), out UserProfile profile))
            {
                // TODO: Support multiple open users ?
                foreach (UserProfile userProfile in GetAllUsers())
                {
                    if (userProfile == LastOpenedUser)
                    {
                        userProfile.AccountState = AccountState.Closed;

                        break;
                    }
                }

                (LastOpenedUser = profile).AccountState = AccountState.Open;

                _accountSaveDataManager.LastOpened = userId;
            }

            _accountSaveDataManager.Save(_profiles);
        }

        public void CloseUser(UserId userId)
        {
            if (_profiles.TryGetValue(userId.ToString(), out UserProfile profile))
            {
                profile.AccountState = AccountState.Closed;
            }

            _accountSaveDataManager.Save(_profiles);
        }

        public void OpenUserOnlinePlay(UserId userId)
        {
            if (_profiles.TryGetValue(userId.ToString(), out UserProfile profile))
            {
                // TODO: Support multiple open online users ?
                foreach (UserProfile userProfile in GetAllUsers())
                {
                    if (userProfile == LastOpenedUser)
                    {
                        userProfile.OnlinePlayState = AccountState.Closed;

                        break;
                    }
                }

                profile.OnlinePlayState = AccountState.Open;
            }

            _accountSaveDataManager.Save(_profiles);
        }

        public void CloseUserOnlinePlay(UserId userId)
        {
            if (_profiles.TryGetValue(userId.ToString(), out UserProfile profile))
            {
                profile.OnlinePlayState = AccountState.Closed;
            }

            _accountSaveDataManager.Save(_profiles);
        }

        public void SetUserImage(UserId userId, byte[] image)
        {
            foreach (UserProfile userProfile in GetAllUsers())
            {
                if (userProfile.UserId == userId)
                {
                    userProfile.Image = image;

                    break;
                }
            }

            _accountSaveDataManager.Save(_profiles);
        }

        public void SetUserName(UserId userId, string name)
        {
            foreach (UserProfile userProfile in GetAllUsers())
            {
                if (userProfile.UserId == userId)
                {
                    userProfile.Name = name;

                    break;
                }
            }

            _accountSaveDataManager.Save(_profiles);
        }

        public void DeleteUser(UserId userId)
        {
            DeleteSaveData(userId);

            _profiles.Remove(userId.ToString(), out _);

            OpenUser(DefaultUserId);

            _accountSaveDataManager.Save(_profiles);
        }

        private void DeleteSaveData(UserId userId)
        {
            SaveDataFilter saveDataFilter = new SaveDataFilter();
            saveDataFilter.SetUserId(new LibHac.Fs.UserId((ulong)userId.High, (ulong)userId.Low));

            Result result = _virtualFileSystem.FsClient.OpenSaveDataIterator(out SaveDataIterator saveDataIterator, SaveDataSpaceId.User, ref saveDataFilter);
            if (result.IsSuccess())
            {
                Span<SaveDataInfo> saveDataInfo = stackalloc SaveDataInfo[10];

                while (true)
                {
                    saveDataIterator.ReadSaveDataInfo(out long readCount, saveDataInfo);

                    if (readCount == 0)
                    {
                        break;
                    }

                    for (int i = 0; i < readCount; i++)
                    {
                        // TODO: We use Directory.Delete workaround because DeleteSaveData softlock without, due to a bug in LibHac 0.12.0.
                        string savePath     = Path.Combine(_virtualFileSystem.GetNandPath(), $"user/save/{saveDataInfo[i].SaveDataId:x16}");
                        string saveMetaPath = Path.Combine(_virtualFileSystem.GetNandPath(), $"user/saveMeta/{saveDataInfo[i].SaveDataId:x16}");

                        Directory.Delete(savePath, true);
                        Directory.Delete(saveMetaPath, true);

                        _virtualFileSystem.FsClient.DeleteSaveData(SaveDataSpaceId.User, saveDataInfo[i].SaveDataId);
                    }
                }
            }
        }

        internal int GetUserCount()
        {
            return _profiles.Count;
        }

        internal bool TryGetUser(UserId userId, out UserProfile profile)
        {
            return _profiles.TryGetValue(userId.ToString(), out profile);
        }

        public IEnumerable<UserProfile> GetAllUsers()
        {
            return _profiles.Values;
        }

        internal IEnumerable<UserProfile> GetOpenedUsers()
        {
            return _profiles.Values.Where(x => x.AccountState == AccountState.Open);
        }

        internal UserProfile GetFirst()
        {
            return _profiles.First().Value;
        }
    }
}