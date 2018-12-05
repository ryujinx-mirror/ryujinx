using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Bcat
{
    class IServiceCreator : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IServiceCreator()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, CreateBcatService                 },
                { 1, CreateDeliveryCacheStorageService }
            };
        }

        public long CreateBcatService(ServiceCtx Context)
        {
            long Id = Context.RequestData.ReadInt64();

            MakeObject(Context, new IBcatService());

            return 0;
        }

        public long CreateDeliveryCacheStorageService(ServiceCtx Context)
        {
            long Id = Context.RequestData.ReadInt64();

            MakeObject(Context, new IDeliveryCacheStorageService());

            return 0;
        }
    }
}
