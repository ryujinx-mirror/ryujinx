using System;
using System.Collections.Generic;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.FileSystem;

namespace Ryujinx.HLE.HOS.Services.Lr
{
    class ILocationResolverManager : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ILocationResolverManager()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, OpenLocationResolver },
            };
        }

        // OpenLocationResolver()
        private long OpenLocationResolver(ServiceCtx Context)
        {
            StorageId StorageId = (StorageId)Context.RequestData.ReadByte();

            MakeObject(Context, new ILocationResolver(StorageId));

            return 0;
        }
    }
}
