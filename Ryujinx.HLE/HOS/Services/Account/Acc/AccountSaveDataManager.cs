using Ryujinx.Common.Configuration;
using Ryujinx.Common.Utilities;
using Ryujinx.Common.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace Ryujinx.HLE.HOS.Services.Account.Acc
{
    class AccountSaveDataManager
    {
        private readonly string _profilesJsonPath = Path.Join(AppDataManager.BaseDirPath, "system", "Profiles.json");

        private struct ProfilesJson
        {
            [JsonPropertyName("profiles")]
            public List<UserProfileJson> Profiles { get; set; }
            [JsonPropertyName("last_opened")]
            public string LastOpened { get; set; }
        }

        private struct UserProfileJson
        {
            [JsonPropertyName("user_id")]
            public string UserId { get; set; }
            [JsonPropertyName("name")]
            public string Name { get; set; }
            [JsonPropertyName("account_state")]
            public AccountState AccountState { get; set; }
            [JsonPropertyName("online_play_state")]
            public AccountState OnlinePlayState { get; set; }
            [JsonPropertyName("last_modified_timestamp")]
            public long LastModifiedTimestamp { get; set; }
            [JsonPropertyName("image")]
            public byte[] Image { get; set; }
        }

        public UserId LastOpened { get; set; }

        public AccountSaveDataManager(ConcurrentDictionary<string, UserProfile> profiles)
        {
            // TODO: Use 0x8000000000000010 system savedata instead of a JSON file if needed.

            if (File.Exists(_profilesJsonPath))
            {
                try 
                {
                    ProfilesJson profilesJson = JsonHelper.DeserializeFromFile<ProfilesJson>(_profilesJsonPath);

                    foreach (var profile in profilesJson.Profiles)
                    {
                        UserProfile addedProfile = new UserProfile(new UserId(profile.UserId), profile.Name, profile.Image, profile.LastModifiedTimestamp);

                        profiles.AddOrUpdate(profile.UserId, addedProfile, (key, old) => addedProfile);
                    }

                    LastOpened = new UserId(profilesJson.LastOpened);
                }
                catch (Exception e) 
                {
                    Logger.Error?.Print(LogClass.Application, $"Failed to parse {_profilesJsonPath}: {e.Message} Loading default profile!");

                    LastOpened = AccountManager.DefaultUserId;
                }
            }
            else
            {
                LastOpened = AccountManager.DefaultUserId;
            }
        }

        public void Save(ConcurrentDictionary<string, UserProfile> profiles)
        {
            ProfilesJson profilesJson = new ProfilesJson()
            {
                Profiles   = new List<UserProfileJson>(),
                LastOpened = LastOpened.ToString()
            };

            foreach (var profile in profiles)
            {
                profilesJson.Profiles.Add(new UserProfileJson()
                {
                    UserId                = profile.Value.UserId.ToString(),
                    Name                  = profile.Value.Name,
                    AccountState          = profile.Value.AccountState,
                    OnlinePlayState       = profile.Value.OnlinePlayState,
                    LastModifiedTimestamp = profile.Value.LastModifiedTimestamp,
                    Image                 = profile.Value.Image,
                });
            }

            File.WriteAllText(_profilesJsonPath, JsonHelper.Serialize(profilesJson, true));
        }
    }
}