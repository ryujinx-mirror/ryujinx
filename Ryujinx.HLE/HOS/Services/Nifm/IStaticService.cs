using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Nifm
{
    class IStaticService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public IStaticService()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 4, CreateGeneralServiceOld },
                { 5, CreateGeneralService    }
            };
        }

        public long CreateGeneralServiceOld(ServiceCtx context)
        {
            MakeObject(context, new IGeneralService());

            return 0;
        }

        public long CreateGeneralService(ServiceCtx context)
        {
            MakeObject(context, new IGeneralService());

            return 0;
        }
    }
}