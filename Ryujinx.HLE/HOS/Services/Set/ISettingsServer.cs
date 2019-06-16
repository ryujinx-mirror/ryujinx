using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.SystemState;
using System;
using System.Collections.Generic;

using static Ryujinx.HLE.HOS.ErrorCode;

namespace Ryujinx.HLE.HOS.Services.Set
{
    class ISettingsServer : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public ISettingsServer()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, GetLanguageCode                },
                { 1, GetAvailableLanguageCodes      },
                { 2, MakeLanguageCode               }, // 4.0.0+
                { 3, GetAvailableLanguageCodeCount  },
              //{ 4, GetRegionCode                  },
                { 5, GetAvailableLanguageCodes2     },
                { 6, GetAvailableLanguageCodeCount2 },
              //{ 7, GetKeyCodeMap                  }, // 4.0.0+
                { 8, GetQuestFlag                   }, // 5.0.0+
              //{ 9, GetKeyCodeMap2                 }, // 6.0.0+
            };
        }

        // GetLanguageCode() -> nn::settings::LanguageCode
        public static long GetLanguageCode(ServiceCtx context)
        {
            context.ResponseData.Write(context.Device.System.State.DesiredLanguageCode);

            return 0;
        }

        // GetAvailableLanguageCodes() -> (u32, buffer<nn::settings::LanguageCode, 0xa>)
        public static long GetAvailableLanguageCodes(ServiceCtx context)
        {
            return GetAvailableLanguagesCodesImpl(
                    context,
                    context.Request.RecvListBuff[0].Position,
                    context.Request.RecvListBuff[0].Size,
                    0xF);
        }

        // MakeLanguageCode(nn::settings::Language language_index) -> nn::settings::LanguageCode
        public static long MakeLanguageCode(ServiceCtx context)
        {
            int languageIndex = context.RequestData.ReadInt32();

            if ((uint)languageIndex >= (uint)SystemStateMgr.LanguageCodes.Length)
            {
                return MakeError(ErrorModule.Settings, SettingsError.LanguageOutOfRange);
            }

            context.ResponseData.Write(SystemStateMgr.GetLanguageCode(languageIndex));

            return 0;
        }

        // GetAvailableLanguageCodeCount() -> u32
        public static long GetAvailableLanguageCodeCount(ServiceCtx context)
        {
            context.ResponseData.Write(Math.Min(SystemStateMgr.LanguageCodes.Length, 0xF));

            return 0;
        }

        // GetAvailableLanguageCodes2() -> (u32, buffer<nn::settings::LanguageCode, 6>)
        public static long GetAvailableLanguageCodes2(ServiceCtx context)
        {
            return GetAvailableLanguagesCodesImpl(
                    context,
                    context.Request.ReceiveBuff[0].Position,
                    context.Request.ReceiveBuff[0].Size,
                    SystemStateMgr.LanguageCodes.Length);
        }

        // GetAvailableLanguageCodeCount2() -> u32
        public static long GetAvailableLanguageCodeCount2(ServiceCtx context)
        {
            context.ResponseData.Write(SystemStateMgr.LanguageCodes.Length);

            return 0;
        }

        // GetQuestFlag() -> bool
        public static long GetQuestFlag(ServiceCtx context)
        {
            context.ResponseData.Write(false);

            Logger.PrintStub(LogClass.ServiceSet);

            return 0;
        }

        public static long GetAvailableLanguagesCodesImpl(ServiceCtx context, long position, long size, int maxSize)
        {
            int count = (int)(size / 8);

            if (count > maxSize)
            {
                count = maxSize;
            }

            for (int index = 0; index < count; index++)
            {
                context.Memory.WriteInt64(position, SystemStateMgr.GetLanguageCode(index));

                position += 8;
            }

            context.ResponseData.Write(count);

            return 0;
        }
    }
}
