using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.SystemState;
using System;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.Settings
{
    [Service("set")]
    class ISettingsServer : IpcService
    {
        public ISettingsServer(ServiceCtx context) { }

        [CommandCmif(0)]
        // GetLanguageCode() -> nn::settings::LanguageCode
        public ResultCode GetLanguageCode(ServiceCtx context)
        {
            context.ResponseData.Write(context.Device.System.State.DesiredLanguageCode);

            return ResultCode.Success;
        }

        [CommandCmif(1)]
        // GetAvailableLanguageCodes() -> (u32, buffer<nn::settings::LanguageCode, 0xa>)
        public ResultCode GetAvailableLanguageCodes(ServiceCtx context)
        {
            return GetAvailableLanguagesCodesImpl(
                    context,
                    context.Request.RecvListBuff[0].Position,
                    context.Request.RecvListBuff[0].Size,
                    0xF);
        }

        [CommandCmif(2)] // 4.0.0+
        // MakeLanguageCode(nn::settings::Language language_index) -> nn::settings::LanguageCode
        public ResultCode MakeLanguageCode(ServiceCtx context)
        {
            int languageIndex = context.RequestData.ReadInt32();

            if ((uint)languageIndex >= (uint)SystemStateMgr.LanguageCodes.Length)
            {
                return ResultCode.LanguageOutOfRange;
            }

            context.ResponseData.Write(SystemStateMgr.GetLanguageCode(languageIndex));

            return ResultCode.Success;
        }

        [CommandCmif(3)]
        // GetAvailableLanguageCodeCount() -> u32
        public ResultCode GetAvailableLanguageCodeCount(ServiceCtx context)
        {
            context.ResponseData.Write(Math.Min(SystemStateMgr.LanguageCodes.Length, 0xF));

            return ResultCode.Success;
        }

        [CommandCmif(4)]
        // GetRegionCode() -> u32 nn::settings::RegionCode
        public ResultCode GetRegionCode(ServiceCtx context)
        {
            // NOTE: Service mount 0x8000000000000050 savedata and read the region code here.

            RegionCode regionCode = (RegionCode)context.Device.System.State.DesiredRegionCode;

            if (regionCode < RegionCode.Min || regionCode > RegionCode.Max)
            {
                regionCode = RegionCode.USA;
            }

            context.ResponseData.Write((uint)regionCode);

            return ResultCode.Success;
        }

        [CommandCmif(5)]
        // GetAvailableLanguageCodes2() -> (u32, buffer<nn::settings::LanguageCode, 6>)
        public ResultCode GetAvailableLanguageCodes2(ServiceCtx context)
        {
            return GetAvailableLanguagesCodesImpl(
                    context,
                    context.Request.ReceiveBuff[0].Position,
                    context.Request.ReceiveBuff[0].Size,
                    SystemStateMgr.LanguageCodes.Length);
        }

        [CommandCmif(6)]
        // GetAvailableLanguageCodeCount2() -> u32
        public ResultCode GetAvailableLanguageCodeCount2(ServiceCtx context)
        {
            context.ResponseData.Write(SystemStateMgr.LanguageCodes.Length);

            return ResultCode.Success;
        }

        [CommandCmif(7)] // 4.0.0+
        // GetKeyCodeMap() -> buffer<nn::kpr::KeyCodeMap, 0x16>
        public ResultCode GetKeyCodeMap(ServiceCtx context)
        {
            return GetKeyCodeMapImpl(context, 1);
        }

        [CommandCmif(8)] // 5.0.0+
        // GetQuestFlag() -> bool
        public ResultCode GetQuestFlag(ServiceCtx context)
        {
            context.ResponseData.Write(false);

            Logger.Stub?.PrintStub(LogClass.ServiceSet);

            return ResultCode.Success;
        }

        [CommandCmif(9)] // 6.0.0+
        // GetKeyCodeMap2() -> buffer<nn::kpr::KeyCodeMap, 0x16>
        public ResultCode GetKeyCodeMap2(ServiceCtx context)
        {
            return GetKeyCodeMapImpl(context, 2);
        }

        [CommandCmif(11)] // 10.1.0+
        // GetDeviceNickName() -> buffer<nn::settings::system::DeviceNickName, 0x16>
        public ResultCode GetDeviceNickName(ServiceCtx context)
        {
            ulong deviceNickNameBufferPosition = context.Request.ReceiveBuff[0].Position;
            ulong deviceNickNameBufferSize = context.Request.ReceiveBuff[0].Size;

            if (deviceNickNameBufferPosition == 0)
            {
                return ResultCode.NullDeviceNicknameBuffer;
            }

            if (deviceNickNameBufferSize != 0x80)
            {
                Logger.Warning?.Print(LogClass.ServiceSet, "Wrong buffer size");
            }

            context.Memory.Write(deviceNickNameBufferPosition, Encoding.ASCII.GetBytes(context.Device.System.State.DeviceNickName + '\0'));

            return ResultCode.Success;
        }

        private ResultCode GetKeyCodeMapImpl(ServiceCtx context, int version)
        {
            if (context.Request.ReceiveBuff[0].Size != 0x1000)
            {
                Logger.Warning?.Print(LogClass.ServiceSet, "Wrong buffer size");
            }

            byte[] keyCodeMap;

            switch ((KeyboardLayout)context.Device.System.State.DesiredKeyboardLayout)
            {
                case KeyboardLayout.EnglishUs:

                    long langCode = context.Device.System.State.DesiredLanguageCode;

                    if (langCode == 0x736e61482d687a) // Zh-Hans
                    {
                        keyCodeMap = KeyCodeMaps.ChineseSimplified;
                    }
                    else if (langCode == 0x746e61482d687a) // Zh-Hant
                    {
                        keyCodeMap = KeyCodeMaps.ChineseTraditional;
                    }
                    else
                    {
                        keyCodeMap = KeyCodeMaps.EnglishUk;
                    }

                    break;
                case KeyboardLayout.EnglishUsInternational:
                    keyCodeMap = KeyCodeMaps.EnglishUsInternational;
                    break;
                case KeyboardLayout.EnglishUk:
                    keyCodeMap = KeyCodeMaps.EnglishUk;
                    break;
                case KeyboardLayout.French:
                    keyCodeMap = KeyCodeMaps.French;
                    break;
                case KeyboardLayout.FrenchCa:
                    keyCodeMap = KeyCodeMaps.FrenchCa;
                    break;
                case KeyboardLayout.Spanish:
                    keyCodeMap = KeyCodeMaps.Spanish;
                    break;
                case KeyboardLayout.SpanishLatin:
                    keyCodeMap = KeyCodeMaps.SpanishLatin;
                    break;
                case KeyboardLayout.German:
                    keyCodeMap = KeyCodeMaps.German;
                    break;
                case KeyboardLayout.Italian:
                    keyCodeMap = KeyCodeMaps.Italian;
                    break;
                case KeyboardLayout.Portuguese:
                    keyCodeMap = KeyCodeMaps.Portuguese;
                    break;
                case KeyboardLayout.Russian:
                    keyCodeMap = KeyCodeMaps.Russian;
                    break;
                case KeyboardLayout.Korean:
                    keyCodeMap = KeyCodeMaps.Korean;
                    break;
                case KeyboardLayout.ChineseSimplified:
                    keyCodeMap = KeyCodeMaps.ChineseSimplified;
                    break;
                case KeyboardLayout.ChineseTraditional:
                    keyCodeMap = KeyCodeMaps.ChineseTraditional;
                    break;
                default: // KeyboardLayout.Default
                    keyCodeMap = KeyCodeMaps.Default;
                    break;
            }

            context.Memory.Write(context.Request.ReceiveBuff[0].Position, keyCodeMap);

            if (version == 1 && context.Device.System.State.DesiredKeyboardLayout == (long)KeyboardLayout.Default)
            {
                context.Memory.Write(context.Request.ReceiveBuff[0].Position, (byte)0x01);
            }

            return ResultCode.Success;
        }

        private ResultCode GetAvailableLanguagesCodesImpl(ServiceCtx context, ulong position, ulong size, int maxSize)
        {
            int count = (int)(size / 8);

            if (count > maxSize)
            {
                count = maxSize;
            }

            for (int index = 0; index < count; index++)
            {
                context.Memory.Write(position, SystemStateMgr.GetLanguageCode(index));

                position += 8;
            }

            context.ResponseData.Write(count);

            return ResultCode.Success;
        }
    }
}
