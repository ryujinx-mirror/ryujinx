using Ryujinx.HLE.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Ryujinx.HLE.HOS.SystemState
{
    public class SystemStateMgr
    {
        internal static string[] LanguageCodes = new string[]
        {
            "ja",
            "en-US",
            "fr",
            "de",
            "it",
            "es",
            "zh-CN",
            "ko",
            "nl",
            "pt",
            "ru",
            "zh-TW",
            "en-GB",
            "fr-CA",
            "es-419",
            "zh-Hans",
            "zh-Hant"
        };

        internal static string[] AudioOutputs = new string[]
        {
            "AudioTvOutput",
            "AudioStereoJackOutput",
            "AudioBuiltInSpeakerOutput"
        };

        internal long DesiredLanguageCode { get; private set; }

        public TitleLanguage DesiredTitleLanguage { get; private set; }

        internal string ActiveAudioOutput { get; private set; }

        public bool DockedMode { get; set; }

        public ColorSet ThemeColor { get; set; }

        public bool InstallContents { get; set; }

        private ConcurrentDictionary<string, UserProfile> _profiles;

        internal UserProfile LastOpenUser { get; private set; }

        public SystemStateMgr()
        {
            SetAudioOutputAsBuiltInSpeaker();

            _profiles = new ConcurrentDictionary<string, UserProfile>();

            UInt128 defaultUuid = new UInt128("00000000000000000000000000000001");

            AddUser(defaultUuid, "Player");

            OpenUser(defaultUuid);
        }

        public void SetLanguage(SystemLanguage language)
        {
            DesiredLanguageCode = GetLanguageCode((int)language);

            switch (language)
            {
                case SystemLanguage.Taiwanese:
                case SystemLanguage.TraditionalChinese:
                    DesiredTitleLanguage = TitleLanguage.Taiwanese;
                    break;
                case SystemLanguage.Chinese:
                case SystemLanguage.SimplifiedChinese:
                    DesiredTitleLanguage = TitleLanguage.Chinese;
                    break;
                default:
                    DesiredTitleLanguage = Enum.Parse<TitleLanguage>(Enum.GetName(typeof(SystemLanguage), language));
                    break;
            }
        }

        public void SetAudioOutputAsTv()
        {
            ActiveAudioOutput = AudioOutputs[0];
        }

        public void SetAudioOutputAsStereoJack()
        {
            ActiveAudioOutput = AudioOutputs[1];
        }

        public void SetAudioOutputAsBuiltInSpeaker()
        {
            ActiveAudioOutput = AudioOutputs[2];
        }

        public void AddUser(UInt128 uuid, string name)
        {
            UserProfile profile = new UserProfile(uuid, name);

            _profiles.AddOrUpdate(uuid.ToString(), profile, (key, old) => profile);
        }

        public void OpenUser(UInt128 uuid)
        {
            if (_profiles.TryGetValue(uuid.ToString(), out UserProfile profile))
            {
                (LastOpenUser = profile).AccountState = OpenCloseState.Open;
            }
        }

        public void CloseUser(UInt128 uuid)
        {
            if (_profiles.TryGetValue(uuid.ToString(), out UserProfile profile))
            {
                profile.AccountState = OpenCloseState.Closed;
            }
        }

        public int GetUserCount()
        {
            return _profiles.Count;
        }

        internal bool TryGetUser(UInt128 uuid, out UserProfile profile)
        {
            return _profiles.TryGetValue(uuid.ToString(), out profile);
        }

        internal IEnumerable<UserProfile> GetAllUsers()
        {
            return _profiles.Values;
        }

        internal IEnumerable<UserProfile> GetOpenUsers()
        {
            return _profiles.Values.Where(x => x.AccountState == OpenCloseState.Open);
        }

        internal static long GetLanguageCode(int index)
        {
            if ((uint)index >= LanguageCodes.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            long code  = 0;
            int  shift = 0;

            foreach (char chr in LanguageCodes[index])
            {
                code |= (long)(byte)chr << shift++ * 8;
            }

            return code;
        }
    }
}
