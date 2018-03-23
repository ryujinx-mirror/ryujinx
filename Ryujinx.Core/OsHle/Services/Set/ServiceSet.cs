using ChocolArm64.Memory;
using Ryujinx.Core.OsHle.Ipc;
using System;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Services.Set
{
    class ServiceSet : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ServiceSet()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 1, GetAvailableLanguageCodes     },
                { 3, GetAvailableLanguageCodeCount }
            };
        }

        private const int LangCodesCount = 13;

        public static long GetAvailableLanguageCodes(ServiceCtx Context)
        {
            int PtrBuffSize = Context.RequestData.ReadInt32();

            if (Context.Request.RecvListBuff.Count > 0)
            {
                long  Position = Context.Request.RecvListBuff[0].Position;
                short Size     = Context.Request.RecvListBuff[0].Size;

                //This should return an array of ints with values matching the LanguageCode enum.
                foreach (long value in new long[] { 0L, 1L, 2L, 3L, 4L, 5L, 6L, 7L })
                {
                    AMemoryHelper.WriteBytes(Context.Memory, Position += 8, BitConverter.GetBytes(value));
                }
            }

            Context.ResponseData.Write(LangCodesCount);

            return 0;
        }

        public static long GetAvailableLanguageCodeCount(ServiceCtx Context)
        {
            Context.ResponseData.Write(LangCodesCount);

            return 0;
        }
    }
}