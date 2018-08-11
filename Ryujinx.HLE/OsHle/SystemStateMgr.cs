using Ryujinx.HLE.Loaders.Npdm;
using System;

namespace Ryujinx.HLE.OsHle
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

        internal string ActiveAudioOutput { get; private set; }
        
        public bool DockedMode { get; set; }

        public SystemStateMgr()
        {
            SetLanguage(SystemLanguage.AmericanEnglish);

            SetAudioOutputAsBuiltInSpeaker();
        }

        public void SetLanguage(SystemLanguage Language)
        {
            DesiredLanguageCode = GetLanguageCode((int)Language);
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
