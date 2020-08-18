using Ryujinx.HLE.HOS.Services.Account.Acc;
using System;

namespace Ryujinx.HLE.HOS.SystemState
{
    public class SystemStateMgr
    {
        public static readonly UserId DefaultUserId = new UserId("00000000000000010000000000000000");

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

        internal long DesiredKeyboardLayout { get; private set; }

        internal SystemLanguage DesiredSystemLanguage { get; private set; }

        internal long DesiredLanguageCode { get; private set; }

        internal uint DesiredRegionCode { get; private set; }

        public TitleLanguage DesiredTitleLanguage { get; private set; }

        internal string ActiveAudioOutput { get; private set; }

        public bool DockedMode { get; set; }

        public ColorSet ThemeColor { get; set; }

        public bool InstallContents { get; set; }

        public AccountUtils Account { get; private set; }

        public SystemStateMgr()
        {
            Account = new AccountUtils();

            Account.AddUser(DefaultUserId, "Player");
            Account.OpenUser(DefaultUserId);

            // TODO: Let user specify.
            DesiredKeyboardLayout = (long)KeyboardLayout.Default;
        }

        public void SetLanguage(SystemLanguage language)
        {
            DesiredSystemLanguage = language;
            DesiredLanguageCode = GetLanguageCode((int)DesiredSystemLanguage);

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

        public void SetRegion(RegionCode region)
        {
            DesiredRegionCode = (uint)region;
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
