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

        private ConcurrentDictionary<string, UserProfile> Profiles;

        internal UserProfile LastOpenUser { get; private set; }

        public SystemStateMgr()
        {
            SetLanguage(SystemLanguage.AmericanEnglish);

            SetAudioOutputAsBuiltInSpeaker();

            Profiles = new ConcurrentDictionary<string, UserProfile>();

            UserId DefaultUuid = new UserId("00000000000000000000000000000001");

            AddUser(DefaultUuid, "Player");
            OpenUser(DefaultUuid);
        }

        public void SetLanguage(SystemLanguage Language)
        {
            DesiredLanguageCode = GetLanguageCode((int)Language);

            DesiredTitleLanguage = Enum.Parse<TitleLanguage>(Enum.GetName(typeof(SystemLanguage), Language));
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

        public void AddUser(UserId Uuid, string Name)
        {
            UserProfile Profile = new UserProfile(Uuid, Name);

            Profiles.AddOrUpdate(Uuid.UserIdHex, Profile, (Key, Old) => Profile);
        }

        public void OpenUser(UserId Uuid)
        {
            if (Profiles.TryGetValue(Uuid.UserIdHex, out UserProfile Profile))
            {
                (LastOpenUser = Profile).AccountState = OpenCloseState.Open;
            }
        }

        public void CloseUser(UserId Uuid)
        {
            if (Profiles.TryGetValue(Uuid.UserIdHex, out UserProfile Profile))
            {
                Profile.AccountState = OpenCloseState.Closed;
            }
        }

        public int GetUserCount()
        {
            return Profiles.Count;
        }

        internal bool TryGetUser(UserId Uuid, out UserProfile Profile)
        {
            return Profiles.TryGetValue(Uuid.UserIdHex, out Profile);
        }

        internal IEnumerable<UserProfile> GetAllUsers()
        {
            return Profiles.Values;
        }

        internal IEnumerable<UserProfile> GetOpenUsers()
        {
            return Profiles.Values.Where(x => x.AccountState == OpenCloseState.Open);
        }

        internal static long GetLanguageCode(int Index)
        {
            if ((uint)Index >= LanguageCodes.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(Index));
            }

            long Code  = 0;
            int  Shift = 0;

            foreach (char Chr in LanguageCodes[Index])
            {
                Code |= (long)(byte)Chr << Shift++ * 8;
            }

            return Code;
        }
    }
}
