using Ryujinx.HLE.HOS.Services.Account.Acc;
using Ryujinx.HLE.Utilities;
using System;

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

        public AccountUtils Account { get; private set; }

        public SystemStateMgr()
        {
            SetAudioOutputAsBuiltInSpeaker();

            Account = new AccountUtils();

            UInt128 defaultUid = new UInt128("00000000000000000000000000000001");

            Account.AddUser(defaultUid, "Player");
            Account.OpenUser(defaultUid);
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
