using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Am
{
    class IStorage : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public byte[] Data { get; }

        public IStorage(byte[] data)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, Open }
            };

            Data = data;
        }

        public long Open(ServiceCtx context)
        {
            MakeObject(context, new IStorageAccessor(this));

            return 0;
        }
    }
}