using Ryujinx.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.OsHle.Objects.Acc
{
    class IProfile : IIpcInterface
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IProfile()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 1, GetBase }
            };
        }

        public long GetBase(ServiceCtx Context)
        {
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