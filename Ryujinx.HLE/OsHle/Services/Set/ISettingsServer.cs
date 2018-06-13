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
                { 3, GetAvailableLanguageCodeCount },
                { 5, GetAvailableLanguageCodes2    }
            };
        }

        public static long GetLanguageCode(ServiceCtx Context)
        {
            Context.ResponseData.Write(Context.Ns.Os.SystemState.DesiredLanguageCode);

            return 0;
        }

        public static long GetAvailableLanguageCodes(ServiceCtx Context)
        {			
	    GetAvailableLanguagesCodesMethod(Context.Request.RecvListBuff[0].Position, Context.Request.RecvListBuff[0].Size, Context);
			
	    return 0;
        }

        public static long GetAvailableLanguageCodeCount(ServiceCtx Context)
        {
            Context.ResponseData.Write(SystemStateMgr.LanguageCodes.Length);

            return 0;
        }
        
        public static long GetAvailableLanguageCodes2(ServiceCtx Context)
        {
	    GetAvailableLanguagesCodesMethod(Context.Request.ReceiveBuff[0].Position, Context.Request.ReceiveBuff[0].Size, Context);
			
	    return 0;
        }
        
	public static long GetAvailableLanguagesCodesMethod(long Position, long Size, ServiceCtx Context)
	{
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
    }
}
