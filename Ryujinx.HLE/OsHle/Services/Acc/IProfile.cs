using Ryujinx.HLE.Logging;
using Ryujinx.HLE.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.OsHle.Services.Acc
{
    class IProfile : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IProfile()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 1, GetBase }
            };
        }

        public long GetBase(ServiceCtx Context)
        {
            Context.Ns.Log.PrintStub(LogClass.ServiceAcc, "Stubbed.");

            Context.ResponseData.Write(0L);
            Context.ResponseData.Write(0L);
            Context.ResponseData.Write(0L);
            Context.ResponseData.Write(0L);
            Context.ResponseData.Write(0L);
            Context.ResponseData.Write(0L);
            Context.ResponseData.Write(0L);

            return 0;
        }
    }
}