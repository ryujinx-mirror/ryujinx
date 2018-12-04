using System.Collections.Generic;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.FileSystem;

namespace Ryujinx.HLE.HOS.Services.Lr
{
    class ILocationResolverManager : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public ILocationResolverManager()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, OpenLocationResolver }
            };
        }

        // OpenLocationResolver()
        private long OpenLocationResolver(ServiceCtx context)
        {
            StorageId storageId = (StorageId)context.RequestData.ReadByte();

            MakeObject(context, new ILocationResolver(storageId));

            return 0;
        }
    }
}
