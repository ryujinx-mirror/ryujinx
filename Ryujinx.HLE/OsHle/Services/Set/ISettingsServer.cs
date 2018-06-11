using Ryujinx.HLE.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.OsHle.Services.Set
{
    class ISettingsServer : IpcService
    {
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
            Context.ResponseData.Write(Context.Ns.Os.SystemState.DesiredLanguageCode);

            return 0;
        }

        public static long GetAvailableLanguageCodes(ServiceCtx Context)
        {
            long Position = Context.Request.RecvListBuff[0].Position;
            long Size     = Context.Request.RecvListBuff[0].Size;

            int Count = (int)(Size / 8);

            if (Count > SystemStateMgr.LanguageCodes.Length)
            {
                Count = SystemStateMgr.LanguageCodes.Length;
            }

            for (int Index = 0; Index < Count; Index++)
            {
                Context.Memory.WriteInt64(Position, SystemStateMgr.GetLanguageCode(Index));

                Position += 8;
            }

            Context.ResponseData.Write(Count);

            return 0;
        }

        public static long GetAvailableLanguageCodeCount(ServiceCtx Context)
        {
            Context.ResponseData.Write(SystemStateMgr.LanguageCodes.Length);

            return 0;
        }
    }
}