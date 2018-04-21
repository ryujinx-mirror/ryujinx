using Ryujinx.Core.OsHle.Ipc;
using System;
using System.Collections.Generic;
using System.IO;

namespace Ryujinx.Core.OsHle.Services.Set
{
    class ISettingsServer : IpcService
    {
        private static string[] LanguageCodes = new string[]
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

        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ISettingsServer()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, GetLanguageCode               },
                { 1, GetAvailableLanguageCodes     },
                { 3, GetAvailableLanguageCodeCount }
            };
        }

        public static long GetLanguageCode(ServiceCtx Context)
        {
            Context.ResponseData.Write(LanguageCodetoLongBE(LanguageCodes[1]));

            return 0;
        }

        private static long LanguageCodetoLongBE(string LanguageCode)
        {
            using (MemoryStream MS = new MemoryStream())
            {
                foreach (char Chr in LanguageCode)
                {
                    MS.WriteByte((byte)Chr);
                }

                for (int Offs = 0; Offs < (8 - LanguageCode.Length); Offs++)
                {
                    MS.WriteByte(0);
                }

                return BitConverter.ToInt64(MS.ToArray(), 0);
            }
        }

        public static long GetAvailableLanguageCodes(ServiceCtx Context)
        {
            long  Position = Context.Request.RecvListBuff[0].Position;
            short Size     = Context.Request.RecvListBuff[0].Size;

            int Count = (int)((uint)Size / 8);

            if (Count > LanguageCodes.Length)
            {
                Count = LanguageCodes.Length;
            }

            for (int Index = 0; Index < Count; Index++)
            {
                string LanguageCode = LanguageCodes[Index];

                foreach (char Chr in LanguageCode)
                {
                    Context.Memory.WriteByte(Position++, (byte)Chr);
                }

                for (int Offs = 0; Offs < (8 - LanguageCode.Length); Offs++)
                {
                    Context.Memory.WriteByte(Position++, 0);
                }
            }

            Context.ResponseData.Write(Count);

            return 0;
        }

        public static long GetAvailableLanguageCodeCount(ServiceCtx Context)
        {
            Context.ResponseData.Write(LanguageCodes.Length);

            return 0;
        }
    }
}