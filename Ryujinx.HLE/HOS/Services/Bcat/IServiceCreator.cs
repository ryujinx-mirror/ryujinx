using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Bcat
{
    class IServiceCreator : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public IServiceCreator()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, CreateBcatService                 },
                { 1, CreateDeliveryCacheStorageService }
            };
        }

        public long CreateBcatService(ServiceCtx context)
        {
            long id = context.RequestData.ReadInt64();

            MakeObject(context, new IBcatService());

            return 0;
        }

        public long CreateDeliveryCacheStorageService(ServiceCtx context)
        {
            long id = context.RequestData.ReadInt64();

            MakeObject(context, new IDeliveryCacheStorageService());

            return 0;
        }
    }
}
