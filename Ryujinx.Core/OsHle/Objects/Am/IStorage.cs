using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

using static Ryujinx.Core.OsHle.Objects.ObjHelper;

namespace Ryujinx.Core.OsHle.Objects.Am
{
    class IStorage : IIpcInterface
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public byte[] Data { get; private set; }

        public IStorage(byte[] Data)
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, Open }
            };

            this.Data = Data;
        }

        public long Open(ServiceCtx Context)
        {
            MakeObject(Context, new IStorageAccessor(this));

            return 0;
        }
    }
}