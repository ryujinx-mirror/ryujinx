using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.SystemState;
using System.Collections.Generic;

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
                { 0, GetLanguageCode               },
                { 1, GetAvailableLanguageCodes     },
                { 3, GetAvailableLanguageCodeCount },
                { 5, GetAvailableLanguageCodes2    }
            };
        }

        public static long GetLanguageCode(ServiceCtx context)
        {
            context.ResponseData.Write(context.Device.System.State.DesiredLanguageCode);

            return 0;
        }

        public static long GetAvailableLanguageCodes(ServiceCtx context)
        {
            GetAvailableLanguagesCodesImpl(
                context,
                context.Request.RecvListBuff[0].Position,
                context.Request.RecvListBuff[0].Size);

            return 0;
        }

        public static long GetAvailableLanguageCodeCount(ServiceCtx context)
        {
            context.ResponseData.Write(SystemStateMgr.LanguageCodes.Length);

            return 0;
        }

        public static long GetAvailableLanguageCodes2(ServiceCtx context)
        {
            GetAvailableLanguagesCodesImpl(
                context,
                context.Request.ReceiveBuff[0].Position,
                context.Request.ReceiveBuff[0].Size);

            return 0;
        }

        public static long GetAvailableLanguagesCodesImpl(ServiceCtx context, long position, long size)
        {
            int count = (int)(size / 8);

            if (count > SystemStateMgr.LanguageCodes.Length)
            {
                count = SystemStateMgr.LanguageCodes.Length;
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
